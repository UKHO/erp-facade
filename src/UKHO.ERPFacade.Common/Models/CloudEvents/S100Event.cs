using System.Diagnostics.CodeAnalysis;
using Newtonsoft.Json;

namespace UKHO.ERPFacade.Common.Models.CloudEvents.S100
{
    [ExcludeFromCodeCoverage]
    public class S100Event : BaseCloudEvent { }

    [ExcludeFromCodeCoverage]
    public class S100EventData
    {
        [JsonProperty("correlationId")]
        public string CorrelationId { get; set; }

        [JsonProperty("products")]
        public List<Product> Products { get; set; }

        [JsonProperty("unitsOfSale")]
        public List<UnitOfSale> UnitsOfSales { get; set; }
    }

    [ExcludeFromCodeCoverage]
    public class Product
    {
        [JsonProperty("productType")]
        public string ProductType { get; set; }
        [JsonProperty("productIdentifier")]
        public string ProductIdentifier { get; set; }
        [JsonProperty("productName")]
        public string ProductName { get; set; }
        [JsonProperty("editionNumber")]
        public int EditionNumber { get; set; }
        [JsonProperty("updateNumber")]
        public int UpdateNumber { get; set; }
        [JsonProperty("mayAffectHoldings")]
        public bool MayAffectHoldings { get; set; }
        [JsonProperty("contentChange")]
        public bool ContentChange { get; set; }
        [JsonProperty("providerName")]
        public string ProviderName { get; set; }
        [JsonProperty("producingAgency")]
        public string ProducingAgency { get; set; }
        [JsonProperty("status")]
        public S100Status Status { get; set; }
        [JsonProperty("replaces")]
        public List<string> Replaces { get; set; }
        [JsonProperty("dataReplacement")]
        public List<string> DataReplacement { get; set; }
        [JsonProperty("boundingBox")]
        public S100BoundingBox BoundingBox { get; set; }
        [JsonProperty("fileSize")]
        public int FileSize { get; set; }
        [JsonProperty("inUnitsOfSale")]
        public List<string> InUnitsOfSale { get; set; }
    }

    [ExcludeFromCodeCoverage]
    public class UnitOfSale
    {
        [JsonProperty("unitName")]
        public string UnitName { get; set; }
        [JsonProperty("title")]
        public string Title { get; set; }
        [JsonProperty("unitSize")]
        public string UnitSize { get; set; }
        [JsonProperty("status")]
        public string Status { get; set; }
        [JsonProperty("isNewUnitOfSale")]
        public bool IsNewUnitOfSale { get; set; }
        [JsonProperty("boundingBox")]
        public S100BoundingBox BoundingBox { get; set; }
        [JsonProperty("compositionChanges")]
        public S100CompositionChanges CompositionChanges { get; set; }
    }

    [ExcludeFromCodeCoverage]
    public class S100Status
    {
        [JsonProperty("statusName")]
        public string StatusName { get; set; }
        [JsonProperty("statusDate")]
        public DateTime StatusDate { get; set; }
        [JsonProperty("isNewCell")]
        public bool IsNewCell { get; set; }
    }

    [ExcludeFromCodeCoverage]
    public class S100BoundingBox
    {
        [JsonProperty("northLimit")]
        public double NorthLimit { get; set; }
        [JsonProperty("southLimit")]
        public int SouthLimit { get; set; }
        [JsonProperty("eastLimit")]
        public int EastLimit { get; set; }
        [JsonProperty("westLimit")]
        public double WestLimit { get; set; }
    }

    [ExcludeFromCodeCoverage]
    public class S100CompositionChanges
    {
        [JsonProperty("addProducts")]
        public List<string> AddProducts { get; set; }
        [JsonProperty("removeProducts")]
        public List<string> RemoveProducts { get; set; }
    }
}
