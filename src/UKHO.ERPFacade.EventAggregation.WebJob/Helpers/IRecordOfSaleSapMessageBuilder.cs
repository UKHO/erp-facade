using System.Xml;
using UKHO.ERPFacade.Common.Models;

namespace UKHO.ERPFacade.EventAggregation.WebJob.Helpers
{
    public interface IRecordOfSaleSapMessageBuilder
    {
        XmlDocument BuildRecordOfSaleSapMessageXml(List<RecordOfSaleEventPayLoad> eventDataList, string correlationId);
    }
}
