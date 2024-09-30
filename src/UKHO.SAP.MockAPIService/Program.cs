using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using UKHO.SAP.MockAPIService.Configuration;
using UKHO.SAP.MockAPIService.StubSetup;
using WireMock.Settings;

namespace UKHO.SAP.MockAPIService
{
    internal static class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        private static IHostBuilder CreateHostBuilder(string[] args)
        => Host.CreateDefaultBuilder(args)
            .ConfigureServices((host, services) => ConfigureServices(services, host.Configuration));

        private static void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<WireMockServerSettings>(configuration.GetSection("WireMockServerSettings"));

            services.Configure<EncEventConfiguration>(configuration.GetSection("EncEventConfiguration"));
            services.Configure<RecordOfSaleEventConfiguration>(configuration.GetSection("RecordOfSaleEventConfiguration"));

            services.AddSingleton<StubFactory>();
            services.AddHostedService<StubManagerHostedService>();
        }
    }
}
