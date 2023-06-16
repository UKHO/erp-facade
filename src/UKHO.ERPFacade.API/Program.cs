using Azure.Extensions.AspNetCore.Configuration.Secrets;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using Microsoft.ApplicationInsights.AspNetCore.Extensions;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Newtonsoft.Json.Serialization;
using Serilog;
using System.Diagnostics.CodeAnalysis;
using System.IO.Abstractions;
using System.Reflection;
using UKHO.ERPFacade.API.Filters;
using UKHO.ERPFacade.API.Helpers;
using UKHO.ERPFacade.API.Models;
using UKHO.ERPFacade.API.Services;
using UKHO.ERPFacade.Common.Configuration;
using UKHO.ERPFacade.Common.HealthCheck;
using UKHO.ERPFacade.Common.HttpClients;
using UKHO.ERPFacade.Common.IO;
using UKHO.ERPFacade.Common.IO.Azure;
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
            WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
            IConfiguration configuration = builder.Configuration;
            IWebHostEnvironment webHostEnvironment = builder.Environment;

            builder.Host.ConfigureAppConfiguration((hostingContext, config) =>
            {
                config.SetBasePath(webHostEnvironment.ContentRootPath)
                .AddJsonFile("appsettings.json", false, true)
                .AddJsonFile($"appsettings.{webHostEnvironment.EnvironmentName}.json", true, true)
                .AddJsonFile("ConfigurationFiles/ScenarioRules.json", true, true)
                .AddJsonFile("ConfigurationFiles/ActionNumbers.json", true, true)
                .AddJsonFile("ConfigurationFiles/SapActions.json", true, true)
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

            var azureAdConfiguration = new AzureADConfiguration();
            builder.Configuration.Bind("AzureADConfiguration", azureAdConfiguration);

            builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                   .AddJwtBearer("AzureAD", options =>
                   {
                       options.Audience = azureAdConfiguration.ClientId;
                       options.Authority = $"{azureAdConfiguration.MicrosoftOnlineLoginUrl}{azureAdConfiguration.TenantId}";
                   });

            builder.Services.AddAuthorization(options =>
            {
                options.DefaultPolicy = new AuthorizationPolicyBuilder()
                .RequireAuthenticatedUser()
                .AddAuthenticationSchemes("AzureAD")
                .Build();
            });

            builder.Services.AddAuthorization(options =>
            {
                options.AddPolicy("WebhookCaller", policy => policy.RequireRole("WebhookCaller"));
                options.AddPolicy("PriceInformationApiCaller", policy => policy.RequireRole("PriceInformationApiCaller"));
            });

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
                options.Limits.MaxRequestBodySize = 50 * 1024 * 1024;
            });

            builder.Services.Configure<AzureStorageConfiguration>(configuration.GetSection("AzureStorageConfiguration"));
            builder.Services.Configure<SapConfiguration>(configuration.GetSection("SapConfiguration"));            
            builder.Services.Configure<SapActionConfiguration>(configuration.GetSection("SapActionConfiguration"));

            sapActionConfiguration = configuration.GetSection("SapActionConfiguration").Get<SapActionConfiguration>()!;

            builder.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

            builder.Services.AddScoped<IAzureTableReaderWriter, AzureTableReaderWriter>();
            builder.Services.AddScoped<IAzureBlobEventWriter, AzureBlobEventWriter>();
            builder.Services.AddScoped<ISapConfiguration, SapConfiguration>();
            builder.Services.AddScoped<ISapMessageBuilder, SapMessageBuilder>();            
            builder.Services.AddScoped<IXmlHelper, XmlHelper>();
            builder.Services.AddScoped<IFileSystemHelper, FileSystemHelper>();
            builder.Services.AddScoped<IFileSystem, FileSystem>();
            builder.Services.AddScoped<IErpFacadeService, ErpFacadeService>();
            builder.Services.AddScoped<IJsonHelper, JsonHelper>();

            builder.Services.AddHealthChecks()
                .AddCheck<SapServiceHealthCheck>("SapServiceHealthCheck");

            builder.Services.AddHttpClient<ISapClient, SapClient>(c =>
            {
                c.BaseAddress = new Uri(configuration.GetValue<string>("SapConfiguration:BaseAddress"));
            });

            var app = builder.Build();

            app.UseLoggingMiddleware();

            app.UseHttpsRedirection();

            app.UseCorrelationIdMiddleware();

            app.MapControllers();

            app.UseAuthorization();

            app.MapHealthChecks("/health");

            app.UseAuthentication();

            app.Run();
        }
    }
}