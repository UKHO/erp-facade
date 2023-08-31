﻿using System.Xml;
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
            var sapPayload = new SapRecordOfSalePayLoad
            {
                CorrelationId = eventData.Data.CorrelationId,
                ServiceType = eventData.Data.RecordsOfSale.ProductType,
                LicTransaction = eventData.Data.RecordsOfSale.TransactionType,
                SoldToAcc = eventData.Data.RecordsOfSale.TransactionType == MaintainHoldingsType ? "" : eventData.Data.RecordsOfSale.DistributorCustomerNumber,
                LicenseEacc = eventData.Data.RecordsOfSale.TransactionType == MaintainHoldingsType ? "" : eventData.Data.RecordsOfSale.ShippingCoNumber,
                LicenceNumber = eventData.Data.RecordsOfSale.SapId,
                VesselName = eventData.Data.RecordsOfSale.TransactionType == MaintainHoldingsType ? "" : eventData.Data.RecordsOfSale.VesselName,
                IMONumber = eventData.Data.RecordsOfSale.TransactionType == MaintainHoldingsType ? "" : eventData.Data.RecordsOfSale.ImoNumber,
                CallSign = eventData.Data.RecordsOfSale.TransactionType == MaintainHoldingsType ? "" : eventData.Data.RecordsOfSale.CallSign,
                ShoreBased = eventData.Data.RecordsOfSale.TransactionType == MaintainHoldingsType ? "" :  eventData.Data.RecordsOfSale.ShoreBased,
                FleetName = eventData.Data.RecordsOfSale.TransactionType == MaintainHoldingsType ? "" : eventData.Data.RecordsOfSale.FleetName,
                Users = eventData.Data.RecordsOfSale.TransactionType == MaintainHoldingsType ? null : eventData.Data.RecordsOfSale.NumberLicenceUsers,
                EndUserId = eventData.Data.RecordsOfSale.TransactionType == MaintainHoldingsType ? "" : eventData.Data.RecordsOfSale.LicenceId,
                ECDISMANUF = eventData.Data.RecordsOfSale.TransactionType == MaintainHoldingsType ? "" : eventData.Data.RecordsOfSale.Upn,
                OrderNumber = eventData.Data.RecordsOfSale.OrderNumber,
                StartDate = eventData.Data.RecordsOfSale.TransactionType == MaintainHoldingsType ? "" : eventData.Data.RecordsOfSale.OrderDate,
                PurachaseOrder = eventData.Data.RecordsOfSale.PoRef,
                EndDate = eventData.Data.RecordsOfSale.TransactionType == MaintainHoldingsType ? "" : eventData.Data.RecordsOfSale.HoldingsExpiryDate,
                LicenceType = eventData.Data.RecordsOfSale.TransactionType == MaintainHoldingsType ? "" : eventData.Data.RecordsOfSale.LicenceType,
                LicenceDuration = eventData.Data.RecordsOfSale.TransactionType == MaintainHoldingsType ? null : eventData.Data.RecordsOfSale.LicenceDuration
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
            sapPayload.PROD = prod;
             
            return sapPayload;
        }
    }
}
