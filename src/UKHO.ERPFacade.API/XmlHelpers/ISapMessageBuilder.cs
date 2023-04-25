using System.Xml;
using UKHO.ERPFacade.API.Models;

namespace UKHO.ERPFacade.API.XmlHelpers
{
    public interface ISapMessageBuilder
    {
        XmlDocument BuildSapMessageXml(string messageTemplateName, EESEvent eesEventtData);
    }
}
