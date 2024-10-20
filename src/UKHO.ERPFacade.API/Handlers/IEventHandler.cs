namespace UKHO.ERPFacade.API.Handlers
{
    public interface IEventHandler<T>
    {
        Task ProcessEventAsync(T eventPayload);
    }
}
