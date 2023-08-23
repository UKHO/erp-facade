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
                _logger.LogError(EventIds.SapXmlTemplateNotFound.ToEventId(), "The SAP message xml template does not exist.");
                throw new FileNotFoundException();
            }

            XmlDocument soapXml = _xmlHelper.CreateXmlDocument(sapXmlTemplatePath);
            string xml = SapXmlPayloadCreation(eventData);

            string sapXml = RemoveNullFields(xml.Replace(ImOrderNameSpace, ""));
            soapXml.SelectSingleNode(XpathZAddsRos).InnerXml = sapXml.SetXmlClosingTags();
            
            return soapXml;
        }

        public string SapXmlPayloadCreation(RecordOfSaleEventPayLoad eventData)
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
 
            return _xmlHelper.CreateRecordOfSaleSapXmlPayLoad(sapPayload);
        }

        private string RemoveNullFields( string xml)
        {
            XmlDocument xmldoc = new();
            xmldoc.LoadXml(xml);

            XmlNamespaceManager mgr = new XmlNamespaceManager(xmldoc.NameTable);
            mgr.AddNamespace("xsi", "http://www.w3.org/2001/XMLSchema-instance");
            
            XmlNodeList nullFields = xmldoc.SelectNodes("//*[@xsi:nil='true']", mgr);

            if (nullFields != null && nullFields.Count > 0)
            {
                for (int i = 0; i < nullFields.Count; i++)
                {
                    XmlDocumentFragment xmlDocFrag = xmldoc.CreateDocumentFragment();
                    string newNode = "<"+ nullFields[i].Name + "></"+nullFields[i].Name +">";
                    xmlDocFrag.InnerXml = newNode;
                  
                    var previousNode= nullFields[i].PreviousSibling;
                    string Xpath = $"//*[local-name()='{previousNode.Name}']";

                    XmlElement element = (XmlElement)xmldoc.SelectSingleNode(Xpath);

                    nullFields[i].ParentNode.RemoveChild(nullFields[i]);

                    XmlNode parent = element.ParentNode;
                    //now, use that parent element and it's InsertAfter method to add new node as sibling to your found element
                    parent.InsertAfter(xmlDocFrag, element);
                }
            }
            return xmldoc.InnerXml;
        }
    }
}
