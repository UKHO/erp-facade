﻿using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace UKHO.ERPFacade.Common.Infrastructure.EventService.EventProvider
{
    [ExcludeFromCodeCoverage]
    public class CloudEvent<T>
    {
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonPropertyName("specversion")]
        public string SpecVersion { get; set; }

        [JsonPropertyName("type")]
        public string Type { get; set; }

        [JsonPropertyName("source")]
        public string Source { get; set; }

        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("time")]
        public DateTime Time { get; set; }

        [JsonPropertyName("subject")]
        public string Subject { get; set; }

        [JsonPropertyName("datacontenttype")]
        public string DataContentType { get; set; }

        [JsonPropertyName("data")]
        public T Data { get; set; }
    }
}
