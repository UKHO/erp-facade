using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UKHO.ERPFacade.API.Dispatcher;
using UKHO.ERPFacade.API.SapMessageBuilders;
using UKHO.ERPFacade.Common.Configuration;
using UKHO.ERPFacade.Common.Constants;
using UKHO.ERPFacade.Common.Exceptions;
using UKHO.ERPFacade.Common.Extensions;
using UKHO.ERPFacade.Common.HttpClients;
using UKHO.ERPFacade.Common.Logging;
using UKHO.ERPFacade.Common.Models;
using UKHO.ERPFacade.Common.Models.CloudEvents;
using UKHO.ERPFacade.Common.Models.TableEntities;
using UKHO.ERPFacade.Common.Operations.IO.Azure;
using Status = UKHO.ERPFacade.Common.Enums.Status;

namespace UKHO.ERPFacade.API.Controllers
{
    [ApiController]
    [Authorize]
    public class WebhookController : BaseController<WebhookController>
    {
        private readonly ILogger<WebhookController> _logger;
        private readonly IEventDispatcher _eventDispatcher;
        private readonly IAzureTableReaderWriter _azureTableReaderWriter;
        private readonly IAzureBlobReaderWriter _azureBlobReaderWriter;
        private readonly IAzureQueueReaderWriter _azureQueueReaderWriter;
        private readonly ILicenceUpdatedSapMessageBuilder _licenceUpdatedSapMessageBuilder;
        private readonly ISapClient _sapClient;
        private readonly IOptions<SapConfiguration> _sapConfig;

        public WebhookController(IHttpContextAccessor contextAccessor,
                                 ILogger<WebhookController> logger,
                                 IEventDispatcher eventDispatcher,
                                 IAzureTableReaderWriter azureTableReaderWriter,
                                 IAzureBlobReaderWriter azureBlobReaderWriter,
                                 IAzureQueueReaderWriter azureQueueReaderWriter,
                                 ILicenceUpdatedSapMessageBuilder licenceUpdatedSapMessageBuilder,
                                 ISapClient sapClient,
                                 IOptions<SapConfiguration> sapConfig)
        : base(contextAccessor)
        {
            _logger = logger;
            _eventDispatcher = eventDispatcher;
            _azureTableReaderWriter = azureTableReaderWriter;
            _azureBlobReaderWriter = azureBlobReaderWriter;
            _azureQueueReaderWriter = azureQueueReaderWriter;
            _licenceUpdatedSapMessageBuilder = licenceUpdatedSapMessageBuilder;
            _sapClient = sapClient;
            _sapConfig = sapConfig ?? throw new ArgumentNullException(nameof(sapConfig));
        }

        [HttpOptions]
        [Route("/webhook/newenccontentpublishedeventreceived")]
        [Route("api/v2/webhook")]
        [Authorize(Policy = "EncContentPublishedWebhookCaller")]
        public IActionResult ReceiveEvents()
        {
            var webhookRequestOrigin = HttpContext.Request.Headers["WebHook-Request-Origin"].FirstOrDefault();

            HttpContext.Response.Headers.Append("WebHook-Allowed-Rate", "*");
            HttpContext.Response.Headers.Append("WebHook-Allowed-Origin", webhookRequestOrigin);

            _logger.LogInformation(EventIds.ErpFacadeWebhookOptionsEndPointRequested.ToEventId(), "ERP facade webhook options endpoint requested.");

            return new OkObjectResult(StatusCodes.Status200OK);
        }

