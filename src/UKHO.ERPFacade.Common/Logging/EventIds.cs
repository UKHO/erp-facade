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
        /// 920005 - TraceId is missing in ENC content published event.
        /// </summary>
        TraceIdMissingInEvent = 920005,
        /// <summary>
        /// 920006 - Storing the received ENC content published event in azure table.
        /// </summary>
        StoreEncContentPublishedEventInAzureTable = 920006,
        /// <summary>
        /// 920007 - ENC content published event in added in azure table successfully.
        /// </summary>
        AddedEncContentPublishedEventInAzureTable = 920007,
        /// <summary>
        /// 920008 - Duplicate ENC contect published event received.
        /// </summary>
        ReceivedDuplicateEncContentPublishedEvent = 920008,
        /// <summary>
        /// 920009 - Existing ENC content published event updated in azure table successfully.
        /// </summary>
        UpdatedEncContentPublishedEventInAzureTable = 920009,
        /// <summary>
        /// 920010 - Uploading the received ENC content published event in blob storage.
        /// </summary>
        UploadEncContentPublishedEventInAzureBlob = 920010,
        /// <summary>
        /// 920011 - ENC content published event is uploaded in blob storage successfully.
        /// </summary>
        UploadedEncContentPublishedEventInAzureBlob = 920011,
    }
}
