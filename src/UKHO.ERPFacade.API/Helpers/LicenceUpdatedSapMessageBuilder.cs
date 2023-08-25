using System.Xml;
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

        private const string SapXmlPath = "SapXmlTemplates\\RosSapRequest.xml";
        private const string XpathZAddsRos = $"//*[local-name()='Z_ADDS_ROS']";
        private const string ImOrderNameSpace = "RecordOfSale";
        private const string TransactionType = "CHANGELICENCE";


        public LicenceUpdatedSapMessageBuilder(ILogger<LicenceUpdatedSapMessageBuilder> logger,
            IXmlHelper xmlHelper,
            IFileSystemHelper fileSystemHelper
        )
        {
            _logger = logger;
            _xmlHelper = xmlHelper;
            _fileSystemHelper = fileSystemHelper;
        }

        public XmlDocument BuildLicenceUpdatedSapMessageXml(RecordOfSaleEventPayLoad eventData, string correlationId)
        {
            string sapXmlTemplatePath = Path.Combine(Environment.CurrentDirectory, SapXmlPath);

            if (!_fileSystemHelper.IsFileExists(sapXmlTemplatePath))
            {
                _logger.LogError(EventIds.LicenceUpdatedSapXmlTemplateNotFound.ToEventId(), "The licence updated SAP message xml template does not exist.");
                throw new FileNotFoundException();
            }

            _logger.LogInformation(EventIds.CreatingLicenceUpdatedSapPayload.ToEventId(), "Creating licence updated SAP Payload.");

            XmlDocument soapXml = _xmlHelper.CreateXmlDocument(sapXmlTemplatePath);

            var sapRecordOfSalePayLoad = SapXmlPayloadCreation(eventData);

            string xml = _xmlHelper.CreateXmlPayLoad(sapRecordOfSalePayLoad);

            string sapXml = xml.Replace(ImOrderNameSpace, "");
           
            soapXml.SelectSingleNode(XpathZAddsRos).InnerXml = sapXml.RemoveNullFields().SetXmlClosingTags();

            _logger.LogInformation(EventIds.CreatedLicenceUpdatedSapPayload.ToEventId(), "Licence updated SAP payload created.");

            return soapXml;
        }

        private SapRecordOfSalePayLoad SapXmlPayloadCreation(RecordOfSaleEventPayLoad eventData)
        {
            var sapPayload = new SapRecordOfSalePayLoad
            {
                CorrelationId = eventData.Data.CorrelationId,
                ServiceType = eventData.Data.Licence.ProductType,
                LicTransaction = eventData.Data.Licence.TransactionType,
                SoldToAcc = eventData.Data.Licence.DistributorCustomerNumber,
                LicenseEacc = eventData.Data.Licence.ShippingCoNumber,
                LicenceNumber = eventData.Data.Licence.SapId,
                VesselName = eventData.Data.Licence.VesselName,
                IMONumber = eventData.Data.Licence.ImoNumber,
                CallSign = eventData.Data.Licence.CallSign,
                ShoreBased = eventData.Data.Licence.ShoreBased,
                FleetName = eventData.Data.Licence.FleetName,
                Users = eventData.Data.Licence.NumberLicenceUsers,
                EndUserId = eventData.Data.Licence.LicenceId,
                ECDISMANUF = eventData.Data.Licence.Upn,
                OrderNumber = eventData.Data.Licence.TransactionType == TransactionType ? "" : eventData.Data.Licence.OrderNumber,
                StartDate = eventData.Data.Licence.TransactionType == TransactionType ? "" : eventData.Data.Licence.OrderDate,
                PurachaseOrder = eventData.Data.Licence.TransactionType == TransactionType ? "" : eventData.Data.Licence.PoRef,
                EndDate = eventData.Data.Licence.TransactionType == TransactionType ? "" : eventData.Data.Licence.HoldingsExpiryDate,
                LicenceType = eventData.Data.Licence.TransactionType == TransactionType ? "" : eventData.Data.Licence.LicenceType,
                LicenceDuration = eventData.Data.Licence.TransactionType == TransactionType ? null : eventData.Data.Licence.LicenceDuration
            };

            sapPayload.PROD = new PROD()
            {
                UnitOfSales = new List<UnitOfSales>()
                {
                    new UnitOfSales()
                    {
                        Id = "",
                        EndDate = "",
                        Duration = "",
                        ReNew = "",
                        Repeat = ""
                    }
                }
            };

            return sapPayload;
        }
    }
}
