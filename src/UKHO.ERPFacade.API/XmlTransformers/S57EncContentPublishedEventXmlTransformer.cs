using System;
using System.Xml;
using Microsoft.Extensions.Options;
using UKHO.ERPFacade.Common.Constants;
using UKHO.ERPFacade.Common.Exceptions;
using UKHO.ERPFacade.Common.Extensions;
using UKHO.ERPFacade.Common.Logging;
using UKHO.ERPFacade.Common.Models;
using UKHO.ERPFacade.Common.Models.CloudEvents.S57Event;
using UKHO.ERPFacade.Common.Models.SapActionConfigurationModels;
using UKHO.ERPFacade.Common.Operations;
using UKHO.ERPFacade.Common.PermitDecryption;
using UKHO.ERPFacade.Common.Providers;

namespace UKHO.ERPFacade.API.XmlTransformers
{
    public class S57EncContentPublishedEventXmlTransformer : BaseXmlTransformer
    {
        private readonly ILogger<S57EncContentPublishedEventXmlTransformer> _logger;
        private readonly IXmlOperations _xmlOperations;
        private readonly IWeekDetailsProvider _weekDetailsProvider;
        private readonly IPermitDecryption _permitDecryption;
        private readonly IOptions<S57EncContentPublishedEventSapActionConfiguration> _s57EncContentPublishedEventSapActionConfig;

        public S57EncContentPublishedEventXmlTransformer(ILogger<S57EncContentPublishedEventXmlTransformer> logger,
                                                         IXmlOperations xmlOperations,
                                                         IWeekDetailsProvider weekDetailsProvider,
                                                         IPermitDecryption permitDecryption,
                                                         IOptions<S57EncContentPublishedEventSapActionConfiguration> s57EncContentPublishedEventSapActionConfig)
        : base()
        {
            _logger = logger;
            _xmlOperations = xmlOperations;
            _weekDetailsProvider = weekDetailsProvider;
            _permitDecryption = permitDecryption;
            _s57EncContentPublishedEventSapActionConfig = s57EncContentPublishedEventSapActionConfig;
        }

        public override XmlDocument BuildXmlPayload<T>(T eventData, string xmlTemplatePath)
        {
            _logger.LogInformation(EventIds.S57EventSapXmlPayloadGenerationStarted.ToEventId(), "Generation of SAP xml payload for S57 enccontentpublished event started.");

            var s57EventXmlPayload = _xmlOperations.CreateXmlDocument(Path.Combine(Environment.CurrentDirectory, xmlTemplatePath));

            if (eventData is S57EventData s57EventData)
            {
                var actionItemNode = s57EventXmlPayload.SelectSingleNode(XmlTemplateInfo.XpathActionItems);

                // Build SAP actions for ENC Cell
                BuildEncCellActions(s57EventData, s57EventXmlPayload, actionItemNode);

                // Build SAP actions for Units
                BuildUnitActions(s57EventData, s57EventXmlPayload, actionItemNode);

                // Finalize SAP XML message
                FinalizeSapXmlMessage(s57EventXmlPayload, s57EventData.CorrelationId, actionItemNode, XmlTemplateInfo.XpathImMatInfo);

                _logger.LogInformation(EventIds.S57EventSapXmlPayloadGenerationCompleted.ToEventId(), "Generation of SAP xml payload for S57 enccontentpublished event completed.");
            }
            return s57EventXmlPayload;
        }

        private void BuildEncCellActions(S57EventData eventData, XmlDocument soapXml, XmlNode actionItemNode)
        {
            foreach (var product in eventData.Products)
            {
                foreach (var action in _s57EncContentPublishedEventSapActionConfig.Value.Actions.Where(x => x.Product == XmlFields.EncCell))
                {
                    var unitOfSale = GetUnitOfSale(action.ActionNumber, eventData.UnitsOfSales, product);

                    if (!ValidateActionRules(action, product))
                        continue;

                    switch (action.ActionNumber)
                    {
                        case 1://CREATE ENC CELL
                        case 10://CANCEL ENC CELL
                            BuildAndAppendActionNode(soapXml, product, unitOfSale, action, eventData, actionItemNode, product.ProductName);
                            break;

                        case 4://REPLACED WITH ENC CELL
                            if (product.ReplacedBy.Any())
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
                            if (product.InUnitsOfSale.Any())
                                BuildAndAppendActionNode(soapXml, product, unitOfSale, action, eventData, actionItemNode, product.ProductName);
                            break;
                    }
                }
            }
        }

