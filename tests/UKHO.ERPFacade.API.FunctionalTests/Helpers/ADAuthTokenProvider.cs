using Microsoft.Identity.Client;
using UKHO.ERPFacade.API.FunctionalTests.Configuration;

namespace UKHO.ERPFacade.API.FunctionalTests.Helpers
{
    public class ADAuthTokenProvider
    {
        static string AzureADToken;
        public ADAuthTokenProvider()
        {
            AzureADToken = null;
        }

        public async Task<string> GetAzureADToken(bool noRole)
        {
            if (noRole)
            {
                AzureADToken = null;
                AzureADToken = await GenerateAzureADToken(Config.TestConfig.AzureADConfiguration.AutoTestClientIdNoRole, Config.TestConfig.AzureADConfiguration.ClientSecretNoRole, AzureADToken);
            }
            else
            {
                AzureADToken = null;
                //AzureADToken = await  GenerateAzureADToken(Config.TestConfig.AzureADConfiguration.AutoTestClientId, Config.TestConfig.AzureADConfiguration.ClientSecret, AzureADToken);
                AzureADToken = "eyJ0eXAiOiJKV1QiLCJhbGciOiJSUzI1NiIsIng1dCI6IlhSdmtvOFA3QTNVYVdTblU3Yk05blQwTWpoQSIsImtpZCI6IlhSdmtvOFA3QTNVYVdTblU3Yk05blQwTWpoQSJ9.eyJhdWQiOiJlODRiMjUwZS0wZWMwLTRlZTUtOGFiZi01OTk5M2Q3YTU3MmQiLCJpc3MiOiJodHRwczovL3N0cy53aW5kb3dzLm5ldC85MTM0Y2E0OC02NjNkLTRhMDUtOTY4YS0zMWE0MmYwYWVkM2UvIiwiaWF0IjoxNzA4OTM4NDE4LCJuYmYiOjE3MDg5Mzg0MTgsImV4cCI6MTcwODk0MjgwOCwiYWNyIjoiMSIsImFpbyI6IkFaUUFhLzhXQUFBQTRlQlpHeEtuT1dWUCtGSnFSZytCRnRZRXNFSktjKy9RVWY0Wjc3Y2lyc1lUTWVtYTdmL1JYaTVZaXEvY0N5enR5L0VDYUc2TUt6bWV5OEpjNy94MWFrcUFFMjdBMmNoY2Y3dXA0WCt2blJWZC9iSHJaRXRaSGVkSExDc01sWDd6eWtsL1RjTjlXMXhYdHRjK2lxdTdYQ25YR0g3RXdHM1lIRG5uWXJ6T3BKSFFtT1dkYnpxSUp1SFNGRHZRRDBpeCIsImFtciI6WyJwd2QiLCJtZmEiXSwiYXBwaWQiOiJlODRiMjUwZS0wZWMwLTRlZTUtOGFiZi01OTk5M2Q3YTU3MmQiLCJhcHBpZGFjciI6IjAiLCJlbWFpbCI6InByYWRlZXAxNTU1OEBtYXN0ZWsuY29tIiwiZmFtaWx5X25hbWUiOiJJbmFzYXBwYSIsImdpdmVuX25hbWUiOiJQcmFkZWVwIiwiaWRwIjoiaHR0cHM6Ly9zdHMud2luZG93cy5uZXQvYWRkMWM1MDAtYTZkNy00ZGJkLWI4OTAtN2Y4Y2I2ZjdkODYxLyIsImlwYWRkciI6IjI3LjcuMjIuMTA3IiwibmFtZSI6IlByYWRlZXAgSSIsIm9pZCI6ImQ1MzkzNzJjLTI2ZWYtNDlkYi1hNTE3LTJlNjJiOTQ1NGVjNyIsInB3ZF9leHAiOiI2ODM0OTQiLCJwd2RfdXJsIjoiaHR0cHM6Ly9wb3J0YWwubWljcm9zb2Z0b25saW5lLmNvbS9DaGFuZ2VQYXNzd29yZC5hc3B4IiwicmgiOiIwLkFRSUFTTW8wa1QxbUJVcVdpakdrTHdydFBnNGxTLWpBRHVWT2lyOVptVDE2VnkwQ0FHQS4iLCJyb2xlcyI6WyJFbmNDb250ZW50UHVibGlzaGVkV2ViaG9va0NhbGxlciIsIlJlY29yZE9mU2FsZVdlYmhvb2tDYWxsZXIiLCJMaWNlbmNlVXBkYXRlZFdlYmhvb2tDYWxsZXIiXSwic2NwIjoiVXNlci5SZWFkIiwic3ViIjoiMEgxZ0xrTTIwN0RWUU9CUlJlQzNhN2JmQVRrUFI5X1RTMW1xYS1xRFlRdyIsInRpZCI6IjkxMzRjYTQ4LTY2M2QtNGEwNS05NjhhLTMxYTQyZjBhZWQzZSIsInVuaXF1ZV9uYW1lIjoicHJhZGVlcDE1NTU4QG1hc3Rlay5jb20iLCJ1dGkiOiJncUJQRF9fS3dVcXlvN0dsalY0X0FBIiwidmVyIjoiMS4wIn0.R9AvSrkeL3gCf8sE3L6ldK1-8pQPVLobGcPrZBOfqCQuWmjXGyBYXM1zwJEm0v-kzGT8NGDIjrnxwOhTfhqm9qVgNNEnF09I8NI7DIJnJdCWe7Rmxkt4LPAyM8OG_4KxCLyIrSqde6TP180ymXWtXZuVS0TieoDyA2ctQumk3a-fGZW-QN_oSV7EV4UgcIna8ZdqItgrifAzpKIY0DdiSO-dyIUvy5s9CCJLVV-8fXAjG2huA-Vy1-CUSLF4po5scVdozEnIa1CwgdxGVdjZt9EfdozpUc6ZfxR0dcl0O73l6YFtIj2l1VoAss4g1l5glFcLW844fWPjs12fbTKoSg";
            }
            return AzureADToken;
        }

        private static async Task<string> GenerateAzureADToken(string clientId, string clientSecret, string token)
        {
            try
            {
                string[] scopes = new string[] { $"{Config.TestConfig.AzureADConfiguration.ClientId}/.default" };
                if (token == null)
                {
                    if (Config.TestConfig.AzureADConfiguration.IsRunningOnLocalMachine)
                    {
                        IPublicClientApplication debugApp = PublicClientApplicationBuilder.Create(Config.TestConfig.AzureADConfiguration.ClientId).
                                                            WithRedirectUri("http://localhost").Build();

                        //Acquiring token through user interaction
                        AuthenticationResult tokenTask = await debugApp.AcquireTokenInteractive(scopes)
                                                                .WithAuthority($"{Config.TestConfig.AzureADConfiguration.MicrosoftOnlineLoginUrl}{Config.TestConfig.AzureADConfiguration.TenantId}", true)
                                                                .ExecuteAsync();
                        token = tokenTask.AccessToken;
                    }
                    else
                    {
                        IConfidentialClientApplication app = ConfidentialClientApplicationBuilder.Create(clientId)
                                                        .WithClientSecret(clientSecret)
                                                        .WithAuthority(new Uri($"{Config.TestConfig.AzureADConfiguration.MicrosoftOnlineLoginUrl}{Config.TestConfig.AzureADConfiguration.TenantId}"))
                                                        .Build();

                        AuthenticationResult tokenTask = await app.AcquireTokenForClient(scopes).ExecuteAsync();
                        token = tokenTask.AccessToken;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
            return token;
        }
    }
}
