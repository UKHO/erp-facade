namespace UKHO.ERPFacade.Common.Logging
{
    public enum EventIds
    {
        /// <summary>
        /// 920001 - An unhandled exception occurred while processing the request.
        /// </summary>
        UnhandledControllerException = 920001,
        /// <summary>
        /// 920002 - New ENC content published event options call started.
        /// </summary>
        NewEncContentPublishedEventOptionsCallStarted = 920002,
        /// <summary>
        /// 920003 - New ENC content published event options call completed.
        /// </summary>
        NewEncContentPublishedEventOptionsCallCompleted = 920003,
        /// <summary>
        /// 920004 - New ENC content published event received by ERP Facade webhook.
        /// </summary>
        NewEncContentPublishedEventReceived = 920004,
        /// <summary>
        /// 920005 - Trace ID not found.
        /// </summary>
        CheckNullTraceId = 920005,
        /// <summary>
        /// 920006 - Uploading the received ENC content published event in Azure Blob storage.
        /// </summary>
        UploadEncContentPublishedEventInAzureBlob = 920006,
        /// <summary>
        /// 920007 - Uploaded ENC content published event in Azure Blob storage successfully.
        /// </summary>
        UploadedEncContentPublishedEventInAzureBlob = 920007,
        /// <summary>
        /// 920008 - Storing the received ENC content published event in Azure table storage.
        /// </summary>
        StoreEncContentPublishedEventInAzureTable = 920008,
        /// <summary>
        /// 920009 - Added new ENC content published event in Azure table storage successfully.
        /// </summary>
        AddedEncContentPublishedEventInAzureTable = 920009,
        /// <summary>
        /// 920010 - ENC content published event already exists!.
        /// </summary>
        CheckDuplicateEncContentPublishedEvent = 920010,
        /// <summary>
        /// 920011 - Updated the existing ENC content published event in Azure table storage successfully.
        /// </summary>
        UpdatedEncContentPublishedEventInAzureTable = 920011,
        
    }
}
