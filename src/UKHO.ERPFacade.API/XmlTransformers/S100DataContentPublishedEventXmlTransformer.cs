using System.Xml;
using Microsoft.Extensions.Options;
using UKHO.ERPFacade.Common.Constants;
using UKHO.ERPFacade.Common.Exceptions;
using UKHO.ERPFacade.Common.Extensions;
using UKHO.ERPFacade.Common.Logging;
using UKHO.ERPFacade.Common.Models.CloudEvents.S100Event;
using UKHO.ERPFacade.Common.Models.SapActionConfigurationModels;
using UKHO.ERPFacade.Common.Operations;

namespace UKHO.ERPFacade.API.XmlTransformers
{
    public class S100DataContentPublishedEventXmlTransformer : BaseXmlTransformer
    {
        private readonly ILogger<S100DataContentPublishedEventXmlTransformer> _logger;
        private readonly IXmlOperations _xmlOperations;
        private readonly IOptions<S100DataContentPublishedEventSapActionConfiguration> _s100DataContentPublishedEventSapActionConfig;

        public S100DataContentPublishedEventXmlTransformer(ILogger<S100DataContentPublishedEventXmlTransformer> logger,
                                                           IXmlOperations xmlOperations,
                                                           IOptions<S100DataContentPublishedEventSapActionConfiguration> s100DataContentPublishedEventSapActionConfig)
        {
            _logger = logger;
            _xmlOperations = xmlOperations;
            _s100DataContentPublishedEventSapActionConfig = s100DataContentPublishedEventSapActionConfig;
        }

        /// <summary>
        /// Generate SAP message xml file.
        /// </summary>
        /// <param name="eventData"></param>        
        /// <returns>XmlDocument</returns>
        public override XmlDocument BuildXmlPayload<T>(T eventData, string xmlTemplatePath)
        {
            _logger.LogInformation(EventIds.S100EventSapXmlPayloadGenerationStarted.ToEventId(), "Generation of SAP xml payload for S-100 data content published event started.");

            var s100EventXmlPayload = _xmlOperations.CreateXmlDocument(Path.Combine(Environment.CurrentDirectory, xmlTemplatePath));

            if (eventData is S100EventData s100EventData)
            {
                var actionItemNode = s100EventXmlPayload.SelectSingleNode(XmlTemplateInfo.XpathActionItems);

                // Build SAP actions for Product
                BuildProductActions(s100EventData, s100EventXmlPayload, actionItemNode);

                // Build SAP actions for Unit Of Sale
                BuildUnitActions(s100EventData, s100EventXmlPayload, actionItemNode);

                // Finalize SAP XML message
                FinalizeSapXmlMessage(s100EventXmlPayload, s100EventData.CorrelationId, actionItemNode, XmlTemplateInfo.XpathImMatInfo);

                _logger.LogInformation(EventIds.S100EventSapXmlPayloadGenerationCompleted.ToEventId(), "Generation of SAP xml payload for S-100 data content published event completed.");
            }
            return s100EventXmlPayload;
        }

        private void BuildProductActions(S100EventData eventData, XmlDocument soapXml, XmlNode actionItemNode)
        {
            foreach (var product in eventData.Products)
            {
                foreach (var action in _s100DataContentPublishedEventSapActionConfig.Value.Actions.Where(x => x.Product == XmlFields.ShopCell))
                {
                    if (!ValidateActionRules(action, product))
                        continue;

                    switch (action.ActionNumber)
                    {
                        case 1://CREATE PRODUCT
                        case 10://CANCEL PRODUCT
                            BuildAndAppendActionNode(soapXml, product, null, action, actionItemNode, product.ProductName);
                            break;

                        case 4://REPLACED WITH PRODUCT
                            if (product.DataReplacement.Any())
                                foreach (var replacedProduct in product.DataReplacement)
                                {
                                    BuildAndAppendActionNode(soapXml, product, null, action, actionItemNode, product.ProductName, replacedProduct);
                                }
                            break;

                        case 6://CHANGE PRODUCT
                            if (product.InUnitsOfSale.Any())
                                BuildAndAppendActionNode(soapXml, product, null, action, actionItemNode, product.ProductName);
                            break;
                    }
                }
            }
        }

