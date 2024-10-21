using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using UKHO.ERPFacade.API.XmlTransformers;
using UKHO.ERPFacade.Common.Configuration;
using UKHO.ERPFacade.Common.Constants;
using UKHO.ERPFacade.Common.Exceptions;
using UKHO.ERPFacade.Common.HttpClients;
using UKHO.ERPFacade.Common.IO;
using UKHO.ERPFacade.Common.IO.Azure;
using UKHO.ERPFacade.Common.Logging;
using UKHO.ERPFacade.Common.Models.CloudEvents;
using UKHO.ERPFacade.Common.Models.CloudEvents.S57;
using UKHO.ERPFacade.Common.Models.TableEntities;

namespace UKHO.ERPFacade.API.Handlers
{
    public class S57EventHandler : IEventHandler
    {
        private readonly ILogger<S57EventHandler> _logger;
        private readonly IBaseXmlTransformer _baseXmlTransformer;
        private readonly IAzureTableHelper _azureTableHelper;
        private readonly IAzureBlobHelper _azureBlobHelper;
        private readonly ISapClient _sapClient;
        private readonly IOptions<SapConfiguration> _sapConfig;

        public S57EventHandler([FromKeyedServices("S57XmlTransformer")] IBaseXmlTransformer baseXmlTransformer,
                                                                        IAzureTableHelper azureTableReaderWriter,
                                                                        IAzureBlobHelper azureBlobEventWriter,
                                                                        ILogger<S57EventHandler> logger,
                                                                        ISapClient sapClient,
                                                                        IOptions<SapConfiguration> sapConfig
                                                                        )
        {
            _baseXmlTransformer = baseXmlTransformer;
            _azureTableHelper = azureTableReaderWriter;
            _azureBlobHelper = azureBlobEventWriter;
            _logger = logger;
            _sapClient = sapClient;
            _sapConfig = sapConfig;
        }

        public async Task ProcessEventAsync(BaseCloudEvent baseCloudEvent)
        {
            S57EventData s57EventData = JsonConvert.DeserializeObject<S57EventData>(baseCloudEvent.Data.ToString());

            EventEntity eventEntity = new()
            {
                RowKey = s57EventData.CorrelationId,
                PartitionKey = "S57",
                Timestamp = DateTime.UtcNow,
                RequestDateTime = null
            };

            await _azureTableHelper.UpsertEntity(eventEntity);

            await _azureBlobHelper.UploadEvent(JsonConvert.SerializeObject(baseCloudEvent, Formatting.Indented), s57EventData.CorrelationId, Constants.S57EncEventFileName);

            var sapPayload = _baseXmlTransformer.BuildXmlPayload(s57EventData, Constants.S57SapXmlTemplatePath);

            await _azureBlobHelper.UploadEvent(sapPayload.ToIndentedString(), s57EventData.CorrelationId, Constants.SapXmlPayloadFileName);

            var response = await _sapClient.PostEventData(sapPayload, _sapConfig.Value.SapEndpointForEncEvent, _sapConfig.Value.SapServiceOperationForEncEvent, _sapConfig.Value.SapUsernameForEncEvent, _sapConfig.Value.SapPasswordForEncEvent);

            if (!response.IsSuccessStatusCode)
            {
                throw new ERPFacadeException(EventIds.RequestToSapFailed.ToEventId(), $"An error occurred while sending a request to SAP. | {response.StatusCode}");
            }

            _logger.LogInformation(EventIds.EncUpdateSentToSap.ToEventId(), "ENC update has been sent to SAP successfully. | {StatusCode}", response.StatusCode);

            await _azureTableHelper.UpdateEntity(eventEntity.PartitionKey, eventEntity.RowKey, new[] { new KeyValuePair<string, DateTime>("RequestDateTime", DateTime.UtcNow) });
        }
    }
}
