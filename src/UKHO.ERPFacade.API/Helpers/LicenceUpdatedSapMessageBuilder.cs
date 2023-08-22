using System.Xml;
using Microsoft.Extensions.Options;
using UKHO.ERPFacade.Common.IO;
using UKHO.ERPFacade.Common.Logging;
using UKHO.ERPFacade.Common.Models;

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
            var sapPayload = new SapRecordOfSalePayLoad();

            sapPayload.CorrelationId = eventData.Data.CorrelationId;
            sapPayload.ServiceType = eventData.Data.Licence.ProductType;
            sapPayload.LicTransaction = eventData.Data.Licence.TransactionType;
            sapPayload.SoldToAcc = eventData.Data.Licence.DistributorCustomerNumber.ToString();
            sapPayload.LicenseEacc = eventData.Data.Licence.ShippingCoNumber.ToString();
            sapPayload.StartDate = eventData.Data.Licence.OrderDate;
            sapPayload.EndDate = eventData.Data.Licence.HoldingsExpiryDate;
            sapPayload.LicenceNumber = eventData.Data.Licence.SapId.ToString();
            sapPayload.VesselName = eventData.Data.Licence.VesselName;
            sapPayload.IMONumber = eventData.Data.Licence.ImoNumber;
            sapPayload.CallSign = eventData.Data.Licence.CallSign;
            sapPayload.ShoreBased = eventData.Data.Licence.LicenceType;
            sapPayload.FleetName = eventData.Data.Licence.FleetName;
            sapPayload.Users = Convert.ToInt32(eventData.Data.Licence.NumberLicenceUsers);
            sapPayload.EndUserId = eventData.Data.Licence.LicenceId.ToString();
            sapPayload.ECDISMANUF = eventData.Data.Licence.Upn;
            sapPayload.LicenceType = eventData.Data.Licence.LicenceTypeId;
            sapPayload.LicenceDuration = 12;
            sapPayload.PurachaseOrder = eventData.Data.Licence.PoRef;
            sapPayload.OrderNumber = eventData.Data.Licence.Ordernumber.ToString();

            if(eventData.Data.Licence.LicenceUpdatedUnitOfSale != null!)
            {
                var unitOfSaleList = new List<UnitOfSales>();

                foreach (var unit in eventData.Data.Licence.LicenceUpdatedUnitOfSale)
                {
                    var unitOfSale = new UnitOfSales()
                    {
                        Id = unit.Id,
                        EndDate = unit.EndDate,
                        Duration = unit.Duration,
                        ReNew = unit.ReNew,
                        Repeat = unit.Repeat
                    };

                    unitOfSaleList.Add(unitOfSale);
                }

                if (unitOfSaleList.Count > 0)
                {
                    var prod = new PROD() { UnitOfSales = unitOfSaleList };
                    sapPayload.PROD = prod;
                } 
            }

            return  _xmlHelper.CreateRecordOfSaleSapXmlPayLoad(sapPayload);
        }
    }
}
