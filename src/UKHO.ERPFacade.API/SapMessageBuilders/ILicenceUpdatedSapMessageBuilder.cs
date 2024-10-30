using System.Xml;
using UKHO.ERPFacade.Common.Models;

namespace UKHO.ERPFacade.API.SapMessageBuilders
{
    public interface ILicenceUpdatedSapMessageBuilder
    {
        XmlDocument BuildLicenceUpdatedSapMessageXml(LicenceUpdatedEventPayLoad eventData, string correlationId);
    }
}
