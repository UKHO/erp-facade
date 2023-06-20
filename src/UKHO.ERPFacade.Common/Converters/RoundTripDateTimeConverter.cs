using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace UKHO.ERPFacade.Common.Converters
{
    /// <summary>
    /// RoundTripDateTimeConverter
    ///
    /// Custom JSON Converter for use with the System.Text.Json.JsonConverterAttribute
    ///
    /// Ensures DateTimes are read and written in the Round Trip datetime format, coercing to Universal time where necessary
    /// so all Round Trip DateTimes are UTC at rest. This is done because DateTime has no storage for Timezone or Offset
    /// it can only record whether it is UTC, Local, or Unknown
    /// </summary>
    public class RoundTripDateTimeConverter : JsonConverter<DateTime>
    {
        public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) => DateTime.Parse(reader.GetString(), null, DateTimeStyles.RoundtripKind).ToUniversalTime();

        public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options) => writer.WriteStringValue(value.ToUniversalTime().ToString("O"));
    }
}
