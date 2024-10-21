namespace UKHO.ERPFacade.Common.Models
{
    public class S100EventData : IEventData
    {
        public string EventType
        {
            get { return "uk.gov.ukho.encpublishing.enccontentpublished.v2"; }
        }

        public string SapXmlPath
        {
            get
            {
                return "SapXmlTemplates\\S100SapActions.xml";
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

        public string SapEndpointForEvent { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public string SapServiceOperationForEvent { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public string SapUsernameForEvent { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public string SapPasswordForEvent { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public string CorrelationId
        {
            get
            {
                return EventData.S100EventData.CorrelationId;
            }
        }
        public dynamic EventData { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
    }
}
