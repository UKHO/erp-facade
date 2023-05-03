using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Xml;
using UKHO.ERPFacade.API.Helpers;
using UKHO.ERPFacade.API.Models;
using UKHO.ERPFacade.Common.HttpClients;
using UKHO.ERPFacade.Common.IO.Azure;
using UKHO.ERPFacade.Common.Logging;

namespace UKHO.ERPFacade.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class WebhookController : BaseController<WebhookController>
    {
        private readonly ILogger<WebhookController> _logger;
        private readonly IAzureTableReaderWriter _azureTableReaderWriter;
        private readonly IAzureBlobEventWriter _azureBlobEventWriter;
        private readonly ISapClient _sapClient;
        private readonly IScenarioBuilder _scenarioBuilder;
        private readonly ISapMessageBuilder _sapMessageBuilder;

        public const string TRACEIDKEY = "data.traceId";

        public WebhookController(IHttpContextAccessor contextAccessor,
                                 ILogger<WebhookController> logger,
                                 IAzureTableReaderWriter azureTableReaderWriter,
                                 IAzureBlobEventWriter azureBlobEventWriter,
                                 ISapClient sapClient,
                                 IScenarioBuilder scenarioBuilder,
                                 ISapMessageBuilder sapMessageBuilder)
        : base(contextAccessor)
        {
            _logger = logger;
            _azureTableReaderWriter = azureTableReaderWriter;
            _azureBlobEventWriter = azureBlobEventWriter;
            _sapClient = sapClient;
            _scenarioBuilder = scenarioBuilder;
            _sapMessageBuilder = sapMessageBuilder;
        }

        [HttpOptions]
        [Route("/webhook/newenccontentpublishedeventoptions")]
        [Authorize(Policy = "WebhookCaller")]
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
        [Authorize(Policy = "WebhookCaller")]
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
                await _azureTableReaderWriter.UpsertEntity(requestJson, traceId);

                _logger.LogInformation(EventIds.UploadEncContentPublishedEventInAzureBlob.ToEventId(), "Uploading the received ENC content published event in blob storage.");
                await _azureBlobEventWriter.UploadEvent(requestJson, traceId);

                List<Scenario> scenarios = _scenarioBuilder.BuildScenarios(JsonConvert.DeserializeObject<EESEvent>(requestJson.ToString()));

                if (scenarios.Count > 0)
                {
                    XmlDocument sapPayload = _sapMessageBuilder.BuildSapMessageXml(scenarios, traceId);

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
    }
}