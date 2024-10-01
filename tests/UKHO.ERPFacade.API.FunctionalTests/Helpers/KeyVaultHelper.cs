using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using UKHO.ERPFacade.API.FunctionalTests.Configuration;

namespace UKHO.ERPFacade.API.FunctionalTests.Helpers
{
    internal class KeyVaultHelper
    {
        public static string GetKeyVaultSecret(string secretName)
        {
            string keyVaultUri = Config.TestConfig.KeyVaultUri;

            var client = new SecretClient(new Uri(keyVaultUri), new DefaultAzureCredential());

            KeyVaultSecret secret = client.GetSecret(secretName);

            return secret.Value;
        }
    }
}
