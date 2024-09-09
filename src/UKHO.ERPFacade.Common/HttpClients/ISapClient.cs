using System.Xml;

namespace UKHO.ERPFacade.Common.HttpClients
{
    public interface ISapClient
    {
        Task<HttpResponseMessage> PostEventData(XmlDocument sapMessageXml, string endpoint, string sapServiceOperation, string username, string password);

        Uri? Uri { get; }
    }
}
