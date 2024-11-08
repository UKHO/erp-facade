using UKHO.ERPFacade.StubService.Configuration;
using WireMock.Matchers;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using WireMock.Server;

namespace UKHO.ERPFacade.StubService.Stubs
{
    public class SapStub : IStub
    {
        private readonly S57EncEventConfiguration _encEventConfiguration;
        private readonly RecordOfSaleEventConfiguration _recordOfSaleEventConfiguration;
        private readonly S100DataEventConfiguration _s100DataEventConfiguration;

        public SapStub(S57EncEventConfiguration encEventConfiguration, RecordOfSaleEventConfiguration recordOfSaleEventConfiguration, S100DataEventConfiguration s100DataEventConfiguration)
        {
            _encEventConfiguration = encEventConfiguration ?? throw new ArgumentNullException(nameof(encEventConfiguration));
            _recordOfSaleEventConfiguration = recordOfSaleEventConfiguration ?? throw new ArgumentNullException(nameof(recordOfSaleEventConfiguration));
            _s100DataEventConfiguration = s100DataEventConfiguration ?? throw new ArgumentNullException(nameof(s100DataEventConfiguration));
        }

        public void ConfigureStub(WireMockServer server)
        {
            //SAP responses for S57 enc content published events
            server
                .Given(Request.Create()
                    .WithPath(new WildcardMatcher(_encEventConfiguration.Url))
                    .WithBody(new RegexMatcher("unauthorize", true))
                    .UsingPost())
                .RespondWith(Response.Create()
                    .WithBody((request) => { return $"Unauthorized"; })
                    .WithStatusCode(401)
                    .WithHeader("Content-Type", "text/xml; charset=utf-8"));

            server
                .Given(Request.Create()
                    .WithPath(new WildcardMatcher(_encEventConfiguration.Url))
                    .WithBody(new RegexMatcher("internalservererror", true))
                    .UsingPost())
                .RespondWith(Response.Create()
                    .WithBody((request) => { return $"Internal Server Error"; })
                    .WithStatusCode(500)
                    .WithHeader("Content-Type", "text/xml; charset=utf-8"));

            server
                .Given(Request.Create()
                    .WithPath(new WildcardMatcher(_encEventConfiguration.Url))
                    .UsingPost())
                .RespondWith(Response.Create()
                    .WithBody((request) => { return $"S57 record received sucecssfully"; })
                    .WithStatusCode(200)
                    .WithHeader("Content-Type", "text/xml; charset=utf-8"));

            server
                .Given(Request.Create()
                    .WithPath(new WildcardMatcher(_encEventConfiguration.Url))
                    .WithBody(new RegexMatcher("delay", true))
                    .UsingPost())
                .RespondWith(Response.Create()
                    .WithBody((request) => { return $"S57 record received sucecssfully after 5 seconds"; })
                    .WithStatusCode(200)
                    .WithHeader("Content-Type", "text/xml; charset=utf-8")
                    .WithDelay(TimeSpan.FromMilliseconds(5000)));

            //SAP responses for record of sale events
            server
                .Given(Request.Create()
                    .WithPath(new WildcardMatcher(_recordOfSaleEventConfiguration.Url))
                    .WithBody(new RegexMatcher("unauthorize", true))
                    .UsingPost())
                .RespondWith(Response.Create()
                    .WithBody((request) => { return $"Unauthorized"; })
                    .WithStatusCode(401)
                    .WithHeader("Content-Type", "text/xml; charset=utf-8"));

            server
                .Given(Request.Create()
                    .WithPath(new WildcardMatcher(_recordOfSaleEventConfiguration.Url))
                    .WithBody(new RegexMatcher("internalservererror", true))
                    .UsingPost())
                .RespondWith(Response.Create()
                    .WithBody((request) => { return $"Internal Server Error"; })
                    .WithStatusCode(500)
                    .WithHeader("Content-Type", "text/xml; charset=utf-8"));

            server
                .Given(Request.Create()
                    .WithPath(new WildcardMatcher(_recordOfSaleEventConfiguration.Url))
                    .UsingPost())
                .RespondWith(Response.Create()
                    .WithBody((request) => { return $"Record of sale record received sucecssfully"; })
                    .WithStatusCode(200)
                    .WithHeader("Content-Type", "text/xml; charset=utf-8"));

            //SAP responses for s100 data events
            server
                .Given(Request.Create()
                    .WithPath(new WildcardMatcher(_s100DataEventConfiguration.Url))
                    .WithBody(new RegexMatcher("unauthorize", true))
                    .UsingPost())
                .RespondWith(Response.Create()
                    .WithBody((request) => { return $"Unauthorized"; })
                    .WithStatusCode(401)
                    .WithHeader("Content-Type", "text/xml; charset=utf-8"));

            server
                .Given(Request.Create()
                    .WithPath(new WildcardMatcher(_s100DataEventConfiguration.Url))
                    .WithBody(new RegexMatcher("internalservererror", true))
                    .UsingPost())
                .RespondWith(Response.Create()
                    .WithBody((request) => { return $"Internal Server Error"; })
                    .WithStatusCode(500)
                    .WithHeader("Content-Type", "text/xml; charset=utf-8"));

            server
                .Given(Request.Create()
                    .WithPath(new WildcardMatcher(_s100DataEventConfiguration.Url))
                    .UsingPost())
                .RespondWith(Response.Create()
                    .WithBody((request) => { return $"S100 record received sucecssfully"; })
                    .WithStatusCode(200)
                    .WithHeader("Content-Type", "text/xml; charset=utf-8"));
        }
    }
}
