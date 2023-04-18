using Azure.Core;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using System;
using System.Net.Http.Headers;
using System.Text;
using System.Web;

namespace UKHO.SAP.MockService
{
    public class BasicAuthHttpModule : IHttpModule
    {
        public void Init(HttpApplication context)
        {
            // Register event handlers
            context.AuthenticateRequest += OnApplicationAuthenticateRequest;
            context.EndRequest += OnApplicationEndRequest;
        }

        private static void OnApplicationAuthenticateRequest(object sender, EventArgs e)
        {
            var request = HttpContext.Current.Request;
            var authHeader = request.Headers["Authorization"];
            if (authHeader != null)
            {
                var authHeaderVal = AuthenticationHeaderValue.Parse(authHeader);

                // RFC 2617 sec 1.2, "scheme" name is case-insensitive
                if (authHeaderVal.Scheme.Equals("basic", StringComparison.OrdinalIgnoreCase) &&
                    authHeaderVal.Parameter != null)
                {
                    AuthenticateUser(authHeaderVal.Parameter);
                }
            }
        }

        // If the request was unauthorized, add the WWW-Authenticate header to the response.
        private static void OnApplicationEndRequest(object sender, EventArgs e)
        {
            var response = HttpContext.Current.Response;
            if (response.StatusCode == 401)
            {
                response.Headers.Add("WWW-Authenticate", "Basic");
                response.StatusDescription = "Unauthorized";
            }
        }

        public void Dispose()
        {
        }

        //Here is where you would validate the username and password.
        private static bool CheckPassword(string username, string password)
        {
            (string keyvaultUsername, string keyvaultPassword) = GetCredentialsFromKeyVault();

            return username == keyvaultUsername && password == keyvaultPassword;
        }

        private static void AuthenticateUser(string credentials)
        {
            try
            {
                var encoding = Encoding.GetEncoding("iso-8859-1");
                credentials = encoding.GetString(Convert.FromBase64String(credentials));

                int separator = credentials.IndexOf(':');
                string name = credentials.Substring(0, separator);
                string password = credentials.Substring(separator + 1);

                if (!CheckPassword(name, password))
                {
                    // Invalid username or password.
                    HttpContext.Current.Response.StatusCode = 401;
                }
            }
            catch (FormatException)
            {
                // Credentials were not formatted correctly.
                HttpContext.Current.Response.StatusCode = 401;
            }
        }

        private static (string username, string password) GetCredentialsFromKeyVault()
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
    }
}