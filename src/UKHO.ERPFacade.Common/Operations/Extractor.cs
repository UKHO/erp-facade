using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using Newtonsoft.Json.Linq;

namespace UKHO.ERPFacade.Common.Operations
{
    [ExcludeFromCodeCoverage]
    public static class Extractor
    {
        public static string ExtractJsonAttributeValue(string name, object obj, Type type)
        {
            var parts = name.Split('.').ToList();
            var currentPart = parts[0];
            PropertyInfo info = type.GetProperty(currentPart)!;
            if (info == null) { return null!; }
            if (name.IndexOf(".") > -1)
            {
                parts.Remove(currentPart);
                return ExtractJsonAttributeValue(string.Join(".", parts), info.GetValue(obj, null), info.PropertyType);
            }
            else
            {
                return info.GetValue(obj, null)!.ToString();
            }
        }

        public static string? ExtractTokenValue(JObject jObject, string key)
        {
            if (jObject == null) return null;

            foreach (var property in jObject.Properties())
            {
                if (string.Equals(property.Name, key, StringComparison.OrdinalIgnoreCase))
                {
                    return property.Value.ToString();
                }

                if (property.Value is JObject nestedObject)
                {
                    var result = ExtractTokenValue(nestedObject, key);
                    if (result != null)
                    {
                        return result;
                    }
                }
            }
            return null;
        }
    }
}
