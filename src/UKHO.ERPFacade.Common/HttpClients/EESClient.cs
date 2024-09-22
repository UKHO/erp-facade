using System.Diagnostics.CodeAnalysis;

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
    }
}
