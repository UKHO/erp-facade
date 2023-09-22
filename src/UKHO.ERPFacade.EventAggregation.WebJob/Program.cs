using System.Diagnostics.CodeAnalysis;
using System.IO.Abstractions;
using System.Reflection;
using Azure.Extensions.AspNetCore.Configuration.Secrets;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Events;
using UKHO.ERPFacade.Common.Configuration;
using UKHO.ERPFacade.Common.HttpClients;
using UKHO.ERPFacade.Common.IO;
using UKHO.ERPFacade.Common.IO.Azure;
using UKHO.ERPFacade.EventAggregation.WebJob.Helpers;
using UKHO.ERPFacade.EventAggregation.WebJob.Services;
using UKHO.Logging.EventHubLogProvider;

namespace UKHO.ERPFacade.EventAggregation.WebJob
{
    [ExcludeFromCodeCoverage]
    public static class Program
    {
        private static readonly InMemoryChannel TelemetryChannel = new();
        private static IConfiguration? ConfigurationBuilder;
        private static readonly string WebJobAssemblyVersion = Assembly.GetExecutingAssembly().GetCustomAttributes<AssemblyFileVersionAttribute>().Single().Version;

        public static void Main(string[] args)
        {
            HostBuilder hostBuilder = BuildHostConfiguration();
            IHost host = hostBuilder.Build();

            using (host)
            {
                host.Run();
            }
        }

        private static HostBuilder BuildHostConfiguration()
        {
            HostBuilder hostBuilder = new();
            hostBuilder.ConfigureAppConfiguration((hostContext, builder) =>
            {
                builder.AddJsonFile("appsettings.json");
                //Add environment specific configuration files.
                var environmentName = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
                if (!string.IsNullOrWhiteSpace(environmentName))
                {
                    builder.AddJsonFile($"appsettings.{environmentName}.json", optional: true);
                }
                var tempConfig = builder.Build();
                string kvServiceUri = tempConfig["KeyVaultSettings:ServiceUri"];
                if (!string.IsNullOrWhiteSpace(kvServiceUri))
                {
                    var secretClient = new SecretClient(new Uri(kvServiceUri), new DefaultAzureCredential(new DefaultAzureCredentialOptions()));
                    builder.AddAzureKeyVault(secretClient, new KeyVaultSecretManager());
                }
#if DEBUG
                //Add development overrides configuration
                builder.AddJsonFile("appsettings.local.overrides.json", true, true);
#endif
                //Add environment variables
                builder.AddEnvironmentVariables();
                ConfigurationBuilder = builder.Build();
            })
             .ConfigureLogging((hostContext, builder) =>
             {
                 builder.AddConfiguration(ConfigurationBuilder.GetSection("Logging"));
#if DEBUG
                 builder.AddSerilog(new LoggerConfiguration()
                                 .WriteTo.File("Logs/UKHO.ERPFacade.EventAggregation.WebJob-.txt", outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss} [{Level}] [{SourceContext}] {Message}{NewLine}{Exception}")
                                 .MinimumLevel.Information()
                                 .MinimumLevel.Override("UKHO", LogEventLevel.Debug)
                                 .CreateLogger(), dispose: true);
#endif
                 builder.AddConsole();

                 EventHubLoggingConfiguration eventhubConfig = ConfigurationBuilder.GetSection("EventHubLoggingConfiguration").Get<EventHubLoggingConfiguration>();
                 if (!string.IsNullOrWhiteSpace(eventhubConfig.ConnectionString))
                 {
                     builder.AddEventHub(config =>
                     {
                         config.Environment = eventhubConfig.Environment;
                         config.DefaultMinimumLogLevel =
                             (LogLevel)Enum.Parse(typeof(LogLevel), eventhubConfig.MinimumLoggingLevel, true);
                         config.MinimumLogLevels["UKHO"] =
                             (LogLevel)Enum.Parse(typeof(LogLevel), eventhubConfig.UkhoMinimumLoggingLevel, true);
                         config.EventHubConnectionString = eventhubConfig.ConnectionString;
                         config.EventHubEntityPath = eventhubConfig.EntityPath;
                         config.System = eventhubConfig.System;
                         config.Service = eventhubConfig.Service;
                         config.NodeName = eventhubConfig.NodeName;
                         config.AdditionalValuesProvider = additionalValues =>
                         {
                             additionalValues["_AssemblyVersion"] = WebJobAssemblyVersion;
                         };
                     });
                 }
             })
             .ConfigureServices((hostContext, services) =>
             {
                 services.AddApplicationInsightsTelemetryWorkerService();

                 services.Configure<TelemetryConfiguration>(
                     (config) =>
                     {
                         config.TelemetryChannel = TelemetryChannel;
                     }
                 );

                 var buildServiceProvider = services.BuildServiceProvider();
                 services.Configure<AzureStorageConfiguration>(ConfigurationBuilder.GetSection("AzureStorageConfiguration"));
                 services.Configure<QueuesOptions>(ConfigurationBuilder.GetSection("QueuesOptions"));
                 services.Configure<SapConfiguration>(ConfigurationBuilder.GetSection("SapConfiguration"));

                 services.AddSingleton<IAggregationService, AggregationService>();
                 services.AddSingleton<IAzureTableReaderWriter, AzureTableReaderWriter>();
                 services.AddSingleton<IAzureBlobEventWriter, AzureBlobEventWriter>();
                 services.AddSingleton<IRecordOfSaleSapMessageBuilder, RecordOfSaleSapMessageBuilder>();
                 services.AddSingleton<IXmlHelper, XmlHelper>();
                 services.AddSingleton<IFileSystemHelper, FileSystemHelper>();
                 services.AddSingleton<IFileSystem, FileSystem>();
                 services.AddDistributedMemoryCache();

                 services.AddHttpClient<ISapClient, SapClient>(c =>
                 {
                     c.BaseAddress = new Uri(ConfigurationBuilder.GetValue<string>("SapConfiguration:SapEndpointBaseAddressForRecordOfSale"));
                 });
             })
              .ConfigureWebJobs(b =>
              {
                  b.AddAzureStorageCoreServices()
                  .AddAzureStorageQueues()
                  .AddAzureStorageBlobs();
              });

            return hostBuilder;
        }
    }
}
