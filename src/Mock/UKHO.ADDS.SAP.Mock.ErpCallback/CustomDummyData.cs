using System.Text.Json.Serialization;

namespace UKHO.ADDS.SAP.Mock.ErpCallback
{
    public class CustomDummyData
    {
        [JsonPropertyName("CustomProductDummies")]
        public CustomProductDummy[]? CustomProductDummies { get; set; }
        
        [JsonPropertyName("DefaultProductDummy")]
        public DefaultProductDummy? DefaultProductDummy { get; set; }
    }

    public class CustomProductDummy : DefaultProductDummy
    {
        [JsonPropertyName("Name")]
        public string? Name { get; set; }
    }

    public class DefaultProductDummy
    {
        [JsonPropertyName("Durations")]
        public string[]? Durations { get; set; }
    }
}