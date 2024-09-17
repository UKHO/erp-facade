using System.Xml;
using Microsoft.Extensions.Options;
using UKHO.ERPFacade.Common.IO;
using UKHO.ERPFacade.Common.Logging;
using UKHO.ERPFacade.Common.Models;
using UKHO.ERPFacade.Common.Providers;
using UKHO.ERPFacade.Common.PermitDecryption;
using UKHO.ERPFacade.Common.Exceptions;

namespace UKHO.ERPFacade.API.Helpers
{
    public class EncContentSapMessageBuilder : IEncContentSapMessageBuilder
    {
        private readonly ILogger<EncContentSapMessageBuilder> _logger;
        private readonly IXmlHelper _xmlHelper;
        private readonly IFileSystemHelper _fileSystemHelper;
        private readonly IOptions<SapActionConfiguration> _sapActionConfig;
        private readonly IWeekDetailsProvider _weekDetailsProvider;
        private readonly IPermitDecryption _permitDecryption;

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
        private const string ProdTypeValue = "S57";
        private const string UnitOfSaleSection = "UnitOfSale";
        private const string UnitSaleType = "unit";
        private const string EncCell = "ENC CELL";
        private const string AvcsUnit = "AVCS UNIT";
        private const string RecDateFormat = "yyyyMMdd";
        private const string RecTimeFormat = "hhmmss";
        private const string UkhoWeekNumberSection = "UkhoWeekNumber";
        private const string ValidFrom = "VALIDFROM";
        private const string WeekNo = "WEEKNO";
        private const string Correction = "CORRECTION";
        private const string IsCorrectionTrue = "Y";
        private const string IsCorrectionFalse = "N";
        private const string ActiveKey = "ACTIVEKEY";
        private const string NextKey = "NEXTKEY";
        private const string UnitOfSaleStatusForSale = "ForSale";
        private const string Agency = "AGENCY";

        public EncContentSapMessageBuilder(ILogger<EncContentSapMessageBuilder> logger,
                                 IXmlHelper xmlHelper,
                                 IFileSystemHelper fileSystemHelper,
                                 IOptions<SapActionConfiguration> sapActionConfig,
                                 IWeekDetailsProvider weekDetailsProvider,
                                 IPermitDecryption permitDecryption
                                 )
        {
            _logger = logger;
            _xmlHelper = xmlHelper;
            _fileSystemHelper = fileSystemHelper;
            _sapActionConfig = sapActionConfig;
            _weekDetailsProvider = weekDetailsProvider;
            _permitDecryption = permitDecryption;
        }

