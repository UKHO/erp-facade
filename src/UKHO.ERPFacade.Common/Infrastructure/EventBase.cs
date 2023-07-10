using System.Diagnostics.CodeAnalysis;

namespace UKHO.ERPFacade.Common.Infrastructure
{
    [ExcludeFromCodeCoverage]
    public abstract class EventBase<T>
    {
        public string EventName { get; set; }

        public string Subject { get; set; }

        public string Id { get; set; }

        public T Data { get; set; }

    }
}
