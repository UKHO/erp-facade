using System.Xml;
using Microsoft.Extensions.Logging;
using UKHO.ERPFacade.Common.Constants;
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

            _logger.LogInformation(EventIds.CreatingRecordOfSaleSapPayload.ToEventId(), "Creating the record of sale SAP Payload. | _X-Correlation-ID : {_X-Correlation-ID}", correlationId);

            XmlDocument soapXml = _xmlHelper.CreateXmlDocument(Path.Combine(Environment.CurrentDirectory, TemplatePaths.RecordOfSaleSapXmlTemplatePath));

            sapRecordOfSalePayLoad = eventDataList[0].Data.RecordsOfSale.TransactionType switch
            {
                Constants.NewLicenceType => BuildNewLicencePayload(eventDataList),
                Constants.MigrateNewLicenceType => BuildMigrateNewLicencePayload(eventDataList),
                Constants.MigrateExistingLicenceType => BuildMigrateExistingLicencePayload(eventDataList),
                Constants.ConvertLicenceType => BuildConvertLicencePayload(eventDataList),
                Constants.MaintainHoldingsType => BuildMaintainHoldingsPayload(eventDataList),
                _ => sapRecordOfSalePayLoad
            };

            string xml = _xmlHelper.CreateXmlPayLoad(sapRecordOfSalePayLoad);

            string sapXml = xml.Replace(Constants.ImOrderNameSpace, "");

            soapXml.SelectSingleNode(Constants.XpathZAddsRos)!.InnerXml = sapXml.RemoveNullFields().SetXmlClosingTags();

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
                    rosNewLicencePayload.ECDISMANUF = eventData.Data.RecordsOfSale.EcdisManufacturerName;
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

        private SapRecordOfSalePayLoad BuildMigrateNewLicencePayload(List<RecordOfSaleEventPayLoad> eventDataList)
        {
            SapRecordOfSalePayLoad rosMigrateNewLicencePayload = new();

            foreach (var eventData in eventDataList)
            {
                if (rosMigrateNewLicencePayload.PROD == null!)
                {
                    rosMigrateNewLicencePayload.CorrelationId = eventData.Data.CorrelationId;
                    rosMigrateNewLicencePayload.ServiceType = eventData.Data.RecordsOfSale.ProductType;
                    rosMigrateNewLicencePayload.LicTransaction = eventData.Data.RecordsOfSale.TransactionType;
                    rosMigrateNewLicencePayload.OrderNumber = eventData.Data.RecordsOfSale.OrderNumber;
                    rosMigrateNewLicencePayload.PurachaseOrder = eventData.Data.RecordsOfSale.PoRef;
                    rosMigrateNewLicencePayload.SoldToAcc = eventData.Data.RecordsOfSale.DistributorCustomerNumber;
                    rosMigrateNewLicencePayload.LicenseEacc = eventData.Data.RecordsOfSale.ShippingCoNumber;
                    rosMigrateNewLicencePayload.VesselName = eventData.Data.RecordsOfSale.VesselName;
                    rosMigrateNewLicencePayload.IMONumber = eventData.Data.RecordsOfSale.ImoNumber;
                    rosMigrateNewLicencePayload.CallSign = eventData.Data.RecordsOfSale.CallSign;
                    rosMigrateNewLicencePayload.ShoreBased = eventData.Data.RecordsOfSale.ShoreBased;
                    rosMigrateNewLicencePayload.Users = eventData.Data.RecordsOfSale.NumberLicenceUsers;
                    rosMigrateNewLicencePayload.EndUserId = eventData.Data.RecordsOfSale.LicenceId;
                    rosMigrateNewLicencePayload.ECDISMANUF = eventData.Data.RecordsOfSale.EcdisManufacturerName;
                    rosMigrateNewLicencePayload.StartDate = eventData.Data.RecordsOfSale.OrderDate;
                    rosMigrateNewLicencePayload.EndDate = eventData.Data.RecordsOfSale.HoldingsExpiryDate;
                    rosMigrateNewLicencePayload.LicenceType = eventData.Data.RecordsOfSale.LicenceType;
                    rosMigrateNewLicencePayload.LicenceDuration = eventData.Data.RecordsOfSale.LicenceDuration;
                    rosMigrateNewLicencePayload.FleetName = string.Empty;
                    rosMigrateNewLicencePayload.LicenceNumber = string.Empty;

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
                    rosMigrateNewLicencePayload.PROD = prod;
                }
                else
                {
                    List<UnitOfSales> existingUnitOfSaleList = rosMigrateNewLicencePayload.PROD.UnitOfSales;
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

            return rosMigrateNewLicencePayload;
        }

        private SapRecordOfSalePayLoad BuildMigrateExistingLicencePayload(List<RecordOfSaleEventPayLoad> eventDataList)
        {
            SapRecordOfSalePayLoad rosMigrateExistingLicencePayload = new();

            foreach (var eventData in eventDataList)
            {
                if (rosMigrateExistingLicencePayload.PROD == null!)
                {
                    rosMigrateExistingLicencePayload.CorrelationId = eventData.Data.CorrelationId;
                    rosMigrateExistingLicencePayload.ServiceType = eventData.Data.RecordsOfSale.ProductType;
                    rosMigrateExistingLicencePayload.LicTransaction = eventData.Data.RecordsOfSale.TransactionType;
                    rosMigrateExistingLicencePayload.OrderNumber = eventData.Data.RecordsOfSale.OrderNumber;
                    rosMigrateExistingLicencePayload.PurachaseOrder = eventData.Data.RecordsOfSale.PoRef;
                    rosMigrateExistingLicencePayload.LicenceNumber = eventData.Data.RecordsOfSale.SapId;
                    rosMigrateExistingLicencePayload.SoldToAcc = string.Empty;
                    rosMigrateExistingLicencePayload.LicenseEacc = string.Empty;
                    rosMigrateExistingLicencePayload.VesselName = string.Empty;
                    rosMigrateExistingLicencePayload.IMONumber = string.Empty;
                    rosMigrateExistingLicencePayload.CallSign = string.Empty;
                    rosMigrateExistingLicencePayload.ShoreBased = string.Empty;
                    rosMigrateExistingLicencePayload.Users = null;
                    rosMigrateExistingLicencePayload.EndUserId = string.Empty;
                    rosMigrateExistingLicencePayload.ECDISMANUF = string.Empty;
                    rosMigrateExistingLicencePayload.StartDate = string.Empty;
                    rosMigrateExistingLicencePayload.EndDate = string.Empty;
                    rosMigrateExistingLicencePayload.LicenceType = string.Empty;
                    rosMigrateExistingLicencePayload.LicenceDuration = null;
                    rosMigrateExistingLicencePayload.FleetName = string.Empty;

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
                    rosMigrateExistingLicencePayload.PROD = prod;
                }
                else
                {
                    List<UnitOfSales> existingUnitOfSaleList = rosMigrateExistingLicencePayload.PROD.UnitOfSales;
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

            return rosMigrateExistingLicencePayload;
        }

        private SapRecordOfSalePayLoad BuildConvertLicencePayload(List<RecordOfSaleEventPayLoad> eventDataList)
        {
            SapRecordOfSalePayLoad rosConvertLicencePayload = new();

            foreach (var eventData in eventDataList)
            {
                if (rosConvertLicencePayload.PROD == null!)
                {
                    rosConvertLicencePayload.CorrelationId = eventData.Data.CorrelationId;
                    rosConvertLicencePayload.ServiceType = eventData.Data.RecordsOfSale.ProductType;
                    rosConvertLicencePayload.LicTransaction = eventData.Data.RecordsOfSale.TransactionType;
                    rosConvertLicencePayload.OrderNumber = eventData.Data.RecordsOfSale.OrderNumber;
                    rosConvertLicencePayload.PurachaseOrder = eventData.Data.RecordsOfSale.PoRef;
                    rosConvertLicencePayload.LicenceNumber = eventData.Data.RecordsOfSale.SapId;
                    rosConvertLicencePayload.SoldToAcc = string.Empty;
                    rosConvertLicencePayload.LicenseEacc = string.Empty;
                    rosConvertLicencePayload.VesselName = string.Empty;
                    rosConvertLicencePayload.IMONumber = string.Empty;
                    rosConvertLicencePayload.CallSign = string.Empty;
                    rosConvertLicencePayload.ShoreBased = string.Empty;
                    rosConvertLicencePayload.Users = null;
                    rosConvertLicencePayload.EndUserId = string.Empty;
                    rosConvertLicencePayload.ECDISMANUF = string.Empty;
                    rosConvertLicencePayload.StartDate = string.Empty;
                    rosConvertLicencePayload.EndDate = string.Empty;
                    rosConvertLicencePayload.LicenceType = eventData.Data.RecordsOfSale.LicenceType;
                    rosConvertLicencePayload.LicenceDuration = eventData.Data.RecordsOfSale.LicenceDuration;
                    rosConvertLicencePayload.FleetName = string.Empty;

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
                    rosConvertLicencePayload.PROD = prod;
                }
                else
                {
                    List<UnitOfSales> existingUnitOfSaleList = rosConvertLicencePayload.PROD.UnitOfSales;
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

            return rosConvertLicencePayload;
        }
    }
}