        /// <summary>
        /// Generate SAP message xml file.
        /// </summary>
        /// <param name="eventData"></param>
        /// <param name="correlationId"></param>
        /// <returns>XmlDocument</returns>
        public XmlDocument BuildSapMessageXml(EncEventPayload eventData, string correlationId)
        {
            string sapXmlTemplatePath = Path.Combine(Environment.CurrentDirectory, SapXmlPath);

            //Check whether template file exists or not
            if (!_fileSystemHelper.IsFileExists(sapXmlTemplatePath))
            {
                _logger.LogError(EventIds.SapXmlTemplateNotFound.ToEventId(), "The SAP message xml template does not exist.");
                throw new FileNotFoundException();
            }

            var ukhoWeekNumber = eventData.Data.UkhoWeekNumber;

            XmlDocument soapXml = _xmlHelper.CreateXmlDocument(sapXmlTemplatePath);

            XmlNode IM_MATINFONode = soapXml.SelectSingleNode(XpathImMatInfo);
            XmlNode actionItemNode = soapXml.SelectSingleNode(XpathActionItems);

            _logger.LogInformation(EventIds.BuildingSapActionStarted.ToEventId(), "Building SAP actions.");
            //Actions for Product section
            foreach (var product in eventData.Data.Products)
            {
                foreach (var action in _sapActionConfig.Value.SapActions.Where(x => x.Product == EncCell))
                {
                    XmlElement actionNode;
                    switch (action.ActionNumber)
                    {
                        case 1:
                            if (IsRuleConditionSatisfiedForSapAction(action, product))
                            {
                                var unitFromAddProduct = GetUnitFromAddProductComposition(eventData.Data.UnitsOfSales, product);
                                if (unitFromAddProduct == null)
                                {
                                    LogException("No unit found in Add product composition while creating SAP action " + action.Action + ".");
                                }

                                actionNode = BuildAction(soapXml, product, unitFromAddProduct, action, ukhoWeekNumber, product.ProductName);
                                actionItemNode.AppendChild(actionNode);
                                _logger.LogInformation(EventIds.SapActionCreated.ToEventId(), "SAP action {ActionName} created.", action.Action);
                            }
                            break;

                        case 4:
                            if (IsRuleConditionSatisfiedForSapAction(action, product) && product.ReplacedBy.Any())
                            {
                                var unitOfSaleForReplace = GetUnitFromRemoveProductComposition(eventData.Data.UnitsOfSales, product);
                                if (unitOfSaleForReplace == null)
                                {
                                    LogException("No unit found in remove product compositions while creating SAP action " + action.Action + ".");
                                }

                                foreach (var replacedProduct in product.ReplacedBy)
                                {
                                    actionNode = BuildAction(soapXml, product, unitOfSaleForReplace, action, ukhoWeekNumber, product.ProductName, replacedProduct);
                                    actionItemNode.AppendChild(actionNode);
                                    _logger.LogInformation(EventIds.SapActionCreated.ToEventId(), "SAP action {ActionName} created.", action.Action);
                                }
                            }
                            break;

                        case 5:
                            foreach (var additionalCoverageProduct in product.AdditionalCoverage)
                            {
                                actionNode = BuildAction(soapXml, product, null, action, ukhoWeekNumber, product.ProductName, additionalCoverageProduct);
                                actionItemNode.AppendChild(actionNode);
                                _logger.LogInformation(EventIds.SapActionCreated.ToEventId(), "SAP action {ActionName} created.", action.Action);
                            }
                            break;

                        case 6:
                        case 8:
                            var unitFromInUnitOfSale = GetUnitFromInUnitOfSale(eventData.Data.UnitsOfSales, product);
                            if (IsRuleConditionSatisfiedForSapAction(action, product) && unitFromInUnitOfSale != null)
                            {
                                actionNode = BuildAction(soapXml, product, unitFromInUnitOfSale, action, ukhoWeekNumber, product.ProductName);
                                actionItemNode.AppendChild(actionNode);
                                _logger.LogInformation(EventIds.SapActionCreated.ToEventId(), "SAP action {ActionName} created.", action.Action);
                            }
                            break;

                        case 10:
                            if (IsRuleConditionSatisfiedForSapAction(action, product))
                            {
                                var unitFromRemoveProduct = GetUnitFromRemoveProductComposition(eventData.Data.UnitsOfSales, product);
                                if (unitFromRemoveProduct == null)
                                {
                                    LogException("No unit found in remove product compositions while creating SAP action " + action.Action + ".");
                                }

                                actionNode = BuildAction(soapXml, product, unitFromRemoveProduct, action, ukhoWeekNumber, product.ProductName);
                                actionItemNode.AppendChild(actionNode);
                                _logger.LogInformation(EventIds.SapActionCreated.ToEventId(), "SAP action {ActionName} created.", action.Action);
                            }
                            break;
                    }
                }
            }
            //Actions for Unit of Sale section
            foreach (var unitOfSale in eventData.Data.UnitsOfSales)
            {
                foreach (var action in _sapActionConfig.Value.SapActions.Where(x => x.Product == AvcsUnit))
                {
                    XmlElement actionNode;
                    switch (action.ActionNumber)
                    {
                        case 2:
                        case 7:
                        case 11:
                            if (IsRuleConditionSatisfiedForSapAction(action, unitOfSale))
                            {
                                actionNode = BuildAction(soapXml, null, unitOfSale, action, ukhoWeekNumber, string.Empty);
                                actionItemNode.AppendChild(actionNode);
                                _logger.LogInformation(EventIds.SapActionCreated.ToEventId(), "SAP action {ActionName} created.", action.Action);
                            }
                            break;

                        case 3:
                            foreach (var addProduct in unitOfSale.CompositionChanges.AddProducts)
                            {
                                actionNode = BuildAction(soapXml, null, unitOfSale, action, ukhoWeekNumber, addProduct);
                                actionItemNode.AppendChild(actionNode);
                                _logger.LogInformation(EventIds.SapActionCreated.ToEventId(), "SAP action {ActionName} created.", action.Action);
                            }
                            break;

                        case 9:
                            foreach (var removeProduct in unitOfSale.CompositionChanges.RemoveProducts)
                            {
                                actionNode = BuildAction(soapXml, null, unitOfSale, action, ukhoWeekNumber, removeProduct);
                                actionItemNode.AppendChild(actionNode);
                                _logger.LogInformation(EventIds.SapActionCreated.ToEventId(), "SAP action {ActionName} created.", action.Action);
                            }
                            break;
                    }
                }
            }

            XmlNode xmlNode = SortXmlPayload(actionItemNode);

            XmlNode noOfActions = soapXml.SelectSingleNode(XpathNoOfActions);
            XmlNode corrId = soapXml.SelectSingleNode(XpathCorrId);
            XmlNode recDate = soapXml.SelectSingleNode(XpathRecDate);
            XmlNode recTime = soapXml.SelectSingleNode(XpathRecTime);

            corrId.InnerText = correlationId;
            noOfActions.InnerText = xmlNode.ChildNodes.Count.ToString();
            recDate.InnerText = DateTime.UtcNow.ToString(RecDateFormat);
            recTime.InnerText = DateTime.UtcNow.ToString(RecTimeFormat);

            IM_MATINFONode.AppendChild(xmlNode);

            return soapXml;
        }

