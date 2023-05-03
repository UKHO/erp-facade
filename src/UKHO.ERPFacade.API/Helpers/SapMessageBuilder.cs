using Microsoft.Extensions.Options;
using System.Collections;
using System.Reflection;
using System.Xml;
using UKHO.ERPFacade.API.Models;
using UKHO.ERPFacade.Common.IO;

namespace UKHO.ERPFacade.API.Helpers
{
    public class SapMessageBuilder : ISapMessageBuilder
    {
        private readonly ILogger<SapMessageBuilder> _logger;
        private readonly IXmlHelper _xmlHelper;
        private readonly IFileSystemHelper _fileSystemHelper;
        private readonly IOptions<SapActionConfiguration> _sapActionConfig;
        private readonly IOptions<ActionNumberConfiguration> _actionNumberConfig;

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
        /// <returns></returns>
        public XmlDocument BuildSapMessageXml(List<Scenario> scenarios, string traceId)
        {
            var sapXmlTemplatePath = Path.Combine(Environment.CurrentDirectory, "SapXmlTemplates\\SAPRequest.xml");

            //Check whether template file exists or not
            if (!_fileSystemHelper.IsFileExists(sapXmlTemplatePath))
            {
                throw new FileNotFoundException("The SAP message xml template does not exist in specified path : " + sapXmlTemplatePath);
            }

            XmlDocument soapXml = _xmlHelper.CreateXmlDocument(sapXmlTemplatePath);

            XmlNode IM_MATINFONode = soapXml.SelectSingleNode($"//*[local-name()='IM_MATINFO']");
            XmlNode actionItemNode = soapXml.SelectSingleNode($"//*[local-name()='ACTIONITEMS']");

            foreach (var scenario in scenarios)
            {
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
                        case 8:
                            foreach (var cell in scenario.InUnitOfSales)
                            {
                                actionNode = BuildAction(soapXml, scenario, action, cell);
                                actionItemNode.AppendChild(actionNode);
                            }
                            break;
                        case 4:
                            foreach (var cell in scenario.Product.ReplacedBy)
                            {
                                actionNode = BuildAction(soapXml, scenario, action, cell);
                                actionItemNode.AppendChild(actionNode);
                            }
                            break;
                        default:
                            actionNode = BuildAction(soapXml, scenario, action, scenario.Product.ProductName);
                            actionItemNode.AppendChild(actionNode);
                            break;
                    }
                }
            }

            XmlNode xmlNode = SortXmlPayload(actionItemNode);

            XmlNode noOfActions = soapXml.SelectSingleNode($"//*[local-name()='NOOFACTIONS']");
            XmlNode corrId = soapXml.SelectSingleNode($"//*[local-name()='CORRID']");
            XmlNode recDate = soapXml.SelectSingleNode($"//*[local-name()='RECDATE']");
            XmlNode recTime = soapXml.SelectSingleNode($"//*[local-name()='RECTIME']");

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

            var sortedActionItemList = actionItemList.Cast<XmlNode>().OrderBy(x => Convert.ToInt32(x.SelectSingleNode("ACTIONNUMBER").InnerText)).ToList();

            foreach (XmlNode actionItem in sortedActionItemList)
            {
                actionItem.SelectSingleNode("ACTIONNUMBER").InnerText = sequenceNumber.ToString();
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
            XmlElement itemNode = soapXml.CreateElement("item");

            XmlElement actionNumberNode = soapXml.CreateElement("ACTIONNUMBER");
            actionNumberNode.InnerText = action.ActionNumber.ToString();

            XmlElement actionNode = soapXml.CreateElement("ACTION");
            actionNode.InnerText = action.Action.ToString();

            XmlElement productNode = soapXml.CreateElement("PRODUCT");
            productNode.InnerText = action.Product.ToString();

            itemNode.AppendChild(actionNumberNode);
            itemNode.AppendChild(actionNode);
            itemNode.AppendChild(productNode);

            List<(int sortingOrder, XmlElement itemNode)> actionAttributeList = new();

            foreach (var node in action.Attributes.Where(x => x.Section == "Product"))
            {
                XmlElement itemSubNode = soapXml.CreateElement(node.XmlNodeName);

                if (node.IsRequired)
                {
                    if (node.XmlNodeName == "REPLACEDBY")
                    {
                        itemSubNode.InnerText = string.IsNullOrWhiteSpace(cell.ToString()) ? string.Empty : cell.ToString().Substring(0, Math.Min(250, cell.ToString().Length));
                    }
                    else
                    {
                        object jsonFieldValue = GetProp(node.JsonPropertyName, scenario.Product, scenario.Product.GetType());
                        itemSubNode.InnerText = string.IsNullOrWhiteSpace(jsonFieldValue.ToString()) ? string.Empty
                            : (node.XmlNodeName == "PRODTYPE" ? GetProdType(jsonFieldValue.ToString()) : jsonFieldValue.ToString().Substring(0, Math.Min(250, jsonFieldValue.ToString().Length)));
                    }
                }
                else
                {
                    itemSubNode.InnerText = string.Empty;
                }
                actionAttributeList.Add((node.SortingOrder, itemSubNode));                
            }

            foreach (var node in action.Attributes.Where(x => x.Section == "UnitOfSale"))
            {
                XmlElement itemSubNode = soapXml.CreateElement(node.XmlNodeName);

                if (node.IsRequired)
                {
                    UnitOfSale unitOfSale = scenario.UnitOfSales.Where(x => x.UnitName == cell).FirstOrDefault();

                    object jsonFieldValue = GetProp(node.JsonPropertyName, unitOfSale, unitOfSale.GetType());
                    itemSubNode.InnerText = string.IsNullOrWhiteSpace(jsonFieldValue.ToString()) ? string.Empty : jsonFieldValue.ToString().Substring(0, Math.Min(250, jsonFieldValue.ToString().Length));
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

        public static object GetProp(string name, object obj, Type type)
        {
            var parts = name.Split('.').ToList();
            var currentPart = parts[0];
            PropertyInfo info = type.GetProperty(currentPart);
            if (info == null) { return null; }
            if (name.IndexOf(".") > -1)
            {
                parts.Remove(currentPart);
                return GetProp(string.Join(".", parts), info.GetValue(obj, null), info.PropertyType);
            }
            else
            {
                return info.GetValue(obj, null).ToString();
            }
        }               

        private static string GetProdType(string prodType)
        {
            var parts = prodType.Split(' ').ToList();
            return parts[1];
        }
    }
}
