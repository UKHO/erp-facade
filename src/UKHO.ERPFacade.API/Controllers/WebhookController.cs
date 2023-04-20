using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
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

        public const string TRACEIDKEY = "data.traceId";

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

            _logger.LogInformation(EventIds.NewEncContentPublishedEventOptionsCallStarted.ToEventId(), "Started processing the Options request for the New ENC Content Published event for webhook. | WebHook-Request-Origin : {webhookRequestOrigin}", webhookRequestOrigin);

            HttpContext.Response.Headers.Add("WebHook-Allowed-Rate", "*");
            HttpContext.Response.Headers.Add("WebHook-Allowed-Origin", webhookRequestOrigin);

            _logger.LogInformation(EventIds.NewEncContentPublishedEventOptionsCallCompleted.ToEventId(), "Completed processing the Options request for the New ENC Content Published event for webhook. | WebHook-Request-Origin : {webhookRequestOrigin}", webhookRequestOrigin);

            return new OkObjectResult(StatusCodes.Status200OK);
        }

        [HttpPost]
        [Route("/webhook/newenccontentpublishedeventreceived")]
        public virtual async Task<IActionResult> NewEncContentPublishedEventReceived([FromBody] JObject requestJson)
        {
            try
            {
                _logger.LogInformation(EventIds.NewEncContentPublishedEventReceived.ToEventId(), "ERP Facade webhook has received new enccontentpublished event from EES.");

                string traceId = requestJson.SelectToken(TRACEIDKEY)?.Value<string>();

                if (string.IsNullOrEmpty(traceId))
                {
                    _logger.LogWarning(EventIds.TraceIdMissingInEvent.ToEventId(), "TraceId is missing in ENC content published event.");
                    return new BadRequestObjectResult(StatusCodes.Status400BadRequest);
                }

                _logger.LogInformation(EventIds.StoreEncContentPublishedEventInAzureTable.ToEventId(), "Storing the received ENC content published event in azure table.");
                await _azureTableReaderWriter.UpsertEntity(requestJson, traceId, GetCurrentCorrelationId());

                _logger.LogInformation(EventIds.UploadEncContentPublishedEventInAzureBlob.ToEventId(), "Uploading the received ENC content published event in blob storage.");
                await _azureBlobEventWriter.UploadEvent(requestJson, traceId, GetCurrentCorrelationId());

                return new OkObjectResult(StatusCodes.Status200OK);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
    }
}