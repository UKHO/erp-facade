using System.Xml;
using UKHO.ERPFacade.API.Models;

namespace UKHO.ERPFacade.API.Helpers
{
    public interface IXmlBuilder
    {
        public XmlDocument BuildSapMessageXml(EESEvent eventData, string traceId);
    }
}
