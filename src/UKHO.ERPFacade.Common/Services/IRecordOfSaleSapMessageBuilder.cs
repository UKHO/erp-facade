using System.Xml;
using UKHO.ERPFacade.Common.Models;

namespace UKHO.ERPFacade.Common.Services
{
    public interface IRecordOfSaleSapMessageBuilder
    {
        XmlDocument BuildRecordOfSaleSapMessageXml(RecordOfSaleEventPayLoad eventData, string correlationId);
    }
}
