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
        SapXmlTemplateNotFound = 940002,

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
        /// 940020 - Required SAP property value is missing S57 enccontentpublished event.
        /// </summary>
        EmptyEventJsonPropertyException = 940020,

        /// <summary>
        /// 940021 - Generation of SAP xml payloa for S57 enccontentpublished event completed.
        /// </summary>
        S57EventSapXmlPayloadGenerationCompleted = 940021,

        /// <summary>
        /// 940022 - S57 event xml payload is stored in azure blob container.
        /// </summary>
        S57EventXmlStoredInAzureBlobContainer = 940022,

        /// <summary>
        /// 940023 - Cleanup webjob started.
        /// </summary>
        CleanupWebjobStarted = 940023,

        /// <summary>
        /// 940024 - Cleanup webjob completed.
        /// </summary>
        CleanupWebjobCompleted = 940024,

        /// <summary>
        /// 940025 - Event data clean up completed successfully.
        /// </summary>
        EventDataCleanupCompleted = 940025,

        /// <summary>
        /// 940026 - EES Health Check Request Sent To EES
        /// </summary>
        EESHealthCheckRequestSentToEES = 940026,

        /// <summary>
        /// 940027 - SAP Is Healthy
        /// </summary>
        SAPIsHealthy = 940027,

        /// <summary>
        /// 940028 - SAP Is Unhealty
        /// </summary>
        SAPIsUnhealthy = 940028,

        /// <summary>
        /// 940029 - EES Is Healthy
        /// </summary>
        EESIsHealthy = 940029,

        /// <summary>
        /// 940030 - EES Is Unhealty
        /// </summary>
        EESIsUnhealthy = 940030,

        /// <summary>
        /// 940031 - Sap Health Check Xml Template Not Found
        /// </summary>
        SapHealthCheckXmlTemplateNotFound = 940031,

        /// <summary>
        /// 940032 - SAP Health Check Request Sent To SAP
        /// </summary>
        SapHealthCheckRequestSentToSap = 940032,

        /// <summary>
        /// 940033 - Error occurred while connecting EES
        /// </summary>
        ErrorOccurredInEES = 940033,


        //Below event ids are for WP - Fleet Manager Record of Sale and Licence Updated events


        /// <summary>
        /// 940034 - Record of Sale published event options call started.
        /// </summary>
        RecordOfSalePublishedEventOptionsCallStarted = 940034,

        /// <summary>
        /// 940035 - Record of Sale published event options call completed.
        /// </summary>
        RecordOfSalePublishedEventOptionsCallCompleted = 940035,

        /// <summary>
        /// 940036 - Record of Sale published event received by ERP Facade webhook.
        /// </summary>
        RecordOfSalePublishedEventReceived = 940036,

        /// <summary>
        /// 940037 - Storing the received Record of sale published event in azure table.
        /// </summary>
        StoreRecordOfSalePublishedEventInAzureTable = 940037,

        /// <summary>
        /// 940038 - Record of sale published event in added in azure table successfully.
        /// </summary>
        AddedRecordOfSalePublishedEventInAzureTable = 940038,

        /// <summary>
        /// 940039 - Duplicate Record of sale published event received.
        /// </summary>
        ReceivedDuplicateRecordOfSalePublishedEvent = 940039,

        /// <summary>
        /// 940040 - Existing Record of sale published event updated in azure table successfully.
        /// </summary>
        UpdatedRecordOfSalePublishedEventInAzureTable = 940040,

        /// <summary>
        /// 940041 - Uploading the received Record of sale published event in blob storage.
        /// </summary>
        UploadRecordOfSalePublishedEventInAzureBlob = 940041,

        /// <summary>
        /// 940042 - Record of sale published event is uploaded in blob storage successfully.
        /// </summary>
        UploadedRecordOfSalePublishedEventInAzureBlob = 940042,

        /// <summary>
        /// 940043 - CorrelationId is missing in Record of sale published event.
        /// </summary>
        CorrelationIdMissingInRecordOfSaleEvent = 940043,

        /// <summary>
        /// 940044 - Licence updated published event options call started.
        /// </summary>
        LicenceUpdatedEventOptionsCallStarted = 940044,

        /// <summary>
        /// 940045 - Licence updated published event options call completed.
        /// </summary>
        LicenceUpdatedEventOptionsCallCompleted = 940045,

        /// <summary>
        /// 940046 - Licence updated published event received by ERP Facade webhook.
        /// </summary>
        LicenceUpdatedEventPublishedEventReceived = 940046,

        /// <summary>
        /// 940047 - CorrelationId is missing in Licence updated published event.
        /// </summary>
        CorrelationIdMissingInLicenceUpdatedEvent = 940047,

        /// <summary>
        /// 940048 - Storing the received Licence updated published event in azure table.
        /// </summary>
        StoreLicenceUpdatedPublishedEventInAzureTable = 940048,

        /// <summary>
        /// 940049 - Uploading the received Licence updated published event in blob storage.
        /// </summary>
        UploadLicenceUpdatedPublishedEventInAzureBlob = 940049,

        /// <summary>
        /// 940050 - Licence updated published event is uploaded in blob storage successfully.
        /// </summary>
        UploadedLicenceUpdatedPublishedEventInAzureBlob = 940050,

        /// <summary>
        /// 940051 - Licence updated published event in added in azure table successfully.
        /// </summary>
        AddedLicenceUpdatedPublishedEventInAzureTable = 940051,

        /// <summary>
        /// 940052 - Duplicate Licence updated published event received.
        /// </summary>
        ReceivedDuplicateLicenceUpdatedPublishedEvent = 940052,

        /// <summary>
        /// 940053 - Existing Licence updated published event updated in azure table successfully.
        /// </summary>
        UpdatedLicenceUpdatedPublishedEventInAzureTable = 940053,

        /// <summary>
        /// 940054 - Status of existing record of sale published event updated in azure table successfully.
        /// </summary>
        UpdatedStatusOfRecordOfSalePublishedEventInAzureTable = 940054,

        /// <summary>
        /// 940055 - Status of existing licence updated published event updated in azure table successfully.
        /// </summary>
        UpdatedStatusOfLicenceUpdatedPublishedEventInAzureTable = 940055,

        /// <summary>
        /// 940056 - The record of sale event data has been sent to SAP successfully.
        /// </summary>
        RecordOfSalePublishedEventDataPushedToSap = 940056,

        /// <summary>
        /// 940057 - The licence updated event data has been sent to SAP successfully.
        /// </summary>
        LicenceUpdatedPublishedEventUpdatePushedToSap = 940057,

        /// <summary>
        /// 940058 - An error occurred while sending record of sale published event data to SAP.
        /// </summary>
        ErrorOccurredInSapForRecordOfSalePublishedEvent = 940058,

        /// <summary>
        /// 940059 - An error occurred while sending licence updated event data to SAP.
        /// </summary>
        ErrorOccurredInSapForLicenceUpdatedPublishedEvent = 940059,

        /// <summary>
        /// 940060 - Uploading Sap Xml payload for licence updated event in Azure blob.
        /// </summary>
        UploadLicenceUpdatedSapXmlPayloadInAzureBlob = 940060,

        /// <summary>
        /// 940061 - SAP xml payload for licence updated event is uploaded in blob storage successfully.
        /// </summary>
        UploadedLicenceUpdatedSapXmlPayloadInAzureBlob = 940061,

        /// <summary>
        /// 940062 - Creating licence updated Sap Xml payload.
        /// </summary>
        CreatingLicenceUpdatedSapPayload = 940062,

        /// <summary>
        /// 940063 - Licence updated SAP xml payload created.
        /// </summary>
        CreatedLicenceUpdatedSapPayload = 940063,

        /// <summary>
        /// 940064 - Licence updated SAP message xml template does not exist.
        /// </summary>
        LicenceUpdatedSapXmlTemplateNotFound = 940064,

        /// <summary>
        /// 940065 - The record of sale SAP message xml template does not exist.
        /// </summary>
        RecordOfSaleSapXmlTemplateNotFound = 940065,

        /// <summary>
        /// 940066 - Creating the record of sale SAP Payload.
        /// </summary>
        CreatingRecordOfSaleSapPayload = 940066,

        /// <summary>
        /// 940067 - The record of sale SAP payload created.
        /// </summary>
        CreatedRecordOfSaleSapPayload = 940067,

        /// <summary>
        /// 940068 - Uploading Sap Xml payload for record of sale event in Azure blob.
        /// </summary>
        UploadRecordOfSaleSapXmlPayloadInAzureBlob = 940068,

        /// <summary>
        /// 940069 - SAP xml payload for record of sale event is uploaded in blob storage successfully.
        /// </summary>
        UploadedRecordOfSaleSapXmlPayloadInAzureBlob = 940069,

        /// <summary>
        /// 940070 - Adding record of sale event payload in Azure Queue storage.
        /// </summary>
        AddMessageToAzureQueue = 940070,

        /// <summary>
        /// 940071 - Record of sale event payload is added in queue storage successfully.
        /// </summary>
        AddedMessageToAzureQueue = 940071,

        /// <summary>
        /// 940072 - Webjob started for merging record of sale events.
        /// </summary>
        WebjobForEventAggregationStarted = 940072,

        /// <summary>
        /// 940073 - Webjob completed for merging record of sale events.
        /// </summary>
        WebjobForEventAggregationCompleted = 940073,

        /// <summary>
        /// 940074 - Webjob started downloading record of sale events from blob.
        /// </summary>
        DownloadRecordOfSaleEventFromAzureBlob = 940074,

        /// <summary>
        /// 940075 - All related events are not present in Azure blob.
        /// </summary>
        AllRelatedEventsAreNotPresentInBlob = 940075,

        /// <summary>
        /// 940076 - The record has been completed already.
        /// </summary>
        RequestAlreadyCompleted = 940076,

        /// <summary>
        /// 940077 - Exception occurred while processing Event aggregation WebJob.
        /// </summary>
        UnhandledWebJobException = 940077,

        /// <summary>
        /// 940078 - Dequeue count of message.
        /// </summary>
        MessageDequeueCount = 940078,

        //Below event ids for - S-100

        /// <summary>
        /// 940079 - S-100 event processing started.
        /// </summary>
        S100EventProcessingStarted = 940079,

        /// <summary>
        /// 940080 - S-100 event entry is added in azure table successfully.
        /// </summary>
        S100EventEntryAddedInAzureTable = 940080,

        /// <summary>
        /// 940081 - S-100 event json payload is stored in azure blob container.
        /// </summary>
        S100EventJsonStoredInAzureBlobContainer = 940081,

        /// <summary>
        /// 940082 - Generation of SAP xml payload for S-100 data content published event started.
        /// </summary>
        S100EventSapXmlPayloadGenerationStarted = 940082,

        /// <summary>
        /// 940083 - Generation of SAP xml payload for S-100 data content published event completed.
        /// </summary>
        S100EventSapXmlPayloadGenerationCompleted = 940083,

        /// <summary>
        /// 940084 - Generation of SAP action for S-100 data content published event started.
        /// </summary>
        S100SapActionGenerationStarted = 940084,

        /// <summary>
        /// 940085 - Generation of SAP action for S-100 data content published event completed.
        /// </summary>
        S100SapActionGenerationCompleted = 940085,

        /// <summary>
        /// 940086 - S-100 SAP action information generation failed.
        /// </summary>
        S100SapActionInformationGenerationFailedException = 940086,

        /// <summary>
        /// 940087 - S-100 event XML payload is stored in azure blob container.
        /// </summary>
        S100EventXMLStoredInAzureBlobContainer = 940087,

        /// <summary>
        /// 940088 - Shared key is missing in request.
        /// </summary>
        SharedApiKeyMissingInRequest = 940088,

        /// <summary>
        /// 940089 - Invalid shared key.
        /// </summary>
        InvalidSharedApiKey = 940089,

        /// <summary>
        /// 940090 - Shared API Key Configuration is missing.
        /// </summary>
        SharedApiKeyConfigurationMissing = 940090,

        /// <summary>
        /// 940091 - S-100 Request to SAP failed.
        /// </summary>
        S100RequestToSapFailedException = 940091,

        /// <summary>
        /// 940092 - S-100 data content sent to SAP.
        /// </summary>
        S100EventUpdateSentToSap = 940092,

        /// <summary>
        /// 940093 - Event data cleaned up for CorrelationId successfully.
        /// </summary>
        EventCleanupSuccessful = 940093,

        /// <summary>
        /// 940094 - ErrorOccurred In CleanupWebJob.
        /// </summary>
        ErrorOccurredInCleanupWebJob = 940094,

        /// <summary>
        /// 940095 - Attempting to publish event to ESS.
        /// </summary>
        StartingEnterpriseEventServiceEventPublisher = 940095,

        /// <summary>
        /// 940096 - Retry attempt to publish EES event.
        /// </summary>
        RetryAttemptForEnterpriseEventServiceEvent = 940096,

        /// <summary>
        /// 940097 - Exception occurred while publishing event to EES.
        /// </summary>
        EnterpriseEventServiceEventPublishException = 940097,

        /// <summary>
        /// 940098 - S-100 sap callBack payload received from SAP.
        /// </summary>
        S100SapCallbackPayloadReceived = 940098,

        /// <summary>
        /// 940099 - CorrelationId is missing in S-100 sap call back.
        /// </summary>
        CorrelationIdMissingInS100SapCallBack = 940099,

        /// <summary>
        /// 940100 - Invalid S-100 SAP callback. Request from ERP Facade to SAP not found.
        /// </summary>
        InvalidS100SapCallback = 940100,

        /// <summary>
        /// 940101 - Valid S-100 SAP callback.
        /// </summary>
        ValidS100SapCallback = 940101,

        /// <summary>
        /// 940102 - Download S100 Unit Of Sale Updated Event is started.
        /// </summary>
        DownloadS100UnitOfSaleUpdatedEventIsStarted = 940102,

        /// <summary>
        /// 940103 - Download S100 Unit Of Sale Updated Event is completed.
        /// </summary>
        DownloadS100UnitOfSaleUpdatedEventIsCompleted = 940103,

        /// <summary>
        /// 940104 - Publishing Unit Of Sale Updated Event To Ees Started.
        /// </summary>
        PublishingUnitOfSaleUpdatedEventToEesStarted = 940104,

        /// <summary>
        /// 940105 - Error occurred while publishing the publishing unit of sale updated event to EES.
        /// </summary>
        ErrorOccurredWhilePublishingUnitOfSaleUpdatedEventToEes = 940105,

        /// <summary>
        /// 940106 - The publishing unit of sale updated event successfully to EES.
        /// </summary>
        UnitOfSaleUpdatedEventPublished = 940106,

        /// <summary>
        /// 940107 - Updated The Enc Event StatusAnd Publish Date Time Entity in enc event table.
        /// </summary>
        S100DataContentPublishedEventTableEntryUpdated = 940107,

        /// <summary>
        /// 940108 - S-100 Unit Of Sale Updated Event Json Stored In Azure Blob Container
        /// </summary>
        S100UnitOfSaleUpdatedEventJsonStoredInAzureBlobContainer = 940108
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
