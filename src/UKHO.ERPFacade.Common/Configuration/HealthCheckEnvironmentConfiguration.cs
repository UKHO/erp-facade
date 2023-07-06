using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UKHO.ERPFacade.Common.Configuration
{
    public class HealthCheckEnvironmentConfiguration
    {
        public string Environment { get; set; }

        public string ExcludeEnvironment { get; set; }
    }
}
