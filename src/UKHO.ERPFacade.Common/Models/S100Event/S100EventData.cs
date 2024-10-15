﻿using Newtonsoft.Json;

namespace UKHO.ERPFacade.Common.Models.S100Event
{
    public class S100EventData
    {
        [JsonProperty("correlationId")]
        public string CorrelationId { get; set; }
        [JsonProperty("products")]
        public List<S100Product> Products { get; set; }
        [JsonProperty("unitsOfSale")]
        public List<S100UnitsOfSale> UnitsOfSale { get; set; }
    }
}