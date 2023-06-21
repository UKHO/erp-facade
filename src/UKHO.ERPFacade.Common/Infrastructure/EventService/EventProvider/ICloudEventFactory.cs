namespace UKHO.ERPFacade.Common.Infrastructure.EventService.EventProvider
{
    public interface ICloudEventFactory
    {
        CloudEvent<TData> Create<TData>(EventBase<TData> domainEvent);
    }
}
