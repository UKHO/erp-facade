﻿using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using UKHO.ERPFacade.Common.Helpers;
using UKHO.ERPFacade.Common.Logging;

namespace UKHO.ERPFacade.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class WebhookController : BaseController<WebhookController>
    {
        private readonly ILogger<WebhookController> _logger;
        private readonly IAzureTableStorageHelper _azureTableStorageHelper;
        private readonly IAzureBlobStorageHelper _azureBlobStorageHelper;

        public WebhookController(IHttpContextAccessor contextAccessor,
                                 ILogger<WebhookController> logger,
                                 IAzureTableStorageHelper azureTableStorageHelper,
                                 IAzureBlobStorageHelper azureBlobStorageHelper)
        : base(contextAccessor)
        {
            _logger = logger;
            _azureTableStorageHelper = azureTableStorageHelper;
            _azureBlobStorageHelper = azureBlobStorageHelper;
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
            _logger.LogInformation(EventIds.NewEncContentPublishedEventReceived.ToEventId(), "ERP Facade webhook has received new enccontentpublished event from EES. | _X-Correlation-ID : {CorrelationId}", GetCurrentCorrelationId());

            string traceId = requestJson.SelectToken("data.traceId")?.Value<string>();

            if (string.IsNullOrEmpty(traceId))
            {
                _logger.LogWarning(EventIds.CheckNullTraceId.ToEventId(), "Trace ID not found! | _X-Correlation-ID : {CorrelationId}", GetCurrentCorrelationId());
                return new BadRequestObjectResult(StatusCodes.Status400BadRequest);
            }

            _logger.LogInformation(EventIds.UploadEncContentPublishedEventInAzureBlob.ToEventId(), "Uploading the received ENC content published event in Azure Blob storage. | _X-Correlation-ID : {CorrelationId}", GetCurrentCorrelationId());
            await _azureBlobStorageHelper.UploadEvent(requestJson, traceId, GetCurrentCorrelationId());

            _logger.LogInformation(EventIds.StoreEncContentPublishedEventInAzureTable.ToEventId(), "Storing the received ENC content published event in Azure table storage. | _X-Correlation-ID : {CorrelationId}", GetCurrentCorrelationId());
            await _azureTableStorageHelper.UpsertEntity(requestJson, traceId, GetCurrentCorrelationId());

            return new OkObjectResult(StatusCodes.Status200OK);
        }
    }
}