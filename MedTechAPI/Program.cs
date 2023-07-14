using Hangfire;
using MedTechAPI.AppCore.AppGlobal.CronJobs;
using MedTechAPI.Extensions;
using MedTechAPI.Extensions.Middleware;
using MedTechAPI.Persistence.ModelBuilders;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.Extensions.Configuration;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers(opt =>
{
    var policy = new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build();
    opt.Filters.Add(new AuthorizeFilter(policy));
});
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

#region CUSTOM SERVICES AND DI
builder.Services.AddCustomServiceCollections(builder);
#region Serilog configuration
string blobConstring = Environment.GetEnvironmentVariable(builder.Configuration.GetValue<string>("AppSettings:AzureBlobConfig:BlobStorageConstring") ?? string.Empty, EnvironmentVariableTarget.Process) ?? builder.Configuration.GetValue<string>("AppSettings:AzureBlobConfig:BlobStorageConstring");
var configuration = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json").Build();
var logToAppInsights = Convert.ToBoolean(configuration["AppSettings:LogToAppInsights"]);
if (builder.Environment.IsDevelopment() && !logToAppInsights)
{
    Log.Logger = new LoggerConfiguration().ReadFrom.Configuration(configuration).CreateLogger();
}
else
{
    Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(configuration)
    .WriteTo.AzureBlobStorage(new Serilog.Formatting.Json.JsonFormatter(), connectionString: blobConstring, restrictedToMinimumLevel: Serilog.Events.LogEventLevel.Information, storageContainerName: "onasonicapplogs")
    .WriteTo.Console(formatter: new Serilog.Formatting.Json.JsonFormatter(), restrictedToMinimumLevel: Serilog.Events.LogEventLevel.Information)
    .CreateLogger();
}
#endregion
#endregion

var app = builder.Build();
try
{
    bool seedDatabase = builder.Configuration.GetValue<bool>("AppSettings:DatabaseOptions:SeedDatabase");
    if (seedDatabase)
    {
        app.SeedDefaultsData().Wait();
    }
}
catch (Exception ex) { Log.Error(ex.Message, ex); }
// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.DefaultModelsExpandDepth(-1);
    });
}

app.UseHttpsRedirection();
app.UseCors("DefaultCorsPolicy");

app.UseAuthentication();
app.UseAuthorization();

app.UseSession(new SessionOptions
{
    IdleTimeout = TimeSpan.FromMinutes(60),
    Cookie = new CookieBuilder
    {
        Name = "MedTech.Session",
        HttpOnly = true,
        Expiration = TimeSpan.FromMinutes(60),
    },
});

app.Use(async (context, next) =>
{
    var req = context.Request;
    Console.Write($"::[{req.Method}] request for path: [{req.Path}] || ");
    await next(context);
});
app.Use(async (context, next) =>
{
    var resp = context.Response;
    Console.WriteLine($"Response::[{resp.StatusCode}]");
    await next(context);
});

app.UseMiddleware<AppSessionManager>();

app.UseHangfireDashboard("/hangfire");

app.MapControllers();

AppBackgroundServices.RecurringJobsSchedule();
app.Run();
