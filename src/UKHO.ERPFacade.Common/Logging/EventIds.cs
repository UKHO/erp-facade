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
        /// 940002 - New ENC content published event options call started.
        /// </summary>
        NewEncContentPublishedEventOptionsCallStarted = 940002,

        /// <summary>
        /// 940003 - New ENC content published event options call completed.
        /// </summary>
        NewEncContentPublishedEventOptionsCallCompleted = 940003,

        /// <summary>
        /// 940004 - New ENC content published event received by ERP Facade webhook.
        /// </summary>
        NewEncContentPublishedEventReceived = 940004,

        /// <summary>
        /// 940005 - CorrelationId is missing in ENC content published event.
        /// </summary>
        CorrelationIdMissingInEvent = 940005,

        /// <summary>
        /// 940006 - Adding or Updating entry for enccontentpublished event in azure table.
        /// </summary>
        AddingEntryForEncContentPublishedEventInAzureTable = 940006,

        /// <summary>
        /// 940007 - ENC content published event in added in azure table successfully.
        /// </summary>
        AddedEntryForEncContentPublishedEventInAzureTable = 940007,

        /// <summary>
        /// 940008 - Duplicate ENC contect published event received.
        /// </summary>
        ReceivedDuplicateEncContentPublishedEvent = 940008,

        /// <summary>
        /// 940009 - Existing ENC content published event updated in azure table successfully.
        /// </summary>
        UpdatedEncContentPublishedEventInAzureTable = 940009,

        /// <summary>
        /// 940010 - Uploading the received ENC content published event in blob storage.
        /// </summary>
        UploadEncContentPublishedEventInAzureBlobStarted = 940010,

        /// <summary>
        /// 940011 - ENC content published event is uploaded in blob storage successfully.
        /// </summary>
        UploadEncContentPublishedEventInAzureBlobCompleted = 940011,

        /// <summary>
        /// 940012 - Request to SAP failed
        /// </summary>
        RequestToSapFailed = 940012,

        /// <summary>
        /// 940013 - ENC Update sent to SAP
        /// </summary>
        EncUpdateSentToSap = 940013,

        /// <summary>
        /// 940014 - Updated RequestTime entity successfully in Azure Table
        /// </summary>
        UpdateRequestTimeEntitySuccessful = 940014,

        /// <summary>
        /// 940015 - Sap Xml Template Not Found
        /// </summary>
        SapXmlTemplateNotFound = 940015,

        /// <summary>
        /// 940016 - Generation of SAP xml payload started
        /// </summary>
        GenerationOfSapXmlPayloadStarted = 940016,

        /// <summary>
        /// 940017 - Sap Action Created
        /// </summary>
        SapActionCreated = 940017,

        /// <summary>
        /// 940018 - Generation of SAP xml payload completed
        /// </summary>
        GenerationOfSapXmlPayloadCompleted = 940018,

        /// <summary>
        /// 940019 - Azure Table Not Found
        /// </summary>
        AzureTableNotFound = 940019,

        /// <summary>
        /// 940020 - Sap Health Check Xml Template Not Found
        /// </summary>
        SapHealthCheckXmlTemplateNotFound = 940020,

        /// <summary>
        /// 940021 - SAP Health Check Request Sent To SAP
        /// </summary>
        SapHealthCheckRequestSentToSap = 940021,

        /// <summary>
        /// 940022 - Uploading the SAP xml payload in blob storage.
        /// </summary>
        UploadSapXmlPayloadInAzureBlobStarted = 940022,

        /// <summary>
        /// 940023 - SAP xml payload is uploaded in blob storage successfully.
        /// </summary>
        UploadSapXmlPayloadInAzureBlobCompleted = 940023,

        /// <summary>
        /// 940024 - Deleted EES entity successfully from Azure Table
        /// </summary>
        DeletedEESEntitySuccessful = 940024,

        /// <summary>
        /// 940025 - Fetching all EES entities from Azure Table.
        /// </summary>
        FetchEESEntities = 940025,

        /// <summary>
        /// 940026 - Deleted container successfully.
        /// </summary>
        DeletedContainerSuccessful = 940026,

        /// <summary>
        /// 940027 - Webjob started cleanup process.
        /// </summary>
        WebjobCleanUpEventStarted = 940027,

        /// <summary>
        /// 940028 - Webjob completed cleanup process.
        /// </summary>
        WebjobCleanUpEventCompleted = 940028,

        /// <summary>
        /// 940029 - Error occurred while connecting EES
        /// </summary>
        ErrorOccurredInEES = 940029,

        /// <summary>
        /// 940030 - EES Health Check Request Sent To EES
        /// </summary>
        EESHealthCheckRequestSentToEES = 940030,

        /// <summary>
        /// 940031 - SAP Is Healthy
        /// </summary>
        SAPIsHealthy = 940031,

        /// <summary>
        /// 940032 - SAP Is Unhealty
        /// </summary>
        SAPIsUnhealthy = 940032,

        /// <summary>
        /// 940033 - EES Is Healthy
        /// </summary>
        EESIsHealthy = 940033,

        /// <summary>
        /// 940034 - EES Is Unhealty
        /// </summary>
        EESIsUnhealthy = 940034,

        /// <summary>
        /// 940035 - Record of Sale published event options call started.
        /// </summary>
        RecordOfSalePublishedEventOptionsCallStarted = 940035,

        /// <summary>
        /// 940036 - Record of Sale published event options call completed.
        /// </summary>
        RecordOfSalePublishedEventOptionsCallCompleted = 940036,

        /// <summary>
        /// 940037 - Record of Sale published event received by ERP Facade webhook.
        /// </summary>
        RecordOfSalePublishedEventReceived = 940037,

        /// <summary>
        /// 940038 - Storing the received Record of sale published event in azure table.
        /// </summary>
        StoreRecordOfSalePublishedEventInAzureTable = 940038,

        /// <summary>
        /// 940039 - Record of sale published event in added in azure table successfully.
        /// </summary>
        AddedRecordOfSalePublishedEventInAzureTable = 940039,

        /// <summary>
        /// 940040 - Duplicate Record of sale published event received.
        /// </summary>
        ReceivedDuplicateRecordOfSalePublishedEvent = 940040,

        /// <summary>
        /// 940041 - Existing Record of sale published event updated in azure table successfully.
        /// </summary>
        UpdatedRecordOfSalePublishedEventInAzureTable = 940041,

        /// <summary>
        /// 940042 - Uploading the received Record of sale published event in blob storage.
        /// </summary>
        UploadRecordOfSalePublishedEventInAzureBlob = 940042,

        /// <summary>
        /// 940043 - Record of sale published event is uploaded in blob storage successfully.
        /// </summary>
        UploadedRecordOfSalePublishedEventInAzureBlob = 940043,

        /// <summary>
        /// 940044 - CorrelationId is missing in Record of sale published event.
        /// </summary>
        CorrelationIdMissingInRecordOfSaleEvent = 940044,

        /// <summary>
        /// 940045 - Licence updated published event options call started.
        /// </summary>
        LicenceUpdatedEventOptionsCallStarted = 940045,

        /// <summary>
        /// 940046 - Licence updated published event options call completed.
        /// </summary>
        LicenceUpdatedEventOptionsCallCompleted = 940046,

        /// <summary>
        /// 940047 - Licence updated published event received by ERP Facade webhook.
        /// </summary>
        LicenceUpdatedEventPublishedEventReceived = 940047,

        /// <summary>
        /// 940048 - CorrelationId is missing in Licence updated published event.
        /// </summary>
        CorrelationIdMissingInLicenceUpdatedEvent = 940048,

        /// <summary>
        /// 940049 - Storing the received Licence updated published event in azure table.
        /// </summary>
        StoreLicenceUpdatedPublishedEventInAzureTable = 940049,

        /// <summary>
        /// 940050 - Uploading the received Licence updated published event in blob storage.
        /// </summary>
        UploadLicenceUpdatedPublishedEventInAzureBlob = 940050,

        /// <summary>
        /// 940051 - Licence updated published event is uploaded in blob storage successfully.
        /// </summary>
        UploadedLicenceUpdatedPublishedEventInAzureBlob = 940051,

        /// <summary>
        /// 940052 - Licence updated published event in added in azure table successfully.
        /// </summary>
        AddedLicenceUpdatedPublishedEventInAzureTable = 940052,

        /// <summary>
        /// 940053 - Duplicate Licence updated published event received.
        /// </summary>
        ReceivedDuplicateLicenceUpdatedPublishedEvent = 940053,

        /// <summary>
        /// 940054 - Existing Licence updated published event updated in azure table successfully.
        /// </summary>
        UpdatedLicenceUpdatedPublishedEventInAzureTable = 940054,

        /// <summary>
        /// 940055 - Status of existing record of sale published event updated in azure table successfully.
        /// </summary>
        UpdatedStatusOfRecordOfSalePublishedEventInAzureTable = 940055,

        /// <summary>
        /// 940056 - Status of existing licence updated published event updated in azure table successfully.
        /// </summary>
        UpdatedStatusOfLicenceUpdatedPublishedEventInAzureTable = 940056,

        /// <summary>
        /// 940057 - The record of sale event data has been sent to SAP successfully.
        /// </summary>
        RecordOfSalePublishedEventDataPushedToSap = 940057,

        /// <summary>
        /// 940058 - The licence updated event data has been sent to SAP successfully.
        /// </summary>
        LicenceUpdatedPublishedEventUpdatePushedToSap = 940058,

        /// <summary>
        /// 940059 - An error occurred while sending record of sale published event data to SAP.
        /// </summary>
        ErrorOccurredInSapForRecordOfSalePublishedEvent = 940059,

        /// <summary>
        /// 940060 - An error occurred while sending licence updated event data to SAP.
        /// </summary>
        ErrorOccurredInSapForLicenceUpdatedPublishedEvent = 940060,

        /// <summary>
        /// 940061 - Uploading Sap Xml payload for licence updated event in Azure blob.
        /// </summary>
        UploadLicenceUpdatedSapXmlPayloadInAzureBlob = 940061,

        /// <summary>
        /// 940062 - SAP xml payload for licence updated event is uploaded in blob storage successfully.
        /// </summary>
        UploadedLicenceUpdatedSapXmlPayloadInAzureBlob = 940062,

        /// <summary>
        /// 940063 - Creating licence updated Sap Xml payload.
        /// </summary>
        CreatingLicenceUpdatedSapPayload = 940063,

        /// <summary>
        /// 940064 - Licence updated SAP xml payload created.
        /// </summary>
        CreatedLicenceUpdatedSapPayload = 940064,

        /// <summary>
        /// 940065 - Licence updated SAP message xml template does not exist.
        /// </summary>
        LicenceUpdatedSapXmlTemplateNotFound = 940065,

        /// <summary>
        /// 940066 - The record of sale SAP message xml template does not exist.
        /// </summary>
        RecordOfSaleSapXmlTemplateNotFound = 940066,

        /// <summary>
        /// 940067 - Creating the record of sale SAP Payload.
        /// </summary>
        CreatingRecordOfSaleSapPayload = 940067,

        /// <summary>
        /// 940068 - The record of sale SAP payload created.
        /// </summary>
        CreatedRecordOfSaleSapPayload = 940068,

        /// <summary>
        /// 940069 - Uploading Sap Xml payload for record of sale event in Azure blob.
        /// </summary>
        UploadRecordOfSaleSapXmlPayloadInAzureBlob = 940069,

        /// <summary>
        /// 940070 - SAP xml payload for record of sale event is uploaded in blob storage successfully.
        /// </summary>
        UploadedRecordOfSaleSapXmlPayloadInAzureBlob = 940070,

        /// <summary>
        /// 940071 - Adding record of sale event payload in Azure Queue storage.
        /// </summary>
        AddMessageToAzureQueue = 940071,

        /// <summary>
        /// 940072 - Record of sale event payload is added in queue storage successfully.
        /// </summary>
        AddedMessageToAzureQueue = 940072,

        /// <summary>
        /// 940073 - Webjob started for merging record of sale events.
        /// </summary>
        WebjobForEventAggregationStarted = 940073,

        /// <summary>
        /// 940074 - Webjob completed for merging record of sale events.
        /// </summary>
        WebjobForEventAggregationCompleted = 940074,

        /// <summary>
        /// 940075 - Webjob started downloading record of sale events from blob.
        /// </summary>
        DownloadRecordOfSaleEventFromAzureBlob = 940075,

        /// <summary>
        /// 940076 - All related events are not present in Azure blob.
        /// </summary>
        AllRelatedEventsAreNotPresentInBlob = 940076,

        /// <summary>
        /// 940077 - The record has been completed already.
        /// </summary>
        RequestAlreadyCompleted = 940077,

        /// <summary>
        /// 940078 - Exception occurred while processing Event aggregation WebJob.
        /// </summary>
        UnhandledWebJobException = 940078,

        /// <summary>
        /// 940079 - Dequeue count of message.
        /// </summary>
        MessageDequeueCount = 940079,

        /// <summary>
        /// 940080 - Exception occurred while decrypting the permit string.
        /// </summary>
        PermitDecryptionException = 940080,

        /// <summary>
        /// 940081 - Unit of Sale not found.
        /// </summary>
        UnitOfSaleNotFoundException = 940081,

        /// <summary>
        /// 940082 - ENC cell SAP action generation started.
        /// </summary>
        EncCellSapActionGenerationStarted = 940082,

        /// <summary>
        /// 940083 - SAP action generation started.
        /// </summary>
        BuildingSapActionStarted = 940083,

        /// <summary>
        /// 940084 - Required SAP property value is found empty enccontentpublished event.
        /// </summary>
        EmptyEventJsonPropertyException = 940084,

        /// <summary>
        /// 940085 - Error while generating SAP action information.
        /// </summary>
        BuildingSapActionInformationException = 940085,

        /// <summary>
        /// 940086 - Required section not found in JSON payload.
        /// </summary>
        RequiredSectionNotFoundException = 940086
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
