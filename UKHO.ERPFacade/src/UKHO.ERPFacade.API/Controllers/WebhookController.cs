using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using System.Xml;
using UKHO.ERPFacade.Common.HttpClients;
using UKHO.ERPFacade.Common.IO;
using UKHO.ERPFacade.Common.Logging;

namespace UKHO.ERPFacade.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class WebhookController : BaseController<WebhookController>
    {
        private readonly ILogger<WebhookController> _logger;
        private readonly IAzureTableReaderWriter _azureTableReaderWriter;
        private readonly IAzureBlobEventWriter _azureBlobEventWriter;
        private readonly ISapClient _sapClient;

        public WebhookController(IHttpContextAccessor contextAccessor,
                                 ILogger<WebhookController> logger,
                                 IAzureTableReaderWriter azureTableReaderWriter,
                                 IAzureBlobEventWriter azureBlobEventWriter,
                                 ISapClient sapClient)
        : base(contextAccessor)
        {
            _logger = logger;
            _azureTableReaderWriter = azureTableReaderWriter;
            _azureBlobEventWriter = azureBlobEventWriter;
            _sapClient = sapClient;
        }

        [HttpOptions]
        [Route("/webhook/newenccontentpublishedeventoptions")]
        public IActionResult NewEncContentPublishedEventOptions()
        {
            var webhookRequestOrigin = HttpContext.Request.Headers["WebHook-Request-Origin"].FirstOrDefault();

            _logger.LogInformation(EventIds.NewEncContentPublishedEventOptionsCallStarted.ToEventId(), "Started processing the Options request for the New ENC Content Published event for webhook. | WebHook-Request-Origin : {webhookRequestOrigin} | _X-Correlation-ID : {CorrelationId}", webhookRequestOrigin, GetCurrentCorrelationId());

            HttpContext.Response.Headers.Add("WebHook-Allowed-Rate", "*");
            HttpContext.Response.Headers.Add("WebHook-Allowed-Origin", webhookRequestOrigin);

            _logger.LogInformation(EventIds.NewEncContentPublishedEventOptionsCallCompleted.ToEventId(), "Completed processing the Options request for the New ENC Content Published event for webhook. | WebHook-Request-Origin : {webhookRequestOrigin} | _X-Correlation-ID : {CorrelationId}", webhookRequestOrigin, GetCurrentCorrelationId());

            return new OkObjectResult(StatusCodes.Status200OK);
        }


        [HttpPost]
        [Route("/webhook/newenccontentpublishedeventreceived")]
        public virtual async Task<IActionResult> NewEncContentPublishedEventReceived([FromBody] JObject requestJson)
        {
            try
            {
                _logger.LogInformation(EventIds.NewEncContentPublishedEventReceived.ToEventId(), "ERP Facade webhook has received new enccontentpublished event from EES. | _X-Correlation-ID : {CorrelationId}", GetCurrentCorrelationId());

                string traceId = requestJson.SelectToken("data.traceId")?.Value<string>();

                if (string.IsNullOrEmpty(traceId))
                {
                    _logger.LogWarning(EventIds.TraceIdMissingInEvent.ToEventId(), "TraceId is missing in ENC content published event. | _X-Correlation-ID : {CorrelationId}", GetCurrentCorrelationId());
                    return new BadRequestObjectResult(StatusCodes.Status400BadRequest);
                }

                _logger.LogInformation(EventIds.StoreEncContentPublishedEventInAzureTable.ToEventId(), "Storing the received ENC content published event in azure table. | _X-Correlation-ID : {CorrelationId}", GetCurrentCorrelationId());
                await _azureTableReaderWriter.UpsertEntity(requestJson, traceId, GetCurrentCorrelationId());

                _logger.LogInformation(EventIds.UploadEncContentPublishedEventInAzureBlob.ToEventId(), "Uploading the received ENC content published event in blob storage. | _X-Correlation-ID : {CorrelationId}", GetCurrentCorrelationId());
                await _azureBlobEventWriter.UploadEvent(requestJson, traceId, GetCurrentCorrelationId());

                //Below two lines are added temporary only to send sample xml to mock service for local testing.
                XmlDocument soapXml = new XmlDocument();
                soapXml.Load(@"..\UKHO.ERPFacade.API\SapXmlTemplates\SAPRequest.xml");

                HttpResponseMessage response = await _sapClient.PostEventData(soapXml, "Z_ADDS_MAT_INFO");

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError("Could not connect to SAP | {StatusCode} | {SapResponse}", response.StatusCode, response.Content?.ReadAsStringAsync().Result);
                    throw new Exception();
                }
                _logger.LogInformation("Data pushed to SAP successfully | {StatusCode} | {SapResponse}", response.StatusCode, response.Content?.ReadAsStringAsync().Result);

                return new OkObjectResult(StatusCodes.Status200OK);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
    }
}