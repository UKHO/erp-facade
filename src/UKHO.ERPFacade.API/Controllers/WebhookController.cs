using System.Xml;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UKHO.ERPFacade.API.Helpers;
using UKHO.ERPFacade.Common.Configuration;
using UKHO.ERPFacade.Common.Exceptions;
using UKHO.ERPFacade.Common.HttpClients;
using UKHO.ERPFacade.Common.IO;
using UKHO.ERPFacade.Common.IO.Azure;
using UKHO.ERPFacade.Common.Logging;
using UKHO.ERPFacade.Common.Models;

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
        private readonly ISapMessageBuilder _sapMessageBuilder;
        private readonly IOptions<SapConfiguration> _sapConfig;

        private const string CorrelationIdKey = "data.correlationId";
        private const string EncEventFileName = "EncPublishingEvent.json";
        private const string SapXmlPayloadFileName = "SapXmlPayload.xml";
        private const string LicenceUpdatedContainerName = "licenceupdatedblobs";
        private const string LicenceUpdatedFileName = "LicenceUpdateEvent.json";

        public WebhookController(IHttpContextAccessor contextAccessor,
                                 ILogger<WebhookController> logger,
                                 IAzureTableReaderWriter azureTableReaderWriter,
                                 IAzureBlobEventWriter azureBlobEventWriter,
                                 ISapClient sapClient,
                                 ISapMessageBuilder sapMessageBuilder,
                                 IOptions<SapConfiguration> sapConfig)
        : base(contextAccessor)
        {
            _logger = logger;
            _azureTableReaderWriter = azureTableReaderWriter;
            _azureBlobEventWriter = azureBlobEventWriter;
            _sapClient = sapClient;
            _sapMessageBuilder = sapMessageBuilder;
            _sapConfig = sapConfig ?? throw new ArgumentNullException(nameof(sapConfig));
        }

        [HttpOptions]
        [Route("/webhook/newenccontentpublishedeventreceived")]
        [Authorize(Policy = "EncContentPublishedWebhookCaller")]
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
        [Authorize(Policy = "EncContentPublishedWebhookCaller")]
        public virtual async Task<IActionResult> NewEncContentPublishedEventReceived([FromBody] JObject encEventJson)
        {
            _logger.LogInformation(EventIds.NewEncContentPublishedEventReceived.ToEventId(), "ERP Facade webhook has received new enccontentpublished event from EES.");

            string correlationId = encEventJson.SelectToken(CorrelationIdKey)?.Value<string>();

            if (string.IsNullOrEmpty(correlationId))
            {
                _logger.LogWarning(EventIds.CorrelationIdMissingInEvent.ToEventId(), "CorrelationId is missing in ENC content published event.");
                return new BadRequestObjectResult(StatusCodes.Status400BadRequest);
            }

            _logger.LogInformation(EventIds.StoreEncContentPublishedEventInAzureTable.ToEventId(), "Storing the received ENC content published event in azure table.");

            await _azureTableReaderWriter.UpsertEntity(correlationId);

            _logger.LogInformation(EventIds.UploadEncContentPublishedEventInAzureBlob.ToEventId(), "Uploading the received ENC content published event in blob storage.");

            await _azureBlobEventWriter.UploadEvent(encEventJson.ToString(), correlationId, EncEventFileName);

            _logger.LogInformation(EventIds.UploadedEncContentPublishedEventInAzureBlob.ToEventId(), "ENC content published event is uploaded in blob storage successfully.");

            XmlDocument sapPayload = _sapMessageBuilder.BuildSapMessageXml(JsonConvert.DeserializeObject<EncEventPayload>(encEventJson.ToString()), correlationId);

            _logger.LogInformation(EventIds.UploadSapXmlPayloadInAzureBlobStarted.ToEventId(), "Uploading the SAP xml payload in blob storage.");

            await _azureBlobEventWriter.UploadEvent(sapPayload.ToIndentedString(), correlationId, SapXmlPayloadFileName);

            _logger.LogInformation(EventIds.UploadSapXmlPayloadInAzureBlobCompleted.ToEventId(), "SAP xml payload is uploaded in blob storage successfully.");

            HttpResponseMessage response = await _sapClient.PostEventData(sapPayload, _sapConfig.Value.SapServiceOperation);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError(EventIds.ErrorOccuredInSap.ToEventId(), "An error occured while processing your request in SAP. | {StatusCode}", response.StatusCode);
                throw new ERPFacadeException(EventIds.ErrorOccuredInSap.ToEventId());
            }
            _logger.LogInformation(EventIds.EncUpdatePushedToSap.ToEventId(), "ENC update has been sent to SAP successfully. | {StatusCode}", response.StatusCode);

            await _azureTableReaderWriter.UpdateRequestTimeEntity(correlationId);

            return new OkObjectResult(StatusCodes.Status200OK);

        }

        [HttpOptions]
        [Route("/webhook/licenceupdatedpublishedeventreceived")]
        [Authorize(Policy = "LicenceUpdatedWebhookCaller")]
        public IActionResult LicenceUpdatedPublishedEventReceivedOption()
        {
            var webhookRequestOrigin = HttpContext.Request.Headers["WebHook-Request-Origin"].FirstOrDefault();

            _logger.LogInformation(EventIds.LicenceUpdatedEventOptionsCallStarted.ToEventId(), "Started processing the Options request for the Licence updated event for webhook. | WebHook-Request-Origin : {webhookRequestOrigin}", webhookRequestOrigin);

            HttpContext.Response.Headers.Add("WebHook-Allowed-Rate", "*");
            HttpContext.Response.Headers.Add("WebHook-Allowed-Origin", webhookRequestOrigin);

            _logger.LogInformation(EventIds.LicenceUpdatedEventOptionsCallCompleted.ToEventId(), "Completed processing the Options request for the Licence updated event for webhook. | WebHook-Request-Origin : {webhookRequestOrigin}", webhookRequestOrigin);

            return new OkObjectResult(StatusCodes.Status200OK);
        }

        [HttpPost]
        [Route("/webhook/licenceupdatedpublishedeventreceived")]
        [Authorize(Policy = "LicenceUpdatedWebhookCaller")]
        public virtual async Task<IActionResult> LicenceUpdatedPublishedEventReceived([FromBody] JObject licenceUpdatedEventJson)
        {
            _logger.LogInformation(EventIds.LicenceUpdatedEventPublishedEventReceived.ToEventId(), "ERP Facade webhook has received new licence updated publish event from EES.");

            string correlationId = licenceUpdatedEventJson.SelectToken(CorrelationIdKey)?.Value<string>();

            if (string.IsNullOrEmpty(correlationId))
            {
                _logger.LogWarning(EventIds.CorrelationIdMissingInLicenceUpdatedEvent.ToEventId(), "CorrelationId is missing in Licence updated published event.");
                return new BadRequestObjectResult(StatusCodes.Status400BadRequest);
            }

            _logger.LogInformation(EventIds.StoreLicenceUpdatedPublishedEventInAzureTable.ToEventId(), "Storing the received Licence updated published event in azure table.");

            await _azureTableReaderWriter.UpsertLicenceUpdatedEntity(correlationId);

            _logger.LogInformation(EventIds.UploadLicenceUpdatedPublishedEventInAzureBlob.ToEventId(), "Uploading the received Licence updated  published event in blob storage.");

            await _azureBlobEventWriter.UploadEvent(licenceUpdatedEventJson.ToString(), LicenceUpdatedContainerName, correlationId + '/' + LicenceUpdatedFileName);

            _logger.LogInformation(EventIds.UploadedLicenceUpdatedPublishedEventInAzureBlob.ToEventId(), "Licence updated  published event is uploaded in blob storage successfully.");
           
            return new OkObjectResult(StatusCodes.Status200OK);

        }
    }
}
