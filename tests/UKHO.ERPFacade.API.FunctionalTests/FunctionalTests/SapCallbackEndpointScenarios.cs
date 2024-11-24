using FluentAssertions;
using NUnit.Framework;
using UKHO.ERPFacade.API.FunctionalTests.Service;

namespace UKHO.ERPFacade.API.FunctionalTests.FunctionalTests
{
    [TestFixture]
    public class SapCallbackEndpointScenarios
    {
        private SapCallbackEndpoint _sapCallbackEndpoint;

        [SetUp]
        public void Setup()
        {
            _sapCallbackEndpoint = new SapCallbackEndpoint();
        }

        [Test]
        public async Task WhenSapCallbackEndpointReceivesNonExistingCorrelationId_ThenSapCallbackEndpointReturns404NotFoundResponse()
        {
            string correlationId = Guid.NewGuid().ToString();
            string payload = $"{{\r\n\"correlationId\": \"{correlationId}\"\r\n}}";
            var response = await _sapCallbackEndpoint.PostSapCallbackEndPointResponseAsync(payload);
            response.StatusCode.Should().Be(System.Net.HttpStatusCode.NotFound);
        }

        [Test]
        public async Task WhenSapCallbackEndpointReceivesInvalidSharedApiKey_ThenSapCallbackEndpointReturns401UnauthorizedResponse()
        {
            string payload = $"{{\r\n\"correlationId\": \"165ce4a4-1d62-4f56-b359-59e178d771041\"\r\n}}";
            var response = await _sapCallbackEndpoint.PostSapCallbackEndPointResponseAsync(payload, true);
            response.StatusCode.Should().Be(System.Net.HttpStatusCode.Unauthorized);
        }
    }
}
