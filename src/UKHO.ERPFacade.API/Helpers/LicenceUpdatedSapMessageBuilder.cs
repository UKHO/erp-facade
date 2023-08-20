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
        private const string XpathZAddsRos = $"//*[local-name()='Z_ADDS_ROS']";
        private const string ImOrderNameSpace = "RecordOfSale";

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

        public XmlDocument BuildLicenceUpdatedSapMessageXml(RecordOfSaleEventPayLoad eventData, string correlationId)
        {
            string sapXmlTemplatePath = Path.Combine(Environment.CurrentDirectory, SapXmlPath);

            if (!_fileSystemHelper.IsFileExists(sapXmlTemplatePath))
            {
                _logger.LogError(EventIds.SapXmlTemplateNotFound.ToEventId(), "The SAP message xml template does not exist.");
                throw new FileNotFoundException();
            }

            XmlDocument soapXml = _xmlHelper.CreateXmlDocument(sapXmlTemplatePath);
            string xml = SapXmlPayloadCreation(eventData);
            string sapXml = xml.Replace(ImOrderNameSpace, ""); ;
            soapXml.SelectSingleNode(XpathZAddsRos).InnerXml = sapXml;

            return soapXml;
        }

        public string SapXmlPayloadCreation(RecordOfSaleEventPayLoad eventData)
        {
            

            var sapPayLaod = new SapRecordOfSalePayLaod();

            sapPayLaod.CorrelationId = eventData.Data.CorrelationId;
            sapPayLaod.ServiceType = eventData.Data.Licence.ProductType;
            sapPayLaod.LicTransaction = eventData.Data.Licence.TransactionType;
            sapPayLaod.SoldToAcc = eventData.Data.Licence.DistributorCustomerNumber.ToString();
            sapPayLaod.LicenseEacc = eventData.Data.Licence.ShippingCoNumber.ToString();
            sapPayLaod.StartDate = eventData.Data.Licence.OrderDate;
            sapPayLaod.EndDate = eventData.Data.Licence.HoldingsExpiryDate;
            sapPayLaod.LicenceNumber = eventData.Data.Licence.SapId.ToString();
            sapPayLaod.VesselName = eventData.Data.Licence.VesselName;
            sapPayLaod.IMONumber = eventData.Data.Licence.ImoNumber;
            sapPayLaod.CallSign = eventData.Data.Licence.CallSign;
            sapPayLaod.ShoreBased = eventData.Data.Licence.LicenceType;
            sapPayLaod.FleetName = eventData.Data.Licence.FleetName;
            sapPayLaod.Users = Convert.ToInt32(eventData.Data.Licence.NumberLicenceUsers);
            sapPayLaod.EndUserId = eventData.Data.Licence.LicenceId.ToString();
            sapPayLaod.ECDISMANUF = eventData.Data.Licence.Upn;
            sapPayLaod.LicenceType = eventData.Data.Licence.LicenceTypeId.ToString();
            sapPayLaod.LicenceDuration = Convert.ToInt32(eventData.Data.Licence.HoldingsExpiryDate);
            sapPayLaod.PurachaseOrder = eventData.Data.Licence.PoRef;
            sapPayLaod.OrderNumber = eventData.Data.Licence.Ordernumber.ToString();

            PROD prod = new();
            var unitOfSaleList = new List<UnitOfSales>()
            {
                new UnitOfSales()
                {
                    Id= eventData.Data.Licence.LicenceUpdatedUnitOfSale[0].Id,
                    EndDate= eventData.Data.Licence.LicenceUpdatedUnitOfSale[0].EndDate,
                    Duration = eventData.Data.Licence.LicenceUpdatedUnitOfSale[0].Duration.ToString(),
                    ReNew = eventData.Data.Licence.LicenceUpdatedUnitOfSale[0].ReNew,
                    Repeat = eventData.Data.Licence.LicenceUpdatedUnitOfSale[0].Repeat
                }
            };
            prod.UnitOfSales = unitOfSaleList;

            sapPayLaod.PROD = prod;


          return  _xmlHelper.CreateRecordOfSaleSapXmlPayLoad(sapPayLaod);
        }
    }
}
