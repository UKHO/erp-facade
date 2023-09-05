using System.Xml;
using Microsoft.Extensions.Logging;
using UKHO.ERPFacade.Common.Models;

namespace UKHO.ERPFacade.EventAggregation.WebJob.Helpers
{
    public class RecordOfSaleSapMessageBuilder : IRecordOfSaleSapMessageBuilder
    {
        private readonly ILogger<RecordOfSaleSapMessageBuilder> _logger;

        public RecordOfSaleSapMessageBuilder(ILogger<RecordOfSaleSapMessageBuilder> logger)
        {
            _logger = logger;
        }

        public XmlDocument BuildSapMessageXml(EncEventPayload eventData, string correlationId)
        {
            //logic to generate sap xml payload.
            return new XmlDocument();
        }
    }
}
