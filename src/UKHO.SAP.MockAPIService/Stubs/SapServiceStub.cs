using UKHO.SAP.MockAPIService.Configuration;
using WireMock.Matchers;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using WireMock.Server;

namespace UKHO.SAP.MockAPIService.Stubs
{
    public class SapServiceStub : IStub
    {
        private readonly EncEventConfiguration _encEventConfiguration;

        public SapServiceStub(EncEventConfiguration encEventConfiguration)
        {
            _encEventConfiguration = encEventConfiguration ?? throw new ArgumentNullException(nameof(encEventConfiguration));
        }

        public void ConfigureStub(WireMockServer server)
        {
            server
                .Given(Request.Create()
                    .WithPath(new WildcardMatcher(_encEventConfiguration.Url))
                    .WithHeader("Accept", new ExactMatcher("text/xml"))
                    .UsingPost())
                .RespondWith(Response.Create().WithBody((request) =>
                {
                    return $"Record successfully received for CorrelationId: TEST";
                })
                .WithStatusCode(200)
                .WithHeader("Content-Type", "text/xml; charset=utf-8"));
        }
    }
}