        private XmlElement BuildAction(XmlDocument soapXml, Product product, UnitOfSale unitOfSale, SapAction action, UkhoWeekNumber ukhoWeekNumber, string childCell, string replacedByOrAddCoverageProduct = null)
        {
            XmlElement itemNode = soapXml.CreateElement(Item);

            XmlElement actionNumberNode = soapXml.CreateElement(ActionNumber);
            actionNumberNode.InnerText = action.ActionNumber.ToString();

            XmlElement actionNode = soapXml.CreateElement(Action);
            actionNode.InnerText = action.Action.ToString();

            XmlElement productNode = soapXml.CreateElement(Product);
            productNode.InnerText = action.Product.ToString();

            XmlElement prodTypeNode = soapXml.CreateElement(ProdType);
            prodTypeNode.InnerText = ProdTypeValue;

            XmlElement childCellNode = soapXml.CreateElement(ChildCell);
            childCellNode.InnerText = childCell;

            itemNode.AppendChild(actionNumberNode);
            itemNode.AppendChild(actionNode);
            itemNode.AppendChild(productNode);
            itemNode.AppendChild(prodTypeNode);
            itemNode.AppendChild(childCellNode);

            List<(int sortingOrder, XmlElement itemNode)> actionAttributeList = new();

            PermitKey? permitKey = (product != null && action.ActionNumber is 1 or 8) ? _permitDecryption.GetPermitKeys(product.Permit) : null;

            foreach (var node in action.Attributes.Where(x => x.Section == ProductSection))
            {
                XmlElement itemSubNode = soapXml.CreateElement(node.XmlNodeName);

                if (node.IsRequired)
                {
                    if (node.XmlNodeName == ReplacedBy && replacedByOrAddCoverageProduct != null)
                    {
                        itemSubNode.InnerText = GetXmlNodeValue(replacedByOrAddCoverageProduct.ToString());
                    }
                    else if (node.XmlNodeName == ActiveKey)
                    {
                        itemSubNode.InnerText = string.Empty;
                        if (permitKey != null && !string.IsNullOrEmpty(permitKey.ActiveKey))
                        {
                            itemSubNode.InnerText = permitKey.ActiveKey;
                        }
                    }
                    else if (node.XmlNodeName == NextKey)
                    {
                        itemSubNode.InnerText = string.Empty;
                        if (permitKey != null && !string.IsNullOrEmpty(permitKey.NextKey))
                        {
                            itemSubNode.InnerText = permitKey.NextKey;
                        }
                    }
                    else
                    {
                        object jsonFieldValue = CommonHelper.ParseXmlNode(node.JsonPropertyName, product, product.GetType());
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

                if (node.IsRequired && unitOfSale != null)
                {
                    object jsonFieldValue = CommonHelper.ParseXmlNode(node.JsonPropertyName, unitOfSale, unitOfSale.GetType());
                    itemSubNode.InnerText = GetXmlNodeValue(jsonFieldValue.ToString(), node.XmlNodeName);
                }
                else
                {
                    itemSubNode.InnerText = string.Empty;
                }
                actionAttributeList.Add((node.SortingOrder, itemSubNode));
            }

            foreach (var node in action.Attributes.Where(x => x.Section == UkhoWeekNumberSection))
            {
                XmlElement itemSubNode = soapXml.CreateElement(node.XmlNodeName);

                if (node.IsRequired)
                {
                    if (IsValidWeekNumber(ukhoWeekNumber))
                    {
                        switch (node.XmlNodeName)
                        {
                            case ValidFrom:
                                string weekDate = _weekDetailsProvider.GetDateOfWeek(
                                ukhoWeekNumber.Year, ukhoWeekNumber.Week, ukhoWeekNumber.CurrentWeekAlphaCorrection);
                                itemSubNode.InnerText = GetXmlNodeValue(weekDate);
                                break;

                            case WeekNo:
                                string weekData = GetUkhoWeekNumberData(ukhoWeekNumber);
                                itemSubNode.InnerText = GetXmlNodeValue(weekData);
                                break;

                            case Correction:
                                itemSubNode.InnerText = ukhoWeekNumber.CurrentWeekAlphaCorrection ? GetXmlNodeValue(IsCorrectionTrue) : GetXmlNodeValue(IsCorrectionFalse);
                                break;
                        }
                    }
                    else
                    {
                        _logger.LogError(EventIds.InvalidUkhoWeekNumber.ToEventId(), "Invalid UkhoWeekNumber field received in enccontentpublished event.");
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

        private XmlNode SortXmlPayload(XmlNode actionItemNode)
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

        private string GetXmlNodeValue(string fieldValue, string xmlNodeName = null)
        {
            if (!string.IsNullOrWhiteSpace(fieldValue))
            {
                if (xmlNodeName == Agency)
                {
                    return !string.IsNullOrEmpty(fieldValue) ? fieldValue.Substring(0, 2) : string.Empty;
                }

                return fieldValue.Substring(0, Math.Min(250, fieldValue.Length));
            }
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

        private UnitOfSale? GetUnitFromInUnitOfSale(List<UnitOfSale> listOfUnitOfSales, Product product)
        {
            return listOfUnitOfSales.FirstOrDefault(x => x.UnitOfSaleType == UnitSaleType && x.Status == UnitOfSaleStatusForSale && product.InUnitsOfSale.Contains(x.UnitName));
        }

        private UnitOfSale? GetUnitFromAddProductComposition(List<UnitOfSale> listOfUnitOfSales, Product product)
        {
            return listOfUnitOfSales.FirstOrDefault(x => x.UnitOfSaleType == UnitSaleType && x.Status == UnitOfSaleStatusForSale && x.CompositionChanges.AddProducts.Contains(product.ProductName));
        }

        private UnitOfSale? GetUnitFromRemoveProductComposition(List<UnitOfSale> listOfUnitOfSales, Product product)
        {
            return listOfUnitOfSales.FirstOrDefault(x => x.UnitOfSaleType == UnitSaleType && x.CompositionChanges.RemoveProducts.Contains(product.ProductName));
        }

        private string GetUkhoWeekNumberData(UkhoWeekNumber ukhoWeekNumber)
        {
            var validWeek = ukhoWeekNumber.Week.ToString("D2");
            var weekNumber = string.Join("", ukhoWeekNumber.Year, validWeek);

            return weekNumber;
        }

        private bool IsValidWeekNumber(UkhoWeekNumber ukhoWeekNumber)
        {
            bool isValid = ukhoWeekNumber != null!;
            if (!isValid) return isValid;

            if (ukhoWeekNumber.Week == 0 || ukhoWeekNumber.Year == 0) isValid = false;

            return isValid;
        }

        private bool IsRuleConditionSatisfiedForSapAction(SapAction action, Object obj)
        {
            bool isConditionSatisfied = false;
            foreach (var rules in action.Rules)
            {
                foreach (var conditions in rules.Conditions)
                {
                    object jsonFieldValue = CommonHelper.ParseXmlNode(conditions.AttributeName, obj, obj.GetType());
                    if (jsonFieldValue != null! && IsValidValue(jsonFieldValue.ToString(), conditions.AttributeValue))
                    {
                        isConditionSatisfied = true;
                    }
                    else
                    {
                        isConditionSatisfied = false;
                        break;
                    }
                }
                if (isConditionSatisfied) break;
            }
            return isConditionSatisfied;
        }

        private void LogException(string errorMessage)
        {
            _logger.LogError(EventIds.UnitOfSaleNotFoundException.ToEventId(), errorMessage);
            throw new ERPFacadeException(EventIds.UnitOfSaleNotFoundException.ToEventId());
        }
    }
}
