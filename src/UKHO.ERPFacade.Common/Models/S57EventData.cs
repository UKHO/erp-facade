namespace UKHO.ERPFacade.Common.Models
{
    public class S57EventData : IEventData
    {
        public string EventType
        {
            get { return "uk.gov.ukho.encpublishing.enccontentpublished.v2"; }
        }

        public string SapXmlPath
        {
            get
            {
                return "SapXmlTemplates\\SAPS57Request.xml";
            }
        }

        public string XpathActionItems
        {
            get
            {
                return $"//*[local-name()='ACTIONITEMS']";
            }
        }

        public string XpathImMatInfo
        {
            get
            {
                return $"//*[local-name()='IM_MATINFO']";
            }
        } 
        public string SapEndpointForEvent { get ; set; }
        public string SapServiceOperationForEvent { get; set; }
        public string SapUsernameForEvent { get; set; }
        public string SapPasswordForEvent { get; set; }
        public string CorrelationId
        {
            get
            {
                return EventData.Data.CorrelationId;
            }
        }
        public dynamic EventData { get; set; }
    }
}
