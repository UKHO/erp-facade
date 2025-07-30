using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Xml;

namespace UKHO.ERPFacade.Common.HttpClients
{
    [ExcludeFromCodeCoverage]
    public class SapClient : ISapClient
    {
        private readonly HttpClient _httpClient;

        public SapClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<HttpResponseMessage> SendUpdateAsync(XmlDocument sapMessageXml, string endpoint, string sapServiceOperation, string username, string password)
        {
            _httpClient.DefaultRequestHeaders.Clear();

            string credentials = "Basic " + Convert.ToBase64String(Encoding.ASCII.GetBytes(username + ":" + password));

            _httpClient.DefaultRequestHeaders.Add("Authorization", credentials);
            _httpClient.DefaultRequestHeaders.Add("Accept", "text/xml");

            return await _httpClient.PostAsync($"{endpoint}?op={sapServiceOperation}", new StringContent(sapMessageXml.InnerXml, Encoding.UTF8, "text/xml"));
        }

        public Uri? Uri => _httpClient.BaseAddress;
    }
}
