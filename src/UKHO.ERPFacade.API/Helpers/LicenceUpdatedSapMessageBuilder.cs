using System.Xml;
using System.Xml.Linq;
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
        private const string ShoredBasedValues = "IMO,Non-IMO";

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
            
            string sapXml= RemoveNullFields(xml);
            soapXml.SelectSingleNode(XpathZAddsRos).InnerXml = sapXml;
        
            return soapXml;
        }

        public string SapXmlPayloadCreation(RecordOfSaleEventPayLoad eventData)
        {
            var sapPayload = new SapRecordOfSalePayLoad();

            sapPayload.CorrelationId = eventData.Data.CorrelationId;
            sapPayload.ServiceType = eventData.Data.Licence.ProductType;
            sapPayload.LicTransaction = eventData.Data.Licence.TransactionType;
            sapPayload.SoldToAcc = eventData.Data.Licence.DistributorCustomerNumber;
            sapPayload.LicenseEacc = eventData.Data.Licence.ShippingCoNumber;
            sapPayload.StartDate = eventData.Data.Licence.OrderDate;
            sapPayload.EndDate = eventData.Data.Licence.HoldingsExpiryDate;
            sapPayload.LicenceNumber = eventData.Data.Licence.SapId;
            sapPayload.VesselName = eventData.Data.Licence.VesselName;
            sapPayload.IMONumber = eventData.Data.Licence.ImoNumber;
            sapPayload.CallSign = eventData.Data.Licence.CallSign;
            sapPayload.ShoreBased = GetShoreBasedValue(eventData.Data.Licence.LicenceType);
            sapPayload.FleetName = eventData.Data.Licence.FleetName;
            sapPayload.Users = eventData.Data.Licence.NumberLicenceUsers;
            sapPayload.EndUserId = eventData.Data.Licence.LicenceId;
            sapPayload.ECDISMANUF = eventData.Data.Licence.Upn;
            sapPayload.LicenceType = eventData.Data.Licence.LicenceTypeId;
            sapPayload.LicenceDuration = eventData.Data.Licence.licenceDuration;
            sapPayload.PurachaseOrder = eventData.Data.Licence.PoRef;
            sapPayload.OrderNumber = eventData.Data.Licence.Ordernumber;

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

        private  string RemoveNullFields( string xml)
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

        private string GetShoreBasedValue(string LicenceType)
        {
            string[] shortBase = ShoredBasedValues.Split(",").ToArray();
            bool flag;
            if (string.IsNullOrEmpty(LicenceType)) return "";

            if (shortBase.Contains(LicenceType))
            {
                flag = false;
                return Convert.ToInt32(flag).ToString();
            }
            else
            {
                flag = true;
                return Convert.ToInt32(flag).ToString();
            }
        }
    }
}
