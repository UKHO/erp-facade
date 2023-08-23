using System.Diagnostics.CodeAnalysis;
using System.Xml.Serialization;
using UKHO.SAP.MockAPIService.Models;

namespace UKHO.SAP.MockAPIService.Services
{
    [ExcludeFromCodeCoverage]
    public class z_adds_ros : Iz_adds_ros
    {
        [return: XmlElement("Z_ADDS_ROSResponse", Namespace = "urn:sap-com:document:sap:rfc:functions")]
        public Z_ADDS_ROSResponse Z_ADDS_ROS([XmlElement("IM_ORDER", Namespace = "")] IM_ORDER iM_ORDER)
        {
            return new Z_ADDS_ROSResponse()
            {
                EX_MESSAGE = "Record successfully received for License GUID :" + iM_ORDER.GUID,
                EX_STATUS = "0"
            };
        }
    }
}
