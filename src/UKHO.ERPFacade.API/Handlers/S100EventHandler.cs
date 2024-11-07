﻿using Newtonsoft.Json;
using UKHO.ERPFacade.API.XmlTransformers;
using UKHO.ERPFacade.Common.Constants;
using UKHO.ERPFacade.Common.Enums;
using UKHO.ERPFacade.Common.Extensions;
using UKHO.ERPFacade.Common.Logging;
using UKHO.ERPFacade.Common.Models.CloudEvents;
using UKHO.ERPFacade.Common.Models.CloudEvents.S100Event;
using UKHO.ERPFacade.Common.Models.TableEntities;
using UKHO.ERPFacade.Common.Operations.IO.Azure;

namespace UKHO.ERPFacade.API.Handlers
{
    public class S100EventHandler : IEventHandler
    {
        public string EventType => EventTypes.S100EventType;

        private readonly ILogger<S100EventHandler> _logger;
        private readonly IAzureTableReaderWriter _azureTableReaderWriter;
        private readonly IAzureBlobReaderWriter _azureBlobReaderWriter;
        private readonly IBaseXmlTransformer _baseXmlTransformer;

        public S100EventHandler(ILogger<S100EventHandler> logger, IAzureTableReaderWriter azureTableReaderWriter, IAzureBlobReaderWriter azureBlobReaderWriter, [FromKeyedServices("S100XmlTransformer")] IBaseXmlTransformer baseXmlTransformer)
        {
            _logger = logger;
            _azureTableReaderWriter = azureTableReaderWriter;
            _azureBlobReaderWriter = azureBlobReaderWriter;
            _baseXmlTransformer = baseXmlTransformer;
        }

        public async Task ProcessEventAsync(BaseCloudEvent baseCloudEvent)
        {
            _logger.LogInformation(EventIds.S100EventProcessingStarted.ToEventId(), "S-100 data content published event processing started.");

            S100EventData s100EventData = JsonConvert.DeserializeObject<S100EventData>(baseCloudEvent.Data.ToString());

            EventEntity eventEntity = new()
            {
                RowKey = s100EventData.CorrelationId,
                PartitionKey = PartitionKeys.S100PartitionKey,
                Timestamp = DateTime.UtcNow,
                RequestDateTime = null,
                ResponseDateTime = null,
                Status = Status.Incomplete.ToString()
            };

            await _azureTableReaderWriter.UpsertEntityAsync(eventEntity);

            _logger.LogInformation(EventIds.S100EventEntryAddedInAzureTable.ToEventId(), "S-100 data content published event entry added in azure table.");

            await _azureBlobReaderWriter.UploadEventAsync(JsonConvert.SerializeObject(baseCloudEvent, Formatting.Indented), s100EventData.CorrelationId, EventPayloadFiles.S100DataEventFileName);

            _logger.LogInformation(EventIds.S100EventJsonStoredInAzureBlobContainer.ToEventId(), "S-100 data content published event json payload is stored in azure blob container.");

            var sapPayload = _baseXmlTransformer.BuildXmlPayload(s100EventData, XmlTemplateInfo.S100SapXmlTemplatePath);

            await _azureBlobReaderWriter.UploadEventAsync(sapPayload.ToIndentedString(), s100EventData.CorrelationId, EventPayloadFiles.SapXmlPayloadFileName);

            _logger.LogInformation(EventIds.S100EventXMLStoredInAzureBlobContainer.ToEventId(), "S-100 data content published event xml payload is stored in azure blob container.");
        }
    }
}
