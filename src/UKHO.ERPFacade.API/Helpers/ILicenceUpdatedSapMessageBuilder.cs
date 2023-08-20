using System.Xml;
using UKHO.ERPFacade.Common.Models;

namespace UKHO.ERPFacade.API.Helpers
{
    public interface ILicenceUpdatedSapMessageBuilder
    {
        XmlDocument BuildLicenceUpdatedSapMessageXml(RecordOfSaleEventPayLoad eventData, string correlationId);
    }
}
