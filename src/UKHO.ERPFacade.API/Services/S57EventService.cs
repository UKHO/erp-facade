using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UKHO.ERPFacade.API.Helpers;
using UKHO.ERPFacade.Common.Constants;
using UKHO.ERPFacade.Common.Models;

namespace UKHO.ERPFacade.API.Handlers
{
    public class S57EventService : IEventHandler
    {
        private readonly EncContentSapMessageBuilder _encContentSapMessageBuilder;
        public string EventType => "uk.gov.ukho.encpublishing.enccontentpublished.v2.2";

        public S57EventService(EncContentSapMessageBuilder encContentSapMessageBuilder)
        {
            _encContentSapMessageBuilder = encContentSapMessageBuilder;
        }

        public Task HandleEventAsync(JObject payload)
        {
            _encContentSapMessageBuilder.BuildSapMessageXml(JsonConvert.DeserializeObject<EncEventPayload>(payload.ToString()), Constants.S57SapXmlTemplatePath);
            return Task.CompletedTask;
        }
    }
}
