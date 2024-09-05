using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Unicode;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace UKHO.ERPFacade.API.Health
{
    public static class HealthResponseWriter
    {
        private const string DefaultContentType = "application/json";

        private static readonly byte[] _emptyResponse = "{}"u8.ToArray();
        private static readonly Lazy<JsonSerializerOptions> _options = new(CreateJsonOptions);

        public static async Task WriteHealthCheckUiResponse(HttpContext httpContext, HealthReport report) 
        {
            if (report != null)
            {
                httpContext.Response.ContentType = DefaultContentType;

                ErpHealthReport uiReport = ErpHealthReport.CreateFrom(report);

                await JsonSerializer.SerializeAsync(httpContext.Response.Body, uiReport, _options.Value).ConfigureAwait(false);
            }
            else
            {
                await httpContext.Response.BodyWriter.WriteAsync(_emptyResponse).ConfigureAwait(false);
            }
        }
        private static JsonSerializerOptions CreateJsonOptions()
        {
            JsonSerializerOptions options = new()
            {
                AllowTrailingCommas = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                Encoder = JavaScriptEncoder.Create(UnicodeRanges.All),
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                WriteIndented = true
            };

            options.Converters.Add(new JsonStringEnumConverter());
            options.Converters.Add(new TimeSpanConverter());

            return options;
        }
    }

    internal class TimeSpanConverter : JsonConverter<TimeSpan>
    {
        public override TimeSpan Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) => default;

        public override void Write(Utf8JsonWriter writer, TimeSpan value, JsonSerializerOptions options) => writer.WriteStringValue(value.ToString());
    }
}
