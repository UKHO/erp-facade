using Microsoft.Extensions.Configuration;
using UKHO.SAP.MockAPIService.Configuration;
using WireMock.Matchers;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using WireMock.Server;

namespace UKHO.SAP.MockAPIService.Stubs
{
    public class SapServiceStub : IStub
    {
        private readonly SapConfiguration _sapConfiguration;
        private readonly IConfiguration _configuration;

        public SapServiceStub(SapConfiguration sapConfiguration, IConfiguration configuration)
        {
            _sapConfiguration = sapConfiguration ?? throw new ArgumentNullException(nameof(sapConfiguration));
            _configuration = configuration;
        }

        public void ConfigureStub(WireMockServer server)
        {
            var sapEndpointForEncEvent = _configuration.GetSection("SapConfiguration:SapEndpointForEncEvent").Value;
            server
                .Given(Request.Create()
                    .WithPath(new WildcardMatcher(sapEndpointForEncEvent))
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
