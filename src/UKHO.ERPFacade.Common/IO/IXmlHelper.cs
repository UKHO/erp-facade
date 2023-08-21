using System.Xml;

namespace UKHO.ERPFacade.Common.IO
{
    public interface IXmlHelper
    {
        public XmlDocument CreateXmlDocument(string xmlPath);
        public string CreateRecordOfSaleSapXmlPayLoad<T>(T anyobject);
    }
}