        private void BuildUnitActions(S57EventData eventData, XmlDocument soapXml, XmlNode actionItemNode)
        {
            foreach (var unitOfSale in eventData.UnitsOfSales)
            {
                foreach (var action in _s57EncContentPublishedEventSapActionConfig.Value.Actions.Where(x => x.Product == XmlFields.AvcsUnit))
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

        private S57UnitOfSale? GetUnitOfSale(int actionNumber, List<S57UnitOfSale> listOfUnitOfSales, S57Product product)
        {
            return actionNumber switch
            {
                //Case 1 : CREATE ENC CELL
                1 => listOfUnitOfSales.FirstOrDefault(x => x.UnitOfSaleType == JsonFields.UnitSaleType &&
                                                           x.Status == JsonFields.UnitOfSaleStatusForSale &&
                                                           x.CompositionChanges.AddProducts.Contains(product.ProductName)),

                //Case 4 : REPLACED WITH ENC CELL 
                //Case 10 : CANCEL ENC CELL
                4 or 10 => listOfUnitOfSales.FirstOrDefault(x => x.UnitOfSaleType == JsonFields.UnitSaleType &&
                                                            x.CompositionChanges.RemoveProducts.Contains(product.ProductName)),

                //Case 6 : CHANGE ENC CELL
                //Case 8 : UPDATE ENC CELL EDITION UPDATE NUMBER
                6 or 8 => listOfUnitOfSales.FirstOrDefault(x => x.UnitOfSaleType == JsonFields.UnitSaleType &&
                                                                x.Status == JsonFields.UnitOfSaleStatusForSale &&
                                                                product.InUnitsOfSale.Contains(x.UnitName)),
                _ => null
            };
        }

        private void BuildAndAppendActionNode(XmlDocument soapXml, S57Product product, S57UnitOfSale unitOfSale, Actions action, S57EventData eventData, XmlNode actionItemNode, string childCell = null, string replacedBy = null)
        {
            _logger.LogInformation(EventIds.S57SapActionGenerationStarted.ToEventId(), "Generation of {ActionName} action started.", action.ActionName);
            var actionNode = BuildAction(soapXml, product, unitOfSale, action, eventData.UkhoWeekNumber, childCell, replacedBy);
            actionItemNode.AppendChild(actionNode);
            _logger.LogInformation(EventIds.S57SapActionGenerationCompleted.ToEventId(), "Generation of {ActionName} action completed", action.ActionName);
        }

        private XmlElement BuildAction(XmlDocument soapXml, S57Product product, S57UnitOfSale unitOfSale, Actions action, S57UkhoWeekNumber ukhoWeekNumber, string childCell, string replacedBy = null)
        {
            DecryptedPermit decryptedPermit = null;

            // Create main item node
            var itemNode = soapXml.CreateElement(XmlTemplateInfo.Item);

            // Add basic action-related nodes
            _xmlOperations.AppendChildNode(itemNode, soapXml, XmlFields.ActionNumber, action.ActionNumber.ToString());
            _xmlOperations.AppendChildNode(itemNode, soapXml, XmlFields.Action, action.ActionName.ToString());
            _xmlOperations.AppendChildNode(itemNode, soapXml, XmlFields.Product, action.Product.ToString());
            _xmlOperations.AppendChildNode(itemNode, soapXml, XmlFields.ProdType, XmlFields.ProdTypeValue);

            // Add child cell node
            _xmlOperations.AppendChildNode(itemNode, soapXml, XmlFields.ChildCell, childCell);

            List<(int sortingOrder, XmlElement node)> actionAttributes = new();

            // Get permit keys for New cell and Updated cell
            if (action.ActionName == ConfigFileFields.CreateEncCell || action.ActionName == ConfigFileFields.UpdateCell)
            {
                decryptedPermit = _permitDecryption.Decrypt(product.Permit);
            }

            // Process ProductSection attributes
            ProcessAttributes(action.ActionName, action.Attributes.Where(x => x.Section == ConfigFileFields.ProductSection), soapXml, product, actionAttributes, decryptedPermit, replacedBy);

            // Process UnitOfSaleSection attributes
            ProcessAttributes(action.ActionName, action.Attributes.Where(x => x.Section == ConfigFileFields.UnitOfSaleSection), soapXml, unitOfSale, actionAttributes, null);

            // Process UkhoWeekNumberSection attributes
            ProcessUkhoWeekNumberAttributes(action.ActionName, action.Attributes.Where(x => x.Section == ConfigFileFields.UkhoWeekNumberSection), soapXml, ukhoWeekNumber, actionAttributes);

            // Sort and append attributes to SAP action
            foreach (var (sortingOrder, node) in actionAttributes.OrderBy(x => x.sortingOrder))
            {
                itemNode.AppendChild(node);
            }

            return itemNode;
        }

        private void ProcessAttributes(string action, IEnumerable<Attributes> attributes, XmlDocument soapXml, object source, List<(int, XmlElement)> actionAttributes, DecryptedPermit decryptedPermit = null, string replacedBy = null)
        {
            foreach (var attribute in attributes)
            {
                try
                {
                    var attributeNode = soapXml.CreateElement(attribute.XmlNodeName);

                    if (attribute.IsRequired)
                    {
                        switch (attribute.XmlNodeName)
                        {
                            case XmlFields.ReplacedBy:
                                attributeNode.InnerText = StringExtension.ToSubstring(replacedBy.ToString(), 0, XmlFields.MaxXmlNodeLength);
                                break;
                            case XmlFields.ActiveKey:
                                attributeNode.InnerText = StringExtension.ToSubstring(decryptedPermit.ActiveKey, 0, XmlFields.MaxXmlNodeLength);
                                break;
                            case XmlFields.NextKey:
                                attributeNode.InnerText = StringExtension.ToSubstring(decryptedPermit.NextKey, 0, XmlFields.MaxXmlNodeLength);
                                break;
                            default:
                                var jsonAttributeValue = Extractor.ExtractJsonAttributeValue(attribute.JsonPropertyName, source, source.GetType()).ToString();
                                // Set value as first 2 characters if the node is Agency, else limit other nodes to 250 characters
                                attributeNode.InnerText = attribute.XmlNodeName == XmlFields.Agency ? StringExtension.ToSubstring(jsonAttributeValue, 0, XmlFields.MaxAgencyXmlNodeLength) : StringExtension.ToSubstring(jsonAttributeValue, 0, XmlFields.MaxXmlNodeLength);
                                break;
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
                    throw new ERPFacadeException(EventIds.S57SapActionInformationGenerationFailedException.ToEventId(), $"Error while generating SAP action information. | Action : {action} | XML Attribute : {attribute.XmlNodeName} | ErrorMessage : {ex.Message}");
                }
            }

        }

        private void ProcessUkhoWeekNumberAttributes(string action, IEnumerable<Attributes> attributes, XmlDocument soapXml, S57UkhoWeekNumber ukhoWeekNumber, List<(int, XmlElement)> actionAttributes)
        {
            foreach (var attribute in attributes)
            {
                try
                {
                    var attributeNode = soapXml.CreateElement(attribute.XmlNodeName);

                    if (attribute.IsRequired)
                    {
                        switch (attribute.XmlNodeName)
                        {
                            case XmlFields.ValidFrom:
                                var validFrom = _weekDetailsProvider.GetDateOfWeek(ukhoWeekNumber.Year.Value, ukhoWeekNumber.Week.Value, ukhoWeekNumber.CurrentWeekAlphaCorrection.Value);
                                attributeNode.InnerText = StringExtension.ToSubstring(validFrom, 0, XmlFields.MaxXmlNodeLength);
                                break;
                            case XmlFields.WeekNo:
                                var weekNo = string.Join(XmlFields.UkhoWeekNoFormatSeparator, ukhoWeekNumber.Year, ukhoWeekNumber.Week.Value.ToString(XmlFields.UkhoWeekNoFormat));
                                attributeNode.InnerText = StringExtension.ToSubstring(weekNo, 0, XmlFields.MaxXmlNodeLength);
                                break;
                            case XmlFields.Correction:
                                attributeNode.InnerText = ukhoWeekNumber.CurrentWeekAlphaCorrection.Value ? XmlFields.IsCorrectionTrue : XmlFields.IsCorrectionFalse;
                                break;
                        }
                    }
                    actionAttributes.Add((attribute.SortingOrder, attributeNode));
                }
                catch (Exception ex)
                {
                    throw new ERPFacadeException(EventIds.S57SapActionInformationGenerationFailedException.ToEventId(), $"Error while generating SAP action information. | Action : {action} | XML Attribute : {attribute.XmlNodeName} | ErrorMessage : {ex.Message}");
                }
            }
        }
    }
}
