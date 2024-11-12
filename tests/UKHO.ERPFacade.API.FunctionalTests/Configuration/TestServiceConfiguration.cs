using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace UKHO.ERPFacade.API.FunctionalTests.Configuration
{
    public static class TestServiceConfiguration
    {
        /// <summary>
        /// This method is used to load the app settings configuration 
        /// </summary>
        /// <returns></returns>
        public static IConfigurationRoot LoadConfiguration()
        {
            var configBuilder = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: false);
            var configurationRoot = configBuilder.Build();

            return configurationRoot;
        }

        /// <summary>
        /// This method is used to load and set the values of app settings configs. 
        /// </summary>
        /// <returns></returns>
        public static ServiceProvider ConfigureServices()
        {
            var services = new ServiceCollection();
            services.AddOptions();

            var configurationRoot = LoadConfiguration();

            services.Configure<AzureADConfiguration>(configurationRoot.GetSection("AzureADConfiguration"));
            services.Configure<ErpFacadeConfiguration>(configurationRoot.GetSection("ErpFacadeConfiguration"));
            services.Configure<AzureStorageConfiguration>(configurationRoot.GetSection("AzureStorageConfiguration"));

            return services.BuildServiceProvider();
        }
    }
}
