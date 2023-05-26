using System.Xml;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UKHO.ERPFacade.API.Helpers;
using UKHO.ERPFacade.API.Models;
using UKHO.ERPFacade.Common.Configuration;
using UKHO.ERPFacade.Common.Exceptions;
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
        //private readonly IScenarioBuilder _scenarioBuilder;
        //private readonly ISapMessageBuilder _sapMessageBuilder;
        private readonly IOptions<SapConfiguration> _sapConfig;
        private readonly IXmlBuilder _xmlBuilder;
        private const string TraceIdKey = "data.traceId";
        private const string RequestFormat = "json";

        public WebhookController(IHttpContextAccessor contextAccessor,
                                 ILogger<WebhookController> logger,
                                 IAzureTableReaderWriter azureTableReaderWriter,
                                 IAzureBlobEventWriter azureBlobEventWriter,
                                 ISapClient sapClient,
                                 IXmlBuilder xmlBuilder,
                                 //IScenarioBuilder scenarioBuilder,
                                 //ISapMessageBuilder sapMessageBuilder,
                                    IOptions<SapConfiguration> sapConfig)
        : base(contextAccessor)
        {
            _logger = logger;
            _azureTableReaderWriter = azureTableReaderWriter;
            _azureBlobEventWriter = azureBlobEventWriter;
            _sapClient = sapClient;
            //_scenarioBuilder = scenarioBuilder;
            //_sapMessageBuilder = sapMessageBuilder;
            _xmlBuilder = xmlBuilder;
            _sapConfig = sapConfig ?? throw new ArgumentNullException(nameof(sapConfig));
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
            _logger.LogInformation(EventIds.NewEncContentPublishedEventReceived.ToEventId(), "ERP Facade webhook has received new enccontentpublished event from EES.");

            string traceId = requestJson.SelectToken(TraceIdKey)?.Value<string>();

            if (string.IsNullOrEmpty(traceId))
            {
                _logger.LogWarning(EventIds.TraceIdMissingInEvent.ToEventId(), "TraceId is missing in ENC content published event.");
                return new BadRequestObjectResult(StatusCodes.Status400BadRequest);
            }

            _logger.LogInformation(EventIds.StoreEncContentPublishedEventInAzureTable.ToEventId(), "Storing the received ENC content published event in azure table.");

            await _azureTableReaderWriter.UpsertEntity(requestJson, traceId);

            _logger.LogInformation(EventIds.UploadEncContentPublishedEventInAzureBlob.ToEventId(), "Uploading the received ENC content published event in blob storage.");

            await _azureBlobEventWriter.UploadEvent(requestJson.ToString(), traceId, traceId + '.' + RequestFormat);

            _logger.LogInformation(EventIds.UploadedEncContentPublishedEventInAzureBlob.ToEventId(), "ENC content published event is uploaded in blob storage successfully.");

            //List<Scenario> scenarios = _scenarioBuilder.BuildScenarios(JsonConvert.DeserializeObject<EESEvent>(requestJson.ToString()));


            //if (scenarios.Count > 0)
            {
                XmlDocument sapPayload = _xmlBuilder.BuildSapMessageXml(JsonConvert.DeserializeObject<EESEvent>(requestJson.ToString()), traceId);

                HttpResponseMessage response = await _sapClient.PostEventData(sapPayload, _sapConfig.Value.SapServiceOperation);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError(EventIds.SapConnectionFailed.ToEventId(), "Could not connect to SAP. | {StatusCode}", response.StatusCode);
                    throw new ERPFacadeException(EventIds.SapConnectionFailed.ToEventId());
                }
                _logger.LogInformation(EventIds.DataPushedToSap.ToEventId(), "Data pushed to SAP successfully. | {StatusCode}", response.StatusCode);

                await _azureTableReaderWriter.UpdateRequestTimeEntity(traceId);

                return new OkObjectResult(StatusCodes.Status200OK);
            }
            //else
            //{
            //    _logger.LogError(EventIds.NoScenarioFound.ToEventId(), "No scenarios found in incoming EES event.");
            //    throw new ERPFacadeException(EventIds.NoScenarioFound.ToEventId());
            //}
        }
    }
}