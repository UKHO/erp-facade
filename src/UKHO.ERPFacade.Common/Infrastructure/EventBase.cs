using System.Diagnostics.CodeAnalysis;

namespace UKHO.ERPFacade.Common.Infrastructure
{
    [ExcludeFromCodeCoverage]
    public abstract class EventBase<T>
    {
        public abstract string EventName { get; }

        public abstract string Subject { get; }

        public T EventData { get; set; }

    }
}
