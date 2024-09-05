using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Options;
using UKHO.ERPFacade.Common.Configuration;

namespace UKHO.ERPFacade.Common.HttpClients
{
    [ExcludeFromCodeCoverage]
    public class EESClient : IEESClient
    {
        private readonly HttpClient _httpClient;
        private readonly IOptions<EESHealthCheckEnvironmentConfiguration> _eesHealthCheckEnvironmentConfiguration;
        public EESClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<HttpResponseMessage> Get(string url)
        {
                return  await _httpClient.GetAsync(url);
        }
    }
}
