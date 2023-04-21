using System.Xml;

namespace UKHO.ERPFacade.Common.Helpers
{
    public interface IXmlHelper
    {
        public XmlDocument CreateXmlDocument(string xmlPath);
    }
}
