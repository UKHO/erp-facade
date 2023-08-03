using System.Diagnostics.CodeAnalysis;

namespace UKHO.ERPFacade.Common.Configuration
{
    [ExcludeFromCodeCoverage]
    public class SapConfiguration : ISapConfiguration
    {
        public string BaseAddress { get; set; }

        public string SapServiceOperation { get; set; }

        public string Username { get; set; }

        public string Password { get; set; }

        public string BaseAddressRos { get; set; }

        public string SapServiceOperationRos { get; set; }

        public string UsernameRos { get; set; }

        public string PasswordRos { get; set; }
    }
}
