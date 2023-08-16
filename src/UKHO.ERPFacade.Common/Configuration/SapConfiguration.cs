using System.Diagnostics.CodeAnalysis;

namespace UKHO.ERPFacade.Common.Configuration
{
    [ExcludeFromCodeCoverage]
    public class SapConfiguration
    {
        public string SapEndpointBaseAddressForEncEvent { get; set; }

        public string SapServiceOperationForEncEvent { get; set; }

        public string SapUsernameForEncEvent { get; set; }

        public string SapPasswordForEncEvent { get; set; }

        public string SapEndpointBaseAddressForRecordOfSale { get; set; }

        public string SapServiceOperationForRecordOfSale { get; set; }

        public string SapUsernameForRecordOfSale { get; set; }

        public string SapPasswordForRecordOfSale { get; set; }
    }
}
