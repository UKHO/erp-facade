

using CloudNative.CloudEvents;
using Newtonsoft.Json;
using UKHO.ERPFacade.API.XmlTransformers;
using UKHO.ERPFacade.Common.Constants;
using UKHO.ERPFacade.Common.Models;

namespace UKHO.ERPFacade.API.Handlers
{
    public class S100EventHandler : IEventHandler
    {
        public string EventType => "uk.gov.UKHO.ENCPublishing.s100DataContentPublished.v1";

        private readonly IBaseXmlTransformer _baseXmlTransformer;
        public S100EventHandler([FromKeyedServices("S100XmlTransformer")] IBaseXmlTransformer baseXmlTransformer)
        {
            _baseXmlTransformer = baseXmlTransformer;
        }
        public Task HandleEventAsync(CloudEvent payload)
        {
            EncEventPayload eventData = new EncEventPayload()
            {
                SpecVersion = payload.SpecVersion.ToString(),
                Type = payload.Type.ToString(),
                Source = payload.Source.ToString(),
                Id = payload.Id.ToString(),
                Time = payload.Time.ToString(),
                Subject = payload.Subject.ToString(),
                DataContentType = payload.DataContentType.ToString(),
                Data = JsonConvert.DeserializeObject<EesEventData>(payload.Data.ToString())
            };

            _baseXmlTransformer.BuildSapMessageXml(eventData, Constants.S57SapXmlTemplatePath);

            return Task.CompletedTask;
        }
    }
}
