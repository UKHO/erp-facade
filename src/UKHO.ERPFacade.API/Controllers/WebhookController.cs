﻿using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Reflection;
using System.Xml;
using UKHO.ERPFacade.API.Models;
using UKHO.ERPFacade.Common.Helpers;
using UKHO.ERPFacade.Common.HttpClients;
using UKHO.ERPFacade.Common.IO;
using UKHO.ERPFacade.Common.Logging;

namespace UKHO.ERPFacade.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    //[Authorize]
    public class WebhookController : BaseController<WebhookController>
    {
        private readonly ILogger<WebhookController> _logger;
        private readonly IAzureTableReaderWriter _azureTableReaderWriter;
        private readonly IAzureBlobEventWriter _azureBlobEventWriter;
        private readonly ISapClient _sapClient;
        private readonly IXmlHelper _xmlHelper;

        private readonly IOptions<SapActionConfiguration> _sapActionConfig;
        private readonly IOptions<ActionNumberConfiguration> _actionNumberConfig;
        private readonly IOptions<ScenarioRuleConfiguration> _scenarioRuleConfig;

        public const string TRACEIDKEY = "data.traceId";

        public WebhookController(IHttpContextAccessor contextAccessor,
                                 ILogger<WebhookController> logger,
                                 IAzureTableReaderWriter azureTableReaderWriter,
                                 IAzureBlobEventWriter azureBlobEventWriter,
                                 ISapClient sapClient,
                                 IXmlHelper xmlHelper,
                                 IOptions<SapActionConfiguration> sapActionConfig,
                                 IOptions<ActionNumberConfiguration> actionNumberConfig,
                                 IOptions<ScenarioRuleConfiguration> scenarioRuleConfig)
        : base(contextAccessor)
        {
            _logger = logger;
            _azureTableReaderWriter = azureTableReaderWriter;
            _azureBlobEventWriter = azureBlobEventWriter;
            _sapClient = sapClient;
            _sapActionConfig = sapActionConfig;
            _actionNumberConfig = actionNumberConfig;
            _scenarioRuleConfig = scenarioRuleConfig;
            _xmlHelper = xmlHelper;
        }

        [HttpOptions]
        [Route("/webhook/newenccontentpublishedeventoptions")]
        //[Authorize(Policy = "WebhookCaller")]
        public IActionResult NewEncContentPublishedEventOptions()
        {
            var webhookRequestOrigin = HttpContext.Request.Headers["WebHook-Request-Origin"].FirstOrDefault();

            _logger.LogInformation(EventIds.NewEncContentPublishedEventOptionsCallStarted.ToEventId(), "Started processing the Options request for the New ENC Content Published event for webhook. | WebHook-Request-Origin : {webhookRequestOrigin}", webhookRequestOrigin);

            HttpContext.Response.Headers.Add("WebHook-Allowed-Rate", "*");
            HttpContext.Response.Headers.Add("WebHook-Allowed-Origin", webhookRequestOrigin);

            _logger.LogInformation(EventIds.NewEncContentPublishedEventOptionsCallCompleted.ToEventId(), "Completed processing the Options request for the New ENC Content Published event for webhook. | WebHook-Request-Origin : {webhookRequestOrigin}", webhookRequestOrigin);

            return new OkObjectResult(StatusCodes.Status200OK);
        }

        [HttpPost]
        [Route("/webhook/newenccontentpublishedeventreceived")]
        //[Authorize(Policy = "WebhookCaller")]
        public virtual async Task<IActionResult> NewEncContentPublishedEventReceived([FromBody] JObject requestJson)
        {
            try
            {
                _logger.LogInformation(EventIds.NewEncContentPublishedEventReceived.ToEventId(), "ERP Facade webhook has received new enccontentpublished event from EES. | _X-Correlation-ID : {CorrelationId}", GetCurrentCorrelationId());

                string traceId = requestJson.SelectToken(TRACEIDKEY)?.Value<string>();

                if (string.IsNullOrEmpty(traceId))
                {
                    _logger.LogWarning(EventIds.TraceIdMissingInEvent.ToEventId(), "TraceId is missing in ENC content published event.");
                    return new BadRequestObjectResult(StatusCodes.Status400BadRequest);
                }

                _logger.LogInformation(EventIds.StoreEncContentPublishedEventInAzureTable.ToEventId(), "Storing the received ENC content published event in azure table.");
                //await _azureTableReaderWriter.UpsertEntity(requestJson, traceId);

                _logger.LogInformation(EventIds.UploadEncContentPublishedEventInAzureBlob.ToEventId(), "Uploading the received ENC content published event in blob storage.");
                //await _azureBlobEventWriter.UploadEvent(requestJson, traceId);

                //Below line is added temporary only to send sample xml to mock service for local testing.

                List<Scenario> scenarios = GetScenarios(JsonConvert.DeserializeObject<EESEvent>(requestJson.ToString()));

                if (scenarios.Count > 0)
                {
                    XmlDocument sapPayload = BuildSapMessageXml(scenarios, traceId);

                    HttpResponseMessage response = await _sapClient.PostEventData(sapPayload, "Z_ADDS_MAT_INFO");

                    if (!response.IsSuccessStatusCode)
                    {
                        _logger.LogError(EventIds.SapConnectionFailed.ToEventId(), "Could not connect to SAP. | {StatusCode} | {SapResponse}", response.StatusCode, response.Content?.ReadAsStringAsync().Result);
                        throw new Exception();
                    }
                    _logger.LogInformation(EventIds.DataPushedToSap.ToEventId(), "Data pushed to SAP successfully. | {StatusCode} | {SapResponse}", response.StatusCode, response.Content?.ReadAsStringAsync().Result);

                    return new OkObjectResult(StatusCodes.Status200OK);
                }
                else
                {
                    _logger.LogWarning("No scenarios found in incoming EES event.");
                    throw new Exception();
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public static object GetProp(string name, object obj, Type type)
        {
            var parts = name.Split('.').ToList();
            var currentPart = parts[0];
            PropertyInfo info = type.GetProperty(currentPart);
            if (info == null) { return null; }
            if (name.IndexOf(".") > -1)
            {
                parts.Remove(currentPart);
                return GetProp(string.Join(".", parts), info.GetValue(obj, null), info.PropertyType);
            }
            else
            {
                return info.GetValue(obj, null).ToString();
            }
        }

        private List<Scenario> GetScenarios(EESEvent eventData)
        {
            List<Scenario> scenarios = new();

            bool restLoop = false;
            bool scenarioIdentified = false;

            //This loop is to identify the scenarios in given EES event request.
            foreach (Product product in eventData.Data.Products)
            {
                Scenario scenarioObj = new();

                foreach (var scenario in _scenarioRuleConfig.Value.ScenarioRules)
                {
                    foreach (var rule in scenario.Rules)
                    {
                        object jsonFieldValue = GetProp(rule.AttributeName, product, product.GetType());

                        if (jsonFieldValue != null && jsonFieldValue.ToString() == rule.AttriuteValue)
                        {
                            restLoop = true;
                            continue;
                        }
                        else
                        {
                            restLoop = false;
                            break;
                        }
                    }
                    if (restLoop)
                    {
                        scenarioObj.ScenarioType = scenario.Scenario;
                        scenarioObj.IsCellReplaced = product.ReplacedBy.Any();
                        scenarioObj.Product = product;
                        scenarioObj.InUnitOfSales = product.InUnitsOfSale;
                        scenarioObj.UnitOfSales = eventData.Data.UnitsOfSales.Where(x => product.InUnitsOfSale.Contains(x.UnitName) ||
                                                                                   (x.CompositionChanges.AddProducts.Contains(product.ProductName) ||
                                                                                    x.CompositionChanges.RemoveProducts.Contains(product.ProductName))).ToList();

                        scenarios.Add(scenarioObj);

                        scenarioIdentified = true;
                    }
                    if (scenarioIdentified) break;
                }
            }
            return scenarios;
        }

        private XmlDocument BuildSapMessageXml(List<Scenario> scenarios, string traceId)
        {
            XmlDocument soapXml = _xmlHelper.CreateXmlDocument(Path.Combine(Environment.CurrentDirectory, "SapXmlTemplates\\SAPRequest.xml"));

            XmlNode IM_MATINFONode = soapXml.SelectSingleNode($"//*[local-name()='IM_MATINFO']");
            XmlNode actionItemNode = soapXml.SelectSingleNode($"//*[local-name()='ACTIONITEMS']");

            foreach (var scenario in scenarios)
            {
                ActionNumber actionNumbers = _actionNumberConfig.Value.Actions.Where(x => x.Scenario == scenario.ScenarioType.ToString()).FirstOrDefault();
                List<int> actions = actionNumbers.ActionNumbers.ToList();

                foreach (var action in _sapActionConfig.Value.SapActions.Where(x => actions.Contains(x.ActionNumber)))
                {
                    XmlElement actionNode;

                    switch (action.ActionNumber)
                    {
                        case 3:
                        case 8:
                            foreach (var cell in scenario.InUnitOfSales)
                            {
                                actionNode = BuildAction(soapXml, scenario, action, cell);
                                actionItemNode.AppendChild(actionNode);
                            }
                            break;
                        case 4:
                            foreach (var cell in scenario.Product.ReplacedBy)
                            {
                                actionNode = BuildAction(soapXml, scenario, action, cell);
                                actionItemNode.AppendChild(actionNode);
                            }
                            break;
                        default:
                            actionNode = BuildAction(soapXml, scenario, action, scenario.Product.ProductName);
                            actionItemNode.AppendChild(actionNode);
                            break;
                    }
                }
            }

            XmlNode xmlNode = SortXmlPayload(actionItemNode);

            XmlNode noOfActions = soapXml.SelectSingleNode($"//*[local-name()='NOOFACTIONS']");
            XmlNode corrId = soapXml.SelectSingleNode($"//*[local-name()='CORRID']");
            XmlNode recDate = soapXml.SelectSingleNode($"//*[local-name()='RECDATE']");
            XmlNode recTime = soapXml.SelectSingleNode($"//*[local-name()='RECTIME']");

            corrId.InnerText = traceId;
            noOfActions.InnerText = xmlNode.ChildNodes.Count.ToString();
            recDate.InnerText = DateTime.UtcNow.ToString("yyyyMMdd");
            recTime.InnerText = DateTime.UtcNow.ToString("hhmmss");

            IM_MATINFONode.AppendChild(xmlNode);

            return soapXml;
        }

        private XmlNode SortXmlPayload(XmlNode actionItemNode)
        {
            List<XmlNode> actionItemList = new();
            int sequenceNumber = 1;

            foreach (XmlNode subNode in actionItemNode)
            {
                actionItemList.Add(subNode);
            }

            var sortedActionItemList = actionItemList.Cast<XmlNode>().OrderBy(x => Convert.ToInt32(x.SelectSingleNode("ACTIONNUMBER").InnerText)).ToList();

            foreach (XmlNode actionItem in sortedActionItemList)
            {
                actionItem.SelectSingleNode("ACTIONNUMBER").InnerText = sequenceNumber.ToString();
                sequenceNumber++;
            }

            foreach (XmlNode actionItem in sortedActionItemList)
            {
                actionItemNode.AppendChild(actionItem);
            }
            return actionItemNode;
        }

        private XmlElement BuildAction(XmlDocument soapXml, Scenario scenario, SapAction action, string cell)
        {
            XmlElement itemNode = soapXml.CreateElement("item");

            XmlElement actionNumberNode = soapXml.CreateElement("ACTIONNUMBER");
            actionNumberNode.InnerText = action.ActionNumber.ToString();

            XmlElement actionNode = soapXml.CreateElement("ACTION");
            actionNode.InnerText = action.Action.ToString();

            XmlElement productNode = soapXml.CreateElement("PRODUCT");
            productNode.InnerText = action.Product.ToString();

            itemNode.AppendChild(actionNumberNode);
            itemNode.AppendChild(actionNode);
            itemNode.AppendChild(productNode);

            foreach (var node in action.Attributes.Where(x => x.Section == "Product"))
            {
                XmlElement itemSubNode = soapXml.CreateElement(node.XmlNodeName);

                if (node.IsRequired)
                {
                    if (node.XmlNodeName == "REPLACEDBY")
                    {
                        itemSubNode.InnerText = string.IsNullOrWhiteSpace(cell.ToString()) ? string.Empty : cell.ToString().Substring(0, Math.Min(250, cell.ToString().Length));
                    }
                    else
                    {
                        object jsonFieldValue = GetProp(node.JsonPropertyName, scenario.Product, scenario.Product.GetType());
                        itemSubNode.InnerText = string.IsNullOrWhiteSpace(jsonFieldValue.ToString()) ? string.Empty : jsonFieldValue.ToString().Substring(0, Math.Min(250, jsonFieldValue.ToString().Length));
                    }
                }
                itemNode.AppendChild(itemSubNode);
            }

            foreach (var node in action.Attributes.Where(x => x.Section == "UnitOfSale"))
            {
                XmlElement itemSubNode = soapXml.CreateElement(node.XmlNodeName);

                if (node.IsRequired)
                {
                    UnitOfSale unitOfSale4 = scenario.UnitOfSales.Where(x => x.UnitName == cell).FirstOrDefault();

                    object jsonFieldValue = GetProp(node.JsonPropertyName, unitOfSale4, unitOfSale4.GetType());
                    itemSubNode.InnerText = string.IsNullOrWhiteSpace(jsonFieldValue.ToString()) ? string.Empty : jsonFieldValue.ToString().Substring(0, Math.Min(250, jsonFieldValue.ToString().Length));
                }
                itemNode.AppendChild(itemSubNode);
            }
            return itemNode;
        }
    }
}