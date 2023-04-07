using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Xml;

namespace UKHO.ERPFacade.Common.Helpers
{
    [ExcludeFromCodeCoverage]
    public class SapClient : ISapClient
    {
        private readonly HttpClient _httpClient;

        public SapClient(HttpClient httpClient)
        {
            _httpClient = httpClient;

            _httpClient.DefaultRequestHeaders.Add("Accept", "text/xml");
            var credentials = "Basic " + Convert.ToBase64String(Encoding.ASCII.GetBytes($"vishal:dukare"));
            _httpClient.DefaultRequestHeaders.Add("Authorization", credentials);
        }

        public async Task<HttpResponseMessage> PostEventData(XmlDocument sapMessageXml, string sapServiceOperation)
        {
            var response = await _httpClient.PostAsync($"?op={sapServiceOperation}", new StringContent(sapMessageXml.InnerXml, Encoding.UTF8, "text/xml"));
            var data = response.Content?.ReadAsStringAsync().Result;
            return response;
        }
    }
}
