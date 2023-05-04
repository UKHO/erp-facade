using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UKHO.ERPFacade.API.FunctionalTests.Helpers
{
    public class JsonPayloadHelper
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
        public Data Data { get; set; }
    }

    public class Data
    {
        [JsonProperty("traceId")]
        public string TraceId { get; set; }

        [JsonProperty("products")]
        public Product[] Products { get; set; }

        [JsonProperty("unitsOfSale")]
        public UnitOfSale[] UnitsOfSales { get; set; }
    }

    public class Product
    {
        [JsonProperty("productType")]
        public string ProductType { get; set; }

        [JsonProperty("dataSetName")]
        public string DataSetName { get; set; }

        [JsonProperty("productName")]
        public string ProductName { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("scale")]
        public int Scale { get; set; }

        [JsonProperty("usageBand")]
        public int UsageBand { get; set; }

        [JsonProperty("editionNumber")]
        public string EditionNumber { get; set; }

        [JsonProperty("updateNumber")]
        public string UpdateNumber { get; set; }

        [JsonProperty("mayAffectHoldings")]
        public bool MayAffectHoldings { get; set; }

        [JsonProperty("contentChanged")]
        public bool ContentChanged { get; set; }

        [JsonProperty("permit")]
        public string Permit { get; set; }

        [JsonProperty("providerCode")]
        public string ProviderCode { get; set; }

        [JsonProperty("providerDesc")]
        public string ProviderDesc { get; set; }

        [JsonProperty("size")]
        public string Size { get; set; }

        [JsonProperty("agency")]
        public string Agency { get; set; }

        [JsonProperty("bundle")]
        public List<Bundle> Bundle { get; set; }

        [JsonProperty("status")]
        public Status Status { get; set; }

        [JsonProperty("replaces")]
        public List<string> Replaces { get; set; }

        [JsonProperty("replacedBy")]
        public List<string> ReplacedBy { get; set; }

        [JsonProperty("additionalCoverage")]
        public List<string> AdditionalCoverage { get; set; }

        [JsonProperty("geographicLimit")]
        public GeographicLimit GeographicLimit { get; set; }

        [JsonProperty("inUnitsOfSale")]
        public List<string> InUnitsOfSale { get; set; }

        [JsonProperty("s63")]
        public S63 S63 { get; set; }

        [JsonProperty("signature")]
        public Signature Signature { get; set; }

        [JsonProperty("ancillaryFiles")]
        public List<AncillaryFile> AncillaryFiles { get; set; }
    }

    public class UnitOfSale
    {
        [JsonProperty("unitName")]
        public string UnitName { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("unitOfSaleType")]
        public string UnitOfSaleType { get; set; }

        [JsonProperty("unitSize")]
        public string UnitSize { get; set; }

        [JsonProperty("unitType")]
        public string UnitType { get; set; }

        [JsonProperty("status")]
        public string Status { get; set; }

        [JsonProperty("isNewUnitOfSale")]
        public bool IsNewUnitOfSale { get; set; }

        [JsonProperty("geographicLimit")]
        public GeographicLimit GeographicLimit { get; set; }

        [JsonProperty("compositionChanges")]
        public CompositionChanges CompositionChanges { get; set; }
    }

    public class Bundle
    {
        [JsonProperty("bundleType")]
        public string BundleType { get; set; }

        [JsonProperty("location")]
        public string Location { get; set; }
    }


    public class Status
    {
        [JsonProperty("statusName")]
        public string StatusName { get; set; }

        [JsonProperty("statusDate")]
        public DateTime StatusDate { get; set; }

        [JsonProperty("isNewCell")]
        public bool IsNewCell { get; set; }
    }

    public class GeographicLimit
    {
        [JsonProperty("boundingBox")]
        public BoundingBox BoundingBox { get; set; }

        [JsonProperty("polygons")]
        public List<Polygons> Polygons { get; set; }
    }

    public class BoundingBox
    {
        [JsonProperty("northLimit")]
        public double NorthLimit { get; set; }

        [JsonProperty("southLimit")]
        public double SouthLimit { get; set; }

        [JsonProperty("eastLimit")]
        public double EastLimit { get; set; }

        [JsonProperty("westLimit")]
        public double WestLimit { get; set; }
    }

    public class Polygons
    {
        [JsonProperty("polygon")]
        public List<Polygon> Polygon { get; set; }
    }

    public class Polygon
    {
        [JsonProperty("latitude")]
        public double Latitude { get; set; }

        [JsonProperty("longitude")]
        public double Longitude { get; set; }
    }

    public class S63
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("hash")]
        public string Hash { get; set; }

        [JsonProperty("location")]
        public string Location { get; set; }

        [JsonProperty("fileSize")]
        public string FileSize { get; set; }

        [JsonProperty("compression")]
        public bool Compression { get; set; }

        [JsonProperty("s57Crc")]
        public string S57Crc { get; set; }
    }

    public class Signature
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("hash")]
        public string Hash { get; set; }

        [JsonProperty("location")]
        public string Location { get; set; }

        [JsonProperty("fileSize")]
        public string FileSize { get; set; }
    }

    public class AncillaryFile
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("hash")]
        public string Hash { get; set; }

        [JsonProperty("location")]
        public string Location { get; set; }

        [JsonProperty("fileSize")]
        public string FileSize { get; set; }
    }

    public class CompositionChanges
    {
        [JsonProperty("addProducts")]
        public List<string> AddProducts { get; set; }

        [JsonProperty("removeProducts")]
        public List<string> RemoveProducts { get; set; }
    }
}
