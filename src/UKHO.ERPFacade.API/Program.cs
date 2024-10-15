using System.Diagnostics.CodeAnalysis;
using System.IO.Abstractions;
using System.Reflection;
using Azure.Extensions.AspNetCore.Configuration.Secrets;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using Elastic.Apm.AspNetCore;
using Microsoft.ApplicationInsights.AspNetCore.Extensions;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Newtonsoft.Json.Serialization;
using Serilog;
using UKHO.ERPFacade.API.Filters;
using UKHO.ERPFacade.API.Health;
using UKHO.ERPFacade.API.Helpers;
using UKHO.ERPFacade.Common.Configuration;
using UKHO.ERPFacade.Common.HealthCheck;
using UKHO.ERPFacade.Common.HttpClients;
using UKHO.ERPFacade.Common.IO;
using UKHO.ERPFacade.Common.IO.Azure;
using UKHO.ERPFacade.Common.Models;
using UKHO.ERPFacade.Common.Providers;
using UKHO.ERPFacade.Common.PermitDecryption;
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
            SapActionConfiguration sapActionConfiguration;

            IHttpContextAccessor httpContextAccessor = new HttpContextAccessor();
            var builder = WebApplication.CreateBuilder(args);
            var configuration = builder.Configuration;
            var webHostEnvironment = builder.Environment;

            builder.Configuration.SetBasePath(webHostEnvironment.ContentRootPath)
                .AddJsonFile("appsettings.json", false, true)
                .AddJsonFile($"appsettings.{webHostEnvironment.EnvironmentName}.json", true, true)
                .AddJsonFile("ConfigurationFiles/SapActions.json", true, true)
#if DEBUG
                //Add development overrides configuration
                .AddJsonFile("appsettings.local.overrides.json", true, true)
