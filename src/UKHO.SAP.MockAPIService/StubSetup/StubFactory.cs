using Microsoft.Extensions.Options;
using UKHO.ERPFacade.Common.Exceptions;
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
        private readonly SapCallbackConfiguration _sapCallbackConfiguration;
        private readonly ErpFacadeConfiguration _erpFacadeConfiguration;

        public StubFactory(IOptions<S57EncEventConfiguration> encEventConfiguration,
                           IOptions<RecordOfSaleEventConfiguration> recordOfSaleEventConfiguration,
                           IOptions<S100DataEventConfiguration> s100DataEventConfiguration,
                           IOptions<EesConfiguration> eesConfiguration,
                           IOptions<SapCallbackConfiguration> sapCallbackConfiguration,
                           IOptions<ErpFacadeConfiguration> erpFacadeConfiguration)
        {
            _encEventConfiguration = encEventConfiguration?.Value ?? throw new ArgumentNullException(nameof(encEventConfiguration));
            _recordOfSaleEventConfiguration = recordOfSaleEventConfiguration.Value ?? throw new ArgumentNullException(nameof(recordOfSaleEventConfiguration));
            _s100DataEventConfiguration = s100DataEventConfiguration.Value ?? throw new ArgumentNullException(nameof(s100DataEventConfiguration));
            _eesConfiguration = eesConfiguration.Value ?? throw new ArgumentNullException(nameof(eesConfiguration));
            _sapCallbackConfiguration = sapCallbackConfiguration.Value ?? throw new ArgumentNullException(nameof(sapCallbackConfiguration));
            _erpFacadeConfiguration = erpFacadeConfiguration.Value ?? throw new ArgumentNullException(nameof(erpFacadeConfiguration));
        }

        public IStub CreateSapStub()
        {
            return new SapStub(_encEventConfiguration, _recordOfSaleEventConfiguration, _s100DataEventConfiguration, _sapCallbackConfiguration, _erpFacadeConfiguration);
        }

        public IStub CreateEesStub()
        {
            return new EesStub(_eesConfiguration);
        }
    }
}
