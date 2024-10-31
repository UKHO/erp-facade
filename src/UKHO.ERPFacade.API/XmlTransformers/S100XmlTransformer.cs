using System.Xml;
using Microsoft.Extensions.Options;
using UKHO.ERPFacade.Common.Constants;
using UKHO.ERPFacade.Common.Exceptions;
using UKHO.ERPFacade.Common.Extensions;
using UKHO.ERPFacade.Common.Logging;
using UKHO.ERPFacade.Common.Models;
using UKHO.ERPFacade.Common.Models.CloudEvents.S100Event;
using UKHO.ERPFacade.Common.Operations;

namespace UKHO.ERPFacade.API.XmlTransformers
{
    public class S100XmlTransformer : BaseXmlTransformer
    {
        private readonly ILogger<S100XmlTransformer> _logger;
        private readonly IXmlOperations _xmlOperations;
        private readonly IOptions<S100SapActionConfiguration> _sapActionConfig;
        public S100XmlTransformer(ILogger<S100XmlTransformer> logger,
                                  IXmlOperations xmlOperations,
                                  IOptions<S100SapActionConfiguration> sapActionConfig)
        {
            _logger = logger;
            _xmlOperations = xmlOperations;
            _sapActionConfig = sapActionConfig;
        }

        /// <summary>
        /// Generate SAP message xml file.
        /// </summary>
        /// <param name="eventData"></param>        
        /// <returns>XmlDocument</returns>
        public override XmlDocument BuildXmlPayload<T>(T eventData, string xmlTemplatePath)
        {
            _logger.LogInformation(EventIds.S100EventSapXmlPayloadGenerationStarted.ToEventId(), "Generation of SAP xml payload for S100 data content published event started.");

            var s100EventXmlPayload = _xmlOperations.CreateXmlDocument(Path.Combine(Environment.CurrentDirectory, xmlTemplatePath));

            if (eventData is S100EventData s100EventData)
            {
                var actionItemNode = s100EventXmlPayload.SelectSingleNode(XmlTemplateInfo.XpathActionItems);

                // Build SAP actions for Product

                // Build SAP actions for Unit Of Sale
                BuildUnitActions(s100EventData, s100EventXmlPayload, actionItemNode);

                // Finalize SAP XML message
                FinalizeSapXmlMessage(s100EventXmlPayload, s100EventData.CorrelationId, actionItemNode);

                _logger.LogInformation(EventIds.S100EventSapXmlPayloadGenerationCompleted.ToEventId(), "Generation of SAP xml payload for S100 data content published event completed.");
            }
            return s100EventXmlPayload;
        }

        private void BuildUnitActions(S100EventData eventData, XmlDocument soapXml, XmlNode actionItemNode)
        {
            foreach (var unitOfSale in eventData.UnitsOfSales)
            {
                foreach (var action in _sapActionConfig.Value.SapActions.Where(x => x.Product == XmlFields.S100UnitOfSale))
                {
                    if (!ValidateActionRules(action, unitOfSale))
                        continue;

                    switch (action.ActionNumber)
                    {
                        case 2://CREATE UNIT OF SALE
                        case 6://CHANGE UNIT OF SALE
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

        private void BuildAndAppendActionNode(XmlDocument soapXml, S100Product product, S100UnitOfSale unitOfSale, SapAction action, XmlNode actionItemNode, string childProduct = null, string replacedBy = null)
        {
            _logger.LogInformation(EventIds.S100SapActionGenerationStarted.ToEventId(), "Generation of {ActionName} action started.", action.Action);
            var actionNode = BuildAction(soapXml, product, unitOfSale, action, childProduct, replacedBy);
            actionItemNode.AppendChild(actionNode);
            _logger.LogInformation(EventIds.S100SapActionGenerationCompleted.ToEventId(), "Generation of {ActionName} action completed", action.Action);
        }

        private XmlElement BuildAction(XmlDocument soapXml, S100Product product, S100UnitOfSale unitOfSale, SapAction action, string childProduct = null, string replacedBy = null)
        {
            // Create main item node
            var itemNode = soapXml.CreateElement(XmlTemplateInfo.Item);

            // Add basic action-related nodes
            _xmlOperations.AppendChildNode(itemNode, soapXml, XmlFields.ActionNumber, action.ActionNumber.ToString());
            _xmlOperations.AppendChildNode(itemNode, soapXml, XmlFields.Action, action.Action.ToString());
            _xmlOperations.AppendChildNode(itemNode, soapXml, XmlFields.Product, action.Product.ToString());

            // Add child cell node
            _xmlOperations.AppendChildNode(itemNode, soapXml, XmlFields.ChildCell, childProduct);

            List<(int sortingOrder, XmlElement node)> actionAttributes = new();

            // Process ProductSection attributes
            ProcessAttributes(action.Action, action.Attributes.Where(x => x.Section == ConfigFileFields.ProductSection), soapXml, product, actionAttributes, replacedBy);

            // Process UnitOfSaleSection attributes
            ProcessAttributes(action.Action, action.Attributes.Where(x => x.Section == ConfigFileFields.UnitOfSaleSection), soapXml, unitOfSale, actionAttributes, null);

            // Sort and append attributes to SAP action
            foreach (var (sortingOrder, node) in actionAttributes.OrderBy(x => x.sortingOrder))
            {
                itemNode.AppendChild(node);
            }

            return itemNode;
        }

        private void ProcessAttributes(string action, IEnumerable<ActionItemAttribute> attributes, XmlDocument soapXml, object source, List<(int, XmlElement)> actionAttributes, string replacedBy = null)
        {
            foreach (var attribute in attributes)
            {
                try
                {
                    var attributeNode = soapXml.CreateElement(attribute.XmlNodeName);

                    if (attribute.IsRequired)
                    {
                        if (attribute.XmlNodeName == XmlFields.ReplacedBy && !IsPropertyNullOrEmpty(attribute.JsonPropertyName, replacedBy))
                        {
                            attributeNode.InnerText = StringExtension.ToSubstring(replacedBy.ToString(), 0, XmlFields.MaxXmlNodeLength);
                        }
                        else
                        {
                            var jsonFieldValue = Extractor.ExtractJsonAttributeValue(attribute.JsonPropertyName, source, source.GetType()).ToString();
                            if (!IsPropertyNullOrEmpty(attribute.JsonPropertyName, jsonFieldValue))
                            {
                                attributeNode.InnerText = StringExtension.ToSubstring(jsonFieldValue, 0, XmlFields.MaxXmlNodeLength);
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
                    throw new ERPFacadeException(EventIds.S100SapActionInformationGenerationFailedException.ToEventId(), $"Error while generating SAP action information. | Action : {action} | XML Attribute : {attribute.XmlNodeName} | ErrorMessage : {ex.Message}");
                }
            }
        }
    }
}
