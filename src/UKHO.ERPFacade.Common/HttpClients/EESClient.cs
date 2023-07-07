using Microsoft.Extensions.Options;
using UKHO.ERPFacade.Common.Configuration;

namespace UKHO.ERPFacade.Common.HttpClients
{
    public class EESClient : IEESClient
    {
        private readonly HttpClient _httpClient;
        private readonly IOptions<EESHealthCheckEnvironmentConfiguration> _eesHealthCheckEnvironmentConfiguration;
        public EESClient(HttpClient httpClient, IOptions<EESHealthCheckEnvironmentConfiguration> eesHealthCheckEnvironmentConfiguration)
        {
            _httpClient = httpClient;
            _eesHealthCheckEnvironmentConfiguration = eesHealthCheckEnvironmentConfiguration;
        }
        public async Task<HttpResponseMessage> EESHealthCheck()
        {
                return  await _httpClient.GetAsync(_eesHealthCheckEnvironmentConfiguration.Value.EESHealthCheckUrl);
        }
    }
}
