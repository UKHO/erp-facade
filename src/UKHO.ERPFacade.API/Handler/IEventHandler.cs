using System.Xml;
using UKHO.ERPFacade.Common.Models;

namespace UKHO.ERPFacade.API.Handler
{
    public interface IEventHandler
    {
        Task HandleEvent(string encEventJson);
    }
}
