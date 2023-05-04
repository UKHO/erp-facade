using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace UKHO.ERPFacade.Common.IO
{
    [ExcludeFromCodeCoverage]
    public static class CommonHelper
    {
        public static object GetProp(string name, object obj, Type type)
        {
            var parts = name.Split('.').ToList();
            var currentPart = parts[0];
            PropertyInfo info = type.GetProperty(currentPart)!;
            if (info == null) { return null!; }
            if (name.IndexOf(".") > -1)
            {
                parts.Remove(currentPart);
                return GetProp(string.Join(".", parts), info.GetValue(obj, null), info.PropertyType);
            }
            else
            {
                return info.GetValue(obj, null)!.ToString();
            }
        }
    }
}
