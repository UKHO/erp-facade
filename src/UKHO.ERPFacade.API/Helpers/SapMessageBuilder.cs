using Microsoft.Extensions.Options;
using System.Xml;
using UKHO.ERPFacade.API.Models;
using UKHO.ERPFacade.Common.IO;
using UKHO.ERPFacade.Common.Logging;

namespace UKHO.ERPFacade.API.Helpers
{
    public class SapMessageBuilder : ISapMessageBuilder
    {
        private readonly ILogger<SapMessageBuilder> _logger;
        private readonly IXmlHelper _xmlHelper;
        private readonly IFileSystemHelper _fileSystemHelper;
        private readonly IOptions<SapActionConfiguration> _sapActionConfig;
        private readonly IOptions<ActionNumberConfiguration> _actionNumberConfig;

        private const string SapXmlPath = "SapXmlTemplates\\SAPRequest.xml";
        private const string XpathImMatInfo = $"//*[local-name()='IM_MATINFO']";
        private const string XpathActionItems = $"//*[local-name()='ACTIONITEMS']";
        private const string XpathNoOfActions = $"//*[local-name()='NOOFACTIONS']";
        private const string XpathCorrId = $"//*[local-name()='CORRID']";
        private const string XpathRecDate = $"//*[local-name()='RECDATE']";
        private const string XpathRecTime = $"//*[local-name()='RECTIME']";
        private const string ActionNumber = "ACTIONNUMBER";
        private const string Item = "item";
        private const string Action = "ACTION";
        private const string Product = "PRODUCT";
        private const string ProductSection = "Product";
        private const string ReplacedBy = "REPLACEDBY";
        private const string ProdType = "PRODTYPE";
        private const string UnitOfSaleSection = "UnitOfSale";

        public SapMessageBuilder(ILogger<SapMessageBuilder> logger,
                                 IXmlHelper xmlHelper,
                                 IFileSystemHelper fileSystemHelper,
                                 IOptions<SapActionConfiguration> sapActionConfig,
                                 IOptions<ActionNumberConfiguration> actionNumberConfig)
        {
            _logger = logger;
            _xmlHelper = xmlHelper;
            _fileSystemHelper = fileSystemHelper;
            _sapActionConfig = sapActionConfig;
            _actionNumberConfig = actionNumberConfig;
        }

        /// <summary>
        /// Generate SAP message xml file.
        /// </summary>
        /// <param name="scenarios"></param>
        /// <param name="traceId"></param>
        /// <returns>XmlDocument</returns>
        public XmlDocument BuildSapMessageXml(List<Scenario> scenarios, string traceId)
        {
            var sapXmlTemplatePath = Path.Combine(Environment.CurrentDirectory, SapXmlPath);

            //Check whether template file exists or not
            if (!_fileSystemHelper.IsFileExists(sapXmlTemplatePath))
            {
                _logger.LogError(EventIds.SapXmlTemplateNotFound.ToEventId(), "The SAP message xml template does not exist.");
                throw new FileNotFoundException();
            }

            XmlDocument soapXml = _xmlHelper.CreateXmlDocument(sapXmlTemplatePath);

            XmlNode IM_MATINFONode = soapXml.SelectSingleNode(XpathImMatInfo);
            XmlNode actionItemNode = soapXml.SelectSingleNode(XpathActionItems);

            foreach (var scenario in scenarios)
            {
                _logger.LogInformation(EventIds.BuildingSapActionStarted.ToEventId(), "Building SAP actions for scenario {Scenario}.", scenario);

                ActionNumber actionNumbers = _actionNumberConfig.Value.Actions.Where(x => x.Scenario == scenario.ScenarioType.ToString()).FirstOrDefault();
                List<int> actions = actionNumbers.ActionNumbers.ToList();

                foreach (var action in _sapActionConfig.Value.SapActions.Where(x => actions.Contains(x.ActionNumber)))
                {
                    XmlElement actionNode;

                    switch (action.ActionNumber)
                    {
                        case 2:
                            var unitOfSale = scenario.UnitOfSales.Where(x => x.UnitName == scenario.Product.ProductName).FirstOrDefault();
                            if (unitOfSale.IsNewUnitOfSale)
                            {
                                actionNode = BuildAction(soapXml, scenario, action, scenario.Product.ProductName);
                                actionItemNode.AppendChild(actionNode);
                            }
                            break;
                        case 3:
                        case 6:
                        case 8:
                            foreach (var cell in scenario.InUnitOfSales)
                            {
                                if (scenario.ScenarioType == ScenarioType.ChangeMoveCell && action.ActionNumber == 8)
                                {
                                    var uos = scenario.UnitOfSales.Where(x => x.UnitName == cell).FirstOrDefault();

                                    if (action.ActionNumber == 8)
                                    {
                                        foreach (var product in uos.CompositionChanges.RemoveProducts)
                                        {
                                            actionNode = BuildAction(soapXml, scenario, action, cell);
                                            actionItemNode.AppendChild(actionNode);
                                        }
                                    }
                                }
                                else
                                {
                                    actionNode = BuildAction(soapXml, scenario, action, cell);
                                    actionItemNode.AppendChild(actionNode);
                                }
                            }
                            break;
                        case 4:
                            foreach (var cell in scenario.Product.ReplacedBy)
                            {
                                actionNode = BuildAction(soapXml, scenario, action, cell);
                                actionItemNode.AppendChild(actionNode);
                            }
                            break;
                        case 11:
                            foreach (var cell in scenario.InUnitOfSales)
                            {
                                var uos = scenario.UnitOfSales.Where(x => x.UnitName == cell).FirstOrDefault();

                                foreach (var product in uos.CompositionChanges.AddProducts)
                                {
                                    actionNode = BuildAction(soapXml, scenario, action, cell);
                                    actionItemNode.AppendChild(actionNode);
                                }
                            }
                            break;
                        default:
                            actionNode = BuildAction(soapXml, scenario, action, scenario.Product.ProductName);
                            actionItemNode.AppendChild(actionNode);
                            break;
                    }
                    _logger.LogInformation(EventIds.SapActionCreated.ToEventId(), "SAP action {ActionName} created for {Scenario}.", action.Action, scenario);
                }
            }

            XmlNode xmlNode = SortXmlPayload(actionItemNode);

            XmlNode noOfActions = soapXml.SelectSingleNode(XpathNoOfActions);
            XmlNode corrId = soapXml.SelectSingleNode(XpathCorrId);
            XmlNode recDate = soapXml.SelectSingleNode(XpathRecDate);
            XmlNode recTime = soapXml.SelectSingleNode(XpathRecTime);

            corrId.InnerText = traceId;
            noOfActions.InnerText = xmlNode.ChildNodes.Count.ToString();
            recDate.InnerText = DateTime.UtcNow.ToString("yyyyMMdd");
            recTime.InnerText = DateTime.UtcNow.ToString("hhmmss");

            IM_MATINFONode.AppendChild(xmlNode);

            return soapXml;
        }

