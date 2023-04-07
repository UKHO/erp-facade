using System.Xml;

namespace UKHO.ERPFacade.Common.Helpers
{
    public interface ISapClient
    {
        Task<HttpResponseMessage> PostEventData(XmlDocument sapMessageXml, string sapServiceOperation);
    }
}
