using System.Xml;
using UKHO.ERPFacade.Common.Models;

namespace UKHO.ERPFacade.API.Helpers
{
    public interface IS100ContentSapMessageBuilder
    {
        XmlDocument BuildSapMessageXml(S100EventPayload eventData, string correlationId);
    }
}
