using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using UKHO.ERPFacade.Common.Models;

namespace UKHO.ERPFacade.EventAggregation.WebJob.Helpers
{
    public interface IRecordOfSaleSapMessageBuilder
    {
        //change below parameters as per record of sale event data.
        XmlDocument BuildSapMessageXml(EncEventPayload eventData, string correlationId);
    }
}
