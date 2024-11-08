﻿using UKHO.SAP.MockAPIService.Configuration;
using WireMock.Matchers;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using WireMock.Server;

namespace UKHO.ERPFacade.StubService.Stubs
{
    public class EesStub : IStub
    {
        private readonly EesConfiguration _eesConfiguration;

        public EesStub(EesConfiguration eesConfiguration)
        {
            _eesConfiguration = eesConfiguration ?? throw new ArgumentNullException(nameof(eesConfiguration));
        }

        public void ConfigureStub(WireMockServer server)
        {
            //EES responses while publishing events
            server
                .Given(Request.Create()
                    .WithPath(new WildcardMatcher(_eesConfiguration.Url))
                    .WithBody(new RegexMatcher("unauthorize", true))
                    .UsingPost())
                .RespondWith(Response.Create()
                    .WithBody((request) => { return $"Unauthorized"; })
                    .WithStatusCode(401)
                    .WithHeader("Content-Type", "application/json; charset=utf-8"));

            server
                .Given(Request.Create()
                    .WithPath(new WildcardMatcher(_eesConfiguration.Url))
                    .WithBody(new RegexMatcher("internalservererror", true))
                    .UsingPost())
                .RespondWith(Response.Create()
                    .WithBody((request) => { return $"Internal Server Error"; })
                    .WithStatusCode(500)
                    .WithHeader("Content-Type", "application/json; charset=utf-8"));

            server
                .Given(Request.Create()
                    .WithPath(new WildcardMatcher(_eesConfiguration.Url))
                    .UsingPost())
                .RespondWith(Response.Create()
                    .WithBody((request) => { return $"Event publsihed to EES sucecssfully"; })
                    .WithStatusCode(200)
                    .WithHeader("Content-Type", "application/json; charset=utf-8"));
        }
    }
}
