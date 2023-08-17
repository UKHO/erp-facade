using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace UKHO.ERPFacade.Common.Models
{
    public class LicenceUpdatedEventPayLoad
    {
        [JsonProperty("specversion")]
        public string Specversion { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("source")]
        public Uri Source { get; set; }

        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("time")]
        public DateTimeOffset Time { get; set; }

        [JsonProperty("subject")]
        public string Subject { get; set; }

        [JsonProperty("datacontenttype")]
        public string Datacontenttype { get; set; }

        [JsonProperty("data")]
        public LicenceData Data { get; set; }
    }

    public partial class LicenceData
    {
        [JsonProperty("correlationId")]
        public Guid CorrelationId { get; set; }

        [JsonProperty("license")]
        public License License { get; set; }
    }

    public partial class License
    {
        [JsonProperty("licenseId")]
        public long LicenseId { get; set; }

        [JsonProperty("licenseGUID")]
        public string LicenseGuid { get; set; }

        [JsonProperty("productType")]
        public string ProductType { get; set; }

        [JsonProperty("transactionType")]
        public string TransactionType { get; set; }

        [JsonProperty("distributorCustomerNumber")]
        public long DistributorCustomerNumber { get; set; }

        [JsonProperty("shippingCoNumber")]
        public long ShippingCoNumber { get; set; }

        [JsonProperty("ordernumber")]
        public long Ordernumber { get; set; }

        [JsonProperty("orderDate")]
        public DateTimeOffset OrderDate { get; set; }

        [JsonProperty("po-ref")]
        public string PoRef { get; set; }

        [JsonProperty("holdingsExpiryDate")]
        public string HoldingsExpiryDate { get; set; }

        [JsonProperty("sapId")]
        public long SapId { get; set; }

        [JsonProperty("vesselName")]
        public string VesselName { get; set; }

        [JsonProperty("imoNumber")]
        public string ImoNumber { get; set; }

        [JsonProperty("callSign")]
        public string CallSign { get; set; }

        [JsonProperty("licenceType")]
        public string LicenceType { get; set; }

        [JsonProperty("licenceTypeID")]
        public long LicenceTypeId { get; set; }

        [JsonProperty("fleetName")]
        public string FleetName { get; set; }

        [JsonProperty("numberLicenceUsers")]
        public string NumberLicenceUsers { get; set; }

        [JsonProperty("upn")]
        public string Upn { get; set; }
    }
}
