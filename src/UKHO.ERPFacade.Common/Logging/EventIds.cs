namespace UKHO.ERPFacade.Common.Logging
{
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
        /// 940014- Webjob started to process the incomplete transactions.
        /// </summary>
        WebjobProcessEventStarted = 940014,

        /// <summary>
        /// 940015- Webjob completed to process the incomplete transactions.
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
        /// 940041 - Uploading the UnitsOfSale updated event payload json in blob storage.
        /// </summary>
        UploadUnitsOfSaleUpdatedEventPayloadInAzureBlob = 940041,

        /// <summary>
        /// 940042 - UnitsOfSale updated event payload json is uploaded in blob storage successfully.
        /// </summary>
        UploadedUnitsOfSaleUpdatedEventPayloadInAzureBlob = 940042,

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
        /// 940047 - Building unit of sale price event started in webjob
        /// </summary>
        AppendingUnitofSalePricesToEncEventInWebJob = 940047,

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
        /// 940058- Webjob started to process the publishing price changes.
        /// </summary>
        WebjobPublishingPriceChangesEventStarted = 940058,

        /// <summary>
        /// 940059- Webjob completed processing the publishing price changes.
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
        /// 940062 - Error occured while connecting EES
        /// </summary>
        ErrorOccuredInEES = 940062,

        /// <summary>
        /// 940063 - Uploading the pricechange event payload json in blob storage.
        /// </summary>
        UploadPriceChangeEventPayloadInAzureBlob = 940063,

        /// <summary>
        /// 940064 - pricechange event payload json is uploaded in blob storage successfully.
        /// </summary>
        UploadedPriceChangeEventPayloadInAzureBlob = 940064,

        /// <summary>
        /// 940065 - pricechange event Pushed To EES
        /// </summary>
        PriceChangeEventPushedToEES = 940065,

        /// <summary>
        /// 940066 - pricechange event created
        /// </summary>
        PriceChangeEventPayloadCreated = 940066
    }
}
