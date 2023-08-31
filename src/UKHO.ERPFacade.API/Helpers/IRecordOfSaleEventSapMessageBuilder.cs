using System.Xml;
using UKHO.ERPFacade.Common.Models;

namespace UKHO.ERPFacade.API.Helpers
{
    public interface IRecordOfSaleEventSapMessageBuilder
    {
        XmlDocument BuildRecordOfSaleEventSapMessageXml(RecordOfSaleEventPayLoad eventData, string correlationId);
    }
}
