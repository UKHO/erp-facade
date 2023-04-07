using Azure.Extensions.AspNetCore.Configuration.Secrets;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using Newtonsoft.Json.Serialization;
using Serilog;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using UKHO.ERPFacade.API.Filters;
using UKHO.ERPFacade.Common.Configuration;
using UKHO.ERPFacade.Common.Helpers;
using UKHO.Logging.EventHubLogProvider;

namespace UKHO.ERPFacade
{
    [ExcludeFromCodeCoverage]
    internal class Program
    {
        [ExcludeFromCodeCoverage]
        internal static void Main(string[] args)
        {
            EventHubLoggingConfiguration eventHubLoggingConfiguration;
            IHttpContextAccessor httpContextAccessor = new HttpContextAccessor();
            WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
            IConfiguration configuration = builder.Configuration;
            IWebHostEnvironment webHostEnvironment = builder.Environment;

            builder.Host.ConfigureAppConfiguration((hostingContext, config) =>
            {
                config.SetBasePath(webHostEnvironment.ContentRootPath)
                .AddJsonFile("appsettings.json", false, true)
                .AddJsonFile($"appsettings.{webHostEnvironment.EnvironmentName}.json", true, true)
#if DEBUG
                //Add development overrides configuration
                .AddJsonFile("appsettings.local.overrides.json", true, true)
#endif
                .AddEnvironmentVariables();
            });

            string kvServiceUri = configuration["KeyVaultSettings:ServiceUri"];
            if (!string.IsNullOrWhiteSpace(kvServiceUri))
            {
                var secretClient = new SecretClient(new Uri(kvServiceUri), new DefaultAzureCredential(
                new DefaultAzureCredentialOptions()));
                builder.Configuration.AddAzureKeyVault(secretClient, new KeyVaultSecretManager());
            }
#if DEBUG
            //create the logger and setup of sinks, filters and properties	
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Information()
                .WriteTo.File("Logs/UKHO.ERPFacade.API-.txt", rollingInterval: RollingInterval.Day, outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss} [{Level}] [{SourceContext}] {Message}{NewLine}{Exception}")
                .CreateLogger();
#endif

            eventHubLoggingConfiguration = configuration.GetSection("EventHubLoggingConfiguration").Get<EventHubLoggingConfiguration>()!;

            builder.Host.ConfigureLogging(logging =>
            {
                logging.ClearProviders();
                if (!string.IsNullOrWhiteSpace(eventHubLoggingConfiguration.ConnectionString))
                {
                    void ConfigAdditionalValuesProvider(IDictionary<string, object> additionalValues)
                    {
                        if (httpContextAccessor.HttpContext != null)
                        {
                            additionalValues["_Environment"] = eventHubLoggingConfiguration.Environment;
                            additionalValues["_System"] = eventHubLoggingConfiguration.System;
                            additionalValues["_Service"] = eventHubLoggingConfiguration.Service;
                            additionalValues["_NodeName"] = eventHubLoggingConfiguration.NodeName;
                            additionalValues["_RemoteIPAddress"] = httpContextAccessor.HttpContext.Connection.RemoteIpAddress.ToString();
                            additionalValues["_User-Agent"] = httpContextAccessor.HttpContext.Request.Headers["User-Agent"].FirstOrDefault() ?? string.Empty;
                            additionalValues["_AssemblyVersion"] = Assembly.GetExecutingAssembly().GetCustomAttributes<AssemblyFileVersionAttribute>().Single().Version;
                            additionalValues["_X-Correlation-ID"] = httpContextAccessor.HttpContext.Request.Headers?[CorrelationIdMiddleware.XCorrelationIdHeaderKey].FirstOrDefault() ?? string.Empty;
                        }
                    }
                    logging.AddEventHub(config =>
                    {
                        config.Environment = eventHubLoggingConfiguration.Environment;
                        config.DefaultMinimumLogLevel =
                            (LogLevel)Enum.Parse(typeof(LogLevel), eventHubLoggingConfiguration.MinimumLoggingLevel, true);
                        config.MinimumLogLevels["UKHO"] =
                            (LogLevel)Enum.Parse(typeof(LogLevel), eventHubLoggingConfiguration.UkhoMinimumLoggingLevel, true);
                        config.EventHubConnectionString = eventHubLoggingConfiguration.ConnectionString;
                        config.EventHubEntityPath = eventHubLoggingConfiguration.EntityPath;
                        config.System = eventHubLoggingConfiguration.System;
                        config.Service = eventHubLoggingConfiguration.Service;
                        config.NodeName = eventHubLoggingConfiguration.NodeName;
                        config.AdditionalValuesProvider = ConfigAdditionalValuesProvider;
                    });
                }
            });

            builder.Services.AddLogging(loggingBuilder =>
            {
                loggingBuilder.AddConfiguration(configuration.GetSection("Logging"));
                loggingBuilder.AddConsole();
                loggingBuilder.AddDebug();
                loggingBuilder.AddSerilog();
                loggingBuilder.AddAzureWebAppDiagnostics();
            });

            builder.Services.AddHeaderPropagation(options =>
            {
                options.Headers.Add(CorrelationIdMiddleware.XCorrelationIdHeaderKey);
            });

            // The following line enables Application Insights telemetry collection.	
            builder.Services.AddApplicationInsightsTelemetry();

            // Add services to the container.
            builder.Services.AddControllers(o =>
            {
                o.AllowEmptyInputInBodyModelBinding = true;
            }).AddNewtonsoftJson(options =>
            {
                options.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
            });

            builder.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            builder.Services.Configure<SapConfiguration>(configuration.GetSection("SapConfiguration"));

            builder.Services.AddHttpClient<ISapClient, SapClient>(c =>
            {
                c.BaseAddress = new Uri(configuration.GetValue<string>("SapConfiguration:BaseAddress"));
            });

            var app = builder.Build();

            app.UseHttpsRedirection();

            app.UseCorrelationIdMiddleware();

            app.MapControllers();

            app.Run();
        }
    }
}
