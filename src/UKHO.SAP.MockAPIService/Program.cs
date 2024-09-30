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
            var sapBaseAddress = configuration.GetSection("SapConfiguration:SapBaseAddress").Value;

            services.Configure<WireMockServerSettings>(options =>
            {
                options.Urls = [sapBaseAddress];
                options.StartAdminInterface = true;
                options.ReadStaticMappings = true;
                options.WatchStaticMappings = true;
            });

            services.Configure<SapConfiguration>(configuration.GetSection("SapConfiguration"));

            services.AddSingleton<StubFactory>();
            services.AddHostedService<StubManagerHostedService>();
        }
    }
}
