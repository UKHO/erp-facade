using Microsoft.Extensions.Configuration;

namespace UKHO.ERPFacade.API.FunctionalTests.Helpers
{
    public class Config
    {
        public TestConfig TestConfig { get; set; }

        public Config()
        {
            IConfiguration ConfigurationRoot = new ConfigurationBuilder()
                               .AddJsonFile("appsettings.json", false)
                               .AddEnvironmentVariables()
                               .Build();

            TestConfig = new();
            ConfigurationRoot.Bind(TestConfig);
        }
    }
}
