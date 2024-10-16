using System.Diagnostics.CodeAnalysis;
using System.Xml;
using System.Xml.Serialization;
using UKHO.ERPFacade.Common.Exceptions;
using UKHO.ERPFacade.Common.Logging;

namespace UKHO.ERPFacade.Common.IO
{
    [ExcludeFromCodeCoverage]
    public class XmlHelper : IXmlHelper
    {
        private readonly IFileSystemHelper _fileSystemHelper;

        XmlHelper(IFileSystemHelper fileSystemHelper)
        {
            _fileSystemHelper = fileSystemHelper;
        }

        public XmlDocument CreateXmlDocument(string xmlPath)
        {
            // Check if SAP XML payload template exists
            if (!_fileSystemHelper.IsFileExists(xmlPath))
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
    }
}
