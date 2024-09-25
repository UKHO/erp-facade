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
        private const string CreateEncCell = "CREATE ENC CELL";
        private const string UpdateCell = "UPDATE ENC CELL EDITION UPDATE NUMBER";

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
        public XmlDocument BuildSapMessageXml(EncEventPayload eventData)
        {
            try
            {
                string sapXmlTemplatePath = Path.Combine(Environment.CurrentDirectory, SapXmlPath);

                // Check if SAP XML payload template exists
                if (!_fileSystemHelper.IsFileExists(sapXmlTemplatePath))
                {
                    _logger.LogError(EventIds.SapXmlTemplateNotFound.ToEventId(), "The SAP xml payload template does not exist.");
                    throw new FileNotFoundException("The SAP xml payload template does not exist.");
                }

                var soapXml = _xmlHelper.CreateXmlDocument(sapXmlTemplatePath);

                var actionItemNode = soapXml.SelectSingleNode(XpathActionItems);

                _logger.LogInformation(EventIds.GenerationOfSapXmlPayloadStarted.ToEventId(), "Generation of SAP XML payload started.");

                // Build SAP actions for ENC Cell
                BuildEncCellActions(eventData, soapXml, actionItemNode);

                // Build SAP actions for Units
                BuildUnitActions(eventData, soapXml, actionItemNode);

                // Finalize SAP XML message
                FinalizeSapXmlMessage(soapXml, eventData.Data.CorrelationId, actionItemNode);

                _logger.LogInformation(EventIds.GenerationOfSapXmlPayloadCompleted.ToEventId(), "Generation of SAP XML payload completed.");

                return soapXml;
            }
            catch (Exception ex)
            {
                throw new ERPFacadeException(EventIds.GenerationOfSapXmlPayloadFailed.ToEventId(), "Error while building SAP XML payload. | {Exception}", ex.Message);
            }
        }

        private void BuildEncCellActions(EncEventPayload eventData, XmlDocument soapXml, XmlNode actionItemNode)
        {
            _logger.LogInformation(EventIds.EncCellSapActionGenerationStarted.ToEventId(), "Building ENC cell SAP actions.");

            foreach (var product in eventData.Data.Products)
            {
                foreach (var action in _sapActionConfig.Value.SapActions.Where(x => x.Product == EncCell))
                {
                    var unitOfSale = GetUnitOfSale(action.ActionNumber, eventData.Data.UnitsOfSales, product);

                    if (!ValidateActionRules(action, product))
                        continue;

                    switch (action.ActionNumber)
                    {
                        case 1://CREATE ENC CELL
                        case 10://CANCEL ENC CELL
                            if (unitOfSale is null)
                            {
                                throw new ERPFacadeException(EventIds.UnitOfSaleNotFoundException.ToEventId(), "Required unit not found in event payload to generate {ActionName} action for {Product}.", action.Action, product.ProductName);
                            }
                            BuildAndAppendActionNode(soapXml, product, unitOfSale, action, eventData, actionItemNode, product.ProductName);
                            break;

                        case 4://REPLACED WITH ENC CELL
                            if (product.ReplacedBy.Any() && unitOfSale is null)
                            {
                                throw new ERPFacadeException(EventIds.UnitOfSaleNotFoundException.ToEventId(), "Required unit not found in event payload to generate {ActionName} action for {Product}.", action.Action, product.ProductName);
                            }
                            foreach (var replacedProduct in product.ReplacedBy)
                            {
                                BuildAndAppendActionNode(soapXml, product, unitOfSale, action, eventData, actionItemNode, product.ProductName, replacedProduct);
                            }
                            break;

                        case 5://ADDITIONAL COVERAGE ENC CELL
                            foreach (var additionalCoverageProduct in product.AdditionalCoverage)
                            {
                                BuildAndAppendActionNode(soapXml, product, null, action, eventData, actionItemNode, product.ProductName, additionalCoverageProduct);
                            }
                            break;

                        case 6://CHANGE ENC CELL
                        case 8://UPDATE ENC CELL EDITION UPDATE NUMBER
                            if (unitOfSale is not null)
                                BuildAndAppendActionNode(soapXml, product, unitOfSale, action, eventData, actionItemNode, product.ProductName);
                            break;
                    }
                }
            }
        }

        private void BuildUnitActions(EncEventPayload eventData, XmlDocument soapXml, XmlNode actionItemNode)
        {
            foreach (var unitOfSale in eventData.Data.UnitsOfSales)
            {
                foreach (var action in _sapActionConfig.Value.SapActions.Where(x => x.Product == AvcsUnit))
                {
                    if (!ValidateActionRules(action, unitOfSale))
                        continue;

                    switch (action.ActionNumber)
                    {
                        case 2://CREATE AVCS UNIT OF SALE
                        case 7://CHANGE AVCS UNIT OF SALE
                        case 11://CANCEL AVCS UNIT OF SALE
                            BuildAndAppendActionNode(soapXml, null, unitOfSale, action, eventData, actionItemNode);
                            break;

                        case 3://ASSIGN CELL TO AVCS UNIT OF SALE
                            foreach (var addProduct in unitOfSale.CompositionChanges.AddProducts)
                            {
                                BuildAndAppendActionNode(soapXml, null, unitOfSale, action, eventData, actionItemNode, addProduct, null);
                            }
                            break;

                        case 9://REMOVE ENC CELL FROM AVCS UNIT OF SALE
                            foreach (var removeProduct in unitOfSale.CompositionChanges.RemoveProducts)
                            {
                                BuildAndAppendActionNode(soapXml, null, unitOfSale, action, eventData, actionItemNode, removeProduct, null);
                            }
                            break;
                    }
                }
            }
        }

        private void FinalizeSapXmlMessage(XmlDocument soapXml, string correlationId, XmlNode actionItemNode)
        {
            var xmlNode = SortXmlPayload(actionItemNode);

            SetXmlNodeValue(soapXml, XpathCorrId, correlationId);
            SetXmlNodeValue(soapXml, XpathNoOfActions, xmlNode.ChildNodes.Count.ToString());
            SetXmlNodeValue(soapXml, XpathRecDate, DateTime.UtcNow.ToString(RecDateFormat));
            SetXmlNodeValue(soapXml, XpathRecTime, DateTime.UtcNow.ToString(RecTimeFormat));

            var IM_MATINFONode = soapXml.SelectSingleNode(XpathImMatInfo);
            IM_MATINFONode.AppendChild(xmlNode);
        }

        /// <summary>
        /// Returns primary unit of sale for given product to get ProductName for ENC cell SAP actions.
        /// </summary>
        /// <param name="actionNumber"></param>
        /// <param name="listOfUnitOfSales"></param>
        /// <param name="product"></param>
        /// <returns></returns>
        private UnitOfSale? GetUnitOfSale(int actionNumber, List<UnitOfSale> listOfUnitOfSales, Product product)
        {
            return actionNumber switch
            {
                //Case 1 : CREATE ENC CELL
                1 => listOfUnitOfSales.FirstOrDefault(x => x.UnitOfSaleType == UnitSaleType &&
                                                           x.Status == UnitOfSaleStatusForSale &&
                                                           x.CompositionChanges.AddProducts.Contains(product.ProductName)),

                //Case 4 : REPLACED WITH ENC CELL 
                //Case 10 : CANCEL ENC CELL
                4 or 10 => listOfUnitOfSales.FirstOrDefault(x => x.UnitOfSaleType == UnitSaleType &&
                                                            x.CompositionChanges.RemoveProducts.Contains(product.ProductName)),

                //Case 6 : CHANGE ENC CELL
                //Case 8 : UPDATE ENC CELL EDITION UPDATE NUMBER
                6 or 8 => listOfUnitOfSales.FirstOrDefault(x => x.UnitOfSaleType == UnitSaleType &&
                                                                x.Status == UnitOfSaleStatusForSale &&
                                                                product.InUnitsOfSale.Contains(x.UnitName)),
                _ => null,
            };
        }

        /// <summary>
        /// Reurns true if given product/unit satisfies rules for given action.
        /// </summary>
        /// <param name="action"></param>
        /// <param name="obj"></param>
        /// <returns></returns>
        private bool ValidateActionRules(SapAction action, Object obj)
        {
            bool isConditionSatisfied = false;

            //Return true if no rules for SAP action.
            if (action.Rules is null) return true;

            foreach (var rules in action.Rules)
            {
                foreach (var conditions in rules.Conditions)
                {
                    object jsonFieldValue = CommonHelper.ParseXmlNode(conditions.AttributeName, obj, obj.GetType());

                    if (jsonFieldValue != null! && jsonFieldValue.ToString() == conditions.AttributeValue)
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

        private void BuildAndAppendActionNode(XmlDocument soapXml, Product product, UnitOfSale unitOfSale, SapAction action, EncEventPayload eventData, XmlNode actionItemNode, string childCell = null, string replacedBy = null)
        {
            _logger.LogInformation(EventIds.BuilingSapActionStarted.ToEventId(), "Building SAP action {ActionName}.", action.Action);
            var actionNode = BuildAction(soapXml, product, unitOfSale, action, eventData.Data.UkhoWeekNumber, childCell, replacedBy);
            actionItemNode.AppendChild(actionNode);
            _logger.LogInformation(EventIds.SapActionCreated.ToEventId(), "SAP action {ActionName} created.", action.Action);
        }

        private XmlElement BuildAction(XmlDocument soapXml, Product product, UnitOfSale unitOfSale, SapAction action, UkhoWeekNumber ukhoWeekNumber, string childCell, string replacedBy = null)
        {
            DecryptedPermit decryptedPermit = null;

            // Create main item node
            var itemNode = soapXml.CreateElement(nameof(Item));

            // Add basic action-related nodes
            AppendChildNode(itemNode, soapXml, ActionNumber, action.ActionNumber.ToString());
            AppendChildNode(itemNode, soapXml, Action, action.Action.ToString());
            AppendChildNode(itemNode, soapXml, Product, action.Product.ToString());
            AppendChildNode(itemNode, soapXml, ProdType, ProdTypeValue);

            // Add child cell node
            AppendChildNode(itemNode, soapXml, ChildCell, childCell);

            List<(int sortingOrder, XmlElement node)> actionAttributes = [];

            // Get permit keys for New cell and Updated cell
            if (product != null && !IsPropertyNullOrEmpty("permit", product.Permit) && (action.Action == CreateEncCell || action.Action == UpdateCell))
            {
                decryptedPermit = _permitDecryption.Decrypt(product.Permit);
            }

            // Process ProductSection attributes
            ProcessAttributes(action.Action, action.Attributes.Where(x => x.Section == ProductSection), soapXml, product, actionAttributes, decryptedPermit, replacedBy);

            // Process UnitOfSaleSection attributes
            ProcessAttributes(action.Action, action.Attributes.Where(x => x.Section == UnitOfSaleSection), soapXml, unitOfSale, actionAttributes, null, null);

            // Process UkhoWeekNumberSection attributes
            ProcessUkhoWeekNumberAttributes(action.Action, action.Attributes.Where(x => x.Section == UkhoWeekNumberSection), soapXml, ukhoWeekNumber, actionAttributes);

            // Sort and append attributes to SAP action
            foreach (var (sortingOrder, node) in actionAttributes.OrderBy(x => x.sortingOrder))
            {
                itemNode.AppendChild(node);
            }

            return itemNode;
        }

        private void AppendChildNode(XmlElement parentNode, XmlDocument doc, string nodeName, string value)
        {
            var childNode = doc.CreateElement(nodeName);
            childNode.InnerText = value ?? string.Empty;
            parentNode.AppendChild(childNode);
        }

        private void ProcessAttributes(string action, IEnumerable<ActionItemAttribute> attributes, XmlDocument soapXml, object source, List<(int, XmlElement)> actionAttributes, DecryptedPermit decryptedPermit = null, string replacedBy = null)
        {
            foreach (var attribute in attributes)
            {
                try
                {
                    var attributeNode = soapXml.CreateElement(attribute.XmlNodeName);

                    if (attribute.IsRequired)
                    {
                        if (attribute.XmlNodeName == ReplacedBy && !IsPropertyNullOrEmpty(attribute.JsonPropertyName, replacedBy))
                        {
                            attributeNode.InnerText = GetXmlNodeValue(replacedBy.ToString());
                        }
                        else if (attribute.XmlNodeName == ActiveKey)
                        {
                            attributeNode.InnerText = string.Empty;
                            if (decryptedPermit != null && !string.IsNullOrEmpty(decryptedPermit.ActiveKey)) attributeNode.InnerText = decryptedPermit.ActiveKey;
                        }
                        else if (attribute.XmlNodeName == NextKey)
                        {
                            attributeNode.InnerText = string.Empty;
                            if (decryptedPermit != null && !string.IsNullOrEmpty(decryptedPermit.NextKey)) attributeNode.InnerText = decryptedPermit.NextKey;
                        }
                        else
                        {
                            var jsonFieldValue = CommonHelper.ParseXmlNode(attribute.JsonPropertyName, source, source.GetType()).ToString();

                            if (!IsPropertyNullOrEmpty(attribute.JsonPropertyName, jsonFieldValue))
                            {
                                attributeNode.InnerText = GetXmlNodeValue(jsonFieldValue.ToString(), attribute.XmlNodeName);
                            }
                        }
                    }
                    else
                    {
                        attributeNode.InnerText = string.Empty;
                    }
                    actionAttributes.Add((attribute.SortingOrder, attributeNode));
                }
                catch (Exception ex)
                {
                    throw new ERPFacadeException(EventIds.BuildingSapActionInformationException.ToEventId(), "Error while generating SAP action information. | Action : {ActionName} | XML Attribute : {attribute.XmlNodeName} | Exception : {Exception}", action, attribute.XmlNodeName, ex.Message);
                }
            }
        }

        private void ProcessUkhoWeekNumberAttributes(string action, IEnumerable<ActionItemAttribute> attributes, XmlDocument soapXml, UkhoWeekNumber ukhoWeekNumber, List<(int, XmlElement)> actionAttributes)
        {
            if (ukhoWeekNumber == null)
            {
                throw new ERPFacadeException(EventIds.RequiredSectionNotFound.ToEventId(), "UkhoWeekNumber section not found in enccontentpublished event payload while creating {Action} action.", action);
            }

            foreach (var attribute in attributes)
            {
                try
                {
                    var attributeNode = soapXml.CreateElement(attribute.XmlNodeName);

                    if (attribute.IsRequired)
                    {
                        switch (attribute.XmlNodeName)
                        {
                            case ValidFrom:
                                var validFrom = _weekDetailsProvider.GetDateOfWeek(ukhoWeekNumber.Year, ukhoWeekNumber.Week, ukhoWeekNumber.CurrentWeekAlphaCorrection);
                                attributeNode.InnerText = GetXmlNodeValue(validFrom);
                                break;
                            case WeekNo:
                                var weekNo = string.Join("", ukhoWeekNumber.Year, ukhoWeekNumber.Week.ToString("D2"));
                                attributeNode.InnerText = GetXmlNodeValue(weekNo);
                                break;
                            case Correction:
                                attributeNode.InnerText = GetXmlNodeValue(ukhoWeekNumber.CurrentWeekAlphaCorrection ? IsCorrectionTrue : IsCorrectionFalse);
                                break;
                        }
                    }
                    actionAttributes.Add((attribute.SortingOrder, attributeNode));
                }
                catch (Exception ex)
                {
                    throw new ERPFacadeException(EventIds.BuildingSapActionInformationException.ToEventId(), "Error while generating SAP action information. | Action : {ActionName} | XML Attribute : {attribute.XmlNodeName} | Exception : {Exception}", action, attribute.XmlNodeName, ex.Message);
                }
            }
        }

        private void SetXmlNodeValue(XmlDocument xmlDoc, string xPath, string value)
        {
            var node = xmlDoc.SelectSingleNode(xPath);
            if (node != null)
            {
                node.InnerText = value;
            }
        }

        private string GetXmlNodeValue(string fieldValue, string xmlNodeName = null)
        {
            // Return empty if fieldValue is null, empty, or whitespace
            if (string.IsNullOrWhiteSpace(fieldValue))
                return string.Empty;

            // Define constants for substring lengths
            const int maxDefaultLength = 250;
            const int agencyCodeLength = 2;

            // Return first 2 characters if the node is Agency, else limit other nodes to 250 characters
            if (xmlNodeName == Agency)
                return CommonHelper.ToSubstring(fieldValue, 0, agencyCodeLength);

            return CommonHelper.ToSubstring(fieldValue, 0, maxDefaultLength);
        }

        private XmlNode SortXmlPayload(XmlNode actionItemNode)
        {
            // Extract all action item nodes
            var actionItems = actionItemNode.Cast<XmlNode>().ToList();
            int sequenceNumber = 1;

            // Sort based on the ActionNumber
            var sortedActionItems = actionItems
                .OrderBy(node => Convert.ToInt32(node.SelectSingleNode(ActionNumber)?.InnerText ?? "0"))
                .ToList();

            // Update the sequence number in the sorted list
            foreach (XmlNode actionItem in sortedActionItems)
            {
                var actionNumberNode = actionItem.SelectSingleNode(ActionNumber);
                if (actionNumberNode != null)
                {
                    actionNumberNode.InnerText = sequenceNumber.ToString();
                    sequenceNumber++;
                }
            }

            // Clear existing children and append sorted action items
            actionItemNode.RemoveAll();
            foreach (XmlNode actionItem in sortedActionItems)
            {
                actionItemNode.AppendChild(actionItem);
            }
            return actionItemNode;
        }

        private bool IsPropertyNullOrEmpty(string propertyName, string propertyValue)
        {
            if (string.IsNullOrEmpty(propertyValue))
            {
                throw new ERPFacadeException(EventIds.EmptyEventJsonPropertyException.ToEventId(), "SAP required property found empty in enccontentpublished event payload. | Property Name : {Property} ", propertyName);
            }
            else return false;
        }
    }
}
