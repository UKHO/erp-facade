using Microsoft.Extensions.Configuration;

namespace UKHO.ERPFacade.API.FunctionalTests.Configuration
{
    public static class Config
    {
        public static TestConfig TestConfig { get; set; }

        public static void ConfigSetup()
        {
            IConfiguration configurationRoot = new ConfigurationBuilder()
                               .AddJsonFile("appsettings.json", false)
#if DEBUG
                //Add development overrides configurationsdfsdfsdf
                .AddJsonFile("appsettings.local.overrides.json", true, true)
#endif
                               .AddEnvironmentVariables()
                               .Build();


            TestConfig = new();
            configurationRoot.Bind(TestConfig);

        }
    }
}
