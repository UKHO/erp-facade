using System.Xml;
using UKHO.ERPFacade.Common.Models;

namespace UKHO.ERPFacade.API.Helpers
{
    public interface IRecordOfSaleSapMessageBuilder
    {
        XmlDocument BuildRecordOfSaleSapMessageXml(RecordOfSaleEventPayLoad eventData, string correlationId);
    }
}
