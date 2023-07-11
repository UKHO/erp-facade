using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Text;
using System.Xml;

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

        public static string WriteXmlClosingTags(this string xmlString)
        {
            var sb = new StringBuilder();
            var xmlTags = xmlString.Split('\r');
            foreach (var tag in xmlTags)
            {
                if (tag.Contains("/>"))
                {
                    var tagValue = tag.Replace("<", "").Replace("/>", "").Trim();
                    var firstPart = tag.Substring(0, tag.IndexOf('<'));
                    var newTag = $"{firstPart}<{tagValue}></{tagValue}>";
                    sb.Append(newTag);
                }
                else
                {
                    sb.Append(tag);
                }
            }
            return sb.ToString();
        }

        public static string ToIndentedString(this XmlDocument doc)
        {
            StringBuilder sb = new StringBuilder();
            XmlWriterSettings settings = new XmlWriterSettings
            {
                Indent = true,
                IndentChars = "  ",
                NewLineChars = "\r\n",
                NewLineHandling = NewLineHandling.Replace
            };
            using (XmlWriter writer = XmlWriter.Create(sb, settings))
            {
                doc.Save(writer);
            }
            return sb.ToString();
        }
    }
}
