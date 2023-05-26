using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.Options;
using System.Data;
using System.Xml;
using UKHO.ERPFacade.API.Models;
using UKHO.ERPFacade.Common.IO;
using UKHO.ERPFacade.Common.Logging;

namespace UKHO.ERPFacade.API.Helpers
{
    public class XMLBuilder: IXmlBuilder
    {
        private readonly ILogger<XMLBuilder> _logger;
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
        private const string ChildCell = "CHILDCELL";
        private const string ProdType = "PRODTYPE";
        private const string UnitOfSaleSection = "UnitOfSale";
        private const string NotForSale = "NotForSale";
        private const string UnitSaleType = "unit";

        public XMLBuilder(ILogger<XMLBuilder> logger,
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
        public XmlDocument BuildSapMessageXml(EESEvent eventData, string traceId)
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

            bool IsConditionSatisfied = false;
            foreach (var products in eventData.Data.Products)
            {
                foreach (var Uos in products.InUnitsOfSale)
                {
                    //_logger.LogInformation(EventIds.BuildingSapActionStarted.ToEventId(), "Building SAP actions for {Scenario} scenario.", scenario.ScenarioType.ToString());

                    var UnitofSale = eventData.Data.UnitsOfSales.Where(x => x.UnitName == Uos).FirstOrDefault();

                    foreach (var action in _sapActionConfig.Value.SapActions)
                    {
                        XmlElement actionNode;


                        switch (action.ActionNumber)
                        {
                            case 1:
                                foreach (var rules in action.Rules)
                                {
                                    foreach (var conditions in rules.Conditions)
                                    {
                                        object jsonFieldValue = CommonHelper.ParseXmlNode(conditions.AttributeName, products, products.GetType());
                                        if (jsonFieldValue != null && IsValidValue(jsonFieldValue.ToString(), conditions.AttributeValue))
                                        {
                                            IsConditionSatisfied = true;
                                        }
                                        else
                                        {
                                            IsConditionSatisfied = false;
                                            break;
                                        }
                                    }
                                }

                                if (IsConditionSatisfied)
                                {
                                    actionNode = BuildAction(soapXml, products, UnitofSale, eventData, action, ProductSection, products.ProductName);
                                    if (!actionItemNode.InnerXml.Contains(actionNode.InnerXml))
                                        actionItemNode.AppendChild(actionNode);
                                    IsConditionSatisfied = false;
                                }
                                break;
                            case 2:
                                foreach (var rules in action.Rules)
                                {
                                    foreach (var conditions in rules.Conditions)
                                    {
                                        object jsonFieldValue = CommonHelper.ParseXmlNode(conditions.AttributeName, UnitofSale, UnitofSale.GetType());
                                        if (jsonFieldValue != null && IsValidValue(jsonFieldValue.ToString(), conditions.AttributeValue))
                                        {
                                            IsConditionSatisfied = true;
                                        }
                                        else
                                        {
                                            IsConditionSatisfied = false;
                                            break;
                                        }
                                    }
                                }
                                if (IsConditionSatisfied)
                                {
                                    actionNode = BuildAction(soapXml, products, UnitofSale, eventData, action, UnitOfSaleSection, UnitofSale.UnitName);
                                    if (!actionItemNode.InnerXml.Contains(actionNode.InnerXml))
                                        actionItemNode.AppendChild(actionNode);
                                    IsConditionSatisfied = false;
                                }
                                break;
                            case 3:
                                foreach (var product in UnitofSale.CompositionChanges.AddProducts)
                                {
                                    actionNode = BuildAction(soapXml, products, UnitofSale, eventData, action, ProductSection, products.ProductName, product);
                                    if (!actionItemNode.InnerXml.Contains(actionNode.InnerXml))
                                        actionItemNode.AppendChild(actionNode);
                                }

                                break;
                                //case 4:
                                //    foreach (var cell in products.InUnitsOfSale)
                                //    {
                                //        var uos = eventData.Data.UnitsOfSales.Where(x => x.UnitOfSaleType == UnitSaleType && x.UnitName == cell).FirstOrDefault();
                                //        if (uos != null)
                                //        {
                                //            foreach (var product in products.ReplacedBy)
                                //            {
                                //                actionNode = BuildAction(soapXml,products, eventData, action, cell, null, product);
                                //                if (!actionItemNode.InnerXml.Contains(actionNode.InnerXml))
                                //                    actionItemNode.AppendChild(actionNode);
                                //            }
                                //        }
                                //    }
                                //    break;
                                //case 6:
                                //    foreach (var cell in scenario.InUnitOfSales)
                                //    {
                                //        actionNode = BuildAction(soapXml, scenario, action, cell);
                                //        if (!actionItemNode.InnerXml.Contains(actionNode.InnerXml))
                                //            actionItemNode.AppendChild(actionNode);
                                //    }
                                //    break;
                                //case 8:
                                //    foreach (var cell in scenario.InUnitOfSales)
                                //    {
                                //        var uos = scenario.UnitOfSales.Where(x => x.UnitName == cell).FirstOrDefault();

                                //        if (uos != null)
                                //        {
                                //            foreach (var product in uos.CompositionChanges.RemoveProducts)
                                //            {
                                //                actionNode = BuildAction(soapXml, scenario, action, cell);
                                //                if (!actionItemNode.InnerXml.Contains(actionNode.InnerXml))
                                //                    actionItemNode.AppendChild(actionNode);
                                //            }
                                //        }
                                //    }
                                //    break;
                                //case 10:
                                //    var uosNotForSale = scenario.UnitOfSales.Where(x => x.Status == NotForSale).FirstOrDefault();
                                //    if (uosNotForSale != null)
                                //    {
                                //        actionNode = BuildAction(soapXml, scenario, action, uosNotForSale.UnitName);
                                //        if (!actionItemNode.InnerXml.Contains(actionNode.InnerXml))
                                //            actionItemNode.AppendChild(actionNode);
                                //    }
                                //    break;
                                //default:
                                //    actionNode = BuildAction(soapXml, products,null,eventData, action,ProductSection, products.ProductName);
                                //    if (!actionItemNode.InnerXml.Contains(actionNode.InnerXml))
                                //        actionItemNode.AppendChild(actionNode);
                                //    break;
                        }
                        //_logger.LogInformation(EventIds.SapActionCreated.ToEventId(), "SAP action {ActionName} created for {Scenario}.", action.Action, scenario.ScenarioType.ToString());
                    } 
                }
            }

            #region UnitOfSale Commented
            //foreach (var unitOfSale in eventData.Data.UnitsOfSales)
            //{
            //    //_logger.LogInformation(EventIds.BuildingSapActionStarted.ToEventId(), "Building SAP actions for {Scenario} scenario.", scenario.ScenarioType.ToString());

            //    foreach (var action in _sapActionConfig.Value.SapActions)
            //    {
            //        XmlElement actionNode;



            //        switch (action.ActionNumber)
            //        {
            //            case 2:
            //                foreach (var rules in action.Rules)
            //                {
            //                    foreach (var conditions in rules.Conditions)
            //                    {
            //                        object jsonFieldValue = CommonHelper.ParseXmlNode(conditions.AttributeName, unitOfSale, unitOfSale.GetType());
            //                        if (jsonFieldValue != null && IsValidValue(jsonFieldValue.ToString(), conditions.AttributeValue))
            //                        {
            //                            IsConditionSatisfied = true;
            //                        }
            //                        else
            //                        {
            //                            IsConditionSatisfied = false;
            //                            break;
            //                        }
            //                    }
            //                }
            //                if (IsConditionSatisfied)
            //                {
            //                    actionNode = BuildAction(soapXml, null, unitOfSale, eventData, action, UnitOfSaleSection, unitOfSale.UnitName);
            //                    if (!actionItemNode.InnerXml.Contains(actionNode.InnerXml))
            //                        actionItemNode.AppendChild(actionNode);
            //                    IsConditionSatisfied = false;
            //                }
            //                break;
            //                //case 3:
            //                //    foreach (var cell in eventData.Data.UnitsOfSales)
            //                //    {
            //                //        var uos = eventData.Data.UnitsOfSales.Where(x => x.UnitName == cell.UnitName).FirstOrDefault();

            //                //        if (uos != null)
            //                //        {
            //                //            foreach (var product in uos.CompositionChanges.AddProducts)
            //                //            {
            //                //                actionNode = BuildAction(soapXml, products, eventData, action, UnitOfSaleSection, product);
            //                //                if (!actionItemNode.InnerXml.Contains(actionNode.InnerXml))
            //                //                    actionItemNode.AppendChild(actionNode);
            //                //            }
            //                //        }
            //                //    }
            //                //    break;
            //                //case 4:
            //                //    foreach (var cell in products.InUnitsOfSale)
            //                //    {
            //                //        var uos = eventData.Data.UnitsOfSales.Where(x => x.UnitOfSaleType == UnitSaleType && x.UnitName == cell).FirstOrDefault();
            //                //        if (uos != null)
            //                //        {
            //                //            foreach (var product in products.ReplacedBy)
            //                //            {
            //                //                actionNode = BuildAction(soapXml, products, eventData, action, cell, null, product);
            //                //                if (!actionItemNode.InnerXml.Contains(actionNode.InnerXml))
            //                //                    actionItemNode.AppendChild(actionNode);
            //                //            }
            //                //        }
            //                //    }
            //                //    break;
            //                //case 6:
            //                //    foreach (var cell in scenario.InUnitOfSales)
            //                //    {
            //                //        actionNode = BuildAction(soapXml, scenario, action, cell);
            //                //        if (!actionItemNode.InnerXml.Contains(actionNode.InnerXml))
            //                //            actionItemNode.AppendChild(actionNode);
            //                //    }
            //                //    break;
            //                //case 8:
            //                //    foreach (var cell in scenario.InUnitOfSales)
            //                //    {
            //                //        var uos = scenario.UnitOfSales.Where(x => x.UnitName == cell).FirstOrDefault();

            //                //        if (uos != null)
            //                //        {
            //                //            foreach (var product in uos.CompositionChanges.RemoveProducts)
            //                //            {
            //                //                actionNode = BuildAction(soapXml, scenario, action, cell);
            //                //                if (!actionItemNode.InnerXml.Contains(actionNode.InnerXml))
            //                //                    actionItemNode.AppendChild(actionNode);
            //                //            }
            //                //        }
            //                //    }
            //                //    break;
            //                //case 10:
            //                //    var uosNotForSale = scenario.UnitOfSales.Where(x => x.Status == NotForSale).FirstOrDefault();
            //                //    if (uosNotForSale != null)
            //                //    {
            //                //        actionNode = BuildAction(soapXml, scenario, action, uosNotForSale.UnitName);
            //                //        if (!actionItemNode.InnerXml.Contains(actionNode.InnerXml))
            //                //            actionItemNode.AppendChild(actionNode);
            //                //    }
            //                //    break;
            //                //default:
            //                //    actionNode = BuildAction(soapXml, products, eventData, action, products.ProductName);
            //                //    if (!actionItemNode.InnerXml.Contains(actionNode.InnerXml))
            //                //        actionItemNode.AppendChild(actionNode);
            //                //    break;
            //        }
            //        //_logger.LogInformation(EventIds.SapActionCreated.ToEventId(), "SAP action {ActionName} created for {Scenario}.", action.Action, scenario.ScenarioType.ToString());
            //    }
            //} 
            #endregion

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

        private static XmlElement BuildAction(XmlDocument soapXml, Product products,UnitOfSale unitOfSale,EESEvent eventData, SapAction action,string Section ,string cell, string childCell = null, string replacedByProduct = null)
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
                    if (node.XmlNodeName == ReplacedBy && replacedByProduct != null)
                    {
                        itemSubNode.InnerText = GetXmlNodeValue(replacedByProduct.ToString());
                    }
                    else if (node.XmlNodeName == ChildCell && childCell != null)
                    {
                        itemSubNode.InnerText = GetXmlNodeValue(childCell.ToString());
                    }
                    else
                    {
                        object jsonFieldValue = CommonHelper.ParseXmlNode(node.JsonPropertyName, products, products.GetType());
                        itemSubNode.InnerText = GetXmlNodeValue(jsonFieldValue.ToString(),node.XmlNodeName);
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
                    //UnitOfSale Uos = new();
                    ////switch (action.ActionNumber)
                    ////{
                    ////    case 1:
                    ////        Uos = eventData.Data.UnitsOfSales.Where(x => products.InUnitsOfSale.Contains(x.UnitName) && x.UnitOfSaleType == UnitSaleType).FirstOrDefault();
                    ////        break;
                    ////}
                    if (unitOfSale != null)
                    {
                        object jsonFieldValue = CommonHelper.ParseXmlNode(node.JsonPropertyName, unitOfSale, unitOfSale.GetType());
                        itemSubNode.InnerText = GetXmlNodeValue(jsonFieldValue.ToString());
                    }
                    else
                    {
                        itemSubNode.InnerText = string.Empty;
                    }
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

        private static string GetProdType(string prodType)
        {
            var parts = prodType.Split(' ').ToList();
            if (parts != null)
                return parts.Count > 1 ? parts[1] : parts[0];
            else
                return string.Empty;
        }

        private bool IsValidValue(string jsonFieldValue, string attributeValue)
        {
            if (attributeValue.Contains('|'))
            {
                string[] values = attributeValue.Split('|');
                foreach (string value in values)
                {
                    if (jsonFieldValue == value.Trim())
                    {
                        return true;
                    }
                }
                return false;
            }
            else
            {
                return jsonFieldValue == attributeValue;
            }
        }


    }
}
