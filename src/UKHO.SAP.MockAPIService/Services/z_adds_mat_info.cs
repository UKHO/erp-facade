using System.Diagnostics.CodeAnalysis;
using System.Xml.Serialization;
using UKHO.SAP.MockAPIService.Models;

namespace UKHO.SAP.MockAPIService.Services
{
    [ExcludeFromCodeCoverage]
    public class z_adds_mat_info : Iz_adds_mat_info
    {
        [return: XmlElement("Z_ADDS_MAT_INFOResponse", Namespace = "urn:sap-com:document:sap:rfc:functions")]
        public Z_ADDS_MAT_INFOResponse Z_ADDS_MAT_INFO([XmlElement("IM_MATINFO", Namespace = "")] IM_MATINFO iM_MATINFO)
        {
            return new Z_ADDS_MAT_INFOResponse()
            {
                EX_MESSAGE = "Record successfully received for CorrelationId :" + iM_MATINFO.CORRID,
                EX_STATUS = "0"
            };
        }
    }
}
