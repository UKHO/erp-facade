using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Xml;
using UKHO.ERPFacade.Common.Configuration;

namespace UKHO.ERPFacade.Common.Helpers
{
    [ExcludeFromCodeCoverage]
    public class SapClient : ISapClient
    {
        private readonly HttpClient _httpClient;
        private readonly SapConfiguration _sapConfig;

        public SapClient(HttpClient httpClient, SapConfiguration sapConfig)
        {
            _httpClient = httpClient;
            _sapConfig = sapConfig;

            _httpClient.DefaultRequestHeaders.Add("Accept", "text/xml");
            var credentials = "Basic " + Convert.ToBase64String(Encoding.ASCII.GetBytes($"{sapConfig.Username}:{sapConfig.Password}"));
            _httpClient.DefaultRequestHeaders.Add("Authorization", credentials);
        }

        public async Task<HttpResponseMessage> PostEventData(XmlDocument sapMessageXml)
        {
            var response = await _httpClient.PostAsync($"?op={_sapConfig.SapServiceOperation}", new StringContent(sapMessageXml.InnerXml, Encoding.UTF8, "text/xml"));

            return response;
        }
    }
}
