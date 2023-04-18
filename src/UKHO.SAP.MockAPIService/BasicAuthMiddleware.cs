using Azure.Core;
using Azure.Security.KeyVault.Secrets;
using System.Net.Http.Headers;
using System.Text;

namespace UKHO.SAP.MockAPIService
{
    public class BasicAuthMiddleware
    {
        private readonly RequestDelegate _next;

        public BasicAuthMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            // Do something with context near the beginning of request processing.
            var request = context.Request;
            var authHeader = request.Headers["Authorization"];
            if (!Microsoft.Extensions.Primitives.StringValues.IsNullOrEmpty(authHeader))
            {
                var authHeaderVal = AuthenticationHeaderValue.Parse(authHeader);

                // RFC 2617 sec 1.2, "scheme" name is case-insensitive
                if (authHeaderVal.Scheme.Equals("basic", StringComparison.OrdinalIgnoreCase) &&
                    authHeaderVal.Parameter != null)
                {
                    AuthenticateUser(context,authHeaderVal.Parameter);
                }
            }

            await _next.Invoke(context);

            // Clean up.
        }

        private static void AuthenticateUser(HttpContext context,string credentials)
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
                    context.Response.StatusCode = 401;
                }
            }
            catch (FormatException)
            {
                // Credentials were not formatted correctly.
                context.Response.StatusCode = 401;
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
            //var client = new SecretClient(new Uri(System.Configuration.ConfigurationManager.AppSettings["KeyVaultUri"]), new DefaultAzureCredential(), options);

            //KeyVaultSecret usernameSecret = client.GetSecret(System.Configuration.ConfigurationManager.AppSettings["UsernameSecretName"]);
            //KeyVaultSecret passwordSecret = client.GetSecret(System.Configuration.ConfigurationManager.AppSettings["PasswordSecretName"]);

            //return (usernameSecret.Value, passwordSecret.Value);
            return ("ZADDUSER", "aW3pBCcw!");

        }

        private static bool CheckPassword(string username, string password)
        {
            (string keyvaultUsername, string keyvaultPassword) = GetCredentialsFromKeyVault();

            return username == keyvaultUsername && password == keyvaultPassword;
        }
    }

    public static class MyMiddlewareExtensions
    {
        public static IApplicationBuilder UseMyMiddleware(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<BasicAuthMiddleware>();
        }
    }
}