        private void BuildUnitActions(S100EventData eventData, XmlDocument soapXml, XmlNode actionItemNode)
        {
            foreach (var unitOfSale in eventData.UnitsOfSales)
            {
                foreach (var action in _s100DataContentPublishedEventSapActionConfig.Value.Actions.Where(x => x.Product == XmlFields.ShopUnit))
                {
                    if (!ValidateActionRules(action, unitOfSale))
                        continue;

                    switch (action.ActionNumber)
                    {
                        case 2://CREATE UNIT OF SALE
                        case 7://CHANGE UNIT OF SALE
                        case 11://CANCEL UNIT OF SALE
                            BuildAndAppendActionNode(soapXml, null, unitOfSale, action, actionItemNode);
                            break;

                        case 3://ASSIGN PRODUCT TO UNIT OF SALE
                            foreach (var addProduct in unitOfSale.CompositionChanges.AddProducts)
                            {
                                BuildAndAppendActionNode(soapXml, null, unitOfSale, action, actionItemNode, addProduct);
                            }
                            break;

                        case 9://REMOVE PRODUCT FROM UNIT OF SALE
                            foreach (var removeProduct in unitOfSale.CompositionChanges.RemoveProducts)
                            {
                                BuildAndAppendActionNode(soapXml, null, unitOfSale, action, actionItemNode, removeProduct);
                            }
                            break;
                    }
                }
            }
        }

        private void BuildAndAppendActionNode(XmlDocument soapXml, S100Product product, S100UnitOfSale unitOfSale, Actions action, XmlNode actionItemNode, string childCell = null, string replacedBy = null)
        {
            _logger.LogInformation(EventIds.S100SapActionGenerationStarted.ToEventId(), "Generation of {ActionName} action started.", action.ActionName);
            var actionNode = BuildAction(soapXml, product, unitOfSale, action, childCell, replacedBy);
            actionItemNode.AppendChild(actionNode);
            _logger.LogInformation(EventIds.S100SapActionGenerationCompleted.ToEventId(), "Generation of {ActionName} action completed.", action.ActionName);
        }

        private XmlElement BuildAction(XmlDocument soapXml, S100Product product, S100UnitOfSale unitOfSale, Actions action, string childCell, string replacedBy = null)
        {
            // Create main item node
            var itemNode = soapXml.CreateElement(XmlTemplateInfo.Item);

            // Add basic action-related nodes
            _xmlOperations.AppendChildNode(itemNode, soapXml, XmlFields.ActionNumber, action.ActionNumber.ToString());
            _xmlOperations.AppendChildNode(itemNode, soapXml, XmlFields.Action, action.ActionName.ToString());
            _xmlOperations.AppendChildNode(itemNode, soapXml, XmlFields.Product, action.Product.ToString());

            List<(int sortingOrder, XmlElement node)> actionAttributes = new();

            // Process ProductSection attributes
            ProcessAttributes(action.ActionName, action.Attributes.Where(x => x.Section == ConfigFileFields.ProductSection), soapXml, product, actionAttributes, childCell, replacedBy);

            // Process UnitOfSaleSection attributes
            ProcessAttributes(action.ActionName, action.Attributes.Where(x => x.Section == ConfigFileFields.UnitOfSaleSection), soapXml, unitOfSale, actionAttributes, childCell, null);

            // Sort and append attributes to SAP action
            foreach (var (sortingOrder, node) in actionAttributes.OrderBy(x => x.sortingOrder))
            {
                itemNode.AppendChild(node);
            }

            return itemNode;
        }

        private void ProcessAttributes(string action, IEnumerable<Attributes> attributes, XmlDocument soapXml, object source, List<(int, XmlElement)> actionAttributes, string childCell, string replacedBy = null)
        {
            foreach (var attribute in attributes)
            {
                try
                {
                    var attributeNode = soapXml.CreateElement(attribute.XmlNodeName);

                    if (attribute.IsRequired)
                    {
                        if (attribute.XmlNodeName == XmlFields.ReplacedBy)
                        {
                            attributeNode.InnerText = StringExtension.ToSubstring(replacedBy.ToString(), 0, XmlFields.MaxXmlNodeLength);
                        }
                        else if (attribute.XmlNodeName == XmlFields.ChildCell)
                        {
                            attributeNode.InnerText = childCell ?? string.Empty;
                        }
                        else
                        {
                            var jsonFieldValue = Extractor.ExtractJsonAttributeValue(attribute.JsonPropertyName, source, source.GetType()).ToString();
                            attributeNode.InnerText = StringExtension.ToSubstring(jsonFieldValue, 0, XmlFields.MaxXmlNodeLength);
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
                    throw new ERPFacadeException(EventIds.S100SapActionInformationGenerationFailedException.ToEventId(), $"Error while generating SAP action information. | Action : {action} | XML Attribute : {attribute.XmlNodeName} | ErrorMessage : {ex.Message}");
                }
            }

        }
    }
}
