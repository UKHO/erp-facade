using Microsoft.Extensions.Logging;
using System.Diagnostics.CodeAnalysis;

namespace UKHO.ERPFacade.Common.Exceptions
{
    [ExcludeFromCodeCoverage]
    [Serializable()]
    public class ERPFacadeException : Exception
    {
        public EventId EventId { get; set; }
        public object[] MessageArguments { get; set; }

        public ERPFacadeException(EventId eventId, string message, params object[] messageArguments) : base(message)
        {
            EventId = eventId;
            MessageArguments = messageArguments ?? [];
        }
    }
}

