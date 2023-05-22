using FluentAssertions;
using NUnit.Framework;
using UKHO.ERPFacade.API.FunctionalTests.Helpers;

namespace UKHO.ERPFacade.API.FunctionalTests.FunctionalTests
{
    [TestFixture]
    public class WebhookScenarios
    {
        private WebhookEndpoint _webhook { get; set; }
        private SAPXmlHelper SapXmlHelper { get; set; }
        private readonly ADAuthTokenProvider _authToken = new();
        public static Boolean noRole = false;

        private static readonly string _webhookPayloadFilePath = "ERPFacadePayloadTestData/WebhookPayload.JSON";

        [SetUp]
        public void Setup()
        {
            _webhook = new WebhookEndpoint();
            SapXmlHelper = new SAPXmlHelper();
        }

        [Test(Description = "WhenValidEventInNewEncContentPublishedEventOptions_ThenWebhookReturns200OkResponse"), Order(1)]
        public async Task WhenValidEventInNewEncContentPublishedEventOptions_ThenWebhookReturns200OkResponse()
        {
            var response = await _webhook.OptionWebhookResponseAsync(await _authToken.GetAzureADToken(false));
            response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
        }

        [Test(Description = "WhenValidEventInNewEncContentPublishedEventReceivedWithValidToken_ThenWebhookReturns200OkResponse"), Order(1)]
        public async Task WhenValidEventInNewEncContentPublishedEventReceivedWithValidToken_ThenWebhookReturns200OkResponse()
        {
            var response = await _webhook.PostWebhookResponseAsync(_webhookPayloadFilePath, await _authToken.GetAzureADToken(false));
            response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
        }

        [Test(Description = "WhenValidEventInNewEncContentPublishedEventReceivedWithInvalidToken_ThenWebhookReturns401UnAuthorizedResponse"), Order(2)]
        public async Task WhenValidEventInNewEncContentPublishedEventReceivedWithInvalidToken_ThenWebhookReturns401UnAuthorizedResponse()
        {
            var response = await _webhook.PostWebhookResponseAsync(_webhookPayloadFilePath, "invalidToken_abcd");
            response.StatusCode.Should().Be(System.Net.HttpStatusCode.Unauthorized);
        }

        [Test(Description = "WhenValidEventInNewEncContentPublishedEventReceivedWithTokenHavingNoRole_ThenWebhookReturns403ForbiddenResponse"), Order(2)]
        public async Task WhenValidEventInNewEncContentPublishedEventReceivedWithTokenHavingNoRole_ThenWebhookReturns403ForbiddenResponse()
        {
            var response = await _webhook.PostWebhookResponseAsync(_webhookPayloadFilePath, await _authToken.GetAzureADToken(true));
            response.StatusCode.Should().Be(System.Net.HttpStatusCode.Forbidden);
        }
    }
}
