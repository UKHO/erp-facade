namespace UKHO.ERPFacade.Common.Infrastructure
{
    public abstract class EventBase<T>
    {
        public abstract string EventName { get; }

        public abstract string Subject { get; }

        public T EventData { get; set; }

    }
}
