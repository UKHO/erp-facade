using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Logging;

namespace UKHO.ERPFacade.Common.Logging
{
    /// <summary>
    /// Event Ids
    /// </summary>
    public enum EventIds
    {
        /// <summary>
        /// 940001 - An unhandled exception occurred while processing the request.
        /// </summary>
        UnhandledException = 940001,

        /// <summary>
        /// 940002 - Sap Xml Template Not Found
        /// </summary>
        SapXmlTemplateNotFoundException = 940002,

        /// <summary>
        /// 940003 - ERP facade OPTIONS endpoint requested.
        /// </summary>
        ErpFacadeWebhookOptionsEndPointRequested = 940003,

        /// <summary>
        /// 940004 - New cloud event received in ERP facade.
        /// </summary>
        NewCloudEventReceived = 940004,

        /// <summary>
        /// 940005 - Invalid event received in ERP facade.
        /// </summary>
        InvalidEventTypeReceived = 940005,

        /// <summary>
        /// 940006 - Aio cell configuration is missing.
        /// </summary>
        AioConfigurationMissingException = 940006,

        /// <summary>
        /// 940007 - S57 event processing started.
        /// </summary>
        S57EventProcessingStarted = 940007,

        /// <summary>
        /// 940008 - S57 enccontentpublished event is specific to AIO cells and, as a result, it is not processed.
        /// </summary>
        S57EventNotProcessedForAioCells = 940008,

        /// <summary>
        /// 940009 - Request to SAP failed.
        /// </summary>
        S57RequestToSapFailedException = 940009,

        /// <summary>
        /// 940010 - S57 enc update sent to SAP.
        /// </summary>
        S57EventUpdateSentToSap = 940010,

        /// <summary>
        /// 940011 - S57 event entry is added in azure table successfully.
        /// </summary>
        S57EventEntryAddedInAzureTable = 940011,

        /// <summary>
        /// 940012 - S57 event json payload is stored in azure blob container.
        /// </summary>
        S57EventJsonStoredInAzureBlobContainer = 940012,

        /// <summary>
        /// 940013 - Generation of SAP xml payload for S57 enccontentpublished event started.
        /// </summary>
        S57EventSapXmlPayloadGenerationStarted = 940013,

        /// <summary>
        /// 940014 - Generation of enc cell SAP actions for S57 enccontentpublished event started.
        /// </summary>
        S57EncCellSapActionGenerationStarted = 940014,

        /// <summary>
        /// 940015 - Generation of SAP action for S57 enccontentpublished event started .
        /// </summary>
        S57SapActionGenerationStarted = 940015,

        /// <summary>
        /// 940016 - Generation of SAP action for S57 enccontentpublished event completed .
        /// </summary>
        S57SapActionGenerationCompleted = 940016,

        /// <summary>
        /// 940017 - Hardware ids configuration missing.
        /// </summary>
        HardwareIdsConfigurationMissingException = 940017,

        /// <summary>
        /// 940018 - Permit decryption failed and could not generate ActiveKey and NextKey.
        /// </summary>
        PermitDecryptionException = 940018,

        /// <summary>
        /// 940019 - Error while generating SAP action information for S57 enccontentpublished event.
        /// </summary>
        S57SapActionInformationGenerationFailedException = 940019,

        /// <summary>
        /// 940020 - Generation of SAP xml payloa for S57 enccontentpublished event completed.
        /// </summary>
        S57EventSapXmlPayloadGenerationCompleted = 940020,

        /// <summary>
        /// 940021 - S57 event xml payload is stored in azure blob container.
        /// </summary>
        S57EventXmlStoredInAzureBlobContainer = 940021,

        /// <summary>
        /// 940022 - Cleanup webjob started.
        /// </summary>
        CleanupWebjobStarted = 940022,

        /// <summary>
        /// 940023 - Cleanup webjob completed.
        /// </summary>
        CleanupWebjobCompleted = 940023,

        /// <summary>
        /// 940024 - Event data clean up completed successfully.
        /// </summary>
        EventDataCleanupCompleted = 940024,

        /// <summary>
        /// 940025 - EES Health Check Request Sent To EES
        /// </summary>
        EESHealthCheckRequestSentToEES = 940025,

        /// <summary>
        /// 940026 - SAP Is Healthy
        /// </summary>
        SAPIsHealthy = 940026,

        /// <summary>
        /// 940027 - SAP Is Unhealty
        /// </summary>
        SAPIsUnhealthy = 940027,

        /// <summary>
        /// 940028 - EES Is Healthy
        /// </summary>
        EESIsHealthy = 940028,

        /// <summary>
        /// 940029 - EES Is Unhealty
        /// </summary>
        EESIsUnhealthy = 940029,

