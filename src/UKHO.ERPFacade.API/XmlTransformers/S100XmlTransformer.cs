using System.Xml;
using Microsoft.Extensions.Options;
using UKHO.ERPFacade.Common.IO;
using UKHO.ERPFacade.Common.Models;
using UKHO.ERPFacade.Common.PermitDecryption;
using UKHO.ERPFacade.Common.Providers;

namespace UKHO.ERPFacade.API.XmlTransformers
{
    public class S100XmlTransformer : BaseXmlTransformer
    {
        public S100XmlTransformer(IXmlHelper xmlHelper, IFileSystemHelper fileSystemHelper) : base(fileSystemHelper, xmlHelper)
        {
        }

        public override XmlDocument BuildXmlPayload<T>(T eventData, string xmlTemplatePath)
        {
            return new XmlDocument();
        }
    }
}
