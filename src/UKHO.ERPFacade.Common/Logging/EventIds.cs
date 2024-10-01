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
        /// 940018 - Updated RequestTime entity successfully in Azure Table
        /// </summary>
        UpdateRequestTimeEntitySuccessful = 940018,

        /// <summary>
        /// 940025 - Sap Xml Template Not Found
        /// </summary>
        SapXmlTemplateNotFound = 940025,

        /// <summary>
        /// 940026 - Generation of SAP xml payload started
        /// </summary>
        GenerationOfSapXmlPayloadStarted = 940026,

        /// <summary>
        /// 940027 - Sap Action Created
        /// </summary>
        SapActionCreated = 940027,

        /// <summary>
        /// 940028 - Generation of SAP xml payload completed
        /// </summary>
        GenerationOfSapXmlPayloadCompleted = 940028,

        /// <summary>
        /// 940030 - Azure Table Not Found
        /// </summary>
        AzureTableNotFound = 940030,

        /// <summary>
        /// 940039 - Sap Health Check Xml Template Not Found
        /// </summary>
        SapHealthCheckXmlTemplateNotFound = 940039,

        /// <summary>
        /// 940040 - SAP Health Check Request Sent To SAP
        /// </summary>
        SapHealthCheckRequestSentToSap = 940040,

        /// <summary>
        /// 940041 - Uploading the SAP xml payload in blob storage.
        /// </summary>
        UploadSapXmlPayloadInAzureBlobStarted = 940041,

        /// <summary>
        /// 940042 - SAP xml payload is uploaded in blob storage successfully.
        /// </summary>
        UploadSapXmlPayloadInAzureBlobCompleted = 940042,

        /// <summary>
        /// 940068 - Deleted EES entity successfully from Azure Table
        /// </summary>
        DeletedEESEntitySuccessful = 940068,
        
        /// <summary>
        /// 940071 - Fetching all EES entities from Azure Table.
        /// </summary>
        FetchEESEntities = 940071,

        /// <summary>
        /// 940072 - Deleted container successfully.
        /// </summary>
        DeletedContainerSuccessful = 940072,

        /// <summary>
        /// 940073 - Webjob started cleanup process.
        /// </summary>
        WebjobCleanUpEventStarted = 940073,

        /// <summary>
        /// 940074 - Webjob completed cleanup process.
        /// </summary>
        WebjobCleanUpEventCompleted = 940074,

        /// <summary>
        /// 940083 - Error occurred while connecting EES
        /// </summary>
        ErrorOccurredInEES = 940083,

        /// <summary>
        /// 940084 - EES Health Check Request Sent To EES
        /// </summary>
        EESHealthCheckRequestSentToEES = 940084,

        /// <summary>
        /// 940085 - SAP Is Healthy
        /// </summary>
        SAPIsHealthy = 940085,

        /// <summary>
        /// 940086 - SAP Is Unhealty
        /// </summary>
        SAPIsUnhealthy = 940086,

        /// <summary>
        /// 940087 - EES Is Healthy
        /// </summary>
        EESIsHealthy = 940087,

        /// <summary>
        /// 940088 - EES Is Unhealty
        /// </summary>
        EESIsUnhealthy = 940088,

        /// <summary>
        /// 940096 - Record of Sale published event options call started.
        /// </summary>
        RecordOfSalePublishedEventOptionsCallStarted = 940096,

        /// <summary>
        /// 940097 - Record of Sale published event options call completed.
        /// </summary>
        RecordOfSalePublishedEventOptionsCallCompleted = 940097,

        /// <summary>
        /// 940098 - Record of Sale published event received by ERP Facade webhook.
        /// </summary>
        RecordOfSalePublishedEventReceived = 940098,

        /// <summary>
        /// 940099 - Storing the received Record of sale published event in azure table.
        /// </summary>
        StoreRecordOfSalePublishedEventInAzureTable = 940099,

        /// <summary>
        /// 940100 - Record of sale published event in added in azure table successfully.
        /// </summary>
        AddedRecordOfSalePublishedEventInAzureTable = 940100,

        /// <summary>
        /// 940101 - Duplicate Record of sale published event received.
        /// </summary>
        ReceivedDuplicateRecordOfSalePublishedEvent = 940101,

        /// <summary>
        /// 940102 - Existing Record of sale published event updated in azure table successfully.
        /// </summary>
        UpdatedRecordOfSalePublishedEventInAzureTable = 940102,

        /// <summary>
        /// 940103 - Uploading the received Record of sale published event in blob storage.
        /// </summary>
        UploadRecordOfSalePublishedEventInAzureBlob = 940103,

        /// <summary>
        /// 940104 - Record of sale published event is uploaded in blob storage successfully.
        /// </summary>
        UploadedRecordOfSalePublishedEventInAzureBlob = 940104,

        /// <summary>
        /// 940105 - CorrelationId is missing in Record of sale published event.
        /// </summary>
        CorrelationIdMissingInRecordOfSaleEvent = 940105,

        /// <summary>
        /// 940106 - Licence updated published event options call started.
        /// </summary>
        LicenceUpdatedEventOptionsCallStarted = 940106,

        /// <summary>
        /// 940107 - Licence updated published event options call completed.
        /// </summary>
        LicenceUpdatedEventOptionsCallCompleted = 940107,

        /// <summary>
        /// 940108 - Licence updated published event received by ERP Facade webhook.
        /// </summary>
        LicenceUpdatedEventPublishedEventReceived = 940108,

        /// <summary>
        /// 940109 - CorrelationId is missing in Licence updated published event.
        /// </summary>
        CorrelationIdMissingInLicenceUpdatedEvent = 940109,

        /// <summary>
        /// 940110 - Storing the received Licence updated published event in azure table.
        /// </summary>
        StoreLicenceUpdatedPublishedEventInAzureTable = 940110,

        /// <summary>
        /// 940111 - Uploading the received Licence updated published event in blob storage.
        /// </summary>
        UploadLicenceUpdatedPublishedEventInAzureBlob = 940111,

        /// <summary>
        /// 940112 - Licence updated published event is uploaded in blob storage successfully.
        /// </summary>
        UploadedLicenceUpdatedPublishedEventInAzureBlob = 940112,

        /// <summary>
        /// 940113 - Licence updated published event in added in azure table successfully.
        /// </summary>
        AddedLicenceUpdatedPublishedEventInAzureTable = 940113,

        /// <summary>
        /// 940114 - Duplicate Licence updated published event received.
        /// </summary>
        ReceivedDuplicateLicenceUpdatedPublishedEvent = 940114,

        /// <summary>
        /// 940115 - Existing Licence updated published event updated in azure table successfully.
        /// </summary>
        UpdatedLicenceUpdatedPublishedEventInAzureTable = 940115,

        /// <summary>
        /// 940116 - Status of existing record of sale published event updated in azure table successfully.
        /// </summary>
        UpdatedStatusOfRecordOfSalePublishedEventInAzureTable = 940116,

        /// <summary>
        /// 940117 - Status of existing licence updated published event updated in azure table successfully.
        /// </summary>
        UpdatedStatusOfLicenceUpdatedPublishedEventInAzureTable = 940117,

        /// <summary>
        /// 940118 - The record of sale event data has been sent to SAP successfully.
        /// </summary>
        RecordOfSalePublishedEventDataPushedToSap = 940118,

        /// <summary>
        /// 940119 - The licence updated event data has been sent to SAP successfully.
        /// </summary>
        LicenceUpdatedPublishedEventUpdatePushedToSap = 940119,

        /// <summary>
        /// 940120 - An error occurred while sending record of sale published event data to SAP.
        /// </summary>
        ErrorOccurredInSapForRecordOfSalePublishedEvent = 940120,

        /// <summary>
        /// 940121 - An error occurred while sending licence updated event data to SAP.
        /// </summary>
        ErrorOccurredInSapForLicenceUpdatedPublishedEvent = 940121,

        /// <summary>
        /// 940122 - Uploading Sap Xml payload for licence updated event in Azure blob.
        /// </summary>
        UploadLicenceUpdatedSapXmlPayloadInAzureBlob = 940122,

        /// <summary>
        /// 940123 - SAP xml payload for licence updated event is uploaded in blob storage successfully.
        /// </summary>
        UploadedLicenceUpdatedSapXmlPayloadInAzureBlob = 940123,

        /// <summary>
        /// 940124 - Creating licence updated Sap Xml payload.
        /// </summary>
        CreatingLicenceUpdatedSapPayload = 940124,

        /// <summary>
        /// 940125 - Licence updated SAP xml payload created.
        /// </summary>
        CreatedLicenceUpdatedSapPayload = 940125,

        /// <summary>
        /// 940126 - Licence updated SAP message xml template does not exist.
        /// </summary>
        LicenceUpdatedSapXmlTemplateNotFound = 940126,

        /// <summary>
        /// 940127 - The record of sale SAP message xml template does not exist.
        /// </summary>
        RecordOfSaleSapXmlTemplateNotFound = 940127,

        /// <summary>
        /// 940128 - Creating the record of sale SAP Payload.
        /// </summary>
        CreatingRecordOfSaleSapPayload = 940128,

        /// <summary>
        /// 940129 - The record of sale SAP payload created.
        /// </summary>
        CreatedRecordOfSaleSapPayload = 940129,

        /// <summary>
        /// 940130 - Uploading Sap Xml payload for record of sale event in Azure blob.
        /// </summary>
        UploadRecordOfSaleSapXmlPayloadInAzureBlob = 940130,

        /// <summary>
        /// 940131 - SAP xml payload for record of sale event is uploaded in blob storage successfully.
        /// </summary>
        UploadedRecordOfSaleSapXmlPayloadInAzureBlob = 940131,

        /// <summary>
        /// 940132 - Adding record of sale event payload in Azure Queue storage.
        /// </summary>
        AddMessageToAzureQueue = 940132,

        /// <summary>
        /// 940133 - Record of sale event payload is added in queue storage successfully.
        /// </summary>
        AddedMessageToAzureQueue = 940133,

        /// <summary>
        /// 940134 - Webjob started for merging record of sale events.
        /// </summary>
        WebjobForEventAggregationStarted = 940134,

        /// <summary>
        /// 940135 - Webjob completed for merging record of sale events.
        /// </summary>
        WebjobForEventAggregationCompleted = 940135,

        /// <summary>
        /// 940136 - Webjob started downloading record of sale events from blob.
        /// </summary>
        DownloadRecordOfSaleEventFromAzureBlob = 940136,

        /// <summary>
        /// 940137 - All related events are not present in Azure blob.
        /// </summary>
        AllRelatedEventsAreNotPresentInBlob = 940137,

        /// <summary>
        /// 940138 - The record has been completed already.
        /// </summary>
        RequestAlreadyCompleted = 940138,

        /// <summary>
        /// 940139 - Exception occurred while processing Event aggregation WebJob.
        /// </summary>
        UnhandledWebJobException = 940139,

        /// <summary>
        /// 940140 - Dequeue count of message.
        /// </summary>
        MessageDequeueCount = 940140,

        /// <summary>
        /// 940142 - Exception occurred while decrypting the permit string.
        /// </summary>
        PermitDecryptionException = 940142,

        /// <summary>
        /// 940143 - Unit of Sale not found.
        /// </summary>
        UnitOfSaleNotFoundException = 940143,

        /// <summary>
        /// 940146 - ENC cell SAP action generation started.
        /// </summary>
        EncCellSapActionGenerationStarted = 940146,

        /// <summary>
        /// 940150 - SAP action generation started.
        /// </summary>
        BuildingSapActionStarted = 940150,

        /// <summary>
        /// 940151 - Required SAP property value is found empty enccontentpublished event.
        /// </summary>
        EmptyEventJsonPropertyException = 940151,

        /// <summary>
        /// 940152 - Error while generating SAP action information.
        /// </summary>
        BuildingSapActionInformationException = 940152,

        /// <summary>
        /// 940153 - Required section not found in JSON payload.
        /// </summary>
        RequiredSectionNotFound = 940153
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
