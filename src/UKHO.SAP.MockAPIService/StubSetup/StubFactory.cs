using Microsoft.Extensions.Options;
using UKHO.ERPFacade.StubService.Configuration;
using UKHO.ERPFacade.StubService.Stubs;
using UKHO.SAP.MockAPIService.Configuration;

namespace UKHO.ERPFacade.StubService.StubSetup
{
    public class StubFactory
    {
        private readonly S57EncEventConfiguration _encEventConfiguration;
        private readonly RecordOfSaleEventConfiguration _recordOfSaleEventConfiguration;
        private readonly S100DataEventConfiguration _s100DataEventConfiguration;
        private readonly EesConfiguration _eesConfiguration;

        public StubFactory(IOptions<S57EncEventConfiguration> encEventConfiguration,
                           IOptions<RecordOfSaleEventConfiguration> recordOfSaleEventConfiguration,
                           IOptions<S100DataEventConfiguration> s100DataEventConfiguration,
                           IOptions<EesConfiguration> eesConfiguration)
        {
            _encEventConfiguration = encEventConfiguration?.Value ?? throw new ArgumentNullException(nameof(encEventConfiguration));
            _recordOfSaleEventConfiguration = recordOfSaleEventConfiguration.Value ?? throw new ArgumentNullException(nameof(recordOfSaleEventConfiguration));
            _s100DataEventConfiguration = s100DataEventConfiguration.Value ?? throw new ArgumentNullException(nameof(s100DataEventConfiguration));
            _eesConfiguration = eesConfiguration.Value ?? throw new ArgumentNullException(nameof(eesConfiguration));
        }

        public IStub CreateSapStub()
        {
            return new SapStub(_encEventConfiguration, _recordOfSaleEventConfiguration, _s100DataEventConfiguration);
        }

        public IStub CreateEesStub()
        {
            return new EesStub(_eesConfiguration);
        }
    }
}