#endif
                .AddEnvironmentVariables();

            var kvServiceUri = configuration["KeyVaultSettings:ServiceUri"];
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

            builder.Logging
                .ClearProviders()
                .AddEventHub(config =>
                {
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
                                additionalValues["_User-Agent"] = httpContextAccessor.HttpContext.Request.Headers.UserAgent.FirstOrDefault() ?? string.Empty;
                                additionalValues["_AssemblyVersion"] = Assembly.GetExecutingAssembly().GetCustomAttributes<AssemblyFileVersionAttribute>().Single().Version;
                                additionalValues["_X-Correlation-ID"] = httpContextAccessor.HttpContext.Request.Headers?[CorrelationIdMiddleware.XCorrelationIdHeaderKey].FirstOrDefault() ?? string.Empty;
                            }
                        }
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

            builder.Services.AddAllElasticApm();

            builder.Services.AddHeaderPropagation(options =>
            {
                options.Headers.Add(CorrelationIdMiddleware.XCorrelationIdHeaderKey);
            });

            var azureAdConfiguration = new AzureADConfiguration();
            builder.Configuration.Bind("AzureADConfiguration", azureAdConfiguration);

            builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                   .AddJwtBearer("AzureAD", options =>
                   {
                       options.Audience = azureAdConfiguration.ClientId;
                       options.Authority = $"{azureAdConfiguration.MicrosoftOnlineLoginUrl}{azureAdConfiguration.TenantId}";
                   });

            builder.Services.AddAuthorizationBuilder()
                .SetDefaultPolicy(new AuthorizationPolicyBuilder()
                .RequireAuthenticatedUser()
                .AddAuthenticationSchemes("AzureAD")
                .Build());

            builder.Services.AddAuthorizationBuilder()
                .AddPolicy("S100DataContentPublishedWebhookCaller", policy => policy.RequireRole("S100DataContentPublishedWebhookCaller"))
                .AddPolicy("EncContentPublishedWebhookCaller", policy => policy.RequireRole("EncContentPublishedWebhookCaller"))
                .AddPolicy("RecordOfSaleWebhookCaller", policy => policy.RequireRole("RecordOfSaleWebhookCaller"))
                .AddPolicy("LicenceUpdatedWebhookCaller", policy => policy.RequireRole("LicenceUpdatedWebhookCaller"));

            // The following line enables Application Insights telemetry collection.
            var options = new ApplicationInsightsServiceOptions { ConnectionString = configuration.GetValue<string>("ApplicationInsights:ConnectionString") };
            builder.Services.AddApplicationInsightsTelemetry(options);

            // Add services to the container.
            builder.Services.AddControllers(o =>
            {
                o.AllowEmptyInputInBodyModelBinding = true;
            }).AddNewtonsoftJson(options =>
            {
                options.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
            });

            builder.Services.Configure<KestrelServerOptions>(options =>
            {
                options.Limits.MaxRequestBodySize = 51 * 1024 * 1024;
            });

            builder.Services.Configure<AzureStorageConfiguration>(configuration.GetSection("AzureStorageConfiguration"));
            builder.Services.Configure<SapConfiguration>(configuration.GetSection("SapConfiguration"));
            builder.Services.Configure<SapActionConfiguration>(configuration.GetSection("SapActionConfiguration"));
            sapActionConfiguration = configuration.GetSection("SapActionConfiguration").Get<SapActionConfiguration>()!;
            builder.Services.Configure<EESHealthCheckEnvironmentConfiguration>(configuration.GetSection("EESHealthCheckEnvironmentConfiguration"));
            builder.Services.Configure<PermitConfiguration>(configuration.GetSection("PermitConfiguration"));

            builder.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

            builder.Services.AddScoped<IAzureTableReaderWriter, AzureTableReaderWriter>();
            builder.Services.AddScoped<IAzureBlobEventWriter, AzureBlobEventWriter>();
            builder.Services.AddScoped<IAzureQueueHelper, AzureQueueHelper>();
            builder.Services.AddScoped<IEncContentSapMessageBuilder, EncContentSapMessageBuilder>();
            builder.Services.AddScoped<IXmlHelper, XmlHelper>();
            builder.Services.AddScoped<IFileSystemHelper, FileSystemHelper>();
            builder.Services.AddScoped<IFileSystem, FileSystem>();
            builder.Services.AddScoped<IEESClient, EESClient>();
            builder.Services.AddScoped<ILicenceUpdatedSapMessageBuilder, LicenceUpdatedSapMessageBuilder>();
            builder.Services.AddScoped<IWeekDetailsProvider, WeekDetailsProvider>();
            builder.Services.AddScoped<IPermitDecryption, PermitDecryption>();
            builder.Services.AddScoped<IS100DataContentSapMessageBuilder, S100DataContentSapMessageBuilder>();

            ConfigureHealthChecks(builder);

            builder.Services.AddHttpClient<ISapClient, SapClient>(c =>
            {
                c.BaseAddress = new Uri(configuration.GetValue<string>("SapConfiguration:SapBaseAddress"));
            });

            var app = builder.Build();

            app.UseHttpsRedirection();

            app.UseCorrelationIdMiddleware();

            app.UseLoggingMiddleware();

            app.MapControllers();

            app.UseAuthorization();

            app.MapHealthChecks("/health", new HealthCheckOptions()
            {
                Predicate = x => true,
                ResponseWriter = HealthResponseWriter.WriteHealthCheckUiResponse
            });

            app.UseAuthentication();
            app.Run();
        }

        private static void ConfigureHealthChecks(WebApplicationBuilder builder)
        {
            builder.Services.AddHealthChecks()
                .AddCheck<SapServiceHealthCheck>("SAP Health Check", failureStatus: HealthStatus.Unhealthy)
                .AddCheck<EESServiceHealthCheck>("EES Health Check", failureStatus: HealthStatus.Unhealthy)
                .AddCheck<MemoryHealthCheck>("ERP Memory Check", failureStatus: HealthStatus.Unhealthy);
        }
    }
}
