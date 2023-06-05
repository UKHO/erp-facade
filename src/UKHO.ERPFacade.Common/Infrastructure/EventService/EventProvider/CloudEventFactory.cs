using Microsoft.Extensions.Options;
using UKHO.ERPFacade.Common.Infrastructure.Config;
using UKHO.ERPFacade.Common.Providers;

namespace UKHO.ERPFacade.Common.Infrastructure.EventService.EventProvider
{
    

    public class CloudEventFactory : ICloudEventFactory
    {
        private readonly IDateTimeProvider _dateTimeProvider;
        private readonly IUniqueIdentifierFactory _uuidProvider;
        private readonly NotificationsConfiguration _notificationOptions;

        public CloudEventFactory(IDateTimeProvider dateTimeProvider, IUniqueIdentifierFactory uuidProvider, IOptions<NotificationsConfiguration> notificationOptions)
        {
            _dateTimeProvider = dateTimeProvider;
            _uuidProvider = uuidProvider;
            _notificationOptions = notificationOptions.Value;
        }

        public CloudEvent<TData> Create<TData>(EventBase<TData> domainEvent)
        {
            var cloudEventData = new CloudEvent<TData>
            {
                Data = domainEvent.EventData,
                Type = domainEvent.EventName,
                Subject = domainEvent.Subject,
                Time = _dateTimeProvider.UtcNow,
                Id = _uuidProvider.Create(),
                Source = _notificationOptions.ApplicationUri,
                SpecVersion = "1.0",
                DataContentType = "application/json",
            };

            return cloudEventData;
        }
    }
}
