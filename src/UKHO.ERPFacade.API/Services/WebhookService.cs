using CloudNative.CloudEvents;
using Microsoft.Extensions.Options;
using UKHO.ERPFacade.API.Handler;
using UKHO.ERPFacade.Common.Configuration;

namespace UKHO.ERPFacade.API.Services
{
    public class WebhookService : IWebhookService
    {
        private readonly IOptions<SapConfiguration> _sapConfig;

        private readonly IServiceProvider _serviceProvider;
        public WebhookService(IServiceProvider serviceProvider, IOptions<SapConfiguration> sapConfig)
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

            await type.HandleEvent(payloadJson);
        }
    }
}
