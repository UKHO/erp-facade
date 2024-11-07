using System.Xml;
using UKHO.ERPFacade.Common.Operations;
using UKHO.ERPFacade.Common.Operations.IO;

namespace UKHO.ERPFacade.API.XmlTransformers
{
    public class S100XmlTransformer : BaseXmlTransformer
    {
        public S100XmlTransformer(IXmlOperations xmlOperations, IFileOperations fileOperations) : base() { }

        public override XmlDocument BuildXmlPayload<T>(T eventData, string xmlTemplatePath)
        {
            return new XmlDocument();
        }
    }
}
