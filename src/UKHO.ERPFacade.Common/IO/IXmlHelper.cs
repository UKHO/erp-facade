using System.Xml;

namespace UKHO.ERPFacade.Common.IO
{
    public interface IXmlHelper
    {
        public XmlDocument CreateXmlDocument(string xmlPath);
        public string CreateXmlPayLoad<T>(T anyobject);
        void AppendChildNode(XmlElement parentNode, XmlDocument doc, string nodeName, string value);
    }
}
