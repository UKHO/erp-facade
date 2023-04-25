using Microsoft.AspNetCore.Mvc;
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
                XmlDocument soapXml = _xmlHelper.CreateXmlDocument(Path.Combine(Environment.CurrentDirectory, "SapXmlTemplates\\SAPRequest.xml"));

                XmlNode actionItemNode = soapXml.SelectSingleNode($"//*[local-name()='ACTIONITEMS']");

                List<Models.Action> actionItems = new();

                EESEvent eventData = JsonConvert.DeserializeObject<EESEvent>(requestJson.ToString());

                List<Scenario> scenarios = new();
                List<UnitOfSale> unitOfSales = new();
                bool restLoop = false;

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
                                scenarioObj.ScenarioType = scenario.Scenario;
                                scenarioObj.Product = product;
                                scenarioObj.InUnitOfSales = product.InUnitsOfSale;
                                scenarioObj.UnitOfSales = eventData.Data.UnitsOfSales.Where(x => product.InUnitsOfSale.Contains(x.UnitName) &&
                                                                                           (x.CompositionChanges.AddProducts.Contains(product.ProductName) ||
                                                                                            x.CompositionChanges.RemoveProducts.Contains(product.ProductName))).ToList();

                                scenarios.Add(scenarioObj);

                                restLoop = true;
                                break;
                            }
                        }
                        if (restLoop) break;
                    }
                }

                foreach (var scenario in scenarios)
                {
                    ActionNumber actionNumbers = _actionNumberConfig.Value.Actions.Where(x => x.Scenario == scenario.ScenarioType.ToString()).FirstOrDefault();
                    List<int> actions = actionNumbers.ActionNumbers.ToList();

                    foreach (var action in _sapActionConfig.Value.SapActions.Where(x => actions.Contains(x.ActionNumber)))
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
                                object jsonFieldValue = GetProp(node.JsonPropertyName, scenario.Product, scenario.Product.GetType());
                                itemSubNode.InnerText = string.IsNullOrWhiteSpace(jsonFieldValue.ToString()) ? string.Empty : jsonFieldValue.ToString().Substring(0, Math.Min(250, jsonFieldValue.ToString().Length));
                            }
                            itemNode.AppendChild(itemSubNode);
                        }

                        foreach (var node in action.Attributes.Where(x => x.Section == "UnitOfSale"))
                        {
                            foreach (var unitOfSale in scenario.InUnitOfSales)
                            {
                                XmlElement itemSubNode = soapXml.CreateElement(node.XmlNodeName);

                                if (node.IsRequired)
                                {
                                    object jsonFieldValue = GetProp(node.JsonPropertyName, unitOfSale, scenario.Product.GetType());
                                    itemSubNode.InnerText = string.IsNullOrWhiteSpace(jsonFieldValue.ToString()) ? string.Empty : jsonFieldValue.ToString().Substring(0, Math.Min(250, jsonFieldValue.ToString().Length));
                                }
                                itemNode.AppendChild(itemSubNode);
                            }
                        }
                        actionItemNode.AppendChild(itemNode);
                    }
                }

                HttpResponseMessage response = await _sapClient.PostEventData(soapXml, "Z_ADDS_MAT_INFO");

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError(EventIds.SapConnectionFailed.ToEventId(), "Could not connect to SAP. | {StatusCode} | {SapResponse}", response.StatusCode, response.Content?.ReadAsStringAsync().Result);
                    throw new Exception();
                }
                _logger.LogInformation(EventIds.DataPushedToSap.ToEventId(), "Data pushed to SAP successfully. | {StatusCode} | {SapResponse}", response.StatusCode, response.Content?.ReadAsStringAsync().Result);

                return new OkObjectResult(StatusCodes.Status200OK);
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
    }
}