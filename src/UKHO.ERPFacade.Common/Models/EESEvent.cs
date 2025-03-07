﻿using Newtonsoft.Json;
using System.Diagnostics.CodeAnalysis;

namespace UKHO.ERPFacade.Common.Models
{
    [ExcludeFromCodeCoverage]
    public class Product
    {
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
        public int? EditionNumber { get; set; }

        [JsonProperty("updateNumber")]
        public int? UpdateNumber { get; set; }

        [JsonProperty("mayAffectHoldings")]
        public bool MayAffectHoldings { get; set; }

        [JsonProperty("contentChange")]
        public bool ContentChange { get; set; }

        [JsonProperty("permit")]
        public string Permit { get; set; }

        [JsonProperty("providerCode")]
        public string ProviderCode { get; set; }

        [JsonProperty("providerName")]
        public string ProviderName { get; set; }

        [JsonProperty("size")]
        public string Size { get; set; }

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

    [ExcludeFromCodeCoverage]
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

        [JsonProperty("providerCode")]
        public string ProviderCode { get; set; }

        [JsonProperty("providerName")]
        public string ProviderName { get; set; }
    }

    [ExcludeFromCodeCoverage]
    public class Bundle
    {
        [JsonProperty("bundleType")]
        public string BundleType { get; set; }

        [JsonProperty("location")]
        public string Location { get; set; }
    }

    [ExcludeFromCodeCoverage]
    public class Status
    {
        [JsonProperty("statusName")]
        public string StatusName { get; set; }

        [JsonProperty("statusDate")]
        public DateTime StatusDate { get; set; }

        [JsonProperty("isNewCell")]
        public bool IsNewCell { get; set; }
    }

    [ExcludeFromCodeCoverage]
    public class GeographicLimit
    {
        [JsonProperty("boundingBox")]
        public BoundingBox BoundingBox { get; set; }
    }

    [ExcludeFromCodeCoverage]
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

    [ExcludeFromCodeCoverage]
    public class S63
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("hash")]
        public string Hash { get; set; }

        [JsonProperty("fileSize")]
        public int FileSize { get; set; }

        [JsonProperty("compression")]
        public bool Compression { get; set; }

        [JsonProperty("s57Crc")]
        public string S57Crc { get; set; }
    }

    [ExcludeFromCodeCoverage]
    public class Signature
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("hash")]
        public string Hash { get; set; }

        [JsonProperty("fileSize")]
        public int FileSize { get; set; }
    }

    [ExcludeFromCodeCoverage]
    public class AncillaryFile
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("hash")]
        public string Hash { get; set; }

        [JsonProperty("fileSize")]
        public int FileSize { get; set; }
    }

    [ExcludeFromCodeCoverage]
    public class CompositionChanges
    {
        [JsonProperty("addProducts")]
        public List<string> AddProducts { get; set; }

        [JsonProperty("removeProducts")]
        public List<string> RemoveProducts { get; set; }
    }

    [ExcludeFromCodeCoverage]
    public class UnitsOfSalePrices
    {
        [JsonProperty("unitName")]
        public string UnitName { get; set; }

        [JsonProperty("price")]
        public List<Price> Price { get; set; }
    }

    [ExcludeFromCodeCoverage]
    public class Price
    {
        [JsonProperty("effectiveDate")]
        public DateTimeOffset EffectiveDate { get; set; }

        [JsonProperty("currency")]
        public string Currency { get; set; }

        [JsonProperty("standard")]
        public Standard Standard { get; set; }
    }

    [ExcludeFromCodeCoverage]
    public class Standard
    {
        [JsonProperty("priceDurations")]
        public List<PriceDurations> PriceDurations { get; set; }
    }

    [ExcludeFromCodeCoverage]
    public class PriceDurations
    {
        [JsonProperty("numberOfMonths")]
        public int NumberOfMonths { get; set; }

        [JsonProperty("rrp")]
        public decimal Rrp { get; set; }
    }

    public enum Provider
    {
        ICE = 1,
        ICE_GB = 2,
        ICE_UK = 3,
        PRIMAR = 4,
        VAR_Unique = 5,
        VAR = 6
    }

    public enum Size
    {
        LARGE = 1,
        MEDIUM = 2,
        SMALL = 3
    }

    public enum BundleType
    {
        DVD = 1
    }

    public enum ProductStatus
    {
        NewEdition = 1,
        Reissue = 2,
        Update = 3,
        CancellationUpdate = 4,
        Withdrawn = 5,
        Suspended = 6
    }
}
