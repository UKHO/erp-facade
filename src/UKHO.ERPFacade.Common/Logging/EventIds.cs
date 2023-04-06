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
        NewEncContentPublishedEventReceived = 920004
    }
}
