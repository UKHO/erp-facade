using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Xml;

namespace UKHO.ERPFacade.Common.Extensions
{
    [ExcludeFromCodeCoverage]
    public static class XmlExtension
    {
        public static string ToIndentedString(this XmlDocument doc)
        {
            using (var memoryStream = new MemoryStream())
            {
                XmlWriterSettings settings = new XmlWriterSettings
                {
                    Indent = true,
                    IndentChars = "  ",
                    NewLineChars = "\r\n",
                    NewLineHandling = NewLineHandling.Replace,
                    Encoding = Encoding.UTF8
                };

                using (XmlWriter writer = XmlWriter.Create(memoryStream, settings))
                {
                    doc.Save(writer);
                }

                return Encoding.UTF8.GetString(memoryStream.ToArray());
            }
        }
    }
}
