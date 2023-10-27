using System.Diagnostics.CodeAnalysis;
using Newtonsoft.Json;

namespace UKHO.ERPFacade.Common.Models
{
    [ExcludeFromCodeCoverage]
    public class RecordOfSaleEventPayLoad
    {
        [JsonProperty("specversion")]
        public string SpecVersion { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("source")]
        public string Source { get; set; }

        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("time")]
        public string Time { get; set; }

        [JsonProperty("subject")]
        public string Subject { get; set; }

        [JsonProperty("datacontenttype")]
        public string DataContentType { get; set; }

        [JsonProperty("data")]
        public RecordOfSaleData Data { get; set; }
    }

    [ExcludeFromCodeCoverage]
    public class RecordOfSaleData
    {
        [JsonProperty("correlationId")]
        public string CorrelationId { get; set; }

        [JsonProperty("relatedEvents")]
        public List<string> RelatedEvents { get; set; }

        [JsonProperty("recordsOfSale")]
        public RecordsOfSale RecordsOfSale { get; set; }
    }

    [ExcludeFromCodeCoverage]
    public class RecordsOfSale
    {
        [JsonProperty("licenseId")]
        public string LicenceId { get; set; }

        [JsonProperty("productType")]
        public string ProductType { get; set; }

        [JsonProperty("transactionType")]
        public string TransactionType { get; set; }

        [JsonProperty("distributorCustomerNumber")]
        public string DistributorCustomerNumber { get; set; }

        [JsonProperty("shippingCoNumber")]
        public string ShippingCoNumber { get; set; }

        [JsonProperty("orderNumber")]
        public string OrderNumber { get; set; }

        [JsonProperty("orderDate")]
        public string OrderDate { get; set; }

        [JsonProperty("po-ref")]
        public string PoRef { get; set; }

        [JsonProperty("holdingsExpiryDate")]
        public string HoldingsExpiryDate { get; set; }

        [JsonProperty("sapId")]
        public string SapId { get; set; }

        [JsonProperty("vesselName")]
        public string VesselName { get; set; }

        [JsonProperty("imoNumber")]
        public string ImoNumber { get; set; }

        [JsonProperty("callSign")]
        public string CallSign { get; set; }

        [JsonProperty("licenceType")]
        public string LicenceType { get; set; }

        [JsonProperty("shoreBased")]
        public string ShoreBased { get; set; }

        [JsonProperty("fleetName")]
        public string FleetName { get; set; }

        [JsonProperty("numberLicenceUsers")]
        public int? NumberLicenceUsers { get; set; }

        [JsonProperty("ecdisManuf")]
        public string EcdisManuf { get; set; }

        [JsonProperty("licenceDuration")]
        public int? LicenceDuration { get; set; }

        [JsonProperty("unitsOfSale")]
        public List<RosUnitOfSale> RosUnitOfSale { get; set; }
    }
}
