using System.Diagnostics.CodeAnalysis;
using System.Net.Http.Headers;
using System.Text;

namespace UKHO.ERPFacade.Common.HttpClients
{
    [ExcludeFromCodeCoverage]
    public class EESClient : IEESClient
    {
        private readonly HttpClient _httpClient;

        public EESClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<HttpResponseMessage> Get(string url)
        {
            return await _httpClient.GetAsync(url);
        }

        public async Task<HttpResponseMessage> PostAsync(string url, string authToken, string cloudEventPayload)
        {
            var httpContent = new StringContent(cloudEventPayload, Encoding.UTF8);
            httpContent.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json; charset=utf-8");

            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authToken);

            return await _httpClient.PostAsync(url, httpContent);
        }
    }
}
