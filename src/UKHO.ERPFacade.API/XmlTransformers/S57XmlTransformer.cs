using System.Xml;
using Microsoft.Extensions.Options;
using UKHO.ERPFacade.API.XmlTransformers;
using UKHO.ERPFacade.Common.Constants;
using UKHO.ERPFacade.Common.Exceptions;
using UKHO.ERPFacade.Common.IO;
using UKHO.ERPFacade.Common.Logging;
using UKHO.ERPFacade.Common.Models;
using UKHO.ERPFacade.Common.Models.CloudEvents.S57;
using UKHO.ERPFacade.Common.PermitDecryption;
using UKHO.ERPFacade.Common.Providers;
using Product = UKHO.ERPFacade.Common.Models.CloudEvents.S57.Product;
using UnitOfSale = UKHO.ERPFacade.Common.Models.CloudEvents.S57.UnitOfSale;

namespace UKHO.ERPFacade.API.Helpers
{
    public class S57XmlTransformer : BaseXmlTransformer
    {
        private readonly ILogger<S57XmlTransformer> _logger;
        private readonly IXmlHelper _xmlHelper;
        private readonly IFileSystemHelper _fileSystemHelper;
        private readonly IOptions<SapActionConfiguration> _sapActionConfig;
        private readonly IWeekDetailsProvider _weekDetailsProvider;
        private readonly IPermitDecryption _permitDecryption;

        public S57XmlTransformer(ILogger<S57XmlTransformer> logger,
                                 IXmlHelper xmlHelper,
                                 IFileSystemHelper fileSystemHelper,
                                 IOptions<SapActionConfiguration> sapActionConfig,
                                 IWeekDetailsProvider weekDetailsProvider,
                                 IPermitDecryption permitDecryption
                                 )
        : base(fileSystemHelper, xmlHelper)
        {
            _logger = logger;
            _xmlHelper = xmlHelper;
            _fileSystemHelper = fileSystemHelper;
            _sapActionConfig = sapActionConfig;
            _weekDetailsProvider = weekDetailsProvider;
            _permitDecryption = permitDecryption;
        }

        public override XmlDocument BuildXmlPayload<T>(T eventData, string xmlTemplatePath)
        {
            var s57EventData = eventData as S57EventData;

            var soapXml = _xmlHelper.CreateXmlDocument(Path.Combine(Environment.CurrentDirectory, xmlTemplatePath));

            var actionItemNode = soapXml.SelectSingleNode(Constants.XpathActionItems);

            _logger.LogInformation(EventIds.GenerationOfSapXmlPayloadStarted.ToEventId(), "Generation of SAP XML payload started.");

            // Build SAP actions for ENC Cell
            BuildEncCellActions(s57EventData, soapXml, actionItemNode);

            // Build SAP actions for Units
            BuildUnitActions(s57EventData, soapXml, actionItemNode);

            // Finalize SAP XML message
            FinalizeSapXmlMessage(soapXml, s57EventData.CorrelationId, actionItemNode);

            _logger.LogInformation(EventIds.GenerationOfSapXmlPayloadCompleted.ToEventId(), "Generation of SAP XML payload completed.");

            return soapXml;
        }

