namespace UKHO.SAP.MockAPIService.Configuration
{
    public class SapConfiguration
    {
        public required string SapBaseAddress { get; set; }
        public required string SapEndpointForEncEvent { get; set; }
        public required string SapEndpointForRecordOfSale { get; set; }
    }
}
