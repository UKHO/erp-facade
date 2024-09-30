using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using UKHO.SAP.MockAPIService.Configuration;
using UKHO.SAP.MockAPIService.Stubs;

namespace UKHO.SAP.MockAPIService.StubSetup
{
    public class StubFactory
    {
        private readonly SapConfiguration _sapConfiguration;
        private readonly IConfiguration _configuration;

        public StubFactory(IOptions<SapConfiguration> sapConfiguration, IConfiguration configuration)
        {
            _sapConfiguration = sapConfiguration?.Value ??
                                       throw new ArgumentNullException(nameof(sapConfiguration));
            _configuration = configuration;
        }

        public IStub CreateSapServiceStub()
        {
            return new SapServiceStub(_sapConfiguration, _configuration);
        }
    }
}
