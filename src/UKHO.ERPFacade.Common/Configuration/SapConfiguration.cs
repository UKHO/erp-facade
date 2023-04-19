using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UKHO.ERPFacade.Common.Configuration
{
    [ExcludeFromCodeCoverage]
    public class SapConfiguration : ISapConfiguration
    {
        public string BaseAddress { get; set; }

        public string SapServiceOperation { get; set; }

        public string Username { get; set; }

        public string Password { get; set; }
    }
}
