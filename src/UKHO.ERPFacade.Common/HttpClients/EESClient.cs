using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using UKHO.ERPFacade.Common.Configuration;

namespace UKHO.ERPFacade.Common.HttpClients
{
    public class EESClient : IEESClient
    {
        private readonly HttpClient _httpClient;
        private readonly IOptions<EESConfiguration> _eesConfiguration;
        public EESClient(HttpClient httpClient, IOptions<EESConfiguration> eesConfiguration)
        {
            _httpClient = httpClient;
            _eesConfiguration= eesConfiguration;
        }
        public async Task<HttpResponseMessage> EESHealthCheck()
        {
            return await _httpClient.GetAsync(_eesConfiguration.Value.Url);
        }
    }
}
