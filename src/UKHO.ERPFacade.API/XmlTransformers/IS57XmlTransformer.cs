using System.Xml;
using UKHO.ERPFacade.Common.Models;

namespace UKHO.ERPFacade.API.Helpers
{
    public interface IS57XmlTransformer
    {
        XmlDocument BuildSapMessageXml(EncEventPayload eventData, string templatePath);
    }
}
