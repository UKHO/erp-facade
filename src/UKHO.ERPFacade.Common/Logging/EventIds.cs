namespace UKHO.ERPFacade.Common.Logging
{
    public enum EventIds
    {
        /// <summary>
        /// 920001 - An unhandled exception occurred while processing the request.
        /// </summary>
        UnhandledControllerException = 920001,
        /// <summary>
        /// 920002 - New ENS event published webhook options call started.
        /// </summary>
        NewEnsEventPublishedWebhookOptionsCallStarted = 920002,
        /// <summary>
        /// 920003 - New ENS event published webhook options call completed.
        /// </summary>
        NewEnsEventPublishedWebhookOptionsCallCompleted = 920003,
        /// <summary>
        /// 920004 - New ENS event received by ERP Facade webhook.
        /// </summary>
        NewEnsEventReceived = 920004
    }
}
