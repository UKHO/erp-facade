namespace UKHO.ERPFacade.API.FunctionalTests.Model
{
    public class LUpdatedJsonPayloadHelper
    {
        public string specversion { get; set; }
        public string type { get; set; }
        public string source { get; set; }
        public string id { get; set; }
        public DateTime time { get; set; }
        public string subject { get; set; }
        public string datacontenttype { get; set; }
        public Data data { get; set; }

        public class Data
        {
            public string correlationId { get; set; }
            public License license { get; set; }
        }

        public class License
        {
            public int licenseId { get; set; }
            public string licenseGUID { get; set; }
            public string productType { get; set; }
            public string transactionType { get; set; }
            public int distributorCustomerNumber { get; set; }
            public int shippingCoNumber { get; set; }
            public int ordernumber { get; set; }
            public string orderDate { get; set; }

            [JsonProperty("po-ref")]
            public string poref { get; set; }
            public string holdingsExpiryDate { get; set; }
            public int sapId { get; set; }
            public string vesselName { get; set; }
            public string imoNumber { get; set; }
            public string callSign { get; set; }
            public string licenceType { get; set; }
            public int licenceTypeID { get; set; }
            public string fleetName { get; set; }
            public string numberLicenceUsers { get; set; }
            public string upn { get; set; }
        }
    }
}
