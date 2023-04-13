using UKHO.ERPFacade.API;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace UKHO.ERPFacade.API.FunctionalTests.Helpers
{
    public class TestConfiguration
    {
        protected IConfigurationRoot ConfigurationRoot;
        public ErpFacadeLocalConfiguration erpfacadeLocalConfig = new();
        public ErpFacadeDevConfiguration erpfacadeDevConfig = new();
        public ESSApiConfiguration EssConfig = new();
        public SapMockConfiguration sapMockConfig= new();
        public SapConfiguration sapConfig = new();
        public class ErpFacadeLocalConfiguration
        {
            public string BaseUrl { get; set; }
        }
        public class ErpFacadeDevConfiguration
        {
            public string BaseUrl { get; set; }
        }

        public class ESSApiConfiguration
        {
            public string BaseUrl { get; set; }
            public string MicrosoftOnlineLoginUrl { get; set; }
            public string TenantId { get; set; }
            public string AutoTestClientId { get; set; }
            public string AutoTestClientSecret { get; set; }
            public string EssClientId { get; set; }
            public bool IsRunningOnLocalMachine { get; set; }
        }
        public class SapMockConfiguration
        {
            public string BaseAddress { get; set; }
            public string SapServiceOperation { get; set; }
            public string Username { get; set; }
            public string Password { get; set; }
        }
        public class SapConfiguration
        {
            public string BaseAddress { get; set; }
            public string SapServiceOperation { get; set; }
            public string Username { get; set; }
            public string Password { get; set; }
        }

        public TestConfiguration()
        {
            ConfigurationRoot = new ConfigurationBuilder()
                               .AddJsonFile("appsettings.json", false)
                               .Build();

            ConfigurationRoot.Bind("ErpFacadeLocalConfiguration", erpfacadeLocalConfig);
            ConfigurationRoot.Bind("ErpFacadeDevConfiguration", erpfacadeDevConfig);
            ConfigurationRoot.Bind("ESSApiConfiguration", EssConfig);
            ConfigurationRoot.Bind("SapMockConfiguration", sapMockConfig);
            ConfigurationRoot.Bind("SapConfiguration", sapConfig);
           
        }
    }
}
