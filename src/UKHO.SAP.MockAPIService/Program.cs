using System.Diagnostics.CodeAnalysis;
using WireMock.Server;
using WireMock.Settings;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using WireMock.Matchers;
using System.Text;
using System.Xml.Linq;

namespace UKHO.SAP.MockAPIService
{
    [ExcludeFromCodeCoverage]
    internal class Program
    {
        [ExcludeFromCodeCoverage]
        internal static void Main(string[] args)
        {
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, true)
                .AddJsonFile("appsettings.local.overrides.json", true, true)
                .Build();

            var server = WireMockServer.Start(new WireMockServerSettings
            {
                Urls = new[] { configuration["SapConfiguration:SapBaseAddress"].ToString() }
            });
            
            server
                .Given(
                    Request.Create()
                        .WithPath($"{configuration["SapConfiguration:SapEndpointForEncEvent"].ToString()}")
                        .WithHeader("Authorization", new ExactMatcher(
                            ValidateAuthorization(
                                configuration["SapConfiguration:SapUsernameForEncEvent"].ToString(),
                                configuration["SapConfiguration:SapPasswordForEncEvent"].ToString())))
                        .WithHeader("Accept", new ExactMatcher("text/xml"))
                        .UsingPost()
                )
                .RespondWith(
                    Response.Create()
                        .WithBody((request) =>
                        {
                            var xmlDoc = XDocument.Parse(request.Body);
                            XNamespace soapNamespace = "http://schemas.xmlsoap.org/soap/envelope/";
                            XNamespace sapNamespace = "urn:sap-com:document:sap:rfc:functions";

                            var corridValue = xmlDoc
                                .Element(soapNamespace + "Envelope")?
                                .Element(soapNamespace + "Body")?
                                .Element(sapNamespace + "Z_ADDS_MAT_INFO")?
                                .Element("IM_MATINFO")?
                                .Element("CORRID")?.Value;

                            // Create the response body dynamically
                            return $"Record successfully received for CorrelationId: {corridValue}";
                        })
                        .WithStatusCode(200)
                        .WithHeader("Content-Type", "text/xml; charset=utf-8")
                );

            server
                .Given(
                    Request.Create()
                        .WithPath($"{configuration["SapConfiguration:SapEndpointForRecordOfSale"].ToString()}")
                        .WithHeader("Authorization", new ExactMatcher(
                            ValidateAuthorization(
                                configuration["SapConfiguration:SapUsernameForRecordOfSale"].ToString(),
                                configuration["SapConfiguration:SapPasswordForRecordOfSale"].ToString())))
                        .WithHeader("Accept", new ExactMatcher("text/xml"))
                        .UsingPost()
                )
                .RespondWith(
                    Response.Create()
                        .WithBody((request) =>
                        {
                            var xmlDoc = XDocument.Parse(request.Body);
                            XNamespace soapNamespace = "http://schemas.xmlsoap.org/soap/envelope/";
                            XNamespace sapNamespace = "urn:sap-com:document:sap:rfc:functions";

                            var corridValue = xmlDoc
                                .Element(soapNamespace + "Envelope")?
                                .Element(soapNamespace + "Body")?
                                .Element(sapNamespace + "Z_ADDS_ROS")?
                                .Element("IM_MATINFO")?
                                .Element("CORRID")?.Value;

                            // Create the response body dynamically
                            return $"Record successfully received for CorrelationId: {corridValue}";
                        })
                        .WithStatusCode(200)
                        .WithHeader("Content-Type", "text/xml; charset=utf-8")
                );

            server
                .Given(
                    Request.Create()
                        .WithPath($"{configuration["SapConfiguration:SapEndpointForEncEvent"].ToString()}")
                        .WithHeader("Authorization", new ExactMatcher(
                            ValidateAuthorization(
                                configuration["SapConfiguration:SapUsernameForEncEvent"].ToString(),
                                configuration["SapConfiguration:SapPasswordForEncEvent"].ToString())))
                        .WithHeader("Accept", new ExactMatcher("text/xml"))
                        .WithBody(new RegexMatcher("Unauthorize", true))
                        .UsingPost()
                )
                .RespondWith(
                    Response.Create()
                        .WithStatusCode(401)
                        .WithHeader("Content-Type", "text/xml; charset=utf-8")
                        .WithBody("Unauthorized")
                );

            server
                .Given(
                    Request.Create()
                        .WithPath($"{configuration["SapConfiguration:SapEndpointForEncEvent"].ToString()}")
                        .WithHeader("Authorization", new ExactMatcher(
                            ValidateAuthorization(
                                configuration["SapConfiguration:SapUsernameForEncEvent"].ToString(),
                                configuration["SapConfiguration:SapPasswordForEncEvent"].ToString())))
                        .WithHeader("Accept", new ExactMatcher("text/xml"))
                        .WithBody(new RegexMatcher("InternalServerError", true))
                        .UsingPost()
                )
                .RespondWith(
                    Response.Create()
                        .WithStatusCode(500)
                        .WithHeader("Content-Type", "text/xml; charset=utf-8")
                        .WithBody("Internal server error")
                );

            server
                .Given(
                    Request.Create()
                        .WithPath($"{configuration["SapConfiguration:SapEndpointForRecordOfSale"].ToString()}")
                        .WithHeader("Authorization", new ExactMatcher(
                            ValidateAuthorization(
                                configuration["SapConfiguration:SapUsernameForRecordOfSale"].ToString(),
                                configuration["SapConfiguration:SapPasswordForRecordOfSale"].ToString())))
                        .WithHeader("Accept", new ExactMatcher("text/xml"))
                        .WithBody(new RegexMatcher("Unauthorize", true))
                        .UsingPost()
                )
                .RespondWith(
                    Response.Create()
                        .WithStatusCode(401)
                        .WithHeader("Content-Type", "text/xml; charset=utf-8")
                        .WithBody("Unauthorized")
                );

            server
                .Given(
                    Request.Create()
                        .WithPath($"{configuration["SapConfiguration:SapEndpointForRecordOfSale"].ToString()}")
                        .WithHeader("Authorization", new ExactMatcher(
                            ValidateAuthorization(
                                configuration["SapConfiguration:SapUsernameForRecordOfSale"].ToString(),
                                configuration["SapConfiguration:SapPasswordForRecordOfSale"].ToString())))
                        .WithHeader("Accept", new ExactMatcher("text/xml"))
                        .WithBody(new RegexMatcher("InternalServerError", true))
                        .UsingPost()
                )
                .RespondWith(
                    Response.Create()
                        .WithStatusCode(500)
                        .WithHeader("Content-Type", "text/xml; charset=utf-8")
                        .WithBody("Internal server error")
                );

            Console.WriteLine("SOAP stub is ready.");
            Console.WriteLine("Press any key to stop the server...");
            Console.ReadKey();

            server.Stop();
        }

        public static string ValidateAuthorization(string username, string password)
        {
            return "Basic " + Convert.ToBase64String(Encoding.ASCII.GetBytes($"{username}:{password}"));
        }
    }
}
