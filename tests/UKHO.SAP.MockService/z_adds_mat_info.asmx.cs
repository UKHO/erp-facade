using System.Web.Services;
using System.Xml.Serialization;

namespace UKHO.SAP.MockService
{
    /// <summary>
    /// Summary description for z_adds_mat_info
    /// </summary>
    [WebService(Namespace = "urn:sap-com:document:sap:rfc:functions")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [System.ComponentModel.ToolboxItem(false)]
    // To allow this Web Service to be called from script, using ASP.NET AJAX, uncomment the following line. 
    // [System.Web.Script.Services.ScriptService]
    public class z_adds_mat_info : Z_ADDS_MAT_INFO
    {
        [WebMethod]
        [return: XmlElement("Z_ADDS_MAT_INFOResponse", Namespace = "urn:sap-com:document:sap:rfc:functions")]
        public Z_ADDS_MAT_INFOResponse Z_ADDS_MAT_INFO([XmlElement("Z_ADDS_MAT_INFO", Namespace = "urn:sap-com:document:sap:rfc:functions")] Z_ADDS_MAT_INFO z_ADDS_MAT_INFO)
        {
            return new Z_ADDS_MAT_INFOResponse()
            {
                EX_MESSAGE = "Record successfully received for " + z_ADDS_MAT_INFO.IM_MATINFO.CORRID,
                EX_STATUS = "0"
            };
        }
    }
}
