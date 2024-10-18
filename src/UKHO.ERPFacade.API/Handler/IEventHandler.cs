using System.Xml;
using UKHO.ERPFacade.Common.Models;

namespace UKHO.ERPFacade.API.Handler
{
    public interface IEventHandler
    {
        string EventType { get; }
        Task HandleEvent(string encEventJson, IEventData eventData);
    }
}
