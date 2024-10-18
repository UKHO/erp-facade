
namespace UKHO.ERPFacade.Common.Models
{
    public interface IEventData
    {
        string EventType { get; }
        string SapXmlPath { get; }        
        string XpathActionItems { get; }
        string XpathImMatInfo { get; }
        string SapEndpointForEvent { get; set; }
        string SapServiceOperationForEvent { get; set; }
        string SapUsernameForEvent { get; set; }
        string SapPasswordForEvent { get; set; }
        string CorrelationId { get; }
        dynamic EventData { get; set; } 
    }
}
