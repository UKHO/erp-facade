
using CloudNative.CloudEvents;
using Microsoft.Extensions.Options;
using UKHO.ERPFacade.API.Handler;
using UKHO.ERPFacade.API.Helpers;
using UKHO.ERPFacade.Common.Configuration;
using UKHO.ERPFacade.Common.Models;
using UKHO.ERPFacade.Common.Models.S100Event;

namespace UKHO.ERPFacade.API.Services
{
    public class WebhookService : IWebhookService
    {        
        private readonly IOptions<SapConfiguration> _sapConfig;

        private readonly IKeyedServiceProvider _serviceProvider;
        public WebhookService(IKeyedServiceProvider serviceProvider, IOptions<SapConfiguration> sapConfig)
        {            
            _sapConfig = sapConfig;
            _serviceProvider = serviceProvider;
        }

        public async Task HandleEvent(string payloadJson, CloudEvent payload)
        {
            if (string.IsNullOrEmpty(payloadJson))
            {
                throw new ArgumentException($"'{nameof(payloadJson)}' cannot be null or empty.", nameof(payloadJson));
            }

            if (payload is null)
            {
                throw new ArgumentNullException(nameof(payload));
            }

            var eventType = payload.Type;            

            IEventHandler type = _serviceProvider.GetKeyedService<IEventHandler>(eventType);
            S57EventData eventData = new S57EventData();
            
            eventData.EventData = (EncEventPayload)payload.Data;

            await type.HandleEvent(payloadJson, eventData);           
        }        
    }
}
