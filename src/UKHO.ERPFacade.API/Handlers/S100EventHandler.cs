using UKHO.ERPFacade.API.XmlTransformers;
using UKHO.ERPFacade.Common.Constants;
using UKHO.ERPFacade.Common.Models.CloudEvents;

namespace UKHO.ERPFacade.API.Handlers
{
    public class S100EventHandler : IEventHandler
    {
        private readonly IBaseXmlTransformer _baseXmlTransformer;

        public S100EventHandler([FromKeyedServices("S100XmlTransformer")] IBaseXmlTransformer baseXmlTransformer)
        {
            _baseXmlTransformer = baseXmlTransformer;
        }


        public async Task ProcessEventAsync(BaseCloudEvent s100EventPayload)
        {
            var xmlPaylod = _baseXmlTransformer.BuildXmlPayload(s100EventPayload.Data, Constants.S57SapXmlTemplatePath);
            await Task.CompletedTask;
        }
    }
}
