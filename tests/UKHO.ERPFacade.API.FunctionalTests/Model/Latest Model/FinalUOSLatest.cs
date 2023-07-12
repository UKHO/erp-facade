namespace UKHO.ERPFacade.API.FunctionalTests.Model.Latest_Model
{
    public class FinalUoSOutput
    {
        public string specversion { get; set; }
        public string type { get; set; }
        public string source { get; set; }
        public string id { get; set; }
        public DateTime time { get; set; }
        public string _COMMENT { get; set; }
        public string subject { get; set; }
        public string datacontenttype { get; set; }
        public Data data { get; set; }
        public string Subject { get; set; }
    }

    public class Data
    {
        public string correlationId { get; set; }
        public Product[] products { get; set; }
        public string _COMMENT { get; set; }
        public Unitsofsale[] unitsOfSale { get; set; }
        public UnitsofSalePrice[] unitsOfSalePrices { get; set; }
    }

    public class Product
    {
        public string productType { get; set; }
        public string dataSetName { get; set; }
        public string productName { get; set; }
        public string title { get; set; }
        public int scale { get; set; }
        public int usageBand { get; set; }
        public int editionNumber { get; set; }
        public int updateNumber { get; set; }
        public bool mayAffectHoldings { get; set; }
        public bool contentChange { get; set; }
        public string permit { get; set; }
        public string providerCode { get; set; }
        public string providerName { get; set; }
        public string size { get; set; }
        public string agency { get; set; }
        public Bundle[] bundle { get; set; }
        public Status status { get; set; }
        public string[] replaces { get; set; }
        public string[] replacedBy { get; set; }
        public object[] additionalCoverage { get; set; }
        public Geographiclimit geographicLimit { get; set; }
        public string[] inUnitsOfSale { get; set; }
        public S63 s63 { get; set; }
        public Signature signature { get; set; }
        public Ancillaryfile[] ancillaryFiles { get; set; }
    }

    public class Status
    {
        public string statusName { get; set; }
        public DateTime statusDate { get; set; }
        public bool isNewCell { get; set; }
    }

    public class Geographiclimit
    {
        public Boundingbox boundingBox { get; set; }
        public Polygon[] polygons { get; set; }
    }

    public class Boundingbox
    {
        public float northLimit { get; set; }
        public float southLimit { get; set; }
        public float eastLimit { get; set; }
        public float westLimit { get; set; }
    }

    public class Polygon
    {
        public Polygon1[] polygon { get; set; }
    }

    public class Polygon1
    {
        public float latitude { get; set; }
        public float longitude { get; set; }
    }

    public class S63
    {
        public string name { get; set; }
        public string hash { get; set; }
        public string location { get; set; }
        public string fileSize { get; set; }
        public bool compression { get; set; }
        public string s57Crc { get; set; }
    }

    public class Signature
    {
        public string name { get; set; }
        public string hash { get; set; }
        public string location { get; set; }
        public string fileSize { get; set; }
    }

    public class Bundle
    {
        public string bundleType { get; set; }
        public string location { get; set; }
    }

    public class Ancillaryfile
    {
        public string name { get; set; }
        public string hash { get; set; }
        public string location { get; set; }
        public string fileSize { get; set; }
    }

    public class Unitsofsale
    {
        public string unitName { get; set; }
        public string title { get; set; }
        public string unitOfSaleType { get; set; }
        public string unitSize { get; set; }
        public string unitType { get; set; }
        public string status { get; set; }
        public bool isNewUnitOfSale { get; set; }
        public Geographiclimit1 geographicLimit { get; set; }
        public Compositionchanges compositionChanges { get; set; }
    }

    public class Geographiclimit1
    {
        public Boundingbox1 boundingBox { get; set; }
        public Polygon2[] polygons { get; set; }
    }

    public class Boundingbox1
    {
        public float northLimit { get; set; }
        public float southLimit { get; set; }
        public float eastLimit { get; set; }
        public float westLimit { get; set; }
    }

    public class Polygon2
    {
        public Polygon3[] polygon { get; set; }
    }

    public class Polygon3
    {
        public float latitude { get; set; }
        public float longitude { get; set; }
    }

    public class Compositionchanges
    {
        public string[] addProducts { get; set; }
        public string[] removeProducts { get; set; }
    }

    public class UnitsofSalePrice
    {
        public string unitName { get; set; }
        public Price[] price { get; set; }
    }

    public class Price
    {
        public DateTime effectiveDate { get; set; }
        public string currency { get; set; }
        public Standard standard { get; set; }
    }

    public class Standard
    {
        public Priceduration[] priceDurations { get; set; }
    }

    public class Priceduration
    {
        public int numberOfMonths { get; set; }
        public string rrp { get; set; }
    }

    public class EffectiveDatesPerProduct
    {
        public string ProductName { get; set; }
        public DateTime EffectiveDates { get; set; }

        public int Duration { get; set; }

        public string rrp { get; set; }

    }

}


