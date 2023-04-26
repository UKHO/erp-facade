namespace UKHO.ERPFacade.Common.Logging
{
    public enum EventIds
    {
        /// <summary>
        /// 940001 - An unhandled exception occurred while processing the request.
        /// </summary>
        UnhandledControllerException = 940001,
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
        /// 940012 - Webjob started to process the incomplete transactions.
        /// </summary>
        WebjobProcessEventStarted = 940012,
        /// <summary>
        /// 940013 - Callback from SAP is timed out.
        /// </summary>
        WebjobCallbackTimeoutEventFromSAP= 940013,
        /// <summary>
        /// 940014 - Empty or null RequestDateTime Column in Azure Table.
        /// </summary>
        EmptyRequestDateTime = 940014,
    }
}
