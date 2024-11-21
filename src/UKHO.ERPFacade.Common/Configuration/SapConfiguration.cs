using System.Diagnostics.CodeAnalysis;

namespace UKHO.ERPFacade.Common.Configuration
{
    [ExcludeFromCodeCoverage]
    public class SapConfiguration
    {
        public string SapBaseAddress { get; set; }

        public string SapEndpointForEncEvent { get; set; }

        public string SapServiceOperationForEncEvent { get; set; }

        public string SapUsernameForEncEvent { get; set; }

        public string SapPasswordForEncEvent { get; set; }

        public string SapEndpointForRecordOfSale { get; set; }

        public string SapServiceOperationForRecordOfSale { get; set; }

        public string SapUsernameForRecordOfSale { get; set; }

        public string SapPasswordForRecordOfSale { get; set; }

        public string SapEndpointForS100Event { get; set; }

        public string SapServiceOperationForS100Event { get; set; }

        public string SapUsernameForS100Event { get; set; }

        public string SapPasswordForS100Event { get; set; }

    }
}
