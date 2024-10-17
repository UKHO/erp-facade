using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UKHO.ERPFacade.API.Helpers;
using UKHO.ERPFacade.Common.Constants;
using UKHO.ERPFacade.Common.Models;

namespace UKHO.ERPFacade.API.Handlers
{
    public class S57EventHandler : IEventHandler
    {
        private readonly IS57XmlTransformer _s57XmlTransformer;

        public string EventType => "uk.gov.ukho.encpublishing.enccontentpublished.v2.2";

        public S57EventHandler(IS57XmlTransformer s57XmlTransformer)
        {
            _s57XmlTransformer = s57XmlTransformer;
        }

        public Task HandleEventAsync(JObject payload)
        {
            _s57XmlTransformer.BuildSapMessageXml(JsonConvert.DeserializeObject<EncEventPayload>(payload.ToString()), Constants.S57SapXmlTemplatePath);
            return Task.CompletedTask;
        }
    }
}
