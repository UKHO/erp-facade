using System.Xml;
using UKHO.ERPFacade.Common.IO;

namespace UKHO.ERPFacade.API.XmlTransformers
{
    public class S100XmlTransformer : BaseXmlTransformer
    {
        public S100XmlTransformer(IXmlHelper xmlHelper, IFileSystemHelper fileSystemHelper) : base(fileSystemHelper, xmlHelper)
        {
        }
        public override XmlDocument BuildXmlPayload<T>(T eventData, string xmlTemplatePath) => throw new NotImplementedException();
    }
}
