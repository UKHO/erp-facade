using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace UKHO.ERPFacade.API.FunctionalTests.Model
{

    public class JsonOutputPriceChangeHelper
    {
       
        public string specversion { get; set; }
        public string type { get; set; }
        public string source { get; set; }
        public string id { get; set; }
        public string time { get; set; }
        public string _COMMENT { get; set; }
        public string subject { get; set; }
        public string datacontenttype { get; set; }
        [JsonProperty("data")]
        public DataPriceChange data { get; set; }

    }

        // Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse);
        public class DataPriceChange
    {
            public string correlationId { get; set; }
            public List<UnitsOfSalePricePriceChangeOutput> unitsOfSalePrices { get; set; }
        }

        public class PricePriceChangeOutput
        {
            public DateTime effectiveDate { get; set; }
            public string currency { get; set; }
            public StandardPriceChangeOutput standard { get; set; }
        }

        public class PriceDurationsPriceChangeOutput
        {
            public string numberOfMonths { get; set; }
            public string rrp { get; set; }
        }

        

        public class StandardPriceChangeOutput
        {
            public List<PriceDurationsPriceChangeOutput> priceDurations { get; set; }
        }

        public class UnitsOfSalePricePriceChangeOutput
        {
            public string unitName { get; set; }
            public List<PricePriceChangeOutput> price { get; set; }
        }
        public class EffectiveDatesPerProductPC
    {
        public string ProductName { get; set; }
        public DateTime EffectiveDates { get; set; }

        public string Duration { get; set; }

        public string rrp { get; set; }

    }
    public class PriceDurations
    {
        public string  Duration { get; set; }

        public string rrp { get; set; }
    }


}


    



    

