using System.Xml;
using UKHO.ERPFacade.API.Models;

namespace UKHO.ERPFacade.API.Helpers
{
    public interface ISapMessageBuilder
    {
        XmlDocument BuildSapMessageXml(EncEventPayload eventData, string correlationId);
    }
}
