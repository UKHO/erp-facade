using System.Xml;

namespace UKHO.ERPFacade.Common.HttpClients
{
    public interface ISapClient
    {
        Task<HttpResponseMessage> PostEventData(XmlDocument sapMessageXml, string sapServiceOperation);
    }
}
