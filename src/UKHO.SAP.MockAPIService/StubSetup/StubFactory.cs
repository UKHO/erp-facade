using Microsoft.Extensions.Options;
using UKHO.SAP.MockAPIService.Configuration;
using UKHO.SAP.MockAPIService.Stubs;

namespace UKHO.SAP.MockAPIService.StubSetup
{
    public class StubFactory
    {
        private readonly EncEventConfiguration _encEventConfiguration;

        public StubFactory(IOptions<EncEventConfiguration> encEventConfiguration)
        {
            _encEventConfiguration = encEventConfiguration?.Value ?? throw new ArgumentNullException(nameof(encEventConfiguration));
        }

        public IStub CreateSapServiceStub()
        {
            return new SapServiceStub(_encEventConfiguration);
        }
    }
}
