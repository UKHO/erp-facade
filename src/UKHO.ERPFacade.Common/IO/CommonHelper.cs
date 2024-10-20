using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace UKHO.ERPFacade.Common.IO
{
    [ExcludeFromCodeCoverage]
    public static class CommonHelper
    {
        public static object ParseXmlNode(string name, object obj, Type type)
        {
            var parts = name.Split('.').ToList();
            var currentPart = parts[0];
            PropertyInfo info = type.GetProperty(currentPart)!;
            if (info == null) { return null!; }
            if (name.IndexOf(".") > -1)
            {
                parts.Remove(currentPart);
                return ParseXmlNode(string.Join(".", parts), info.GetValue(obj, null), info.PropertyType);
            }
            else
            {
                return info.GetValue(obj, null)!.ToString();
            }
        }

        public static string ToSubstring(string value, int startIndex, int length)
        {
            return value.Substring(startIndex, Math.Min(length, value.Length));
        }
    }
}
