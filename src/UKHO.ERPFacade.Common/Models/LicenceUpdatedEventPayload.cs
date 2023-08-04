using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;


namespace UKHO.ERPFacade.Common.Models
{
    [ExcludeFromCodeCoverage]
    public class LicenceUpdatedEventPayload
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
        public LicenseData Data { get; set; }
    }

    public partial class LicenseData
    {
        [JsonProperty("correlationId")]
        public Guid CorrelationId { get; set; }

        [JsonProperty("license")]
        public License License { get; set; }
    }

    public partial class License
    {
        [JsonProperty("licenseId")]
        public string LicenseId { get; set; }

        [JsonProperty("licenseGUID")]
        public string LicenseGuid { get; set; }

        [JsonProperty("productType")]
        public string ProductType { get; set; }

        [JsonProperty("transactionType")]
        public string TransactionType { get; set; }

        [JsonProperty("distributorCustomerNumber")]
        public string DistributorCustomerNumber { get; set; }

        [JsonProperty("shippingCoNumber")]
        public string ShippingCoNumber { get; set; }

        [JsonProperty("ordernumber")]
        public string Ordernumber { get; set; }

        [JsonProperty("orderDate")]
        public DateTimeOffset OrderDate { get; set; }

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

        [JsonProperty("licenceTypeID")]
        public int LicenceTypeId { get; set; }

        [JsonProperty("fleetName")]
        public string FleetName { get; set; }

        [JsonProperty("numberLicenceUsers")]
        public string NumberLicenceUsers { get; set; }

        [JsonProperty("upn")]
        public string Upn { get; set; }

        [JsonProperty("unitsOfSale")]
        public UnitsOfSale[] UnitsOfSale { get; set; }
    }

    public partial class UnitsOfSale
    {
        [JsonProperty("unitName")]
        public string UnitName { get; set; }

        [JsonProperty("endDate")]
        public string EndDate { get; set; }

        [JsonProperty("duration")]
        public string Duration { get; set; }

        [JsonProperty("renew")]
        public string Renew { get; set; }

        [JsonProperty("repeat")]
        public string Repeat { get; set; }
    }
}
