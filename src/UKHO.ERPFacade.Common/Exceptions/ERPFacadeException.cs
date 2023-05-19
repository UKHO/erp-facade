using Microsoft.Extensions.Logging;
using System.Diagnostics.CodeAnalysis;

namespace UKHO.ERPFacade.Common.Exceptions
{
    [ExcludeFromCodeCoverage]
    [Serializable()]
    public class ERPFacadeException : Exception
    {
        public EventId EventId { get; set; }

        public ERPFacadeException(EventId eventId) : base()
        {
            EventId = eventId;
        }
    }
}