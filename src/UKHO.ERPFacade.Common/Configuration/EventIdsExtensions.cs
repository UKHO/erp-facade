using Microsoft.Extensions.Logging;
using UKHO.ERPFacade.Common.Logging;

namespace UKHO.ERPFacade.Common.Configuration
{
    public static class EventIdExtensions
    {
        public static EventId ToEventId(this EventIds eventId)
        {
            return new EventId((int)eventId, eventId.ToString());
        }
    }
}
