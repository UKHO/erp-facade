using Microsoft.Extensions.Options;
using UKHO.ERPFacade.StubService.Configuration;
using UKHO.ERPFacade.StubService.Stubs;

namespace UKHO.ERPFacade.StubService.StubSetup
{
    public class StubFactory
    {
        private readonly S57EncEventConfiguration _encEventConfiguration;
        private readonly RecordOfSaleEventConfiguration _recordOfSaleEventConfiguration;
        private readonly S100DataEventConfiguration _s100DataEventConfiguration;

        public StubFactory(IOptions<S57EncEventConfiguration> encEventConfiguration, IOptions<RecordOfSaleEventConfiguration> recordOfSaleEventConfiguration, IOptions<S100DataEventConfiguration> s100DataEventConfiguration)
        {
            _encEventConfiguration = encEventConfiguration?.Value ?? throw new ArgumentNullException(nameof(encEventConfiguration));
            _recordOfSaleEventConfiguration = recordOfSaleEventConfiguration.Value ?? throw new ArgumentNullException(nameof(recordOfSaleEventConfiguration));
            _s100DataEventConfiguration = s100DataEventConfiguration.Value ?? throw new ArgumentNullException(nameof(s100DataEventConfiguration));
        }

        public IStub CreateSapServiceStub()
        {
            return new SapServiceStub(_encEventConfiguration, _recordOfSaleEventConfiguration, _s100DataEventConfiguration);
        }
    }
}
