using Common.DbAccess;
using Common.Interface;
using Common.Service;
using Hangfire;
using Hangfire.PostgreSql;
using MedTechAPI.AppCore.AppGlobal.Interface;
using MedTechAPI.AppCore.AppGlobal.Mapper;
using MedTechAPI.AppCore.AppGlobal.Repository;
using MedTechAPI.AppCore.Interfaces;
using MedTechAPI.AppCore.MedicCenter.Interface;
using MedTechAPI.AppCore.MedicCenter.Repository;
using MedTechAPI.AppCore.Navigation.Interfaces;
using MedTechAPI.AppCore.Navigation.Repository;
using MedTechAPI.AppCore.PatientService.Interface;
using MedTechAPI.AppCore.PatientService.Repository;
using MedTechAPI.AppCore.ProfileManagement.Repository;
using MedTechAPI.AppCore.Repository;
using MedTechAPI.Domain.Config;
using MedTechAPI.Domain.Entities.SetupConfigurations;
using MedTechAPI.Extensions.Middleware;
using MedTechAPI.Extensions.Services;
using MedTechAPI.Helpers;
using MedTechAPI.Persistence;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace MedTechAPI.Extensions
{
    public static class ServiceExtensions
    {
        public static IServiceCollection AddCustomServiceCollections(this IServiceCollection services, WebApplicationBuilder builder)
        {
            string encryptionKey = Environment.GetEnvironmentVariable("EncryptionKey", EnvironmentVariableTarget.Process) ?? builder.Configuration.GetValue<string>("AppSettings:Encryption:Key");
            string RedisConfig = Environment.GetEnvironmentVariable(builder.Configuration.GetConnectionString("RedisCon") ?? string.Empty, EnvironmentVariableTarget.Process) ?? builder.Configuration.GetConnectionString("RedisCon");
            string MongoDbCon = Environment.GetEnvironmentVariable(builder.Configuration.GetConnectionString("MongoDbConnect") ?? string.Empty, EnvironmentVariableTarget.Process) ?? builder.Configuration.GetConnectionString("MongoDbConnect");
            string dbConstring = Environment.GetEnvironmentVariable(builder.Configuration.GetConnectionString("Default") ?? string.Empty, EnvironmentVariableTarget.Process) ?? builder.Configuration.GetConnectionString("Default");
            string rabbitMqConstring = Environment.GetEnvironmentVariable(builder.Configuration.GetValue<string>("AppSettings:MessageBroker:RabbitMq:ConString") ?? string.Empty, EnvironmentVariableTarget.Process) ?? builder.Configuration.GetValue<string>("AppSettings:MessageBroker:RabbitMq:ConString");

            services.Configure<AppSettings>(builder.Configuration.GetSection(nameof(AppSettings)));
            services.Configure<SessionConfig>(builder.Configuration.GetSection(nameof(SessionConfig)));
            services.Configure<EmailConfig>(builder.Configuration.GetSection(nameof(EmailConfig)));

            //services.AddSingleton<ISqlDataAccess>(new SqlDataAccess(builder.Configuration.GetConnectionString("Default")));
            services.AddSingleton<ISqlDataAccess>((svcProvider) => Factories<SqlDataAccess>.SqlDataAccessService(serviceProvider: svcProvider, conString: dbConstring));
            services.AddScoped<TokenService>((svcProvider) => {

                var sessionConfig = svcProvider.GetRequiredService<IOptions<SessionConfig>>();
                var UserGroupRepo = svcProvider.GetRequiredService<IUserGroupRepository>();
                var MenuRepo = svcProvider.GetRequiredService<IMenuRepository>();
                return new TokenService(encryptionKey, sessionConfig, UserGroupRepo, MenuRepo); 
            });
            services.AddSingleton<IMongoDataAccess>((svcProvider) => new MongoDataAccess(MongoDbCon, "onasonic"));

            services.AddDbContext<AppDbContext>(options =>
                options.UseNpgsql(dbConstring, options => options.CommandTimeout((int)TimeSpan.FromMinutes(10).TotalSeconds))
                .UseLoggerFactory(LoggerFactory.Create(buildr =>
                {
                    if (builder.Environment.IsDevelopment())
                    {
                        buildr.AddDebug();
                    }
                }))
                );

            services.AddHangfire(x => x.UsePostgreSqlStorage(dbConstring));
            services.AddHangfireServer();

            try
            {
                services.AddScoped<ICacheService>((svcProvider) =>
                {
                    var cxnMultiplexer = StackExchange.Redis.ConnectionMultiplexer.Connect(RedisConfig);
                    var appSettings = svcProvider.GetRequiredService<IOptions<AppSettings>>();
                    return new CacheService(cxnMultiplexer, appSettings.Value.AppKey);
                });
            }
            catch (Exception ex)
            {
                OnaxTools.Logger.LogException(ex, "[RedisConnection]");
            }
            services.AddStackExchangeRedisCache((options) =>
            {
                options.Configuration = RedisConfig;
                options.InstanceName = string.Concat(builder.Configuration.GetValue<string>($"{nameof(AppSettings)}:{nameof(AppSettings.AppKey)}"), "Session:");
                //options.Configuration = 
            });

            services.AddSession((sessionOptions) =>
            {
                sessionOptions.Cookie.Name = "onasc.cookie";
                sessionOptions.IdleTimeout = TimeSpan.FromMinutes(60);
                //sessionOptions.Cookie.IsEssential = true;
            });

            services.AddHttpContextAccessor();
            services.AddHttpClient();

            services.AddAuthentication(opt =>
            {
                opt.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                opt.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddCookie(x =>
            {
                x.Cookie.Name = "jwt";
            }).AddJwtBearer(options =>
            {
                byte[] key = Array.Empty<byte>();
                if (builder.Environment.IsDevelopment())
                {
                    key = Encoding.UTF8.GetBytes(builder.Configuration.GetValue<string>("AppSettings:Encryption:Key"));
                }
                else
                {
                    key = Encoding.UTF8.GetBytes(encryptionKey);
                }
                options.RequireHttpsMetadata = false;
                options.SaveToken = false;
                options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
                {
                    //TODO: Confirm/Add sliding expiration
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = false,
                    ValidateAudience = false,
                };

                options.Events = new JwtBearerEvents
                {
                    OnMessageReceived = context =>
                    {
                        string tokenName = builder.Configuration.GetValue<string>($"{nameof(SessionConfig)}:{nameof(SessionConfig.Auth)}:{nameof(SessionConfig.Auth.token)}");
                        context.Token = context.Request.Cookies[key: tokenName];
                        return Task.CompletedTask;
                    }
                };
            });

            var configMap = new AutoMapper.MapperConfiguration(cfg =>
            {
                cfg.AddProfile(new AutoMapperProfile());
            });
            var mapper = configMap.CreateMapper();
            services.AddSingleton(mapper);

            services.AddCors(opt =>
            {
                opt.AddPolicy("DefaultCorsPolicy", policy =>
                {
                    policy.WithOrigins("http://localhost:4500", "https://localhost:4500")
                            .AllowAnyHeader()
                            .AllowAnyMethod()
                            .AllowCredentials();
                });
            });

            services.AddScoped<IUserServiceRepository, UserServiceRepository>();
            services.AddScoped<IAppSessionContextRepository, AppSessionContextRepository>();
            services.AddScoped<IUserGroupRepository, UserGroupRepository>();

            services.AddScoped<IMenuRepository, MenuRepository>();

            services.AddScoped<IAppActivityLogRepository, AppActivityLogRepository>();
            services.AddScoped<IMessageRepository, MessageServiceRepository>();

            services.AddScoped<IHMORepository, HMORepository>();
            services.AddScoped<IPatientRespository, PatientRespository>();
            services.AddScoped<ICountryRepository, CountryRepository>();
            services.AddScoped<IStateLocationRepository, StateLocationRepository>();
            services.AddScoped<IGenderCategoryRespository, GenderCategoryRespository>();
            services.AddScoped<ISalutationRepository, SalutationRepository>();

            services.AddScoped<IMedicCompanyRepository, MedicCompanyRepository>();
            services.AddScoped<IMedicBranchRepository, MedicBranchRepository>();
            services.AddScoped<IFilesUploadHelperService, FilesUploadHelperService>();

            return services;
        }
    }
}
