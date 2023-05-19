using Microsoft.Extensions.Logging;
using System.Diagnostics.CodeAnalysis;

namespace UKHO.ERPFacade.Common.Logging
{
    [ExcludeFromCodeCoverage]
    public static class EventIdExtensions
    {
        public static EventId ToEventId(this EventIds eventId)
        {
            return new EventId((int)eventId, eventId.ToString());
        }
    }
}