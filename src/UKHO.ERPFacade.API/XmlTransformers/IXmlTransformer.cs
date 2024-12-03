using System.Xml;
using UKHO.ERPFacade.Common.Models.SapActionConfigurationModels;

namespace UKHO.ERPFacade.API.XmlTransformers
{
    public interface IXmlTransformer
    {
        XmlDocument BuildXmlPayload<T>(T eventData, string xmlTemplatePath);
        bool ValidateActionRules(Actions action, object obj);
        void FinalizeSapXmlMessage(XmlDocument soapXml, string correlationId, XmlNode actionItemNode, string xmlPathInfo);
    }

}
