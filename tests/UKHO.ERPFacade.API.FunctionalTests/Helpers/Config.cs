using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Security.Authentication.ExtendedProtection;

namespace UKHO.ERPFacade.API.FunctionalTests.Helpers
{
    public class Config
    {
        public TestConfig TestConfig { get; set; }

        public Config()
        {
            IConfiguration ConfigurationRoot = new ConfigurationBuilder()
                               .AddJsonFile("appsettings.json", false)
#if DEBUG
                //Add development overrides configuration
                .AddJsonFile("appsettings.local.overrides.json", true, true)
#endif
                               .AddEnvironmentVariables()
                               .Build();

            
            TestConfig = new();
            ConfigurationRoot.Bind(TestConfig);
         


        }
    }
}
