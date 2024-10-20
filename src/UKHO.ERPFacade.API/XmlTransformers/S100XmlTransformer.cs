using System.Xml;
using UKHO.ERPFacade.API.XmlTransformers;
using UKHO.ERPFacade.Common.IO;

namespace UKHO.ERPFacade.API.Helpers
{
    public class S100XmlTransformer : BaseXmlTransformer
    {
        private readonly ILogger<S100XmlTransformer> _logger;
        private readonly IXmlHelper _xmlHelper;

        public S100XmlTransformer(ILogger<S100XmlTransformer> logger,
                                 IXmlHelper xmlHelper,
                                 IFileSystemHelper fileSystemHelper
                                 ) : base(fileSystemHelper, xmlHelper)
        {
            _logger = logger;
            _xmlHelper = xmlHelper;
        }

        /// <summary>
        /// Generate SAP message xml file.
        /// </summary>
        /// <param name="eventData"></param>        
        /// <returns>XmlDocument</returns>
        public override XmlDocument BuildXmlPayload<T>(T eventData, string templatePath)
        {
            var soapXml = _xmlHelper.CreateXmlDocument(Path.Combine(Environment.CurrentDirectory, templatePath));
            return soapXml;
        }
    }
}
