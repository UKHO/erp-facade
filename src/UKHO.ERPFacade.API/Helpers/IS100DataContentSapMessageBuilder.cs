using System.Xml;

namespace UKHO.ERPFacade.API.Helpers
{
    public interface IS100DataContentSapMessageBuilder
    {
        XmlDocument BuildS100SapMessageXml(string s100EventData);
    }
}
