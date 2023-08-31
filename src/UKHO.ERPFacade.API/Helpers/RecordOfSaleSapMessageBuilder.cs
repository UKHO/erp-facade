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
            string sapXmlTemplatePath = Path.Combine(Environment.CurrentDirectory, SapXmlPath);

            if (!_fileSystemHelper.IsFileExists(sapXmlTemplatePath))
            {
                _logger.LogError(EventIds.RecordOfSaleSapXmlTemplateNotFound.ToEventId(), "The record of sale SAP message xml template does not exist.");
                throw new FileNotFoundException();
            }

            _logger.LogInformation(EventIds.CreatingRecordOfSaleSapPayload.ToEventId(), "Creating the record of sale SAP Payload.");

            XmlDocument soapXml = _xmlHelper.CreateXmlDocument(sapXmlTemplatePath);

            var sapRecordOfSalePayLoad = SapXmlPayloadCreation(eventData);

            string xml = _xmlHelper.CreateXmlPayLoad(sapRecordOfSalePayLoad);

            string sapXml = xml.Replace(ImOrderNameSpace, "");
           
            soapXml.SelectSingleNode(XpathZAddsRos).InnerXml = sapXml.RemoveNullFields().SetXmlClosingTags();

            _logger.LogInformation(EventIds.CreatedRecordOfSaleSapPayload.ToEventId(), "The record of sale SAP payload created.");

            return soapXml;
        }

        private SapRecordOfSalePayLoad SapXmlPayloadCreation(RecordOfSaleEventPayLoad eventData)
        {
            SapRecordOfSalePayLoad sapPayload = new();

            if(eventData.Data.RecordsOfSale.TransactionType == MaintainHoldingsType)
            {
                sapPayload.CorrelationId = eventData.Data.CorrelationId;
                sapPayload.ServiceType = eventData.Data.RecordsOfSale.ProductType;
                sapPayload.LicTransaction = eventData.Data.RecordsOfSale.TransactionType;
                sapPayload.LicenceNumber = eventData.Data.RecordsOfSale.SapId;
                sapPayload.OrderNumber = eventData.Data.RecordsOfSale.OrderNumber;
                sapPayload.PurachaseOrder = eventData.Data.RecordsOfSale.PoRef;
                sapPayload.SoldToAcc = string.Empty;
                sapPayload.LicenseEacc = string.Empty;
                sapPayload.VesselName = string.Empty;
                sapPayload.IMONumber = string.Empty;
                sapPayload.CallSign = string.Empty;
                sapPayload.ShoreBased = string.Empty;
                sapPayload.FleetName = string.Empty;
                sapPayload.Users = null;
                sapPayload.EndUserId = string.Empty;
                sapPayload.ECDISMANUF = string.Empty;
                sapPayload.StartDate = string.Empty;
                sapPayload.EndDate = string.Empty;
                sapPayload.LicenceType = string.Empty;
                sapPayload.LicenceDuration = null;

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
                sapPayload.PROD = prod; 
            }

            if (eventData.Data.RecordsOfSale.TransactionType == NewLicenceType)
            {
                sapPayload.CorrelationId = eventData.Data.CorrelationId;
                sapPayload.ServiceType = eventData.Data.RecordsOfSale.ProductType;
                sapPayload.LicTransaction = eventData.Data.RecordsOfSale.TransactionType;
                sapPayload.LicenceNumber = string.Empty;
                sapPayload.OrderNumber = eventData.Data.RecordsOfSale.OrderNumber;
                sapPayload.PurachaseOrder = eventData.Data.RecordsOfSale.PoRef;
                sapPayload.SoldToAcc = eventData.Data.RecordsOfSale.DistributorCustomerNumber;
                sapPayload.LicenseEacc = eventData.Data.RecordsOfSale.ShippingCoNumber;
                sapPayload.VesselName = eventData.Data.RecordsOfSale.VesselName;
                sapPayload.IMONumber = eventData.Data.RecordsOfSale.ImoNumber;
                sapPayload.CallSign = eventData.Data.RecordsOfSale.CallSign;
                sapPayload.ShoreBased = eventData.Data.RecordsOfSale.ShoreBased;
                sapPayload.FleetName = string.Empty;
                sapPayload.Users = eventData.Data.RecordsOfSale.NumberLicenceUsers;
                sapPayload.EndUserId = eventData.Data.RecordsOfSale.LicenceId;
                sapPayload.ECDISMANUF = eventData.Data.RecordsOfSale.Upn;
                sapPayload.StartDate = eventData.Data.RecordsOfSale.OrderDate;
                sapPayload.EndDate = eventData.Data.RecordsOfSale.HoldingsExpiryDate;
                sapPayload.LicenceType = eventData.Data.RecordsOfSale.LicenceType;
                sapPayload.LicenceDuration = eventData.Data.RecordsOfSale.LicenceDuration;

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
                sapPayload.PROD = prod;
            }
            
            return sapPayload;
        }
    }
}
