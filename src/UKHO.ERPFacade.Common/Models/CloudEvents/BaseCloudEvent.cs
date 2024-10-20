using Newtonsoft.Json;

namespace UKHO.ERPFacade.Common.Models.CloudEvents
{
    public class BaseCloudEvent<T> where T : class
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
        public T Data { get; set; }

        public BaseCloudEvent(T data)
        {
            Data = data;
        }
    }
}
