using Azure.Extensions.AspNetCore.Configuration.Secrets;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using UKHO.ERPFacade.Common.Configuration;
using UKHO.Logging.EventHubLogProvider;

namespace UKHO.ERPFacade.WebJob
{
    [ExcludeFromCodeCoverage]
    public static class Program
    {
        private static readonly InMemoryChannel TelemetryChannel = new();
        private static readonly string WebJobAssemblyVersion = Assembly.GetExecutingAssembly().GetCustomAttributes<AssemblyFileVersionAttribute>().Single().Version;

        public static void Main()
        {
            try
            {
                int delayTime = 5000;

                //Build configuration
                IConfigurationRoot configuration = BuildConfiguration();

                ServiceCollection serviceCollection = new();

                //Configure required services
                ConfigureServices(serviceCollection, configuration);

                //Create service provider. This will be used in logging.
                ServiceProvider serviceProvider = serviceCollection.BuildServiceProvider();

                try
                {
                    serviceProvider.GetService<ErpFacadeWebJob>().Start();
                }
                finally
                {
                    //Ensure all buffered app insights logs are flushed into Azure
                    TelemetryChannel.Flush();
                    Task.Delay(delayTime);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception: {ex.Message}{Environment.NewLine} Stack trace: {ex.StackTrace}");
                throw;
            }
        }

        private static IConfigurationRoot BuildConfiguration()
        {
            IConfigurationBuilder configBuilder = new ConfigurationBuilder().AddJsonFile("appsettings.json", false, true);

            //Add environment specific configuration files.
            string environmentName = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
            if (!string.IsNullOrWhiteSpace(environmentName))
            {
                configBuilder.AddJsonFile($"appsettings.{environmentName}.json", optional: true);
            }

            using (var config = (ConfigurationRoot)configBuilder.Build())
            {
                string kvServiceUri = config["KeyVaultSettings:ServiceUri"];
                if (!string.IsNullOrWhiteSpace(kvServiceUri))
                {
                    var secretClient = new SecretClient(new Uri(kvServiceUri), new DefaultAzureCredential(
                                                            new DefaultAzureCredentialOptions()));
                    configBuilder.AddAzureKeyVault(secretClient, new KeyVaultSecretManager());
                }
            }
#if DEBUG
            //Add development overrides configuration
            configBuilder.AddJsonFile("appsettings.local.overrides.json", true, true);
#endif

            //Add environment variables
            configBuilder.AddEnvironmentVariables();

            return configBuilder.Build();
        }

        private static void ConfigureServices(IServiceCollection serviceCollection, IConfiguration configuration)
        {
            serviceCollection.AddApplicationInsightsTelemetryWorkerService();

#if DEBUG
            //create the logger and setup of sinks, filters and properties	
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Information()
                .WriteTo.File("Logs/UKHO.ERPFacade.WebJob-.txt", rollingInterval: RollingInterval.Day, outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss} [{Level}] [{SourceContext}] {Message}{NewLine}{Exception}")
                .CreateLogger();
#endif
            serviceCollection.AddLogging(loggingBuilder =>
            {
                loggingBuilder.AddConfiguration(configuration.GetSection("Logging"));
                loggingBuilder.AddConsole();
                loggingBuilder.AddDebug();
                loggingBuilder.AddSerilog();
                loggingBuilder.AddAzureWebAppDiagnostics();

                EventHubLoggingConfiguration eventHubConfig = configuration.GetSection("EventHubLoggingConfiguration").Get<EventHubLoggingConfiguration>();

                if (eventHubConfig != null && !string.IsNullOrWhiteSpace(eventHubConfig.ConnectionString))
                {
                    loggingBuilder.AddEventHub(config =>
                    {
                        config.Environment = eventHubConfig.Environment;
                        config.DefaultMinimumLogLevel =
                            (LogLevel)Enum.Parse(typeof(LogLevel), eventHubConfig.MinimumLoggingLevel, true);
                        config.MinimumLogLevels["UKHO"] =
                            (LogLevel)Enum.Parse(typeof(LogLevel), eventHubConfig.UkhoMinimumLoggingLevel, true);
                        config.EventHubConnectionString = eventHubConfig.ConnectionString;
                        config.EventHubEntityPath = eventHubConfig.EntityPath;
                        config.System = eventHubConfig.System;
                        config.Service = eventHubConfig.Service;
                        config.NodeName = eventHubConfig.NodeName;
                        config.AdditionalValuesProvider = additionalValues =>
                        {
                            additionalValues["_AssemblyVersion"] = WebJobAssemblyVersion;
                        };
                    });
                }
            });

            serviceCollection.Configure<TelemetryConfiguration>(
                (config) =>
                {
                    config.TelemetryChannel = TelemetryChannel;
                }
            );

            if (configuration != null)
            {
                serviceCollection.Configure<ErpFacadeWebJobConfiguration>(configuration.GetSection("ErpFacadeWebJobConfiguration"));
                serviceCollection.Configure<AzureStorageConfiguration>(configuration.GetSection("AzureStorageConfiguration"));

                serviceCollection.AddSingleton(configuration);
            }

            serviceCollection.AddSingleton<ErpFacadeWebJob>();
        }
    }
}