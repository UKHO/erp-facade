using UKHO.ERPFacade.API.XmlTransformers;
using UKHO.ERPFacade.Common.Constants;
using UKHO.ERPFacade.Common.Models.CloudEvents.S100;

namespace UKHO.ERPFacade.API.Handlers
{
    public class S100EventHandler : IEventHandler<S100Event>
    {
        private readonly IBaseXmlTransformer _baseXmlTransformer;

        public S100EventHandler([FromKeyedServices("S100XmlTransformer")] IBaseXmlTransformer baseXmlTransformer)
        {
            _baseXmlTransformer = baseXmlTransformer;
        }

        public async Task ProcessEventAsync(S100Event s100EventPayload)
        {
            var xmlPaylod = _baseXmlTransformer.BuildXmlPayload(s100EventPayload.Data, Constants.S57SapXmlTemplatePath);
            await Task.CompletedTask;
        }
    }
}
