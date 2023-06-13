using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UKHO.ERPFacade.API.FunctionalTests.Model
{
    internal class ProductUoSInfo
    {
        
        [JsonProperty("unitSize")]
        public string UnitSize { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("unitType")]
        public string UnitType { get; set; }

        [JsonProperty("unitOfSaleType")]
        public string UnitOfSaleType;

        [JsonProperty("unitName")]
        public string UnitName;


    }
}
