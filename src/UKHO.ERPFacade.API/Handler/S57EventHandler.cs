using System.Xml;
using UKHO.ERPFacade.Common.HttpClients;
using UKHO.ERPFacade.Common.IO.Azure;
using UKHO.ERPFacade.Common.IO;
using UKHO.ERPFacade.Common.Models;
using UKHO.ERPFacade.Common.Exceptions;
using UKHO.ERPFacade.Common.Logging;
using Microsoft.Extensions.Options;
using UKHO.ERPFacade.Common.Constants;
using UKHO.ERPFacade.Common.PermitDecryption;
using UKHO.ERPFacade.Common.Providers;
using Newtonsoft.Json;
using UKHO.ERPFacade.Common.Configuration;

namespace UKHO.ERPFacade.API.Handler
{
    public class S57EventHandler : EventHandler<EncEventPayload>
    {
        private readonly ILogger<S57EventHandler> _logger;
        private readonly IOptions<SapActionConfiguration> _sapActionConfig;        
        private readonly IWeekDetailsProvider _weekDetailsProvider;
        private readonly IPermitDecryption _permitDecryption;
        private readonly IOptions<SapConfiguration> _sapConfig;

        public S57EventHandler(ILogger<S57EventHandler> logger,
                                    IOptions<SapActionConfiguration> sapActionConfig,
                                    IAzureTableReaderWriter azureTableReaderWriter,
                                    IAzureBlobEventWriter azureBlobEventWriter,
                                    ISapClient sapClient,
                                    IXmlHelper xmlHelper,
                                    IFileSystemHelper fileSystemHelper,
                                    IWeekDetailsProvider weekDetailsProvider,
                                    IPermitDecryption permitDecryption,
                                    IOptions<SapConfiguration> sapConfig) : base(logger,azureTableReaderWriter,
                                                                                azureBlobEventWriter, sapClient, xmlHelper, fileSystemHelper)
        {
            _logger = logger;
            _sapActionConfig = sapActionConfig;
            _weekDetailsProvider = weekDetailsProvider;
            _permitDecryption = permitDecryption;
            _sapConfig = sapConfig;
        }

        public override IEventData PrepareModel(string encEventJson)
        {
            IEventData eventData = new S57EventData();
            eventData.EventData= JsonConvert.DeserializeObject<EncEventPayload>(encEventJson);
            eventData.SapEndpointForEvent = _sapConfig.Value.SapEndpointForEncEvent;
            eventData.SapUsernameForEvent = _sapConfig.Value.SapUsernameForEncEvent;
            eventData.SapPasswordForEvent = _sapConfig.Value.SapPasswordForEncEvent;
            eventData.SapServiceOperationForEvent = _sapConfig.Value.SapServiceOperationForEncEvent;
            return eventData;
        }

        public override async Task BuildEncCellActions(EncEventPayload eventData, XmlDocument soapXml, XmlNode? actionItemNode)
        {            
            _logger.LogInformation(EventIds.EncCellSapActionGenerationStarted.ToEventId(), "Building ENC cell SAP actions.");

            foreach (var product in eventData.Data.Products)
            {
                foreach (var action in _sapActionConfig.Value.SapActions.Where(x => x.Product == Constants.EncCell))
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
            await Task.FromResult(0);

        }

        public override async Task BuildUnitActions(EncEventPayload eventData, XmlDocument soapXml, XmlNode actionItemNode)
        {
            foreach (var unitOfSale in eventData.Data.UnitsOfSales)
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
            await Task.FromResult(0);
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
        private void BuildAndAppendActionNode(XmlDocument soapXml, Product product, UnitOfSale unitOfSale, SapAction action, EncEventPayload eventData, XmlNode actionItemNode, string childCell = null, string replacedBy = null)
        {
            _logger.LogInformation(EventIds.BuildingSapActionStarted.ToEventId(), "Building SAP action {ActionName}.", action.Action);
            var actionNode = BuildAction(soapXml, product, unitOfSale, action, eventData.Data.UkhoWeekNumber, childCell, replacedBy);
            actionItemNode.AppendChild(actionNode);
            _logger.LogInformation(EventIds.SapActionCreated.ToEventId(), "SAP action {ActionName} created.", action.Action);
        }

        private XmlElement BuildAction(XmlDocument soapXml, Product product, UnitOfSale unitOfSale, SapAction action, UkhoWeekNumber ukhoWeekNumber, string childCell, string replacedBy = null)
        {
            DecryptedPermit decryptedPermit = null;

            // Create main item node
            var itemNode = soapXml.CreateElement(Constants.Item);

            // Add basic action-related nodes
            AppendChildNode(itemNode, soapXml, Constants.ActionNumber, action.ActionNumber.ToString());
            AppendChildNode(itemNode, soapXml, Constants.Action, action.Action.ToString());
            AppendChildNode(itemNode, soapXml, Constants.Product, action.Product.ToString());
            AppendChildNode(itemNode, soapXml, Constants.ProdType, Constants.ProdTypeValue);

            // Add child cell node
            AppendChildNode(itemNode, soapXml, Constants.ChildCell, childCell);

            List<(int sortingOrder, XmlElement node)> actionAttributes = new();

            // Get permit keys for New cell and Updated cell
            if (action.Action == Constants.CreateEncCell || action.Action == Constants.UpdateCell)
            {
                decryptedPermit = _permitDecryption.Decrypt(product.Permit);
            }

            // Process ProductSection attributes
            ProcessAttributes(action.Action, action.Attributes.Where(x => x.Section == Constants.ProductSection), soapXml, product, actionAttributes, decryptedPermit, replacedBy);

            // Process UnitOfSaleSection attributes
            ProcessAttributes(action.Action, action.Attributes.Where(x => x.Section == Constants.UnitOfSaleSection), soapXml, unitOfSale, actionAttributes, null, null);

            // Process UkhoWeekNumberSection attributes
            ProcessUkhoWeekNumberAttributes(action.Action, action.Attributes.Where(x => x.Section == Constants.UkhoWeekNumberSection), soapXml, ukhoWeekNumber, actionAttributes);

            // Sort and append attributes to SAP action
            foreach (var (sortingOrder, node) in actionAttributes.OrderBy(x => x.sortingOrder))
            {
                itemNode.AppendChild(node);
            }

            return itemNode;
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

    }
}
