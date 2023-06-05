using Newtonsoft.Json;

namespace UKHO.ERPFacade.API.FunctionalTests.Model
{
    public class UnitOfSalePrice
    {
        public string EventName { get; set; }
        public string Subject { get; set; }
        public Eventdata EventData { get; set; }
    }

    public class Eventdata   
    {
        [JsonProperty("data")]
        public DataPrices Data { get; set; }
    }
    public class DataPrices
    {

        [JsonProperty("unitsOfSalePrices")]
        public UnitsOfSalesPrices[] UnitsOfSalesPrices { get; set; }
    }
    public class UnitsOfSalesPrices
    {
        public string unitName { get; set; }
        public PriceUoS[] price { get; set; }
    }

    public class PriceUoS
    {
        public DateTime effectiveDate { get; set; }
        public string currency { get; set; }
        public StandardUoS standard { get; set; }
        public Pays pays { get; set; }
    }

    public class StandardUoS
    {
        public PricedurationUoS[] priceDurations { get; set; }
    }

    public class PricedurationUoS
    {
        public string numberOfMonths { get; set; }
        public string rrp { get; set; }
    }

    public class Pays
    {
        public Priceduration1[] priceDurations { get; set; }
    }

    public class Priceduration1
    {
        public string numberOfMonths { get; set; }
        public string rrp { get; set; }
    }





}
