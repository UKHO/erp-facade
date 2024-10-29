using System.Xml;
using UKHO.ERPFacade.Common.Constants;
using UKHO.ERPFacade.Common.Extensions;
using UKHO.ERPFacade.Common.Logging;
using UKHO.ERPFacade.Common.Models;
using UKHO.ERPFacade.Common.Operations;
using UKHO.ERPFacade.Common.Operations.IO;

namespace UKHO.ERPFacade.API.SapMessageBuilders
{
    public class LicenceUpdatedSapMessageBuilder : ILicenceUpdatedSapMessageBuilder
    {
        private readonly ILogger<LicenceUpdatedSapMessageBuilder> _logger;
        private readonly IXmlOperations _xmlOperations;
        private readonly IFileOperations _fileOperations;

        public LicenceUpdatedSapMessageBuilder(ILogger<LicenceUpdatedSapMessageBuilder> logger,
                                               IXmlOperations xmlOperations,
                                               IFileOperations fileOperations)
        {
            _logger = logger;
            _xmlOperations = xmlOperations;
            _fileOperations = fileOperations;
        }

        public XmlDocument BuildLicenceUpdatedSapMessageXml(LicenceUpdatedEventPayLoad eventData, string correlationId)
        {
            string sapXmlTemplatePath = Path.Combine(Environment.CurrentDirectory, XmlTemplateInfo.RecordOfSaleSapXmlTemplatePath);

            if (!_fileOperations.IsFileExists(sapXmlTemplatePath))
            {
                _logger.LogError(EventIds.LicenceUpdatedSapXmlTemplateNotFound.ToEventId(), "The licence updated SAP message xml template does not exist.");
                throw new FileNotFoundException();
            }

            _logger.LogInformation(EventIds.CreatingLicenceUpdatedSapPayload.ToEventId(), "Creating licence updated SAP Payload.");

            XmlDocument soapXml = _xmlOperations.CreateXmlDocument(sapXmlTemplatePath);

            var sapRecordOfSalePayLoad = BuildChangeLicencePayload(eventData);

            string xml = _xmlOperations.CreateXmlPayLoad(sapRecordOfSalePayLoad);

            string sapXml = xml.Replace(XmlFields.ImOrderNameSpace, "");

            soapXml.SelectSingleNode(XmlTemplateInfo.XpathZAddsRos).InnerXml = sapXml.RemoveNullFields().SetXmlClosingTags();

            _logger.LogInformation(EventIds.CreatedLicenceUpdatedSapPayload.ToEventId(), "Licence updated SAP payload created.");

            return soapXml;
        }

        private SapRecordOfSalePayLoad BuildChangeLicencePayload(LicenceUpdatedEventPayLoad eventData)
        {
            var changeLicencePayload = new SapRecordOfSalePayLoad
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
                ECDISMANUF = eventData.Data.Licence.EcdisManufacturerName,
                OrderNumber = string.Empty,
                StartDate = string.Empty,
                PurachaseOrder = string.Empty,
                EndDate = string.Empty,
                LicenceType = string.Empty,
                LicenceDuration = null
            };

            changeLicencePayload.PROD = new PROD()
            {
                UnitOfSales = new List<UnitOfSales>()
                {
                    new UnitOfSales()
                    {
                        Id = string.Empty,
                        EndDate = string.Empty,
                        Duration = string.Empty,
                        ReNew = string.Empty,
                        Repeat = string.Empty
                    }
                }
            };

            return changeLicencePayload;
        }
    }
}
