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
            var sapPaylaod = new SapRecordOfSalePayLaod();

            sapPaylaod.CorrelationId = eventData.Data.CorrelationId;
            sapPaylaod.ServiceType = eventData.Data.Licence.ProductType;
            sapPaylaod.LicTransaction = eventData.Data.Licence.TransactionType;
            sapPaylaod.SoldToAcc = eventData.Data.Licence.DistributorCustomerNumber.ToString();
            sapPaylaod.LicenseEacc = eventData.Data.Licence.ShippingCoNumber.ToString();
            sapPaylaod.StartDate = eventData.Data.Licence.OrderDate;
            sapPaylaod.EndDate = eventData.Data.Licence.HoldingsExpiryDate;
            sapPaylaod.LicenceNumber = eventData.Data.Licence.SapId.ToString();
            sapPaylaod.VesselName = eventData.Data.Licence.VesselName;
            sapPaylaod.IMONumber = eventData.Data.Licence.ImoNumber;
            sapPaylaod.CallSign = eventData.Data.Licence.CallSign;
            sapPaylaod.ShoreBased = eventData.Data.Licence.LicenceType;
            sapPaylaod.FleetName = eventData.Data.Licence.FleetName;
            sapPaylaod.Users = Convert.ToInt32(eventData.Data.Licence.NumberLicenceUsers);
            sapPaylaod.EndUserId = eventData.Data.Licence.LicenceId.ToString();
            sapPaylaod.ECDISMANUF = eventData.Data.Licence.Upn;
            sapPaylaod.LicenceType = eventData.Data.Licence.LicenceTypeId.ToString();
            sapPaylaod.LicenceDuration = Convert.ToInt32(eventData.Data.Licence.HoldingsExpiryDate);
            sapPaylaod.PurachaseOrder = eventData.Data.Licence.PoRef;
            sapPaylaod.OrderNumber = eventData.Data.Licence.Ordernumber.ToString();
           
            var unitOfSaleList = new List<UnitOfSales>();

            foreach (var unit in eventData.Data.Licence.LicenceUpdatedUnitOfSale)
            {
                var unitOfSale = new UnitOfSales()
                {
                    Id = unit.Id,
                    EndDate = unit.EndDate,
                    Duration = unit.Duration.ToString(),
                    ReNew = unit.ReNew,
                    Repeat = unit.Repeat
                };

                unitOfSaleList.Add(unitOfSale);
            }

            if (unitOfSaleList.Count > 0)
            {
                var prod = new PROD() { UnitOfSales = unitOfSaleList };
                sapPaylaod.PROD = prod;
            }

          return  _xmlHelper.CreateRecordOfSaleSapXmlPayLoad(sapPaylaod);
        }
    }
}
