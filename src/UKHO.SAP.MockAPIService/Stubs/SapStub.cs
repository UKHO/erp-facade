using System.Text;
using System.Xml.Linq;
using UKHO.ERPFacade.StubService.Configuration;
using UKHO.SAP.MockAPIService.Configuration;
using WireMock;
using WireMock.Matchers;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using WireMock.Server;
using WireMock.Types;
using WireMock.Util;

namespace UKHO.ERPFacade.StubService.Stubs
{
    public class SapStub : IStub
    {
        private readonly S57EncEventConfiguration _encEventConfiguration;
        private readonly RecordOfSaleEventConfiguration _recordOfSaleEventConfiguration;
        private readonly S100DataEventConfiguration _s100DataEventConfiguration;
        private readonly SapCallbackConfiguration _sapCallbackConfiguration;

        public SapStub(S57EncEventConfiguration encEventConfiguration, RecordOfSaleEventConfiguration recordOfSaleEventConfiguration, S100DataEventConfiguration s100DataEventConfiguration, SapCallbackConfiguration sapCallbackConfiguration)
        {
            _encEventConfiguration = encEventConfiguration ?? throw new ArgumentNullException(nameof(encEventConfiguration));
            _recordOfSaleEventConfiguration = recordOfSaleEventConfiguration ?? throw new ArgumentNullException(nameof(recordOfSaleEventConfiguration));
            _s100DataEventConfiguration = s100DataEventConfiguration ?? throw new ArgumentNullException(nameof(s100DataEventConfiguration));
            _sapCallbackConfiguration = sapCallbackConfiguration ?? throw new ArgumentNullException(nameof(s100DataEventConfiguration));
        }

        public void ConfigureStub(WireMockServer server)
        {
            //SAP responses for S57 enc content published events
            server
                .Given(Request.Create()
                    .WithPath(new WildcardMatcher(_encEventConfiguration.Url))
                    .WithBody(new RegexMatcher("SAP401Unauthorized", true))
                    .UsingPost())
                .RespondWith(Response.Create()
                    .WithBody((request) => { return $"Unauthorized"; })
                    .WithStatusCode(401)
                    .WithHeader("Content-Type", "text/xml; charset=utf-8"));

            server
                .Given(Request.Create()
                    .WithPath(new WildcardMatcher(_encEventConfiguration.Url))
                    .WithBody(new RegexMatcher("SAP500InternalServerError", true))
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
                    .WithBody(new RegexMatcher("SAP401Unauthorized", true))
                    .UsingPost())
                .RespondWith(Response.Create()
                    .WithBody((request) => { return $"Unauthorized"; })
                    .WithStatusCode(401)
                    .WithHeader("Content-Type", "text/xml; charset=utf-8"));

            server
                .Given(Request.Create()
                    .WithPath(new WildcardMatcher(_recordOfSaleEventConfiguration.Url))
                    .WithBody(new RegexMatcher("SAP500InternalServerError", true))
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
                    .WithBody(new RegexMatcher("SAP401Unauthorized", true))
                    .UsingPost())
                .RespondWith(Response.Create()
                    .WithBody((request) => { return $"Unauthorized"; })
                    .WithStatusCode(401)
                    .WithHeader("Content-Type", "text/xml; charset=utf-8"));

            server
                .Given(Request.Create()
                    .WithPath(new WildcardMatcher(_s100DataEventConfiguration.Url))
                    .WithBody(new RegexMatcher("SAP500InternalServerError", true))
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
                    .WithHeader("Content-Type", "text/xml; charset=utf-8")
                    .WithCallback(request =>
                    {
                        var requestBody = request.Body.ToString();
                        var xmlDocument = XDocument.Parse(requestBody);

                        XNamespace soapNs = "http://schemas.xmlsoap.org/soap/envelope/";
                        XNamespace rfcNs = "urn:sap-com:document:sap:rfc:functions";

                        var correlationIdElement = xmlDocument
                            .Element(soapNs + "Envelope")?
                            .Element(soapNs + "Body")?
                            .Element(rfcNs + "Z_ADDS_MAT_INFO")?
                            .Element("IM_MATINFO")?
                            .Element("CORRID");

                        var correlationId = correlationIdElement?.Value;

                        if (correlationId.Contains("SAP200OkInvalidCorIdInCallback"))
                            correlationId = Guid.NewGuid().ToString();
                        if (correlationId.Contains("SAP200OkEmptyCorIdInCallback"))
                            correlationId = null;

                        string payload = string.Format("{{\"correlationid\":\"{0}\"}}", correlationId);

                        Task.Run(async () =>
                        {
                            await Task.Delay(5000);

                            using var httpClient = new HttpClient();
                            var callbackRequest = new HttpRequestMessage(HttpMethod.Post, _sapCallbackConfiguration.Url)
                            {
                                Content = new StringContent(payload, Encoding.UTF8, "application/json"),
                            };
                            callbackRequest.Headers.Add("X-API-Key", _sapCallbackConfiguration.SharedApiKey);
                            var callbackResponse = await httpClient.SendAsync(callbackRequest);
                        });

                        return new ResponseMessage
                        {
                            StatusCode = 200,
                            BodyData = new BodyData
                            {
                                BodyAsString = "Request received. Callback will be triggered.",
                                DetectedBodyType = BodyType.String
                            }
                        };
                    }));
        }
    }
}
