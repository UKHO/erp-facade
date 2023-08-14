using System.ServiceModel;
using System.Xml.Serialization;
using UKHO.SAP.MockAPIService.Models;

namespace UKHO.SAP.MockAPIService.Services
{
    [ServiceContract(Namespace = "urn:sap-com:document:sap:rfc:functions")]
    public interface Iz_adds_ros
    {
        [OperationContract(Name = "Z_ADDS_ROS")]
        [return: XmlElement("Z_ADDS_ROSResponse", Namespace = "urn:sap-com:document:sap:rfc:functions")]
        public Z_ADDS_ROSResponse Z_ADDS_ROS([XmlElement("ZSALES_ADDS", Namespace = "")] ZSALES_ADDS zSALES_ADDS);
    }
}
