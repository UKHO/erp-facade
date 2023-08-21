using System.Diagnostics.CodeAnalysis;
using System.Xml;
using System.Xml.Serialization;
using UKHO.ERPFacade.Common.Models;

namespace UKHO.ERPFacade.Common.IO
{
    [ExcludeFromCodeCoverage]
    public class XmlHelper : IXmlHelper
    {
        public XmlDocument CreateXmlDocument(string xmlPath)
        {
            XmlDocument xmlDocument = new();
            xmlDocument.Load(xmlPath);
            return xmlDocument;
        }

        public string CreateRecordOfSaleSapXmlPayLoad1(SapRecordOfSalePayLoad sapRecordOfSalePayLoad)
        {
            var xml = string.Empty;

            // Remove Declaration  
            var settings = new XmlWriterSettings
            {
                Indent = true,
                OmitXmlDeclaration = true
            };

            // Remove Namespace  
            var ns = new XmlSerializerNamespaces(new[] { XmlQualifiedName.Empty });

            using (var stream = new StringWriter())
            using (var writer = XmlWriter.Create(stream, settings))
            {
                var serializer = new XmlSerializer(typeof(SapRecordOfSalePayLoad));
                serializer.Serialize(writer, sapRecordOfSalePayLoad, ns);
                xml = stream.ToString();
            }

            return xml;
        }

        public  string CreateRecordOfSaleSapXmlPayLoad<T>(T anyobject)
        {
            var xml = string.Empty;

            // Remove Declaration  
            var settings = new XmlWriterSettings
            {
                Indent = true,
                OmitXmlDeclaration = true
            };

            // Remove Namespace  
            var ns = new XmlSerializerNamespaces(new[] { XmlQualifiedName.Empty });

            using (var stream = new StringWriter())
            using (var writer = XmlWriter.Create(stream, settings))
            {
                var serializer = new XmlSerializer(typeof(T));
                serializer.Serialize(writer, anyobject, ns);
                xml = stream.ToString();
            }

            return xml;
        }
    }
}
