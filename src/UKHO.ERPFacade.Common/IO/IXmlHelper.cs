using System.Xml;

namespace UKHO.ERPFacade.Common.IO
{
    public interface IXmlHelper
    {
        XmlDocument CreateXmlDocument(string xmlPath);
        string CreateXmlPayLoad<T>(T anyobject);
        void AppendChildNode(XmlElement parentNode, XmlDocument doc, string nodeName, string value);
    }
}
