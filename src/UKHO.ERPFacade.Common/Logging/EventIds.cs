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
        /// 940006 - Storing the received ENC content published event in azure table.
        /// </summary>
        StoreEncContentPublishedEventInAzureTable = 940006,

        /// <summary>
        /// 940007 - ENC content published event in added in azure table successfully.
        /// </summary>
        AddedEncContentPublishedEventInAzureTable = 940007,

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
        UploadEncContentPublishedEventInAzureBlob = 940010,

        /// <summary>
        /// 940011 - ENC content published event is uploaded in blob storage successfully.
        /// </summary>
        UploadedEncContentPublishedEventInAzureBlob = 940011,

        /// <summary>
        /// 940012 - Error occured while connecting SAP
        /// </summary>
        ErrorOccuredInSap = 940012,

        /// <summary>
        /// 940013 - ENC Update pushed to SAP
        /// </summary>
        EncUpdatePushedToSap = 940013,

        /// <summary>
        /// 940014 - Webjob started to process the incomplete transactions.
        /// </summary>
        WebjobProcessEventStarted = 940014,

        /// <summary>
        /// 940015 - Webjob completed to process the incomplete transactions.
        /// </summary>
        WebjobProcessEventCompleted = 940015,

        /// <summary>
        /// 940016 - Callback from SAP is timed out.
        /// </summary>
        WebjobCallbackTimeoutEventFromSAP = 940016,

        /// <summary>
        /// 940017 - Empty or null RequestDateTime Column in Azure Table.
        /// </summary>
        EmptyRequestDateTime = 940017,

        /// <summary>
        /// 940018 - Updated RequestTime entity successfully in Azure Table
        /// </summary>
        UpdateRequestTimeEntitySuccessful = 940018,

        /// <summary>
        /// 940019 - Updated ResponseTime entity successfully in Azure Table
        /// </summary>
        UpdateResponseTimeEntitySuccessful = 940019,

        /// <summary>
        /// 940020 - Updated entity successfully in Azure Table
        /// </summary>
        UpdateEntitySuccessful = 940020,

        /// <summary>
        /// 940021 - CorrelationId is missing in price information payload recieved from SAP.
        /// </summary>
        CorrelationIdMissingInSAPPriceInformationPayload = 940021,

        /// <summary>
        /// 940022 - Invalid SAP callback. Request from ERP Facade to SAP not found for CorrelationId.
        /// </summary>
        ERPFacadeToSAPRequestNotFound = 940022,

        /// <summary>
        /// 940023 - Valid SAP callback for CorrelationId.
        /// </summary>
        ERPFacadeToSAPRequestFound = 940023,

        /// <summary>
        /// 940024 - No Scenario Found
        /// </summary>
        NoScenarioFound = 940024,

        /// <summary>
        /// 940025 - Sap Xml Template Not Found
        /// </summary>
        SapXmlTemplateNotFound = 940025,

        /// <summary>
        /// 940026 - Building Sap Actions Started
        /// </summary>
        BuildingSapActionStarted = 940026,

        /// <summary>
        /// 940027 - Sap Action Created
        /// </summary>
        SapActionCreated = 940027,

        /// <summary>
        /// 940028 - Identifying Scenario Started
        /// </summary>
        IdentifyScenarioStarted = 940028,

        /// <summary>
        /// 940029 - Scenario Identified
        /// </summary>
        ScenarioIdentified = 940029,

        /// <summary>
        /// 940030 - Azure Table Not Found
        /// </summary>
        AzureTableNotFound = 940030,

        /// <summary>
        /// 940031 - Environment Name
        /// </summary>
        EnvironmentName = 940031,

        /// <summary>
        /// 940032 - UnitOfSale price information payload received from SAP
        /// </summary>
        SapUnitsOfSalePriceInformationPayloadReceived = 940032,

        /// <summary>
        /// 940033 - Downloading existing ees event from azure blob storage
        /// </summary>
        DownloadEncEventPayloadStarted = 940033,

        /// <summary>
        /// 940034 - Downloaded existing ees event from azure blob storage successfully
        /// </summary>
        DownloadEncEventPayloadCompleted = 940034,

        /// <summary>
        /// 940035 - No price information found in incoming SAP event
        /// </summary>
        NoDataFoundInSAPPriceInformationPayload = 940035,

        /// <summary>
        /// 940036 - Building unit of sale price event started
        /// </summary>
        AppendingUnitofSalePricesToEncEvent = 940036,

        /// <summary>
        /// 940037 - Unit of sale price event created
        /// </summary>
        UnitsOfSaleUpdatedEventPayloadCreated = 940037,

        /// <summary>
        /// 940038 - UnitsOfSale updated event exceeds the size limit of 1 MB
        /// </summary>
        UnitsOfSaleUpdatedEventSizeLimit = 940038,

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
        /// 940043 - UnitsOfSale updated event Pushed To EES
        /// </summary>
        UnitsOfSaleUpdatedEventPushedToEES = 940043,

        /// <summary>
        /// 940044 - Attempting to send cloudEvent to Enterprise Event Service
        /// </summary>
        StartingEnterpriseEventServiceEventPublisher = 940044,

        /// <summary>
        /// 940045 - Successfully sent cloudEvent to Enterprise Event Service
        /// </summary>
        EnterpriseEventServiceEventPublisherSuccess = 940045,

        /// <summary>
        /// 940046 - Failed to send event cloudEvent to the enterprise event service
        /// </summary>
        EnterpriseEventServiceEventPublisherFailure = 940046,

        /// <summary>
        /// 940047 - Webjob started building pricechange event
        /// </summary>
        WebjobStartedBuildingPriceChangeEvent = 940047,

        /// <summary>
        /// 940048 - Updated Price change status entity successfully in Azure Table
        /// </summary>
        UpdatedPriceChangeStatusEntitySuccessful = 940048,

        /// <summary>
        /// 940049 - Updated Price master status entity successfully in Azure Table
        /// </summary>
        UpdatedPriceChangeMasterStatusEntitySuccessful = 940049,

        /// <summary>
        /// 940050 - Downloading the price change information event from blob storage.
        /// </summary>
        DownloadBulkPriceInformationEventFromAzureBlob = 940050,

        /// <summary>
        /// 940051 - Sliced event is uploaded in blob storage successfully.
        /// </summary>
        UploadedSlicedEventInAzureBlob = 940051,

        /// <summary>
        /// 940052 - Sliced event is uploaded in blob storage successfully for incomplete unit prices.
        /// </summary>
        UploadedSlicedEventInAzureBlobForUnitPrices = 940052,

        /// <summary>
        /// 940053 - Bulk price information payload received from SAP
        /// </summary>
        SapBulkPriceInformationPayloadReceived = 940053,

        /// <summary>
        /// 940054 - Storing the received Bulk price information event in azure table.
        /// </summary>
        StoreBulkPriceInformationEventInAzureTable = 940054,

        /// <summary>
        /// 940055 - Uploading the received Bulk price information event in blob storage.
        /// </summary>
        UploadBulkPriceInformationEventInAzureBlob = 940055,

        /// <summary>
        /// 940056 - Bulk price information event is uploaded in blob storage successfully.
        /// </summary>
        UploadedBulkPriceInformationEventInAzureBlob = 940056,

        /// <summary>
        /// 940057 - Bulk price information event in added in azure table successfully.
        /// </summary>
        AddedBulkPriceInformationEventInAzureTable = 940057,

        /// <summary>
        /// 940058 - Webjob started to process the publishing price changes.
        /// </summary>
        WebjobPublishingPriceChangesEventStarted = 940058,

        /// <summary>
        /// 940059 - Webjob completed processing the publishing price changes.
        /// </summary>
        WebjobPublishingPriceChangesEventCompleted = 940059,

        /// <summary>
        /// 940060 - Unit Price Change event in added in azure table successfully.
        /// </summary>
        AddedUnitPriceChangeEventInAzureTable = 940060,

        /// <summary>
        /// 940061 - UnitsOfSale NotFound In SAP PriceInformation Payload
        /// </summary>
        UnitsOfSaleNotFoundInSAPPriceInformationPayload = 940061,

        /// <summary>
        /// 940062 - Uploading the received Price information event in blob storage.
        /// </summary>
        UploadPriceInformationEventInAzureBlob = 940062,

        /// <summary>
        /// 940063 - Price information event is uploaded in blob storage successfully.
        /// </summary>
        UploadedPriceInformationEventInAzureBlob = 940063,

        /// <summary>
        /// 940064 - Fetching master entities from azure table.
        /// </summary>
        FetchMasterEntities = 940064,

        /// <summary>
        /// 940065 - Fetching create date of blob.
        /// </summary>
        FetchBlobCreateDate = 940065,

        /// <summary>
        /// 940066 - Deleted Price master entity successfully from Azure Table
        /// </summary>
        DeletedPriceChangeMasterEntitySuccessful = 940066,

        /// <summary>
        /// 940067 - Deleted unit price change entity successfully from Azure Table
        /// </summary>
        DeletedUnitPriceChangeEntitySuccessful = 940067,

        /// <summary>
        /// 940068 - Deleted EES entity successfully from Azure Table
        /// </summary>
        DeletedEESEntitySuccessful = 940068,

        /// <summary>
        /// 940069 - Fetching all blob present inside the container.
        /// </summary>
        FetchBlobsFromContainer = 940069,

        /// <summary>
        /// 940070 - Deleted blob from storage container successfully.
        /// </summary>
        DeletedBlobSuccessful = 940070,

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
        /// 940075 - Error occured while connecting EES
        /// </summary>
        ErrorOccuredInEES = 940075,

        /// <summary>
        /// 940076 - Uploading the pricechange event payload json in blob storage.
        /// </summary>
        UploadPriceChangeEventPayloadInAzureBlob = 940076,

        /// <summary>
        /// 940077 - pricechange event payload json is uploaded in blob storage successfully.
        /// </summary>
        UploadedPriceChangeEventPayloadInAzureBlob = 940077,

        /// <summary>
        /// 940079 - pricechange event created
        /// </summary>
        PriceChangeEventPayloadCreated = 940079,

        /// <summary>
        /// 940080 - Uploading the UnitsOfSale updated event payload json in blob storage.
        /// </summary>
        UploadUnitsOfSaleUpdatedEventPayloadInAzureBlob = 940080,

        /// <summary>
        /// 940081 - UnitsOfSale updated event payload json is uploaded in blob storage successfully.
        /// </summary>
        UploadedUnitsOfSaleUpdatedEventPayloadInAzureBlob = 940081,

        /// <summary>
        /// 940082 - Updated PublishDateTime entity successfully in Azure Table
        /// </summary>
        UpdatePublishDateTimeEntitySuccessful = 940082,

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
        /// 940089 - Failed to connect to the enterprise event service
        /// </summary>
        EnterpriseEventServiceEventConnectionFailure = 940089,

        /// <summary>
        /// 940090 - Count of products to be sliced
        /// </summary>
        ProductsToSliceCount = 940090,

        /// <summary>
        /// 940091 - Count of products that are published and unpublished
        /// </summary>
        ProductsPublishedUnpublishedCount = 940091,

        /// <summary>
        /// 940092 - Count of unpublished products
        /// </summary>
        ProductsUnpublishedCount = 940092,

        /// <summary>
        /// 940093 - Uploading the Sliced Price information event in blob storage.
        /// </summary>
        UploadSlicedPriceInformationEventInAzureBlob = 940093,

        /// <summary>
        /// 940094 - Sliced Price information event is uploaded in blob storage successfully.
        /// </summary>
        UploadedSlicedPriceInformationEventInAzureBlob = 940094,

        /// <summary>
        /// 940095 - Count of pending products to be sliced
        /// </summary>
        PendingProductsToSliceCount = 940095,

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
        UploadedRecordOfSaleSapXmlPayloadInAzureBlob = 940131
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
