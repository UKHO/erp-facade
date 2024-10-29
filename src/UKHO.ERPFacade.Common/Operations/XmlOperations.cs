using System.Diagnostics.CodeAnalysis;
using System.Xml;
using System.Xml.Serialization;
using UKHO.ERPFacade.Common.Exceptions;
using UKHO.ERPFacade.Common.IO;
using UKHO.ERPFacade.Common.Logging;
using UKHO.ERPFacade.Common.Operations.IO;

namespace UKHO.ERPFacade.Common.Operations
{
    [ExcludeFromCodeCoverage]
    public class XmlOperations : IXmlOperations
    {
        private readonly IFileOperations _fileOperations;

        public XmlOperations(IFileOperations fileOperations)
        {
            _fileOperations = fileOperations;
        }

        public XmlDocument CreateXmlDocument(string xmlPath)
        {
            // Check if SAP XML payload template exists
            if (!_fileOperations.IsFileExists(xmlPath))
            {
                throw new ERPFacadeException(EventIds.SapXmlTemplateNotFound.ToEventId(), "The SAP XML payload template does not exist.");
            }
            XmlDocument xmlDocument = new();
            xmlDocument.Load(xmlPath);
            return xmlDocument;
        }

        public string CreateXmlPayLoad<T>(T anyobject)
        {
            string xml;

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

        public void AppendChildNode(XmlElement parentNode, XmlDocument doc, string nodeName, string value)
        {
            var childNode = doc.CreateElement(nodeName);
            childNode.InnerText = value ?? string.Empty;
            parentNode.AppendChild(childNode);
        }
    }
}
