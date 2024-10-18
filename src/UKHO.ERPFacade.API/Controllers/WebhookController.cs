using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UKHO.ERPFacade.API.Helpers;
using UKHO.ERPFacade.API.Services;
using UKHO.ERPFacade.Common.Configuration;
using UKHO.ERPFacade.Common.Constants;
using UKHO.ERPFacade.Common.Exceptions;
using UKHO.ERPFacade.Common.HttpClients;
using UKHO.ERPFacade.Common.IO;
using UKHO.ERPFacade.Common.IO.Azure;
using UKHO.ERPFacade.Common.Logging;
using UKHO.ERPFacade.Common.Models;
using UKHO.ERPFacade.Common.Models.TableEntities;
using Status = UKHO.ERPFacade.Common.Enums.Status;

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
        private readonly IAzureQueueHelper _azureQueueHelper;
        private readonly ISapClient _sapClient;
        private readonly IS57Service _s57Service;
        private readonly IOptions<SapConfiguration> _sapConfig;
        private readonly ILicenceUpdatedSapMessageBuilder _licenceUpdatedSapMessageBuilder;

        public WebhookController(IHttpContextAccessor contextAccessor,
                                 ILogger<WebhookController> logger,
                                 IAzureTableReaderWriter azureTableReaderWriter,
                                 IAzureBlobEventWriter azureBlobEventWriter,
                                 IAzureQueueHelper azureQueueHelper,
                                 ISapClient sapClient,
                                 IS57Service s57Service,
                                 IOptions<SapConfiguration> sapConfig,
                                 ILicenceUpdatedSapMessageBuilder licenceUpdatedSapMessageBuilder)
        : base(contextAccessor)
        {
            _logger = logger;
            _azureTableReaderWriter = azureTableReaderWriter;
            _azureBlobEventWriter = azureBlobEventWriter;
            _azureQueueHelper = azureQueueHelper;
            _sapClient = sapClient;
            _s57Service = s57Service;
            _licenceUpdatedSapMessageBuilder = licenceUpdatedSapMessageBuilder;
            _sapConfig = sapConfig ?? throw new ArgumentNullException(nameof(sapConfig));
        }

        [HttpOptions]
        [Route("/webhook/newenccontentpublishedeventreceived")]
        [Authorize(Policy = "EncContentPublishedWebhookCaller")]
        public IActionResult NewEncContentPublishedEventOptions()
        {
            var webhookRequestOrigin = HttpContext.Request.Headers["WebHook-Request-Origin"].FirstOrDefault();

            _logger.LogInformation(EventIds.NewEncContentPublishedEventOptionsCallStarted.ToEventId(), "Started processing the Options request for the New ENC Content Published event for webhook. | WebHook-Request-Origin : {webhookRequestOrigin}", webhookRequestOrigin);

            HttpContext.Response.Headers.Append("WebHook-Allowed-Rate", "*");
            HttpContext.Response.Headers.Append("WebHook-Allowed-Origin", webhookRequestOrigin);

            _logger.LogInformation(EventIds.NewEncContentPublishedEventOptionsCallCompleted.ToEventId(), "Completed processing the Options request for the New ENC Content Published event for webhook. | WebHook-Request-Origin : {webhookRequestOrigin}", webhookRequestOrigin);

            return new OkObjectResult(StatusCodes.Status200OK);
        }

        [HttpPost]
        [Route("/webhook/newenccontentpublishedeventreceived")]
        [Authorize(Policy = "EncContentPublishedWebhookCaller")]
        public virtual async Task<IActionResult> NewEncContentPublishedEventReceived([FromBody] JObject encEventJson)
        {
            _logger.LogInformation(EventIds.NewEncContentPublishedEventReceived.ToEventId(), "ERP Facade webhook has received new enccontentpublished event from EES.");

            var correlationId = encEventJson.SelectToken(Constants.CorrelationIdKey)?.Value<string>();

            if (string.IsNullOrEmpty(correlationId))
            {
                _logger.LogWarning(EventIds.CorrelationIdMissingInEvent.ToEventId(), "CorrelationId is missing in enccontentpublished event.");
                return new BadRequestObjectResult(StatusCodes.Status400BadRequest);
            }

            await _s57Service.ProcessS57Event(encEventJson);

            return new OkObjectResult(StatusCodes.Status200OK);
        }

        [HttpOptions]
        [Route("/webhook/recordofsalepublishedeventreceived")]
        [Authorize(Policy = "RecordOfSaleWebhookCaller")]
        [NonAction]
        public IActionResult RecordOfSalePublishedEventOptions()
        {
            var webhookRequestOrigin = HttpContext.Request.Headers["WebHook-Request-Origin"].FirstOrDefault();

            _logger.LogInformation(EventIds.RecordOfSalePublishedEventOptionsCallStarted.ToEventId(), "Started processing the Options request for the Record of Sale Published event for webhook. | WebHook-Request-Origin : {webhookRequestOrigin}", webhookRequestOrigin);

            HttpContext.Response.Headers.Append("WebHook-Allowed-Rate", "*");
            HttpContext.Response.Headers.Append("WebHook-Allowed-Origin", webhookRequestOrigin);

            _logger.LogInformation(EventIds.RecordOfSalePublishedEventOptionsCallCompleted.ToEventId(), "Completed processing the Options request for the Record of Sale Published event for webhook. | WebHook-Request-Origin : {webhookRequestOrigin}", webhookRequestOrigin);

            return new OkObjectResult(StatusCodes.Status200OK);
        }

        [HttpPost]
        [Route("/webhook/recordofsalepublishedeventreceived")]
        [Authorize(Policy = "RecordOfSaleWebhookCaller")]
        [NonAction]
        public virtual async Task<IActionResult> RecordOfSalePublishedEventReceived([FromBody] JObject recordOfSaleEventJson)
        {
            _logger.LogInformation(EventIds.RecordOfSalePublishedEventReceived.ToEventId(), "ERP Facade webhook has received record of sale event from EES.");

            var correlationId = recordOfSaleEventJson.SelectToken(Constants.CorrelationIdKey)?.Value<string>();
            var eventId = recordOfSaleEventJson.SelectToken(Constants.EventIdKey)?.Value<string>();

            if (string.IsNullOrEmpty(correlationId))
            {
                _logger.LogWarning(EventIds.CorrelationIdMissingInRecordOfSaleEvent.ToEventId(), "CorrelationId is missing in Record of Sale published event.");
                return new BadRequestObjectResult(StatusCodes.Status400BadRequest);
            }

            RecordOfSaleEventEntity recordOfSaleEvent = new()
            {
                RowKey = Guid.NewGuid().ToString(),
                PartitionKey = Guid.NewGuid().ToString(),
                Timestamp = DateTime.UtcNow,
                CorrelationId = correlationId,
                Status = Status.Incomplete.ToString()
            };

            _logger.LogInformation(EventIds.StoreRecordOfSalePublishedEventInAzureTable.ToEventId(), "Storing the received Record of sale published event in azure table.");
            await _azureTableReaderWriter.UpsertEntity(correlationId, Constants.RecordOfSaleEventTableName, recordOfSaleEvent);

            _logger.LogInformation(EventIds.UploadRecordOfSalePublishedEventInAzureBlob.ToEventId(), "Uploading the received Record of sale published event in blob storage.");
            await _azureBlobEventWriter.UploadEvent(recordOfSaleEventJson.ToString(), Constants.RecordOfSaleEventContainerName, correlationId + '/' + eventId + Constants.RecordOfSaleEventFileExtension);
            _logger.LogInformation(EventIds.UploadedRecordOfSalePublishedEventInAzureBlob.ToEventId(), "Record of sale published event is uploaded in blob storage successfully.");

            _logger.LogInformation(EventIds.AddMessageToAzureQueue.ToEventId(), "Adding the received Record of sale published event in queue storage.");
            await _azureQueueHelper.AddMessage(recordOfSaleEventJson);
            _logger.LogInformation(EventIds.AddedMessageToAzureQueue.ToEventId(), "Record of sale published event is added in queue storage successfully.");

            return new OkObjectResult(StatusCodes.Status200OK);
        }

        [HttpOptions]
        [Route("/webhook/licenceupdatedpublishedeventreceived")]
        [Authorize(Policy = "LicenceUpdatedWebhookCaller")]
        [NonAction]
        public IActionResult LicenceUpdatedPublishedEventReceivedOption()
        {
            var webhookRequestOrigin = HttpContext.Request.Headers["WebHook-Request-Origin"].FirstOrDefault();

            _logger.LogInformation(EventIds.LicenceUpdatedEventOptionsCallStarted.ToEventId(), "Started processing the Options request for the Licence updated event for webhook. | WebHook-Request-Origin : {webhookRequestOrigin}", webhookRequestOrigin);

            HttpContext.Response.Headers.Append("WebHook-Allowed-Rate", "*");
            HttpContext.Response.Headers.Append("WebHook-Allowed-Origin", webhookRequestOrigin);

            _logger.LogInformation(EventIds.LicenceUpdatedEventOptionsCallCompleted.ToEventId(), "Completed processing the Options request for the Licence updated event for webhook. | WebHook-Request-Origin : {webhookRequestOrigin}", webhookRequestOrigin);

            return new OkObjectResult(StatusCodes.Status200OK);
        }

        [HttpPost]
        [Route("/webhook/licenceupdatedpublishedeventreceived")]
        [Authorize(Policy = "LicenceUpdatedWebhookCaller")]
        [NonAction]
        public virtual async Task<IActionResult> LicenceUpdatedPublishedEventReceived([FromBody] JObject licenceUpdatedEventJson)
        {
            _logger.LogInformation(EventIds.LicenceUpdatedEventPublishedEventReceived.ToEventId(), "ERP Facade webhook has received new licence updated publish event from EES.");

            var correlationId = licenceUpdatedEventJson.SelectToken(Constants.CorrelationIdKey)?.Value<string>();

            if (string.IsNullOrEmpty(correlationId))
            {
                _logger.LogWarning(EventIds.CorrelationIdMissingInLicenceUpdatedEvent.ToEventId(), "CorrelationId is missing in Licence updated published event.");
                return new BadRequestObjectResult(StatusCodes.Status400BadRequest);
            }

            LicenseUpdatedEventEntity licenceUpdatedEventEntity = new()
            {
                RowKey = Guid.NewGuid().ToString(),
                PartitionKey = Guid.NewGuid().ToString(),
                Timestamp = DateTime.UtcNow,
                CorrelationId = correlationId,
                Status = Status.Incomplete.ToString()
            };

            _logger.LogInformation(EventIds.StoreLicenceUpdatedPublishedEventInAzureTable.ToEventId(), "Storing the received Licence updated published event in azure table.");
            await _azureTableReaderWriter.UpsertEntity(correlationId, Constants.LicenceUpdatedEventTableName, licenceUpdatedEventEntity);

            _logger.LogInformation(EventIds.UploadLicenceUpdatedPublishedEventInAzureBlob.ToEventId(), "Uploading the received Licence updated  published event in blob storage.");
            await _azureBlobEventWriter.UploadEvent(licenceUpdatedEventJson.ToString(), Constants.LicenceUpdatedEventContainerName, correlationId + '/' + Constants.LicenceUpdatedEventFileName);
            _logger.LogInformation(EventIds.UploadedLicenceUpdatedPublishedEventInAzureBlob.ToEventId(), "Licence updated  published event is uploaded in blob storage successfully.");

            var sapPayload = _licenceUpdatedSapMessageBuilder.BuildLicenceUpdatedSapMessageXml(JsonConvert.DeserializeObject<LicenceUpdatedEventPayLoad>(licenceUpdatedEventJson.ToString()), correlationId);

            _logger.LogInformation(EventIds.UploadLicenceUpdatedSapXmlPayloadInAzureBlob.ToEventId(), "Uploading the SAP xml payload for licence updated event in blob storage.");
            await _azureBlobEventWriter.UploadEvent(sapPayload.ToIndentedString(), Constants.LicenceUpdatedEventContainerName, correlationId + '/' + Constants.SapXmlPayloadFileName);
            _logger.LogInformation(EventIds.UploadedLicenceUpdatedSapXmlPayloadInAzureBlob.ToEventId(), "SAP xml payload for licence updated event is uploaded in blob storage successfully.");

            var response = await _sapClient.PostEventData(sapPayload, _sapConfig.Value.SapEndpointForRecordOfSale, _sapConfig.Value.SapServiceOperationForRecordOfSale, _sapConfig.Value.SapUsernameForRecordOfSale, _sapConfig.Value.SapPasswordForRecordOfSale);

            if (!response.IsSuccessStatusCode)
            {
                throw new ERPFacadeException(EventIds.ErrorOccurredInSapForLicenceUpdatedPublishedEvent.ToEventId(), $"An error occurred while sending licence updated event data to SAP. | {response.StatusCode}");
            }

            _logger.LogInformation(EventIds.LicenceUpdatedPublishedEventUpdatePushedToSap.ToEventId(), "The licence updated event data has been sent to SAP successfully. | {StatusCode}", response.StatusCode);

            await _azureTableReaderWriter.UpdateEntity(correlationId, Constants.LicenceUpdatedEventTableName, new[] { new KeyValuePair<string, string>("Status", Status.Complete.ToString()) });

            return new OkObjectResult(StatusCodes.Status200OK);
        }
    }
}
