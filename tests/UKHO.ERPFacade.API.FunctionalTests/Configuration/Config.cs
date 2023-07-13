using Microsoft.Extensions.Configuration;

namespace UKHO.ERPFacade.API.FunctionalTests.Configuration
{
    public static class Config
    {
        public static TestConfig TestConfig { get; set; }

        public static void ConfigSetup()
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