        /// <summary>
        /// 940030 - Sap Health Check Xml Template Not Found
        /// </summary>
        SapHealthCheckXmlTemplateNotFound = 940030,

        /// <summary>
        /// 940031 - SAP Health Check Request Sent To SAP
        /// </summary>
        SapHealthCheckRequestSentToSap = 940031,

        /// <summary>
        /// 940032 - Error occurred while connecting EES
        /// </summary>
        ErrorOccurredInEES = 940032,


        //Below event ids are for WP - Fleet Manager Record of Sale and Licence Updated events


        /// <summary>
        /// 940033 - Record of Sale published event options call started.
        /// </summary>
        RecordOfSalePublishedEventOptionsCallStarted = 940033,

        /// <summary>
        /// 940034 - Record of Sale published event options call completed.
        /// </summary>
        RecordOfSalePublishedEventOptionsCallCompleted = 940034,

        /// <summary>
        /// 940035 - Record of Sale published event received by ERP Facade webhook.
        /// </summary>
        RecordOfSalePublishedEventReceived = 940035,

        /// <summary>
        /// 940036 - Storing the received Record of sale published event in azure table.
        /// </summary>
        StoreRecordOfSalePublishedEventInAzureTable = 940036,

        /// <summary>
        /// 940037 - Record of sale published event in added in azure table successfully.
        /// </summary>
        AddedRecordOfSalePublishedEventInAzureTable = 940037,

        /// <summary>
        /// 940038 - Duplicate Record of sale published event received.
        /// </summary>
        ReceivedDuplicateRecordOfSalePublishedEvent = 940038,

        /// <summary>
        /// 940039 - Existing Record of sale published event updated in azure table successfully.
        /// </summary>
        UpdatedRecordOfSalePublishedEventInAzureTable = 940039,

        /// <summary>
        /// 940040 - Uploading the received Record of sale published event in blob storage.
        /// </summary>
        UploadRecordOfSalePublishedEventInAzureBlob = 940040,

        /// <summary>
        /// 940041 - Record of sale published event is uploaded in blob storage successfully.
        /// </summary>
        UploadedRecordOfSalePublishedEventInAzureBlob = 940041,

        /// <summary>
        /// 940042 - CorrelationId is missing in Record of sale published event.
        /// </summary>
        CorrelationIdMissingInRecordOfSaleEvent = 940042,

        /// <summary>
        /// 940043 - Licence updated published event options call started.
        /// </summary>
        LicenceUpdatedEventOptionsCallStarted = 940043,

        /// <summary>
        /// 940044 - Licence updated published event options call completed.
        /// </summary>
        LicenceUpdatedEventOptionsCallCompleted = 940044,

        /// <summary>
        /// 940045 - Licence updated published event received by ERP Facade webhook.
        /// </summary>
        LicenceUpdatedEventPublishedEventReceived = 940045,

        /// <summary>
        /// 940046 - CorrelationId is missing in Licence updated published event.
        /// </summary>
        CorrelationIdMissingInLicenceUpdatedEvent = 940046,

        /// <summary>
        /// 940047 - Storing the received Licence updated published event in azure table.
        /// </summary>
        StoreLicenceUpdatedPublishedEventInAzureTable = 940047,

        /// <summary>
        /// 940048 - Uploading the received Licence updated published event in blob storage.
        /// </summary>
        UploadLicenceUpdatedPublishedEventInAzureBlob = 940048,

        /// <summary>
        /// 940049 - Licence updated published event is uploaded in blob storage successfully.
        /// </summary>
        UploadedLicenceUpdatedPublishedEventInAzureBlob = 940049,

        /// <summary>
        /// 940050 - Licence updated published event in added in azure table successfully.
        /// </summary>
        AddedLicenceUpdatedPublishedEventInAzureTable = 940050,

        /// <summary>
        /// 940051 - Duplicate Licence updated published event received.
        /// </summary>
        ReceivedDuplicateLicenceUpdatedPublishedEvent = 940051,

        /// <summary>
        /// 940052 - Existing Licence updated published event updated in azure table successfully.
        /// </summary>
        UpdatedLicenceUpdatedPublishedEventInAzureTable = 940052,

        /// <summary>
        /// 940053 - Status of existing record of sale published event updated in azure table successfully.
        /// </summary>
        UpdatedStatusOfRecordOfSalePublishedEventInAzureTable = 940053,

        /// <summary>
        /// 940054 - Status of existing licence updated published event updated in azure table successfully.
        /// </summary>
        UpdatedStatusOfLicenceUpdatedPublishedEventInAzureTable = 940054,

        /// <summary>
        /// 940055 - The record of sale event data has been sent to SAP successfully.
        /// </summary>
        RecordOfSalePublishedEventDataPushedToSap = 940055,

