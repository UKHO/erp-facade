using Microsoft.Extensions.Options;
using UKHO.ERPFacade.Common.Infrastructure.Config;
using UKHO.ERPFacade.Common.Providers;

namespace UKHO.ERPFacade.Common.Infrastructure.EventService.EventProvider
{
    public class CloudEventFactory : ICloudEventFactory
    {
        private readonly IDateTimeProvider _dateTimeProvider;
        private readonly EnterpriseEventServiceConfiguration _erpPublishEventSource;

        public CloudEventFactory(IDateTimeProvider dateTimeProvider, IOptions<EnterpriseEventServiceConfiguration> erpPublishEventSource)
        {
            _dateTimeProvider = dateTimeProvider;
            _erpPublishEventSource = erpPublishEventSource.Value;
        }

        public CloudEvent<TData> Create<TData>(EventBase<TData> domainEvent)
        {
            var cloudEventData = new CloudEvent<TData>
            {
                Data = domainEvent.Data,
                Type = domainEvent.EventName,
                Subject = domainEvent.Subject,
                Time = _dateTimeProvider.UtcNow,
                Id = domainEvent.Id,
                Source = _erpPublishEventSource.ApplicationUri,
                SpecVersion = "1.0",
                DataContentType = "application/json",
            };

            return cloudEventData;
        }
    }
}
