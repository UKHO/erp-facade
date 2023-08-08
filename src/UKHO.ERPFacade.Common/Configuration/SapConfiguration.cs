using System.Diagnostics.CodeAnalysis;

namespace UKHO.ERPFacade.Common.Configuration
{
    [ExcludeFromCodeCoverage]
    public class SapConfiguration
    {
        public string BaseAddress { get; set; }

        public string SapServiceOperation { get; set; }

        public string Username { get; set; }

        public string Password { get; set; }

        public string RosBaseAddress { get; set; }

        public string RosUsername { get; set; }

        public string RosPassword { get; set; }
    }
}
