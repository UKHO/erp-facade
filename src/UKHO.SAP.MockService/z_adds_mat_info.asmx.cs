using Azure.Core;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
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

            (string headerUsername, string headerPassword) = ExtractCredentialsFromHeader();

            (string keyvaultUsername, string keyvaultPassword) = GetCredentialsFromKeyVault();

            if (!string.IsNullOrEmpty(headerUsername) && !string.IsNullOrEmpty(headerPassword) &&
                headerUsername == keyvaultUsername && headerPassword == keyvaultPassword)
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

        private (string username, string password) GetCredentialsFromKeyVault()
        {
            SecretClientOptions options = new SecretClientOptions()
            {
                Retry =
                {
                    Delay= TimeSpan.FromSeconds(2),
                    MaxDelay = TimeSpan.FromSeconds(16),
                    MaxRetries = 5,
                    Mode = RetryMode.Exponential
                 }
            };
            var client = new SecretClient(new Uri(System.Configuration.ConfigurationManager.AppSettings["KeyVaultUri"]), new DefaultAzureCredential(), options);

            KeyVaultSecret usernameSecret = client.GetSecret(System.Configuration.ConfigurationManager.AppSettings["UsernameSecretName"]);
            KeyVaultSecret passwordSecret = client.GetSecret(System.Configuration.ConfigurationManager.AppSettings["PasswordSecretName"]);

            return (usernameSecret.Value, passwordSecret.Value);
        }

        private (string username, string password) ExtractCredentialsFromHeader()
        {
            string headerUsername = string.Empty;
            string headerPassword = string.Empty;

            string authHeader = HttpContext.Current.Request.Headers["Authorization"];
            string[] authHeaderParts = authHeader.Split(' ');

            if (authHeaderParts.Length == 2 || authHeaderParts[0] == "Basic")
            {
                string decodedCredentials = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(authHeaderParts[1]));

                string[] credentials = decodedCredentials.Split(':');

                headerUsername = credentials[0];
                headerPassword = credentials[1];
            }

            return (headerUsername, headerPassword);
        }
    }
}
