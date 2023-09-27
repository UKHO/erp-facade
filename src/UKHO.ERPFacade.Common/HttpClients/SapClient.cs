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

        public SapClient(HttpClient httpClient, IOptions<SapConfiguration> sapConfig)
        {
            _httpClient = httpClient;
        }

        public async Task<HttpResponseMessage> PostEventData(XmlDocument sapMessageXml, string endpoint, string sapServiceOperation, string username, string password)
        {
            var credentials = "Basic " + Convert.ToBase64String(Encoding.ASCII.GetBytes(username + ":" + password));

            if (_httpClient.DefaultRequestHeaders.Authorization is null)
                _httpClient.DefaultRequestHeaders.Add("Authorization", credentials);
            _httpClient.DefaultRequestHeaders.Add("Accept", "text/xml");

            return await _httpClient.PostAsync($"{endpoint}?op={sapServiceOperation}", new StringContent(sapMessageXml.InnerXml, Encoding.UTF8, "text/xml"));
        }
    }
}
