using System.IdentityModel.Tokens.Jwt;
using System.Reflection;
using System.Xml;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;
using UKHO.ERPFacade.Common.IO;
using UKHO.ERPFacade.Common.Logging;
using UKHO.ERPFacade.Common.Models;
using System.Xml.Serialization;
using System.IO;
using UKHO.ERPFacade.Common;

namespace UKHO.ERPFacade.API.Helpers
{
    public class LicenceUpdatedSapMessageBuilder : ILicenceUpdatedSapMessageBuilder
    {
        private readonly ILogger<LicenceUpdatedSapMessageBuilder> _logger;
        private readonly IXmlHelper _xmlHelper;
        private readonly IFileSystemHelper _fileSystemHelper;
        private readonly IOptions<LicenceUpdatedSapActionConfiguration> _sapActionConfig;

        private const string SapXmlPath = "SapXmlTemplates\\LicenceUpdatedSapRequest.xml";
        private const string XpathImOrder = $"//*[local-name()='IM_ORDER']";

        public LicenceUpdatedSapMessageBuilder(ILogger<LicenceUpdatedSapMessageBuilder> logger,
            IXmlHelper xmlHelper,
            IFileSystemHelper fileSystemHelper,
            IOptions<LicenceUpdatedSapActionConfiguration> sapActionConfig
        )
        {
            _logger = logger;
            _xmlHelper = xmlHelper;
            _fileSystemHelper = fileSystemHelper;
            _sapActionConfig = sapActionConfig;
        }
        public LicenceUpdatedSapMessageBuilder(){}
        public XmlDocument BuildLicenceUpdatedSapMessageXml(LicenceUpdatedEventPayLoad eventData, string correlationId)
        {

            string sapXmlTemplatePath = Path.Combine(Environment.CurrentDirectory, SapXmlPath);

            if (!_fileSystemHelper.IsFileExists(sapXmlTemplatePath))
            {
                _logger.LogError(EventIds.SapXmlTemplateNotFound.ToEventId(), "The SAP message xml template does not exist.");
                throw new FileNotFoundException();
            }

            XmlDocument soapXml = _xmlHelper.CreateXmlDocument(sapXmlTemplatePath);
            //Test Data
            var sapPaylaod = new SapRecordOfSalePayLaod();

            sapPaylaod.CorrelationId = "CorrId1234";
            sapPaylaod.ServiceType = "123";
            sapPaylaod.LicTransaction = "122";
            sapPaylaod.SoldToAcc = "CorrId1234";
            sapPaylaod.LicenseEacc = "123";
            sapPaylaod.StartDate = "122";
            sapPaylaod.EndDate = "CorrId1234";
            sapPaylaod.LicenceNumber = "123";
            sapPaylaod.VesselName = "122";
            sapPaylaod.IMONumber = "CorrId1234";
            sapPaylaod.CallSign = "123";
            sapPaylaod.ShoreBased = "122";
            sapPaylaod.FleetName = "CorrId1234";
            sapPaylaod.Users = "123";
            sapPaylaod.EndUserId = "122";
            sapPaylaod.ECDISMANUF = "CorrId1234";
            sapPaylaod.LicenceType = "123";

            sapPaylaod.LicenceDuration = "122";
            sapPaylaod.PurachaseOrder = "123";

            sapPaylaod.OrderNumber = "CorrId1234";

            var Prod = new List<Common.UnitOfSale>()
            {
                new Common.UnitOfSale()
                {
                    Id="Sam",
                    EndDate="EDAA",
                    Duration = "12",
                    ReNew = "Y",
                    Repeat = "N"
                }
            };

            var itemProd = new PROD();
            itemProd.UnitOfSales = Prod;
            ;
            sapPaylaod.PROD = itemProd;
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
                var serializer = new XmlSerializer(typeof(SapRecordOfSalePayLaod));
                serializer.Serialize(writer, sapPaylaod, ns);
                xml = stream.ToString();
            }

            soapXml.SelectSingleNode(XpathImOrder).InnerXml = xml;
            return soapXml;
        }
    }
}
