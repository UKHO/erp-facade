using System.Diagnostics.CodeAnalysis;
using Azure.Extensions.AspNetCore.Configuration.Secrets;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using Newtonsoft.Json.Serialization;
using SoapCore;
using UKHO.ERPFacade.Common.Configuration;
using UKHO.ERPFacade.Common.Operations.IO.Azure;
using UKHO.SAP.MockAPIService.Filters;
using UKHO.SAP.MockAPIService.Services;

namespace UKHO.SAP.MockAPIService
{
    [ExcludeFromCodeCoverage]
    internal class Program
    {
        [ExcludeFromCodeCoverage]
        internal static void Main(string[] args)
        {
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

            // Add services to the container.

            builder.Services.AddSoapCore();

            builder.Services.Configure<SapConfiguration>(configuration.GetSection("SapConfiguration"));
            builder.Services.Configure<AzureStorageConfiguration>(configuration.GetSection("AzureStorageConfiguration"));

            builder.Services.AddSingleton<Iz_adds_mat_info, z_adds_mat_info>();
            builder.Services.AddSingleton<Iz_adds_ros, z_adds_ros>();
            builder.Services.AddSingleton<IAzureBlobReaderWriter, AzureBlobReaderWriter>();
            builder.Services.AddHealthChecks();

            builder.Services.AddControllers(o =>
            {
                o.AllowEmptyInputInBodyModelBinding = true;
            }).AddNewtonsoftJson(options =>
            {
                options.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
            });

            var app = builder.Build();

            app.UseHttpsRedirection();

            app.UseWhen(context => (!context.Request.Path.StartsWithSegments("/api") &&
                   !context.Request.Path.StartsWithSegments("/health")), appBuilder =>
            {
                appBuilder.BasicAuthCustomMiddleware();
            });

            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.UseSoapEndpoint<Iz_adds_mat_info>("/z_adds_mat_info.asmx", new SoapEncoderOptions(), SoapSerializer.XmlSerializer);
                endpoints.UseSoapEndpoint<Iz_adds_ros>("/z_adds_ros.asmx", new SoapEncoderOptions(), SoapSerializer.XmlSerializer);
                endpoints.MapHealthChecks("/health");
            });

            app.Run();
        }
    }
}
