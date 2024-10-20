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
using UKHO.ERPFacade.Common.Models.CloudEvents.S57;
using UKHO.ERPFacade.Common.Models.TableEntities;

namespace UKHO.ERPFacade.API.Handlers
{
    public class S57EventHandler : IEventHandler<S57Event>
    {
        private readonly ILogger<S57EventHandler> _logger;
        private readonly IBaseXmlTransformer _baseXmlTransformer;
        private readonly IAzureTableHelper _azureTableReaderWriter;
        private readonly IAzureBlobHelper _azureBlobEventWriter;
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
            _azureTableReaderWriter = azureTableReaderWriter;
            _azureBlobEventWriter = azureBlobEventWriter;
            _logger = logger;
            _sapClient = sapClient;
            _sapConfig = sapConfig;
        }

        public async Task ProcessEventAsync(S57Event s57EventPayload)
        {
            EventEntity eventEntity = new()
            {
                RowKey = s57EventPayload.Data.CorrelationId,
                PartitionKey = "S57",
                Timestamp = DateTime.UtcNow,
                RequestDateTime = null
            };

            await _azureTableReaderWriter.UpsertEntity(s57EventPayload.Data.CorrelationId, eventEntity);
            await _azureBlobEventWriter.UploadEvent(JsonConvert.SerializeObject(s57EventPayload, Formatting.Indented), s57EventPayload.Data.CorrelationId, Constants.S57EncEventFileName);

            var sapPayload = _baseXmlTransformer.BuildXmlPayload(s57EventPayload.Data, Constants.S57SapXmlTemplatePath);

            await _azureBlobEventWriter.UploadEvent(sapPayload.ToIndentedString(), s57EventPayload.Data.CorrelationId, Constants.SapXmlPayloadFileName);

            var response = await _sapClient.PostEventData(sapPayload, _sapConfig.Value.SapEndpointForEncEvent, _sapConfig.Value.SapServiceOperationForEncEvent, _sapConfig.Value.SapUsernameForEncEvent, _sapConfig.Value.SapPasswordForEncEvent);

            if (!response.IsSuccessStatusCode)
            {
                throw new ERPFacadeException(EventIds.RequestToSapFailed.ToEventId(), $"An error occurred while sending a request to SAP. | {response.StatusCode}");
            }

            _logger.LogInformation(EventIds.EncUpdateSentToSap.ToEventId(), "ENC update has been sent to SAP successfully. | {StatusCode}", response.StatusCode);

            await _azureTableReaderWriter.UpdateEntity(eventEntity.CorrelationId, Constants.S57EventTableName, new[] { new KeyValuePair<string, DateTime>("RequestDateTime", DateTime.UtcNow) });
        }
    }
}
