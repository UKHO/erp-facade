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
                BuildProductActions(s100EventData, s100EventXmlPayload, actionItemNode);

                // Build SAP actions for Unit Of Sale


                // Finalize SAP XML message
                FinalizeSapXmlMessage(s100EventXmlPayload, s100EventData.CorrelationId, actionItemNode);

                _logger.LogInformation(EventIds.S100EventSapXmlPayloadGenerationCompleted.ToEventId(), "Generation of SAP xml payload for S100 data content published event completed.");
            }
            return s100EventXmlPayload;
        }

        private void BuildProductActions(S100EventData eventData, XmlDocument soapXml, XmlNode actionItemNode)
        {
            _logger.LogInformation(EventIds.ProductSapActionGenerationStarted.ToEventId(), "Product SapAction Generation Started.");
            foreach (var product in eventData.Products)
            {
                foreach (var action in _sapActionConfig.Value.SapActions.Where(x => x.Product == XmlFields.Product))
                {
                    var unitOfSale = GetUnitOfSale(action.ActionNumber, eventData.UnitsOfSales, product);

                    if (!ValidateActionRules(action, product))
                        continue;

                    switch (action.ActionNumber)
                    {
                        case 1://CREATE PRODUCT
                        case 10://CANCEL PRODUCT
                            if (unitOfSale is null)
                            {
                                throw new ERPFacadeException(EventIds.RequiredUnitNotFoundException.ToEventId(), $"Required unit not found in S100 data content published event for {product.ProductName} to generate {action.Action} action.");
                            }
                            BuildAndAppendActionNode(soapXml, product, unitOfSale, action, actionItemNode, product.ProductName);
                            break;

                        case 4://REPLACED WITH PRODUCT
                            if (product.DataReplacement.Any() && unitOfSale is null)
                            {
                                throw new ERPFacadeException(EventIds.RequiredUnitNotFoundException.ToEventId(), $"Required unit not found in S100 data content published event for {product.ProductName} to generate {action.Action} action.");
                            }
                            foreach (var replacedProduct in product.DataReplacement)
                            {
                                BuildAndAppendActionNode(soapXml, product, unitOfSale, action, actionItemNode, product.ProductName, replacedProduct);
                            }
                            break;

                        case 7://CHANGE PRODUCT
                            if (unitOfSale is not null)
                                BuildAndAppendActionNode(soapXml, product, unitOfSale, action, actionItemNode, product.ProductName);
                            break;
                    }
                }
            }
            _logger.LogInformation(EventIds.ProductSapActionGenerationCompleted.ToEventId(), "Product SapAction Generation Completed.");
        }

        private S100UnitOfSale? GetUnitOfSale(int actionNumber, List<S100UnitOfSale> listOfUnitOfSales, S100Product product)
        {
            return actionNumber switch
            {
                //Case 1 : CREATE PRODUCT
                1 => listOfUnitOfSales.FirstOrDefault(x => x.Status == JsonFields.UnitOfSaleStatusForSale && x.CompositionChanges.AddProducts.Contains(product.ProductName)),

                //Case 4 : REPLACED WITH PRODUCT
                //Case 10 : CANCEL PRODUCT
                4 or 10 => listOfUnitOfSales.FirstOrDefault(x => x.CompositionChanges.RemoveProducts.Contains(product.ProductName)),

                //Case 7 : CHANGE PRODUCT
                7 => listOfUnitOfSales.FirstOrDefault(x => x.Status == JsonFields.UnitOfSaleStatusForSale && product.InUnitsOfSale.Contains(x.UnitName)),
                _ => null,
            };
        }

        private void BuildAndAppendActionNode(XmlDocument soapXml, S100Product product, S100UnitOfSale unitOfSale, SapAction action, XmlNode actionItemNode, string childCell = null, string replacedBy = null)
        {
            _logger.LogInformation(EventIds.S100SapActionGenerationStarted.ToEventId(), "Generation of {ActionName} action started.", action.Action);
            var actionNode = BuildAction(soapXml, product, unitOfSale, action, childCell, replacedBy);
            actionItemNode.AppendChild(actionNode);
            _logger.LogInformation(EventIds.S100SapActionGenerationCompleted.ToEventId(), "Generation of {ActionName} action completed.", action.Action);
        }

        private XmlElement BuildAction(XmlDocument soapXml, S100Product product, S100UnitOfSale unitOfSale, SapAction action, string childCell, string replacedBy = null)
        {
            // Create main item node
            var itemNode = soapXml.CreateElement(XmlTemplateInfo.Item);

            // Add basic action-related nodes
            _xmlOperations.AppendChildNode(itemNode, soapXml, XmlFields.ActionNumber, action.ActionNumber.ToString());
            _xmlOperations.AppendChildNode(itemNode, soapXml, XmlFields.Action, action.Action.ToString());
            _xmlOperations.AppendChildNode(itemNode, soapXml, XmlFields.Product, action.Product.ToString());

            // Add child cell node
            _xmlOperations.AppendChildNode(itemNode, soapXml, XmlFields.ChildCell, childCell);

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
