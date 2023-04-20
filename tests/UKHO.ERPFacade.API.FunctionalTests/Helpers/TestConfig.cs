using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UKHO.ERPFacade.API.FunctionalTests.Helpers
{
        public class TestConfig
        {
            public string PayloadFolder { get; set; }
            public string WebhookPayloadFileName { get; set; }
            public Erpfacadeconfiguration ErpFacadeConfiguration { get; set; }
        }

        public class Erpfacadeconfiguration
        {
            public string BaseUrl { get; set; }
        }
}
