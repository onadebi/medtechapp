using Common.DbAccess;

namespace MedTechAPI.Helpers
{
    public class Factories<T> where T : class
    {
        public static ILogger<T> AppLoggerFactory(IConfiguration AppLogLevel)
        {
            var config = AppLogLevel;
            ILogger<T> logger = LoggerFactory.Create(builder =>
            {
                var logLevel = config.GetSection("Logging");
                builder.AddConfiguration(logLevel);
                builder.AddDebug(); //does all log levels
                builder.AddConsole();
            }).CreateLogger<T>();
            return logger;
        }

        public static ISqlDataAccess SqlDataAccessService(IServiceProvider serviceProvider, string conString)
        {
            var env = serviceProvider.GetRequiredService<IWebHostEnvironment>();
            var config = serviceProvider.GetRequiredService<IConfiguration>();
            ILogger<SqlDataAccess> logger = Factories<SqlDataAccess>.AppLoggerFactory(config);
            if (env.IsDevelopment())
            {
                string devAppConstring = string.IsNullOrWhiteSpace(conString) ? config.GetConnectionString("Default") : conString;
                //string devConString = Environment.GetEnvironmentVariable(devAppConstring, EnvironmentVariableTarget.Process) ?? string.Empty;
                return new SqlDataAccess(logger, conString: devAppConstring);
            }
            else
            {
                return new SqlDataAccess(logger, conString);
            }
        }
    }
}
