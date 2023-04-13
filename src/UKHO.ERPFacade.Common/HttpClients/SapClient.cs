using Microsoft.Extensions.Options;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Xml;
using UKHO.ERPFacade.Common.Configuration;

namespace UKHO.ERPFacade.Common.HttpClients
{
    [ExcludeFromCodeCoverage]
    public class SapClient : ISapClient
    {
        private readonly HttpClient _httpClient;
        private readonly IOptions<SapConfiguration> _sapConfig;

        public SapClient(HttpClient httpClient, IOptions<SapConfiguration> sapConfig)
        {
            _httpClient = httpClient;
            _sapConfig = sapConfig;

            var credentials = "Basic " + Convert.ToBase64String(Encoding.ASCII.GetBytes(_sapConfig.Value.Username + ":" + _sapConfig.Value.Password));

            _httpClient.DefaultRequestHeaders.Add("Authorization", credentials);
            _httpClient.DefaultRequestHeaders.Add("Accept", "text/xml");
        }

        public async Task<HttpResponseMessage> PostEventData(XmlDocument sapMessageXml, string sapServiceOperation)
        {
            return await _httpClient.PostAsync($"?op={sapServiceOperation}", new StringContent(sapMessageXml.InnerXml, Encoding.UTF8, "text/xml"));
        }
    }
}