        /// <summary>
        /// 940056 - The licence updated event data has been sent to SAP successfully.
        /// </summary>
        LicenceUpdatedPublishedEventUpdatePushedToSap = 940056,

        /// <summary>
        /// 940057 - An error occurred while sending record of sale published event data to SAP.
        /// </summary>
        RecordOfSaleRequestToSapFailedException = 940057,

        /// <summary>
        /// 940058 - An error occurred while sending licence updated event data to SAP.
        /// </summary>
        LicenceUpdatedRequestToSapFailedException = 940058,

        /// <summary>
        /// 940059 - Uploading Sap Xml payload for licence updated event in Azure blob.
        /// </summary>
        UploadLicenceUpdatedSapXmlPayloadInAzureBlob = 940059,

        /// <summary>
        /// 940060 - SAP xml payload for licence updated event is uploaded in blob storage successfully.
        /// </summary>
        UploadedLicenceUpdatedSapXmlPayloadInAzureBlob = 940060,

        /// <summary>
        /// 940061 - Creating licence updated Sap Xml payload.
        /// </summary>
        CreatingLicenceUpdatedSapPayload = 940061,

        /// <summary>
        /// 940062 - Licence updated SAP xml payload created.
        /// </summary>
        CreatedLicenceUpdatedSapPayload = 940062,

        /// <summary>
        /// 940063 - Licence updated SAP message xml template does not exist.
        /// </summary>
        LicenceUpdatedSapXmlTemplateNotFound = 940063,

        /// <summary>
        /// 940064 - The record of sale SAP message xml template does not exist.
        /// </summary>
        RecordOfSaleSapXmlTemplateNotFound = 940064,

        /// <summary>
        /// 940065 - Creating the record of sale SAP Payload.
        /// </summary>
        CreatingRecordOfSaleSapPayload = 940065,

        /// <summary>
        /// 940066 - The record of sale SAP payload created.
        /// </summary>
        CreatedRecordOfSaleSapPayload = 940066,

        /// <summary>
        /// 940067 - Uploading Sap Xml payload for record of sale event in Azure blob.
        /// </summary>
        UploadRecordOfSaleSapXmlPayloadInAzureBlob = 940067,

        /// <summary>
        /// 940068 - SAP xml payload for record of sale event is uploaded in blob storage successfully.
        /// </summary>
        UploadedRecordOfSaleSapXmlPayloadInAzureBlob = 940068,

        /// <summary>
        /// 940069 - Adding record of sale event payload in Azure Queue storage.
        /// </summary>
        AddMessageToAzureQueue = 940069,

        /// <summary>
        /// 940070 - Record of sale event payload is added in queue storage successfully.
        /// </summary>
        AddedMessageToAzureQueue = 940070,

        /// <summary>
        /// 940071 - Webjob started for merging record of sale events.
        /// </summary>
        WebjobForEventAggregationStarted = 940071,

        /// <summary>
        /// 940072 - Webjob completed for merging record of sale events.
        /// </summary>
        WebjobForEventAggregationCompleted = 940072,

        /// <summary>
        /// 940073 - Webjob started downloading record of sale events from blob.
        /// </summary>
        DownloadRecordOfSaleEventFromAzureBlob = 940073,

        /// <summary>
        /// 940074 - All related events are not present in Azure blob.
        /// </summary>
        AllRelatedEventsAreNotPresentInBlob = 940074,

        /// <summary>
        /// 940075 - The record has been completed already.
        /// </summary>
        RequestAlreadyCompleted = 940075,

        /// <summary>
        /// 940076 - Exception occurred while processing Event aggregation WebJob.
        /// </summary>
        UnhandledWebJobException = 940076,

        /// <summary>
        /// 940077 - Dequeue count of message.
        /// </summary>
        MessageDequeueCount = 940077,

        //Below event ids for - S-100

        /// <summary>
        /// 940078 - S-100 event processing started.
        /// </summary>
        S100EventProcessingStarted = 940078,

        /// <summary>
        /// 940079 - S-100 event entry is added in azure table successfully.
        /// </summary>
        S100EventEntryAddedInAzureTable = 940079,

        /// <summary>
        /// 940080 - S-100 event json payload is stored in azure blob container.
        /// </summary>
        S100EventJsonStoredInAzureBlobContainer = 940080,

        /// <summary>
        /// 940081 - Generation of SAP xml payload for S-100 data content published event started.
        /// </summary>
        S100EventSapXmlPayloadGenerationStarted = 940081,

        /// <summary>
        /// 940082 - Generation of SAP xml payload for S-100 data content published event completed.
        /// </summary>
        S100EventSapXmlPayloadGenerationCompleted = 940082,

