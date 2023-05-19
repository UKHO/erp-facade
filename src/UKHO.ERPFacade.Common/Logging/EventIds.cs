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
        /// 940005 - TraceId is missing in ENC content published event.
        /// </summary>
        TraceIdMissingInEvent = 940005,

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
        /// 940012 - Could not connect to SAP
        /// </summary>
        SapConnectionFailed = 940012,

        /// <summary>
        /// 940013 - Data pushed to SAP
        /// </summary>
        DataPushedToSap = 940013,

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
        /// 940021 - Correlation Id is missing in the event received from SAP
        /// </summary>
        CorrIdMissingInSAPEvent = 940021,
        /// <summary>
        /// 940022 - Blob does not exist for traceID given in SAP event.
        /// </summary>
        BlobNotFoundInAzure = 940022,

        /// <summary>
        /// 940023 - Blob exists for traceID given in SAP event.
        /// </summary>
        BlobExistsInAzure = 940023,

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
        /// 940031 - UnitOfSale price event received from SAP
        /// </summary>
        SapUnitOfSalePriceEventReceived = 940031,

        /// <summary>
        /// 940032 - Downloading exisiting ees event from azure blob storage
        /// </summary>
        DownloadExistingEesEventFromBlob = 940032,

        /// <summary>
        /// 940033 - Downloaded exisiting ees event from azure blob storage successfully
        /// </summary>
        DownloadedExistingEesEventFromBlob = 940033,

        /// <summary>
        /// 940034 - No price information found in incoming SAP event
        /// </summary>
        NoPriceInformationFound = 940034,

        /// <summary>
        /// 940035 - Building unit of sale price event started
        /// </summary>
        BuildingPriceEventStarted = 940035,

        /// <summary>
        /// 940036 - Unit of sale price event created
        /// </summary>
        PriceEventCreated = 940036,

        /// <summary>
        /// 940037 - Bulk price event received from SAP
        /// </summary>
        SapBulkPriceEventReceived = 940037,

        /// <summary>
        /// 940038 - No bulk price information found in incoming SAP event
        /// </summary>
        NoBulkPriceInformationFound = 940038,

        /// <summary>
        /// 940039 - Building bulk price event started
        /// </summary>
        BuildingBulkPriceEventStarted = 940039,

        /// <summary>
        /// 940040 - Bulk price event created
        /// </summary>
        BulkPriceEventCreated = 940040,

        /// <summary>
        /// 940041 - Price Event exceeds the size limit of 1 MB
        /// </summary>
        PriceEventExceedSizeLimit = 940041
    }
}