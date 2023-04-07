using System;
using System.Web;
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
            Z_ADDS_MAT_INFOResponse response;
            string authHeader = HttpContext.Current.Request.Headers["Authorization"];
            string[] authHeaderParts = authHeader.Split(' ');

            if (authHeaderParts.Length != 2 || authHeaderParts[0] != "Basic")
            {
                return new Z_ADDS_MAT_INFOResponse()
                {
                    EX_MESSAGE = "Invalid Authorization header",
                    EX_STATUS = "Failed"
                };
            }

            string decodedCredentials = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(authHeaderParts[1]));

            string[] credentials = decodedCredentials.Split(':');

            string username = credentials[0];
            string password = credentials[1];

            if (!string.IsNullOrEmpty(username) && !string.IsNullOrEmpty(password) && username == "vishal" && password == "dukare")
            {
                response = new Z_ADDS_MAT_INFOResponse()
                {
                    EX_MESSAGE = "Request Accepted by SAP",
                    EX_STATUS = "OK"
                };
            }
            else
            {
                response = new Z_ADDS_MAT_INFOResponse()
                {
                    EX_MESSAGE = "Invalid Username/Password",
                    EX_STATUS = "Unauthorized"
                };
            }
            return response;
        }
    }
}
