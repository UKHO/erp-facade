using System.ServiceModel;
using System.Xml.Serialization;
using UKHO.SAP.MockAPIService.Models;

namespace UKHO.SAP.MockAPIService.Services
{
    [ServiceContract(Namespace = "urn:sap-com:document:sap:rfc:functions")]
    public interface Iz_adds_mat_info
    {
        [OperationContract(Name = "Z_ADDS_MAT_INFO")]
        [return: XmlElement("Z_ADDS_MAT_INFOResponse", Namespace = "urn:sap-com:document:sap:rfc:functions")]
        public Z_ADDS_MAT_INFOResponse Z_ADDS_MAT_INFO([XmlElement("IM_MATINFO", Namespace = "")] IM_MATINFO iM_MATINFO);
    }
}