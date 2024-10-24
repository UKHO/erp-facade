﻿using System.Xml;
using Microsoft.Extensions.Options;
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

namespace UKHO.ERPFacade.API.XmlTransformers
{
    public class S57XmlTransformer : BaseXmlTransformer
    {
        private readonly ILogger<S57XmlTransformer> _logger;
        private readonly IXmlHelper _xmlHelper;
        private readonly IWeekDetailsProvider _weekDetailsProvider;
        private readonly IPermitDecryption _permitDecryption;
        private readonly IOptions<SapActionConfiguration> _sapActionConfig;

        public S57XmlTransformer(ILogger<S57XmlTransformer> logger,
                                 IXmlHelper xmlHelper,
                                 IFileSystemHelper fileSystemHelper,
                                 IWeekDetailsProvider weekDetailsProvider,
                                 IPermitDecryption permitDecryption,
                                 IOptions<SapActionConfiguration> sapActionConfig)
        : base(fileSystemHelper, xmlHelper)
        {
            _logger = logger;
            _xmlHelper = xmlHelper;
            _weekDetailsProvider = weekDetailsProvider;
            _permitDecryption = permitDecryption;
            _sapActionConfig = sapActionConfig;
        }

        public override XmlDocument BuildXmlPayload<T>(T eventData, string xmlTemplatePath)
        {
            _logger.LogInformation(EventIds.S57EventSapXmlPayloadGenerationStarted.ToEventId(), "Generation of SAP xml payload for S57 enccontentpublished event started.");

            var s57EventXmlPayload = _xmlHelper.CreateXmlDocument(Path.Combine(Environment.CurrentDirectory, xmlTemplatePath));

            if (eventData is S57EventData s57EventData)
            {
                var actionItemNode = s57EventXmlPayload.SelectSingleNode(Constants.XpathActionItems);

                // Build SAP actions for ENC Cell
                BuildEncCellActions(s57EventData, s57EventXmlPayload, actionItemNode);

                // Build SAP actions for Units
                BuildUnitActions(s57EventData, s57EventXmlPayload, actionItemNode);

                // Finalize SAP XML message
                FinalizeSapXmlMessage(s57EventXmlPayload, s57EventData.CorrelationId, actionItemNode);

                _logger.LogInformation(EventIds.S57EventSapXmlPayloadGenerationCompleted.ToEventId(), "Generation of SAP xml payload for S57 enccontentpublished event completed.");
            }
            return s57EventXmlPayload;
        }

        private void BuildEncCellActions(S57EventData eventData, XmlDocument soapXml, XmlNode actionItemNode)
        {
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
                                throw new ERPFacadeException(EventIds.RequiredUnitNotFoundException.ToEventId(), $"Required unit not found in S57 enccontentpublished event for {product.ProductName} to generate {action.Action} action.");
                            }
                            BuildAndAppendActionNode(soapXml, product, unitOfSale, action, eventData, actionItemNode, product.ProductName);
                            break;

                        case 4://REPLACED WITH ENC CELL
                            if (product.ReplacedBy.Any() && unitOfSale is null)
                            {
                                throw new ERPFacadeException(EventIds.RequiredUnitNotFoundException.ToEventId(), $"Required unit not found in S57 enccontentpublished event for {product.ProductName} to generate {action.Action} action.");
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

        private UnitOfSale? GetUnitOfSale(int actionNumber, List<UnitOfSale> listOfUnitOfSales, Product product)
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
            _logger.LogInformation(EventIds.S57SapActionGenerationStarted.ToEventId(), "Generation of {ActionName} action started.", action.Action);
            var actionNode = BuildAction(soapXml, product, unitOfSale, action, eventData.UkhoWeekNumber, childCell, replacedBy);
            actionItemNode.AppendChild(actionNode);
            _logger.LogInformation(EventIds.S57SapActionGenerationCompleted.ToEventId(), "Generation of {ActionName} action completed", action.Action);
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
                                if (!IsPropertyNullOrEmpty(attribute.JsonPropertyName, replacedBy)) attributeNode.InnerText = CommonHelper.ToSubstring(replacedBy.ToString(), 0, Constants.MaxXmlNodeLength);
                                break;
                            case Constants.ActiveKey:
                                if (!IsPropertyNullOrEmpty(attribute.JsonPropertyName, decryptedPermit.ActiveKey)) attributeNode.InnerText = CommonHelper.ToSubstring(decryptedPermit.ActiveKey, 0, Constants.MaxXmlNodeLength);
                                break;
                            case Constants.NextKey:
                                if (!IsPropertyNullOrEmpty(attribute.JsonPropertyName, decryptedPermit.NextKey)) attributeNode.InnerText = CommonHelper.ToSubstring(decryptedPermit.NextKey, 0, Constants.MaxXmlNodeLength);
                                break;
                            default:
                                var jsonFieldValue = CommonHelper.ParseXmlNode(attribute.JsonPropertyName, source, source.GetType()).ToString();
                                if (!IsPropertyNullOrEmpty(attribute.JsonPropertyName, jsonFieldValue))
                                {
                                    // Set value as first 2 characters if the node is Agency, else limit other nodes to 250 characters
                                    attributeNode.InnerText = attribute.XmlNodeName == Constants.Agency ? CommonHelper.ToSubstring(jsonFieldValue, 0, Constants.MaxAgencyXmlNodeLength) : CommonHelper.ToSubstring(jsonFieldValue, 0, Constants.MaxXmlNodeLength);
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
                    throw new ERPFacadeException(EventIds.S57SapActionInformationGenerationFailedException.ToEventId(), $"Error while generating SAP action information. | Action : {action} | XML Attribute : {attribute.XmlNodeName} | ErrorMessage : {ex.Message}");
                }
            }
        }

        private void ProcessUkhoWeekNumberAttributes(string action, IEnumerable<ActionItemAttribute> attributes, XmlDocument soapXml, UkhoWeekNumber ukhoWeekNumber, List<(int, XmlElement)> actionAttributes)
        {
            if (ukhoWeekNumber == null)
            {
                throw new ERPFacadeException(EventIds.RequiredWeekDetailsNotFoundException.ToEventId(), $"UkhoWeekNumber details not found in S57 enccontentpublished event to generate {action} action.");
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
                                    attributeNode.InnerText = CommonHelper.ToSubstring(validFrom, 0, Constants.MaxXmlNodeLength);
                                    break;
                                case Constants.WeekNo:
                                    var weekNo = string.Join("", ukhoWeekNumber.Year, ukhoWeekNumber.Week.Value.ToString("D2"));
                                    attributeNode.InnerText = CommonHelper.ToSubstring(weekNo, 0, Constants.MaxXmlNodeLength);
                                    break;
                                case Constants.Correction:
                                    attributeNode.InnerText = ukhoWeekNumber.CurrentWeekAlphaCorrection.Value ? Constants.IsCorrectionTrue : Constants.IsCorrectionFalse;
                                    break;
                            }
                        }
                        else
                        {
                            throw new ERPFacadeException(EventIds.EmptyEventJsonPropertyException.ToEventId(), $"Required property value is empty in enccontentpublished event payload. | Property Name : {attribute.JsonPropertyName}");
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