        private void BuildEncCellActions(S57EventData eventData, XmlDocument soapXml, XmlNode actionItemNode)
        {
            _logger.LogInformation(EventIds.EncCellSapActionGenerationStarted.ToEventId(), "Building ENC cell SAP actions.");

            foreach (var product in eventData.Products)
            {
                foreach (var action in _sapActionConfig.Value.SapActions.Where(x => x.Product == Constants.EncCell))
                {
                    var unitOfSale = GetUnitOfSale(action.ActionNumber, eventData.UnitsOfSales, product);

                    if (!ValidateActionRules(action, product))
                        continue;

                    switch (action.ActionNumber)
                    {
                        case 1://CREATE ENC CELL
                        case 10://CANCEL ENC CELL
                            if (unitOfSale is null)
                            {
                                throw new ERPFacadeException(EventIds.UnitOfSaleNotFoundException.ToEventId(), $"Required unit not found in event payload to generate {action.Action} action for {product.ProductName}.");
                            }
                            BuildAndAppendActionNode(soapXml, product, unitOfSale, action, eventData, actionItemNode, product.ProductName);
                            break;

                        case 4://REPLACED WITH ENC CELL
                            if (product.ReplacedBy.Any() && unitOfSale is null)
                            {
                                throw new ERPFacadeException(EventIds.UnitOfSaleNotFoundException.ToEventId(), $"Required unit not found in event payload to generate {action.Action} action for {product.ProductName}.");
                            }
                            foreach (var replacedProduct in product.ReplacedBy)
                            {
                                BuildAndAppendActionNode(soapXml, product, unitOfSale, action, eventData, actionItemNode, product.ProductName, replacedProduct);
                            }
                            break;

                        case 5://ADDITIONAL COVERAGE ENC CELL
                            foreach (var additionalCoverageProduct in product.AdditionalCoverage)
                            {
                                product.AdditionalCoverage.Add(additionalCoverageProduct);

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

        private void BuildUnitActions(S57EventData eventData, XmlDocument soapXml, XmlNode actionItemNode)
        {
            foreach (var unitOfSale in eventData.UnitsOfSales)
            {
                foreach (var action in _sapActionConfig.Value.SapActions.Where(x => x.Product == Constants.AvcsUnit))
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

        private UnitOfSale? GetUnitOfSale(int actionNumber, List<Common.Models.CloudEvents.S57.UnitOfSale> listOfUnitOfSales, Common.Models.CloudEvents.S57.Product product)
        {
            return actionNumber switch
            {
                //Case 1 : CREATE ENC CELL
                1 => listOfUnitOfSales.FirstOrDefault(x => x.UnitOfSaleType == Constants.UnitSaleType &&
                                                           x.Status == Constants.UnitOfSaleStatusForSale &&
                                                           x.CompositionChanges.AddProducts.Contains(product.ProductName)),

                //Case 4 : REPLACED WITH ENC CELL 
                //Case 10 : CANCEL ENC CELL
                4 or 10 => listOfUnitOfSales.FirstOrDefault(x => x.UnitOfSaleType == Constants.UnitSaleType &&
                                                            x.CompositionChanges.RemoveProducts.Contains(product.ProductName)),

                //Case 6 : CHANGE ENC CELL
                //Case 8 : UPDATE ENC CELL EDITION UPDATE NUMBER
                6 or 8 => listOfUnitOfSales.FirstOrDefault(x => x.UnitOfSaleType == Constants.UnitSaleType &&
                                                                x.Status == Constants.UnitOfSaleStatusForSale &&
                                                                product.InUnitsOfSale.Contains(x.UnitName)),
                _ => null,
            };
        }

        private void BuildAndAppendActionNode(XmlDocument soapXml, Product product, UnitOfSale unitOfSale, SapAction action, S57EventData eventData, XmlNode actionItemNode, string childCell = null, string replacedBy = null)
        {
            _logger.LogInformation(EventIds.BuildingSapActionStarted.ToEventId(), "Building SAP action {ActionName}.", action.Action);
            var actionNode = BuildAction(soapXml, product, unitOfSale, action, eventData.UkhoWeekNumber, childCell, replacedBy);
            actionItemNode.AppendChild(actionNode);
            _logger.LogInformation(EventIds.SapActionCreated.ToEventId(), "SAP action {ActionName} created.", action.Action);
        }

        private XmlElement BuildAction(XmlDocument soapXml, Product product, UnitOfSale unitOfSale, SapAction action, UkhoWeekNumber ukhoWeekNumber, string childCell, string replacedBy = null)
        {
            DecryptedPermit decryptedPermit = null;

            // Create main item node
            var itemNode = soapXml.CreateElement(Constants.Item);

            // Add basic action-related nodes
            _xmlHelper.AppendChildNode(itemNode, soapXml, Constants.ActionNumber, action.ActionNumber.ToString());
            _xmlHelper.AppendChildNode(itemNode, soapXml, Constants.Action, action.Action.ToString());
            _xmlHelper.AppendChildNode(itemNode, soapXml, Constants.Product, action.Product.ToString());
            _xmlHelper.AppendChildNode(itemNode, soapXml, Constants.ProdType, Constants.ProdTypeValue);

            // Add child cell node
            _xmlHelper.AppendChildNode(itemNode, soapXml, Constants.ChildCell, childCell);

            List<(int sortingOrder, XmlElement node)> actionAttributes = new();

            // Get permit keys for New cell and Updated cell
            if (action.Action == Constants.CreateEncCell || action.Action == Constants.UpdateCell)
            {
                decryptedPermit = _permitDecryption.Decrypt(product.Permit);
            }

            // Process ProductSection attributes
            ProcessAttributes(action.Action, action.Attributes.Where(x => x.Section == Constants.ProductSection), soapXml, product, actionAttributes, decryptedPermit, replacedBy);

            // Process UnitOfSaleSection attributes
            ProcessAttributes(action.Action, action.Attributes.Where(x => x.Section == Constants.UnitOfSaleSection), soapXml, unitOfSale, actionAttributes, null);

            // Process UkhoWeekNumberSection attributes
            ProcessUkhoWeekNumberAttributes(action.Action, action.Attributes.Where(x => x.Section == Constants.UkhoWeekNumberSection), soapXml, ukhoWeekNumber, actionAttributes);

            // Sort and append attributes to SAP action
            foreach (var (sortingOrder, node) in actionAttributes.OrderBy(x => x.sortingOrder))
            {
                itemNode.AppendChild(node);
            }

            return itemNode;
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
                        switch (attribute.XmlNodeName)
                        {
                            case Constants.ReplacedBy:
                                if (!IsPropertyNullOrEmpty(attribute.JsonPropertyName, replacedBy)) attributeNode.InnerText = GetXmlNodeValue(replacedBy.ToString(), attribute.XmlNodeName);
                                break;
                            case Constants.ActiveKey:
                                if (!IsPropertyNullOrEmpty(attribute.JsonPropertyName, decryptedPermit.ActiveKey)) attributeNode.InnerText = GetXmlNodeValue(decryptedPermit.ActiveKey, attribute.XmlNodeName);
                                break;
                            case Constants.NextKey:
                                if (!IsPropertyNullOrEmpty(attribute.JsonPropertyName, decryptedPermit.NextKey)) attributeNode.InnerText = GetXmlNodeValue(decryptedPermit.NextKey, attribute.XmlNodeName);
                                break;
                            default:
                                var jsonFieldValue = CommonHelper.ParseXmlNode(attribute.JsonPropertyName, source, source.GetType()).ToString();
                                if (!IsPropertyNullOrEmpty(attribute.JsonPropertyName, jsonFieldValue))
                                {
                                    attributeNode.InnerText = GetXmlNodeValue(jsonFieldValue.ToString(), attribute.XmlNodeName);
                                }
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
                    throw new ERPFacadeException(EventIds.BuildingSapActionInformationException.ToEventId(), $"Error while generating SAP action information. | Action : {action} | XML Attribute : {attribute.XmlNodeName} | ErrorMessage : {ex.Message}");
                }
            }
        }

        private void ProcessUkhoWeekNumberAttributes(string action, IEnumerable<ActionItemAttribute> attributes, XmlDocument soapXml, UkhoWeekNumber ukhoWeekNumber, List<(int, XmlElement)> actionAttributes)
        {
            if (ukhoWeekNumber == null)
            {
                throw new ERPFacadeException(EventIds.RequiredSectionNotFoundException.ToEventId(), $"UkhoWeekNumber section not found in enccontentpublished event payload while creating {action} action.");
            }

            foreach (var attribute in attributes)
            {
                try
                {
                    var attributeNode = soapXml.CreateElement(attribute.XmlNodeName);

                    if (attribute.IsRequired)
                    {
                        if (ukhoWeekNumber.Year.HasValue && ukhoWeekNumber.Week.HasValue && ukhoWeekNumber.CurrentWeekAlphaCorrection.HasValue)
                        {
                            switch (attribute.XmlNodeName)
                            {
                                case Constants.ValidFrom:
                                    var validFrom = _weekDetailsProvider.GetDateOfWeek(ukhoWeekNumber.Year.Value, ukhoWeekNumber.Week.Value, ukhoWeekNumber.CurrentWeekAlphaCorrection.Value);
                                    attributeNode.InnerText = GetXmlNodeValue(validFrom, attribute.XmlNodeName);
                                    break;
                                case Constants.WeekNo:
                                    var weekNo = string.Join("", ukhoWeekNumber.Year, ukhoWeekNumber.Week.Value.ToString("D2"));
                                    attributeNode.InnerText = GetXmlNodeValue(weekNo, attribute.XmlNodeName);
                                    break;
                                case Constants.Correction:
                                    attributeNode.InnerText = GetXmlNodeValue(ukhoWeekNumber.CurrentWeekAlphaCorrection.Value ? Constants.IsCorrectionTrue : Constants.IsCorrectionFalse, attribute.XmlNodeName);
                                    break;
                            }
                        }
                        else
                        {
                            throw new ERPFacadeException(EventIds.EmptyEventJsonPropertyException.ToEventId(), $"Required details are missing in enccontentpublished event payload. | Property Name : {attribute.JsonPropertyName}");
                        }
                    }
                    actionAttributes.Add((attribute.SortingOrder, attributeNode));
                }
                catch (Exception ex)
                {
                    throw new ERPFacadeException(EventIds.BuildingSapActionInformationException.ToEventId(), $"Error while generating SAP action information. | Action : {action} | XML Attribute : {attribute.XmlNodeName} | ErrorMessage : {ex.Message}");
                }
            }
        }

        private string GetXmlNodeValue(string fieldValue, string xmlNodeName = null)
        {
            // Return first 2 characters if the node is Agency, else limit other nodes to 250 characters
            return xmlNodeName == Constants.Agency ? CommonHelper.ToSubstring(fieldValue, 0, Constants.MaxAgencyXmlNodeLength) : CommonHelper.ToSubstring(fieldValue, 0, Constants.MaxXmlNodeLength);
        }
    }
}