        /// <summary>
        /// 940083 - Generation of SAP action for S-100 data content published event started.
        /// </summary>
        S100SapActionGenerationStarted = 940083,

        /// <summary>
        /// 940084 - Generation of SAP action for S-100 data content published event completed.
        /// </summary>
        S100SapActionGenerationCompleted = 940084,

        /// <summary>
        /// 940085 - S-100 SAP action information generation failed.
        /// </summary>
        S100SapActionInformationGenerationFailedException = 940085,

        /// <summary>
        /// 940086 - S-100 event XML payload is stored in azure blob container.
        /// </summary>
        S100EventXMLStoredInAzureBlobContainer = 940086,

        /// <summary>
        /// 940087 - Shared key is missing in request.
        /// </summary>
        SharedApiKeyMissingInRequest = 940087,

        /// <summary>
        /// 940088 - Invalid shared key.
        /// </summary>
        InvalidSharedApiKey = 940088,

        /// <summary>
        /// 940089 - Shared API Key Configuration is missing.
        /// </summary>
        SharedApiKeyConfigurationMissingException = 940089,

        /// <summary>
        /// 940090 - S-100 Request to SAP failed.
        /// </summary>
        S100RequestToSapFailedException = 940090,

        /// <summary>
        /// 940091 - S-100 data content sent to SAP.
        /// </summary>
        S100EventUpdateSentToSap = 940091,

        /// <summary>
        /// 940092 - Event data cleaned up for CorrelationId successfully.
        /// </summary>
        EventCleanupSuccessful = 940092,

        /// <summary>
        /// 940093 - ErrorOccurred In CleanupWebJob.
        /// </summary>
        ErrorOccurredInCleanupWebJob = 940093,

        /// <summary>
        /// 940094 - Attempting to publish event to ESS.
        /// </summary>
        StartingEnterpriseEventServiceEventPublisher = 940094,

        /// <summary>
        /// 940095 - Retry attempt to publish EES event.
        /// </summary>
        RetryAttemptForEnterpriseEventServiceEvent = 940095,

        /// <summary>
        /// 940096 - Exception occurred while publishing event to EES.
        /// </summary>
        EnterpriseEventServiceEventPublishException = 940096,

        /// <summary>
        /// 940097 - S-100 sap callBack payload received from SAP.
        /// </summary>
        S100SapCallbackPayloadReceived = 940097,

        /// <summary>
        /// 940098 - CorrelationId is missing in S-100 sap call back.
        /// </summary>
        CorrelationIdMissingInS100SapCallBack = 940098,

        /// <summary>
        /// 940099 - Invalid S-100 SAP callback. Request from ERP Facade to SAP not found.
        /// </summary>
        InvalidS100SapCallback = 940099,

        /// <summary>
        /// 940100 - Valid S-100 SAP callback.
        /// </summary>
        ValidS100SapCallback = 940100,

        /// <summary>
        /// 940101 - Download S100 Unit Of Sale Updated Event is started.
        /// </summary>
        DownloadS100UnitOfSaleUpdatedEventIsStarted = 940101,

        /// <summary>
        /// 940102 - Download S100 Unit Of Sale Updated Event is completed.
        /// </summary>
        DownloadS100UnitOfSaleUpdatedEventIsCompleted = 940102,

        /// <summary>
        /// 940103 - Publishing Unit Of Sale Updated Event To Ees Started.
        /// </summary>
        PublishingUnitOfSaleUpdatedEventToEesStarted = 940103,

        /// <summary>
        /// 940104 - Error occurred while publishing the publishing unit of sale updated event to EES.
        /// </summary>
        PublishingUnitOfSaleUpdatedEventToEesFailedException = 940104,

        /// <summary>
        /// 940105 - The publishing unit of sale updated event successfully to EES.
        /// </summary>
        UnitOfSaleUpdatedEventPublished = 940105,

        /// <summary>
        /// 940106 - Updated The Enc Event StatusAnd Publish Date Time Entity in enc event table.
        /// </summary>
        S100DataContentPublishedEventTableEntryUpdated = 940106,

        /// <summary>
        /// 940107 - S-100 Unit Of Sale Updated Event Json Stored In Azure Blob Container
        /// </summary>
        S100UnitOfSaleUpdatedEventJsonStoredInAzureBlobContainer = 940107
    }

    /// <summary>
    /// EventId Extensions
    /// </summary>
    [ExcludeFromCodeCoverage]
    public static class EventIdExtensions
    {
        /// <summary>
        /// Event Id
        /// </summary>
        /// <param name="eventId"></param>
        /// <returns></returns>
        public static EventId ToEventId(this EventIds eventId)
        {
            return new EventId((int)eventId, eventId.ToString());
        }
    }
}
