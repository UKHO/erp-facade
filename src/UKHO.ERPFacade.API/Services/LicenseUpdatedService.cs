using Azure.Data.Tables;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Xml;
using UKHO.ERPFacade.API.Helpers;
using UKHO.ERPFacade.Common.Configuration;
using UKHO.ERPFacade.Common.Enums;
using UKHO.ERPFacade.Common.Exceptions;
using UKHO.ERPFacade.Common.HttpClients;
using UKHO.ERPFacade.Common.IO;
using UKHO.ERPFacade.Common.IO.Azure;
using UKHO.ERPFacade.Common.Logging;
using UKHO.ERPFacade.Common.Models;
using UKHO.ERPFacade.Common.Models.TableEntities;

namespace UKHO.ERPFacade.API.Services
{
    public class LicenseUpdatedService : ILicenseUpdatedService
    {
        private const string LicenceUpdatedContainerName = "licenceupdatedblobs";
        private const string LicenceUpdatedEventFileName = "LicenceUpdatedEvent.json";
        private const string SapXmlPayloadFileName = "SapXmlPayload.xml";
        private const string LicenceUpdateTableName = "licenceupdatedevents";
        private readonly ILogger<AzureTableReaderWriter> _logger;
        private readonly IAzureTableReaderWriter _azureTableReaderWriter;
        private readonly IAzureBlobEventWriter _azureBlobEventWriter;
        private readonly ISapClient _sapClient;
        private readonly IOptions<SapConfiguration> _sapConfig;
        private readonly ILicenceUpdatedSapMessageBuilder _licenceUpdatedSapMessageBuilder;

        public LicenseUpdatedService(ILogger<AzureTableReaderWriter> logger,
            IAzureTableReaderWriter azureTableReaderWriter,
            IAzureBlobEventWriter azureBlobEventWriter,
            ISapClient sapClient,
            IOptions<SapConfiguration> sapConfig,
            ILicenceUpdatedSapMessageBuilder licenceUpdatedSapMessageBuilder)
        {
            _logger = logger;
            _azureTableReaderWriter = azureTableReaderWriter;
            _azureBlobEventWriter = azureBlobEventWriter;
            _sapClient = sapClient;
            _sapConfig = sapConfig ?? throw new ArgumentNullException(nameof(sapConfig));
            _licenceUpdatedSapMessageBuilder = licenceUpdatedSapMessageBuilder;
        }

        public async Task ProcessLicenseUpdatedPublishedEvent(string correlationId, JObject licenceUpdatedEventJson)
        {
            _logger.LogInformation(EventIds.StoreLicenceUpdatedPublishedEventInAzureTable.ToEventId(), "Storing the received Licence updated published event in azure table.");

            LicenseUpdatedEventEntity licenceUpdatedEventEntity = new()
            {
                RowKey = Guid.NewGuid().ToString(),
                PartitionKey = Guid.NewGuid().ToString(),
                Timestamp = DateTime.UtcNow,
                CorrelationId = correlationId,
                Status = Statuses.Incomplete.ToString()
            };

            await _azureTableReaderWriter.UpsertEntity(correlationId, LicenceUpdateTableName, licenceUpdatedEventEntity);

            _logger.LogInformation(EventIds.UploadLicenceUpdatedPublishedEventInAzureBlob.ToEventId(), "Uploading the received Licence updated  published event in blob storage.");
            await _azureBlobEventWriter.UploadEvent(licenceUpdatedEventJson.ToString(), LicenceUpdatedContainerName, correlationId + '/' + LicenceUpdatedEventFileName);
            _logger.LogInformation(EventIds.UploadedLicenceUpdatedPublishedEventInAzureBlob.ToEventId(), "Licence updated  published event is uploaded in blob storage successfully.");

            XmlDocument sapPayload = _licenceUpdatedSapMessageBuilder.BuildLicenceUpdatedSapMessageXml(JsonConvert.DeserializeObject<LicenceUpdatedEventPayLoad>(licenceUpdatedEventJson.ToString()), correlationId);

            _logger.LogInformation(EventIds.UploadLicenceUpdatedSapXmlPayloadInAzureBlob.ToEventId(), "Uploading the SAP xml payload for licence updated event in blob storage.");
            await _azureBlobEventWriter.UploadEvent(sapPayload.ToIndentedString(), LicenceUpdatedContainerName, correlationId + '/' + SapXmlPayloadFileName);
            _logger.LogInformation(EventIds.UploadedLicenceUpdatedSapXmlPayloadInAzureBlob.ToEventId(), "SAP xml payload for licence updated event is uploaded in blob storage successfully.");

            HttpResponseMessage response = await _sapClient.PostEventData(sapPayload, _sapConfig.Value.SapEndpointForRecordOfSale, _sapConfig.Value.SapServiceOperationForRecordOfSale, _sapConfig.Value.SapUsernameForRecordOfSale, _sapConfig.Value.SapPasswordForRecordOfSale);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError(EventIds.ErrorOccurredInSapForLicenceUpdatedPublishedEvent.ToEventId(), "An error occurred while sending licence updated event data to SAP. | {StatusCode}", response.StatusCode);
                throw new ERPFacadeException(EventIds.ErrorOccurredInSapForLicenceUpdatedPublishedEvent.ToEventId());
            }

            _logger.LogInformation(EventIds.LicenceUpdatedPublishedEventUpdatePushedToSap.ToEventId(), "The licence updated event data has been sent to SAP successfully. | {StatusCode}", response.StatusCode);

            var licenseUpdatedEventEntitiesToUpdate = new[]
                        { new KeyValuePair<string, string>("Status", Statuses.Complete.ToString()) };
            await _azureTableReaderWriter.UpdateEntity(correlationId, LicenceUpdateTableName, licenseUpdatedEventEntitiesToUpdate);
        }
    }
}