        [HttpPost]
        [Route("/webhook/newenccontentpublishedeventreceived")]
        [Route("api/v2/webhook")]
        [Authorize(Policy = "EncContentPublishedWebhookCaller")]
        public virtual async Task<IActionResult> ReceiveEventsAsync([FromBody] JObject cloudEvent)
        {
            _logger.LogInformation(EventIds.NewCloudEventReceived.ToEventId(), "ERP facade received new cloud event.");

            var baseCloudEvent = JsonConvert.DeserializeObject<BaseCloudEvent>(cloudEvent.ToString());

            var isEventDispatched = await _eventDispatcher.DispatchEventAsync(baseCloudEvent);

            if (!isEventDispatched)
            {
                var error = new List<Error>
                {
                    new Error()
                    {
                        Source = ErrorDetails.Source,
                        Description = ErrorDetails.UnknownEventTypeMessage
                    }
                };
                return BuildBadRequestErrorResponse(error);
            }
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

            var correlationId = recordOfSaleEventJson.SelectToken(JsonFields.CorrelationIdKey)?.Value<string>();
            var eventId = recordOfSaleEventJson.SelectToken(JsonFields.EventIdKey)?.Value<string>();

            if (string.IsNullOrEmpty(correlationId))
            {
                _logger.LogWarning(EventIds.CorrelationIdMissingInRecordOfSaleEvent.ToEventId(), "CorrelationId is missing in Record of Sale published event.");
                return new BadRequestObjectResult(StatusCodes.Status400BadRequest);
            }

            EventEntity eventEntity = new()
            {
                RowKey = correlationId,
                PartitionKey = PartitionKeys.ROSPartitionKey,
                Timestamp = DateTime.UtcNow,
                Status = Status.Incomplete.ToString()
            };

            _logger.LogInformation(EventIds.StoreRecordOfSalePublishedEventInAzureTable.ToEventId(), "Storing the received Record of sale published event in azure table.");
            await _azureTableReaderWriter.UpsertEntityAsync(eventEntity);

            _logger.LogInformation(EventIds.UploadRecordOfSalePublishedEventInAzureBlob.ToEventId(), "Uploading the received Record of sale published event in blob storage.");
            await _azureBlobReaderWriter.UploadEventAsync(recordOfSaleEventJson.ToString(), AzureStorage.RecordOfSaleEventContainerName, correlationId + '/' + eventId + EventPayloadFiles.RecordOfSaleEventFileExtension);
            _logger.LogInformation(EventIds.UploadedRecordOfSalePublishedEventInAzureBlob.ToEventId(), "Record of sale published event is uploaded in blob storage successfully.");

            _logger.LogInformation(EventIds.AddMessageToAzureQueue.ToEventId(), "Adding the received Record of sale published event in queue storage.");
            await _azureQueueReaderWriter.AddMessageAsync(recordOfSaleEventJson);
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

            var correlationId = licenceUpdatedEventJson.SelectToken(JsonFields.CorrelationIdKey)?.Value<string>();

            if (string.IsNullOrEmpty(correlationId))
            {
                _logger.LogWarning(EventIds.CorrelationIdMissingInLicenceUpdatedEvent.ToEventId(), "CorrelationId is missing in Licence updated published event.");
                return new BadRequestObjectResult(StatusCodes.Status400BadRequest);
            }

            EventEntity eventEntity = new()
            {
                RowKey = correlationId,
                PartitionKey = PartitionKeys.LUPPartitionKey,
                Timestamp = DateTime.UtcNow,
                Status = Status.Incomplete.ToString()
            };

            _logger.LogInformation(EventIds.StoreLicenceUpdatedPublishedEventInAzureTable.ToEventId(), "Storing the received Licence updated published event in azure table.");
            await _azureTableReaderWriter.UpsertEntityAsync(eventEntity);

            _logger.LogInformation(EventIds.UploadLicenceUpdatedPublishedEventInAzureBlob.ToEventId(), "Uploading the received Licence updated  published event in blob storage.");
            await _azureBlobReaderWriter.UploadEventAsync(licenceUpdatedEventJson.ToString(), AzureStorage.LicenceUpdatedEventContainerName, correlationId + '/' + EventPayloadFiles.LicenceUpdatedEventFileName);
            _logger.LogInformation(EventIds.UploadedLicenceUpdatedPublishedEventInAzureBlob.ToEventId(), "Licence updated  published event is uploaded in blob storage successfully.");

            var sapPayload = _licenceUpdatedSapMessageBuilder.BuildLicenceUpdatedSapMessageXml(JsonConvert.DeserializeObject<LicenceUpdatedEventPayLoad>(licenceUpdatedEventJson.ToString()), correlationId);

            _logger.LogInformation(EventIds.UploadLicenceUpdatedSapXmlPayloadInAzureBlob.ToEventId(), "Uploading the SAP xml payload for licence updated event in blob storage.");
            await _azureBlobReaderWriter.UploadEventAsync(sapPayload.ToIndentedString(), AzureStorage.LicenceUpdatedEventContainerName, correlationId + '/' + EventPayloadFiles.SapXmlPayloadFileName);
            _logger.LogInformation(EventIds.UploadedLicenceUpdatedSapXmlPayloadInAzureBlob.ToEventId(), "SAP xml payload for licence updated event is uploaded in blob storage successfully.");

            var response = await _sapClient.PostEventData(sapPayload, _sapConfig.Value.SapEndpointForRecordOfSale, _sapConfig.Value.SapServiceOperationForRecordOfSale, _sapConfig.Value.SapUsernameForRecordOfSale, _sapConfig.Value.SapPasswordForRecordOfSale);

            if (!response.IsSuccessStatusCode)
            {
                throw new ERPFacadeException(EventIds.ErrorOccurredInSapForLicenceUpdatedPublishedEvent.ToEventId(), $"An error occurred while sending licence updated event data to SAP. | {response.StatusCode}");
            }

            _logger.LogInformation(EventIds.LicenceUpdatedPublishedEventUpdatePushedToSap.ToEventId(), "The licence updated event data has been sent to SAP successfully. | {StatusCode}", response.StatusCode);

            await _azureTableReaderWriter.UpdateEntityAsync(PartitionKeys.LUPPartitionKey, correlationId, new Dictionary<string, object> { { "Status", Status.Complete.ToString() } });

            return new OkObjectResult(StatusCodes.Status200OK);
        }

    }
}
