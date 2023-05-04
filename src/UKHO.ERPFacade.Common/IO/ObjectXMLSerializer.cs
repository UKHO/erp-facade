using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Xml.Serialization;
using System.Xml;

namespace UKHO.ERPFacade.Common.IO
{
    [ExcludeFromCodeCoverage]
    public class ObjectXMLSerializer<T> where T : class
    {
        public static string SerializeObject(T serializableObject)
        {
            if (serializableObject == null)
            {
                return null;
            }

            var serializer = new XmlSerializer(typeof(T));

            var sb = new StringBuilder();

            var settings = new XmlWriterSettings { Indent = true, };

            using (var xw = XmlWriter.Create(sb, settings))
            {
                serializer.Serialize(xw, serializableObject);
            }

            return sb.ToString();
        }
    }
}