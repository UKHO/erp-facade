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
            public List<UnitsOfSalePrice> unitsOfSalePrices { get; set; }
        }

        public class Price
        {
            public DateTime effectiveDate { get; set; }
            public string currency { get; set; }
            public Standard standard { get; set; }
        }

        public class PriceDuration
        {
            public int numberOfMonths { get; set; }
            public string rrp { get; set; }
        }

        public class Root
        {
            
        }

        public class Standard
        {
            public List<PriceDuration> priceDurations { get; set; }
        }

        public class UnitsOfSalePrice
        {
            public string unitName { get; set; }
            public List<Price> price { get; set; }
        }


    }


    



    

