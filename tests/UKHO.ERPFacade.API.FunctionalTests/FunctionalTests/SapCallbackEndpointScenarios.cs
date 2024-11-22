using FluentAssertions;
using NUnit.Framework;
using UKHO.ERPFacade.API.FunctionalTests.Service;

namespace UKHO.ERPFacade.API.FunctionalTests.FunctionalTests
{
    [TestFixture]
    public class SapCallbackEndpointScenarios
    {
        private WebhookEndpoint _webhookEndpoint;

        [SetUp]
        public void Setup()
        {
            _webhookEndpoint = new WebhookEndpoint();
        }

        [Test]
        public async Task WhenSAPEndpointHitWithNonExistingCorrelationId_ThenSAPShouldReturns404NotFoundResponse()
        {
            string payload = "{\r\n\"correlationId\": \"Random-28b3-46ea-b009-5943250a9a42\"\r\n}";
            var response = await _webhookEndpoint.PostSapCallbackEndPointResponseAsync(payload);
            response.StatusCode.Should().Be(System.Net.HttpStatusCode.NotFound);
        }

        [Test]
        public async Task WhenSAPEndpointHitWithInvalidKey_ThenSAPShouldReturns401UnauthorizedResponse()
        {
            string payload = $"{{\r\n\"correlationId\": \"165ce4a4-1d62-4f56-b359-59e178d771041\"\r\n}}";
            var response = await _webhookEndpoint.PostSapCallbackEndPointResponseAsync(payload, true);
            response.StatusCode.Should().Be(System.Net.HttpStatusCode.Unauthorized);
        }
    }
}
