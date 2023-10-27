using System.Xml;
using Microsoft.Extensions.Logging;
using UKHO.ERPFacade.Common.IO;
using UKHO.ERPFacade.Common.Logging;
using UKHO.ERPFacade.Common.Models;

namespace UKHO.ERPFacade.EventAggregation.WebJob.Helpers
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
        private const string MigrateExistingLicenceType = "MIGRATEEXISTINGLICENCE";

        public RecordOfSaleSapMessageBuilder(ILogger<RecordOfSaleSapMessageBuilder> logger,
            IXmlHelper xmlHelper,
            IFileSystemHelper fileSystemHelper
        )
        {
            _logger = logger;
            _xmlHelper = xmlHelper;
            _fileSystemHelper = fileSystemHelper;
        }

        public XmlDocument BuildRecordOfSaleSapMessageXml(List<RecordOfSaleEventPayLoad> eventDataList, string correlationId)
        {
            SapRecordOfSalePayLoad sapRecordOfSalePayLoad = null!;

            string sapXmlTemplatePath = Path.Combine(Environment.CurrentDirectory, SapXmlPath);

            if (!_fileSystemHelper.IsFileExists(sapXmlTemplatePath))
            {
                _logger.LogError(EventIds.RecordOfSaleSapXmlTemplateNotFound.ToEventId(), "The record of sale SAP message xml template does not exist. | _X-Correlation-ID : {_X-Correlation-ID}", correlationId);
                throw new FileNotFoundException();
            }

            _logger.LogInformation(EventIds.CreatingRecordOfSaleSapPayload.ToEventId(), "Creating the record of sale SAP Payload. | _X-Correlation-ID : {_X-Correlation-ID}", correlationId);

            XmlDocument soapXml = _xmlHelper.CreateXmlDocument(sapXmlTemplatePath);

            sapRecordOfSalePayLoad = eventDataList[0].Data.RecordsOfSale.TransactionType switch
            {
                NewLicenceType => BuildNewLicencePayload(eventDataList),
                MaintainHoldingsType => BuildMaintainHoldingsPayload(eventDataList),
                MigrateExistingLicenceType => BuildMigrateExistingLicencePayload(eventDataList),
                _ => sapRecordOfSalePayLoad
            };

            string xml = _xmlHelper.CreateXmlPayLoad(sapRecordOfSalePayLoad);

            string sapXml = xml.Replace(ImOrderNameSpace, "");

            soapXml.SelectSingleNode(XpathZAddsRos)!.InnerXml = sapXml.RemoveNullFields().SetXmlClosingTags();

            _logger.LogInformation(EventIds.CreatedRecordOfSaleSapPayload.ToEventId(), "The record of sale SAP payload created. | _X-Correlation-ID : {_X-Correlation-ID}", correlationId);

            return soapXml;
        }

        private SapRecordOfSalePayLoad BuildNewLicencePayload(List<RecordOfSaleEventPayLoad> eventDataList)
        {
            SapRecordOfSalePayLoad rosNewLicencePayload = new();

            foreach (var eventData in eventDataList)
            {
                if (rosNewLicencePayload.PROD == null!)
                {
                    rosNewLicencePayload.CorrelationId = eventData.Data.CorrelationId;
                    rosNewLicencePayload.ServiceType = eventData.Data.RecordsOfSale.ProductType;
                    rosNewLicencePayload.LicTransaction = eventData.Data.RecordsOfSale.TransactionType;
                    rosNewLicencePayload.OrderNumber = eventData.Data.RecordsOfSale.OrderNumber;
                    rosNewLicencePayload.PurachaseOrder = eventData.Data.RecordsOfSale.PoRef;
                    rosNewLicencePayload.SoldToAcc = eventData.Data.RecordsOfSale.DistributorCustomerNumber;
                    rosNewLicencePayload.LicenseEacc = eventData.Data.RecordsOfSale.ShippingCoNumber;
                    rosNewLicencePayload.VesselName = eventData.Data.RecordsOfSale.VesselName;
                    rosNewLicencePayload.IMONumber = eventData.Data.RecordsOfSale.ImoNumber;
                    rosNewLicencePayload.CallSign = eventData.Data.RecordsOfSale.CallSign;
                    rosNewLicencePayload.ShoreBased = eventData.Data.RecordsOfSale.ShoreBased;
                    rosNewLicencePayload.Users = eventData.Data.RecordsOfSale.NumberLicenceUsers;
                    rosNewLicencePayload.EndUserId = eventData.Data.RecordsOfSale.LicenceId;
                    rosNewLicencePayload.ECDISMANUF = eventData.Data.RecordsOfSale.EcdisManuf;
                    rosNewLicencePayload.StartDate = eventData.Data.RecordsOfSale.OrderDate;
                    rosNewLicencePayload.EndDate = eventData.Data.RecordsOfSale.HoldingsExpiryDate;
                    rosNewLicencePayload.LicenceType = eventData.Data.RecordsOfSale.LicenceType;
                    rosNewLicencePayload.LicenceDuration = eventData.Data.RecordsOfSale.LicenceDuration;
                    rosNewLicencePayload.FleetName = string.Empty;
                    rosNewLicencePayload.LicenceNumber = string.Empty;

                    PROD prod = new();
                    List<UnitOfSales> unitOfSaleList = eventData.Data.RecordsOfSale.RosUnitOfSale.Select(rosUnitOfSale => new UnitOfSales()
                    {
                        Id = rosUnitOfSale.Id,
                        EndDate = rosUnitOfSale.EndDate,
                        Duration = rosUnitOfSale.Duration,
                        ReNew = rosUnitOfSale.ReNew,
                        Repeat = string.Empty
                    })
                        .ToList();

                    prod.UnitOfSales = unitOfSaleList;
                    rosNewLicencePayload.PROD = prod;
                }

                else
                {
                    List<UnitOfSales> existingUnitOfSaleList = rosNewLicencePayload.PROD.UnitOfSales;
                    existingUnitOfSaleList.AddRange(eventData.Data.RecordsOfSale.RosUnitOfSale.Select(rosUnitOfSale => new UnitOfSales()
                    {
                        Id = rosUnitOfSale.Id,
                        EndDate = rosUnitOfSale.EndDate,
                        Duration = rosUnitOfSale.Duration,
                        ReNew = rosUnitOfSale.ReNew,
                        Repeat = string.Empty
                    }));
                }
            }

            return rosNewLicencePayload;
        }

        private SapRecordOfSalePayLoad BuildMaintainHoldingsPayload(List<RecordOfSaleEventPayLoad> eventDataList)
        {
            SapRecordOfSalePayLoad rosMaintainHoldingsPayload = new();

            foreach (var eventData in eventDataList)
            {
                if (rosMaintainHoldingsPayload.PROD == null!)
                {
                    rosMaintainHoldingsPayload.CorrelationId = eventData.Data.CorrelationId;
                    rosMaintainHoldingsPayload.ServiceType = eventData.Data.RecordsOfSale.ProductType;
                    rosMaintainHoldingsPayload.LicTransaction = eventData.Data.RecordsOfSale.TransactionType;
                    rosMaintainHoldingsPayload.OrderNumber = eventData.Data.RecordsOfSale.OrderNumber;
                    rosMaintainHoldingsPayload.PurachaseOrder = eventData.Data.RecordsOfSale.PoRef;
                    rosMaintainHoldingsPayload.LicenceNumber = eventData.Data.RecordsOfSale.SapId;
                    rosMaintainHoldingsPayload.SoldToAcc = string.Empty;
                    rosMaintainHoldingsPayload.LicenseEacc = string.Empty;
                    rosMaintainHoldingsPayload.VesselName = string.Empty;
                    rosMaintainHoldingsPayload.IMONumber = string.Empty;
                    rosMaintainHoldingsPayload.CallSign = string.Empty;
                    rosMaintainHoldingsPayload.ShoreBased = string.Empty;
                    rosMaintainHoldingsPayload.Users = null;
                    rosMaintainHoldingsPayload.EndUserId = string.Empty;
                    rosMaintainHoldingsPayload.ECDISMANUF = string.Empty;
                    rosMaintainHoldingsPayload.StartDate = string.Empty;
                    rosMaintainHoldingsPayload.EndDate = string.Empty;
                    rosMaintainHoldingsPayload.LicenceType = string.Empty;
                    rosMaintainHoldingsPayload.LicenceDuration = null;
                    rosMaintainHoldingsPayload.FleetName = string.Empty;

                    PROD prod = new();
                    List<UnitOfSales> unitOfSaleList = eventData.Data.RecordsOfSale.RosUnitOfSale.Select(rosUnitOfSale => new UnitOfSales()
                    {
                        Id = rosUnitOfSale.Id,
                        EndDate = rosUnitOfSale.EndDate,
                        Duration = rosUnitOfSale.Duration,
                        ReNew = rosUnitOfSale.ReNew,
                        Repeat = rosUnitOfSale.Repeat
                    })
                        .ToList();

                    prod.UnitOfSales = unitOfSaleList;
                    rosMaintainHoldingsPayload.PROD = prod;
                }
                else
                {
                    List<UnitOfSales> existingUnitOfSaleList = rosMaintainHoldingsPayload.PROD.UnitOfSales;
                    existingUnitOfSaleList.AddRange(eventData.Data.RecordsOfSale.RosUnitOfSale.Select(rosUnitOfSale => new UnitOfSales()
                    {
                        Id = rosUnitOfSale.Id,
                        EndDate = rosUnitOfSale.EndDate,
                        Duration = rosUnitOfSale.Duration,
                        ReNew = rosUnitOfSale.ReNew,
                        Repeat = rosUnitOfSale.Repeat
                    }));
                }
            }

            return rosMaintainHoldingsPayload;
        }

        private SapRecordOfSalePayLoad BuildMigrateExistingLicencePayload(List<RecordOfSaleEventPayLoad> eventDataList)
        {
            SapRecordOfSalePayLoad rosMaintainHoldingsPayload = new();

            foreach (var eventData in eventDataList)
            {
                if (rosMaintainHoldingsPayload.PROD == null!)
                {
                    rosMaintainHoldingsPayload.CorrelationId = eventData.Data.CorrelationId;
                    rosMaintainHoldingsPayload.ServiceType = eventData.Data.RecordsOfSale.ProductType;
                    rosMaintainHoldingsPayload.LicTransaction = eventData.Data.RecordsOfSale.TransactionType;
                    rosMaintainHoldingsPayload.OrderNumber = eventData.Data.RecordsOfSale.OrderNumber;
                    rosMaintainHoldingsPayload.PurachaseOrder = eventData.Data.RecordsOfSale.PoRef;
                    rosMaintainHoldingsPayload.LicenceNumber = eventData.Data.RecordsOfSale.SapId;
                    rosMaintainHoldingsPayload.SoldToAcc = string.Empty;
                    rosMaintainHoldingsPayload.LicenseEacc = string.Empty;
                    rosMaintainHoldingsPayload.VesselName = string.Empty;
                    rosMaintainHoldingsPayload.IMONumber = string.Empty;
                    rosMaintainHoldingsPayload.CallSign = string.Empty;
                    rosMaintainHoldingsPayload.ShoreBased = string.Empty;
                    rosMaintainHoldingsPayload.Users = null;
                    rosMaintainHoldingsPayload.EndUserId = string.Empty;
                    rosMaintainHoldingsPayload.ECDISMANUF = string.Empty;
                    rosMaintainHoldingsPayload.StartDate = string.Empty;
                    rosMaintainHoldingsPayload.EndDate = string.Empty;
                    rosMaintainHoldingsPayload.LicenceType = string.Empty;
                    rosMaintainHoldingsPayload.LicenceDuration = null;
                    rosMaintainHoldingsPayload.FleetName = string.Empty;

                    PROD prod = new();
                    List<UnitOfSales> unitOfSaleList = eventData.Data.RecordsOfSale.RosUnitOfSale.Select(rosUnitOfSale => new UnitOfSales()
                    {
                        Id = rosUnitOfSale.Id,
                        EndDate = rosUnitOfSale.EndDate,
                        Duration = rosUnitOfSale.Duration,
                        ReNew = rosUnitOfSale.ReNew,
                        Repeat = rosUnitOfSale.Repeat
                    })
                        .ToList();

                    prod.UnitOfSales = unitOfSaleList;
                    rosMaintainHoldingsPayload.PROD = prod;
                }
                else
                {
                    List<UnitOfSales> existingUnitOfSaleList = rosMaintainHoldingsPayload.PROD.UnitOfSales;
                    existingUnitOfSaleList.AddRange(eventData.Data.RecordsOfSale.RosUnitOfSale.Select(rosUnitOfSale => new UnitOfSales()
                    {
                        Id = rosUnitOfSale.Id,
                        EndDate = rosUnitOfSale.EndDate,
                        Duration = rosUnitOfSale.Duration,
                        ReNew = rosUnitOfSale.ReNew,
                        Repeat = rosUnitOfSale.Repeat
                    }));
                }
            }

            return rosMaintainHoldingsPayload;
        }
    }
}
