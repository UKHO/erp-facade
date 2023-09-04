using System.Xml;
using UKHO.ERPFacade.Common.Models;

namespace UKHO.ERPFacade.API.Helpers
{
    public interface IEncContentSapMessageBuilder
    {
        XmlDocument BuildSapMessageXml(EncEventPayload eventData, string correlationId);
    }
}
