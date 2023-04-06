using Microsoft.Extensions.Logging;

namespace UKHO.ERPFacade.Common.Logging
{
    public static class EventIdExtensions
    {
        public static EventId ToEventId(this EventIds eventId)
        {
            return new EventId((int)eventId, eventId.ToString());
        }
    }
}