        private static XmlNode SortXmlPayload(XmlNode actionItemNode)
        {
            List<XmlNode> actionItemList = new();
            int sequenceNumber = 1;

            foreach (XmlNode subNode in actionItemNode)
            {
                actionItemList.Add(subNode);
            }

            var sortedActionItemList = actionItemList.Cast<XmlNode>().OrderBy(x => Convert.ToInt32(x.SelectSingleNode(ActionNumber).InnerText)).ToList();

            foreach (XmlNode actionItem in sortedActionItemList)
            {
                actionItem.SelectSingleNode(ActionNumber).InnerText = sequenceNumber.ToString();
                sequenceNumber++;
            }

            foreach (XmlNode actionItem in sortedActionItemList)
            {
                actionItemNode.AppendChild(actionItem);
            }
            return actionItemNode;
        }

        private static XmlElement BuildAction(XmlDocument soapXml, Scenario scenario, SapAction action, string cell)
        {
            XmlElement itemNode = soapXml.CreateElement(Item);

            XmlElement actionNumberNode = soapXml.CreateElement(ActionNumber);
            actionNumberNode.InnerText = action.ActionNumber.ToString();

            XmlElement actionNode = soapXml.CreateElement(Action);
            actionNode.InnerText = action.Action.ToString();

            XmlElement productNode = soapXml.CreateElement(Product);
            productNode.InnerText = action.Product.ToString();

            itemNode.AppendChild(actionNumberNode);
            itemNode.AppendChild(actionNode);
            itemNode.AppendChild(productNode);

            List<(int sortingOrder, XmlElement itemNode)> actionAttributeList = new();

            foreach (var node in action.Attributes.Where(x => x.Section == ProductSection))
            {
                XmlElement itemSubNode = soapXml.CreateElement(node.XmlNodeName);

                if (node.IsRequired)
                {
                    if (node.XmlNodeName == ReplacedBy)
                    {
                        itemSubNode.InnerText = GetXmlNodeValue(cell.ToString());
                    }
                    else
                    {
                        object jsonFieldValue = CommonHelper.ParseXmlNode(node.JsonPropertyName, scenario.Product, scenario.Product.GetType());
                        itemSubNode.InnerText = GetXmlNodeValue(jsonFieldValue.ToString(), node.XmlNodeName);
                    }
                }
                else
                {
                    itemSubNode.InnerText = string.Empty;
                }
                actionAttributeList.Add((node.SortingOrder, itemSubNode));
            }

            foreach (var node in action.Attributes.Where(x => x.Section == UnitOfSaleSection))
            {
                XmlElement itemSubNode = soapXml.CreateElement(node.XmlNodeName);

                if (node.IsRequired)
                {
                    UnitOfSale unitOfSale = scenario.UnitOfSales.Where(x => x.UnitName == cell).FirstOrDefault();

                    object jsonFieldValue = CommonHelper.ParseXmlNode(node.JsonPropertyName, unitOfSale, unitOfSale.GetType());
                    itemSubNode.InnerText = GetXmlNodeValue(jsonFieldValue.ToString());
                }
                else
                {
                    itemSubNode.InnerText = string.Empty;
                }
                actionAttributeList.Add((node.SortingOrder, itemSubNode));
            }
            var sortedActionAttributeList = actionAttributeList.OrderBy(x => x.sortingOrder).ToList();

            foreach (var itemAttribute in sortedActionAttributeList)
            {
                itemNode.AppendChild(itemAttribute.itemNode);
            }
            return itemNode;
        }

        private static string GetProdType(string prodType)
        {
            var parts = prodType.Split(' ').ToList();
            if (parts != null)
                return parts.Count > 1 ? parts[1] : parts[0];
            else
                return string.Empty;
        }

        private static string GetXmlNodeValue(string fieldValue, string xmlNodeName = null)
        {
            if (string.IsNullOrWhiteSpace(fieldValue))
                return string.Empty;

            if (xmlNodeName == ProdType)
            {
                return GetProdType(fieldValue);
            }

            return fieldValue.Substring(0, Math.Min(250, fieldValue.Length));
        }
    }
}