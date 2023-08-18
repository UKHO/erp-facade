using System.Xml;
using Microsoft.Extensions.Options;
using UKHO.ERPFacade.Common.IO;
using UKHO.ERPFacade.Common.Logging;
using UKHO.ERPFacade.Common.Models;
using System.Xml.Serialization;

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

            LicenceData licenceData = eventData.Data;
            Licence licence =  licenceData.Licence;

            LicenceUpdatedUnitOfSale licenceUpdatedUnitOfSale = new();

            var sapPaylaod = new SapRecordOfSalePayLaod();

            sapPaylaod.CorrelationId = licenceData.CorrelationId;
            sapPaylaod.ServiceType = licence.ProductType;
            sapPaylaod.LicTransaction = licence.TransactionType;
            sapPaylaod.SoldToAcc = licence.DistributorCustomerNumber.ToString();
            sapPaylaod.LicenseEacc = licence.ShippingCoNumber.ToString();
            sapPaylaod.StartDate = licence.OrderDate;
            sapPaylaod.EndDate = licence.HoldingsExpiryDate;
            sapPaylaod.LicenceNumber = licence.SapId.ToString();
            sapPaylaod.VesselName = licence.VesselName;
            sapPaylaod.IMONumber = licence.ImoNumber;
            sapPaylaod.CallSign = licence.CallSign;
            sapPaylaod.ShoreBased = licence.LicenceType;
            sapPaylaod.FleetName = licence.FleetName;
            sapPaylaod.Users = Convert.ToInt32(licence.NumberLicenceUsers);
            sapPaylaod.EndUserId = licence.LicenceId.ToString();
            sapPaylaod.ECDISMANUF = licence.Upn;
            sapPaylaod.LicenceType = licence.LicenceTypeId.ToString();
            sapPaylaod.LicenceDuration = Convert.ToInt32(licence.HoldingsExpiryDate);
            sapPaylaod.PurachaseOrder = licence.PoRef;
            sapPaylaod.OrderNumber = licence.Ordernumber.ToString();

            PROD prod = new();
            var unitOfSaleList = new List<UnitOfSales>()
            {
                new UnitOfSales()
                {
                    Id= licenceUpdatedUnitOfSale.Id,
                    EndDate= licenceUpdatedUnitOfSale.EndDate,
                    Duration = licenceUpdatedUnitOfSale.Duration.ToString(),
                    ReNew = licenceUpdatedUnitOfSale.ReNew,
                    Repeat = licenceUpdatedUnitOfSale.Repeat
                }
            };

            prod.UnitOfSales = unitOfSaleList;
            sapPaylaod.PROD = prod;

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
