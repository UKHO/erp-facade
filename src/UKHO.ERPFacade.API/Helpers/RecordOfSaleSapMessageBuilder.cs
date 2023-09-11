using System.Xml;
using UKHO.ERPFacade.Common.IO;
using UKHO.ERPFacade.Common.Logging;
using UKHO.ERPFacade.Common.Models;

namespace UKHO.ERPFacade.API.Helpers
{
    public class RecordOfSaleSapMessageBuilder : IRecordOfSaleSapMessageBuilder
    {
        private readonly ILogger<RecordOfSaleSapMessageBuilder> _logger;
        private readonly IXmlHelper _xmlHelper;
        private readonly IFileSystemHelper _fileSystemHelper;

        private const string SapXmlPath = "SapXmlTemplates\\RosSapRequest.xml";
        private const string XpathZAddsRos = $"//*[local-name()='Z_ADDS_ROS']";
        private const string ImOrderNameSpace = "RecordOfSale";
        private const string MaintainHoldingsType = "MAINTAINHOLDINGS";
        private const string NewLicenceType = "NEWLICENCE";

        public RecordOfSaleSapMessageBuilder(ILogger<RecordOfSaleSapMessageBuilder> logger,
            IXmlHelper xmlHelper,
            IFileSystemHelper fileSystemHelper
        )
        {
            _logger = logger;
            _xmlHelper = xmlHelper;
            _fileSystemHelper = fileSystemHelper;
        }

        public XmlDocument BuildRecordOfSaleSapMessageXml(RecordOfSaleEventPayLoad eventData, string correlationId)
        {
            SapRecordOfSalePayLoad sapRecordOfSalePayLoad = null;

            string sapXmlTemplatePath = Path.Combine(Environment.CurrentDirectory, SapXmlPath);

            if (!_fileSystemHelper.IsFileExists(sapXmlTemplatePath))
            {
                _logger.LogError(EventIds.RecordOfSaleSapXmlTemplateNotFound.ToEventId(), "The record of sale SAP message xml template does not exist.");
                throw new FileNotFoundException();
            }

            _logger.LogInformation(EventIds.CreatingRecordOfSaleSapPayload.ToEventId(), "Creating the record of sale SAP Payload.");

            XmlDocument soapXml = _xmlHelper.CreateXmlDocument(sapXmlTemplatePath);

            switch (eventData.Data.RecordsOfSale.TransactionType)
            {
                case NewLicenceType:
                    sapRecordOfSalePayLoad = BuildNewLicencePayload(eventData);
                    break;

                case MaintainHoldingsType:
                    sapRecordOfSalePayLoad = BuildMaintainHoldingsPayload(eventData);
                    break;
            }

            string xml = _xmlHelper.CreateXmlPayLoad(sapRecordOfSalePayLoad);

            string sapXml = xml.Replace(ImOrderNameSpace, "");

            soapXml.SelectSingleNode(XpathZAddsRos).InnerXml = sapXml.RemoveNullFields().SetXmlClosingTags();

            _logger.LogInformation(EventIds.CreatedRecordOfSaleSapPayload.ToEventId(), "The record of sale SAP payload created.");

            return soapXml;
        }

        private SapRecordOfSalePayLoad BuildNewLicencePayload(RecordOfSaleEventPayLoad eventData)
        {
            var newLicencePayload = new SapRecordOfSalePayLoad
            {
                CorrelationId = eventData.Data.CorrelationId,
                ServiceType = eventData.Data.RecordsOfSale.ProductType,
                LicTransaction = eventData.Data.RecordsOfSale.TransactionType,
                OrderNumber = eventData.Data.RecordsOfSale.OrderNumber,
                PurachaseOrder = eventData.Data.RecordsOfSale.PoRef,
                SoldToAcc = eventData.Data.RecordsOfSale.DistributorCustomerNumber,
                LicenseEacc = eventData.Data.RecordsOfSale.ShippingCoNumber,
                VesselName = eventData.Data.RecordsOfSale.VesselName,
                IMONumber = eventData.Data.RecordsOfSale.ImoNumber,
                CallSign = eventData.Data.RecordsOfSale.CallSign,
                ShoreBased = eventData.Data.RecordsOfSale.ShoreBased,
                Users = eventData.Data.RecordsOfSale.NumberLicenceUsers,
                EndUserId = eventData.Data.RecordsOfSale.LicenceId,
                ECDISMANUF = eventData.Data.RecordsOfSale.Upn,
                StartDate = eventData.Data.RecordsOfSale.OrderDate,
                EndDate = eventData.Data.RecordsOfSale.HoldingsExpiryDate,
                LicenceType = eventData.Data.RecordsOfSale.LicenceType,
                LicenceDuration = eventData.Data.RecordsOfSale.LicenceDuration,
                FleetName = string.Empty,
                LicenceNumber = string.Empty
            };

            PROD prod = new();
            List<UnitOfSales> unitOfSaleList = new();

            foreach (var rosUnitOfSale in eventData.Data.RecordsOfSale.RosUnitOfSale)
            {
                UnitOfSales unitOfSales = new()
                {
                    Id = rosUnitOfSale.Id,
                    EndDate = rosUnitOfSale.EndDate,
                    Duration = rosUnitOfSale.Duration,
                    ReNew = rosUnitOfSale.ReNew,
                    Repeat = string.Empty
                };

                unitOfSaleList.Add(unitOfSales);
            }

            prod.UnitOfSales = unitOfSaleList;
            newLicencePayload.PROD = prod;

            return newLicencePayload;
        }

        private SapRecordOfSalePayLoad BuildMaintainHoldingsPayload(RecordOfSaleEventPayLoad eventData)
        {
            var maintainHoldingsPayload = new SapRecordOfSalePayLoad
            {
                CorrelationId = eventData.Data.CorrelationId,
                ServiceType = eventData.Data.RecordsOfSale.ProductType,
                LicTransaction = eventData.Data.RecordsOfSale.TransactionType,
                OrderNumber = eventData.Data.RecordsOfSale.OrderNumber,
                PurachaseOrder = eventData.Data.RecordsOfSale.PoRef,
                LicenceNumber = eventData.Data.RecordsOfSale.SapId,
                SoldToAcc = string.Empty,
                LicenseEacc = string.Empty,
                VesselName = string.Empty,
                IMONumber = string.Empty,
                CallSign = string.Empty,
                ShoreBased = string.Empty,
                Users = null,
                EndUserId = string.Empty,
                ECDISMANUF = string.Empty,
                StartDate = string.Empty,
                EndDate = string.Empty,
                LicenceType = string.Empty,
                LicenceDuration = null,
                FleetName = string.Empty
            };

            PROD prod = new();
            List<UnitOfSales> unitOfSaleList = new();

            foreach (var rosUnitOfSale in eventData.Data.RecordsOfSale.RosUnitOfSale)
            {
                UnitOfSales unitOfSales = new()
                {
                    Id = rosUnitOfSale.Id,
                    EndDate = rosUnitOfSale.EndDate,
                    Duration = rosUnitOfSale.Duration,
                    ReNew = rosUnitOfSale.ReNew,
                    Repeat = rosUnitOfSale.Repeat
                };

                unitOfSaleList.Add(unitOfSales);
            }

            prod.UnitOfSales = unitOfSaleList;
            maintainHoldingsPayload.PROD = prod;

            return maintainHoldingsPayload;
        }
    }
}
