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

            services.Configure<S57EncEventConfiguration>(configuration.GetSection("S57EncEventConfiguration"));
            services.Configure<RecordOfSaleEventConfiguration>(configuration.GetSection("RecordOfSaleEventConfiguration"));
            services.Configure<S100DataEventConfiguration>(configuration.GetSection("S100DataEventConfiguration"));
            services.Configure<EesConfiguration>(configuration.GetSection("EesConfiguration"));
            services.Configure<SapCallbackConfiguration>(configuration.GetSection("SapCallbackConfiguration"));
            services.Configure<ErpFacadeConfiguration>(configuration.GetSection("ErpFacadeConfiguration"));

            services.AddSingleton<StubFactory>();
            services.AddHostedService<StubManagerHostedService>();
        }
    }
}
