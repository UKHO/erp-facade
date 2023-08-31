using System.Xml;
using UKHO.ERPFacade.Common.IO;
using UKHO.ERPFacade.Common.Logging;
using UKHO.ERPFacade.Common.Models;

namespace UKHO.ERPFacade.API.Helpers
{
    public class RecordOfSaleEventSapMessageBuilder : IRecordOfSaleEventSapMessageBuilder{
        private readonly ILogger<RecordOfSaleEventSapMessageBuilder> _logger;
        private readonly IXmlHelper _xmlHelper;
        private readonly IFileSystemHelper _fileSystemHelper;

        private const string SapXmlPath = "SapXmlTemplates\\RosSapRequest.xml";
        private const string XpathZAddsRos = $"//*[local-name()='Z_ADDS_ROS']";
        private const string ImOrderNameSpace = "RecordOfSale";
        private const string TransactionType = "MAINTAINHOLDINGS";


        public RecordOfSaleEventSapMessageBuilder(ILogger<RecordOfSaleEventSapMessageBuilder> logger,
            IXmlHelper xmlHelper,
            IFileSystemHelper fileSystemHelper
        )
        {
            _logger = logger;
            _xmlHelper = xmlHelper;
            _fileSystemHelper = fileSystemHelper;
        }

        public XmlDocument BuildRecordOfSaleEventSapMessageXml(RecordOfSaleEventPayLoad eventData, string correlationId)
        {
            string sapXmlTemplatePath = Path.Combine(Environment.CurrentDirectory, SapXmlPath);

            if (!_fileSystemHelper.IsFileExists(sapXmlTemplatePath))
            {
                _logger.LogError(EventIds.RecordOfSaleSapXmlTemplateNotFound.ToEventId(), "The record of sale SAP message xml template does not exist.");
                throw new FileNotFoundException();
            }

            _logger.LogInformation(EventIds.CreatingRecordOfSaleSapPayload.ToEventId(), "Creating the record of sale SAP Payload.");

            XmlDocument soapXml = _xmlHelper.CreateXmlDocument(sapXmlTemplatePath);

            var sapRecordOfSalePayLoad = SapRecordOfSaleXmlPayloadCreation(eventData);

            string xml = _xmlHelper.CreateXmlPayLoad(sapRecordOfSalePayLoad);

            string sapXml = xml.Replace(ImOrderNameSpace, "");
           
            soapXml.SelectSingleNode(XpathZAddsRos).InnerXml = sapXml.RemoveNullFields().SetXmlClosingTags();

            _logger.LogInformation(EventIds.CreatedRecordOfSaleSapPayload.ToEventId(), "The record of sale SAP payload created.");

            return soapXml;
        }

        private SapRecordOfSalePayLoad SapRecordOfSaleXmlPayloadCreation(RecordOfSaleEventPayLoad eventData)
        {
            var sapPayload = new SapRecordOfSalePayLoad
            {
                CorrelationId = eventData.Data.CorrelationId,
                ServiceType = eventData.Data.RecordOfSale.ProductType,
                LicTransaction = eventData.Data.RecordOfSale.TransactionType,
                SoldToAcc = eventData.Data.RecordOfSale.TransactionType == TransactionType ? "" : eventData.Data.RecordOfSale.DistributorCustomerNumber,
                LicenseEacc = eventData.Data.RecordOfSale.TransactionType == TransactionType ? "" : eventData.Data.RecordOfSale.ShippingCoNumber,
                LicenceNumber = eventData.Data.RecordOfSale.SapId,
                VesselName = eventData.Data.RecordOfSale.TransactionType == TransactionType ? "" : eventData.Data.RecordOfSale.VesselName,
                IMONumber = eventData.Data.RecordOfSale.TransactionType == TransactionType ? "" : eventData.Data.RecordOfSale.ImoNumber,
                CallSign = eventData.Data.RecordOfSale.TransactionType == TransactionType ? "" : eventData.Data.RecordOfSale.CallSign,
                ShoreBased = eventData.Data.RecordOfSale.TransactionType == TransactionType ? "" :  eventData.Data.RecordOfSale.ShoreBased,
                FleetName = eventData.Data.RecordOfSale.TransactionType == TransactionType ? "" : eventData.Data.RecordOfSale.FleetName,
                Users = eventData.Data.RecordOfSale.TransactionType == TransactionType ? 0 : eventData.Data.RecordOfSale.NumberLicenceUsers,
                EndUserId = eventData.Data.RecordOfSale.TransactionType == TransactionType ? "" : eventData.Data.RecordOfSale.LicenceId,
                ECDISMANUF = eventData.Data.RecordOfSale.TransactionType == TransactionType ? "" : eventData.Data.RecordOfSale.Upn,
                OrderNumber = eventData.Data.RecordOfSale.OrderNumber,
                StartDate = eventData.Data.RecordOfSale.TransactionType == TransactionType ? "" : eventData.Data.RecordOfSale.OrderDate,
                PurachaseOrder = eventData.Data.RecordOfSale.PoRef,
                EndDate = eventData.Data.RecordOfSale.TransactionType == TransactionType ? "" : eventData.Data.RecordOfSale.HoldingsExpiryDate,
                LicenceType = eventData.Data.RecordOfSale.TransactionType == TransactionType ? "" : eventData.Data.RecordOfSale.LicenceType,
                LicenceDuration = eventData.Data.RecordOfSale.TransactionType == TransactionType ? null : eventData.Data.RecordOfSale.LicenceDuration
            };

            sapPayload.PROD = new PROD()
            {
                UnitOfSales = new List<UnitOfSales>()
                {
                    new UnitOfSales()
                    {
                        Id = eventData.Data.RecordOfSale.RecordOfSaleEventUnitOfSale[0].Id,
                        EndDate = eventData.Data.RecordOfSale.RecordOfSaleEventUnitOfSale[0].EndDate,
                        Duration = eventData.Data.RecordOfSale.RecordOfSaleEventUnitOfSale[0].Duration,
                        ReNew = eventData.Data.RecordOfSale.RecordOfSaleEventUnitOfSale[0].ReNew,
                        Repeat = eventData.Data.RecordOfSale.RecordOfSaleEventUnitOfSale[0].Repeat
                    }
                }
            };

            return sapPayload;
        }

    }
}
