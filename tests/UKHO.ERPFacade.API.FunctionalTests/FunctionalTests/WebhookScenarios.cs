using FluentAssertions;
using NUnit.Framework;
using UKHO.ERPFacade.API.FunctionalTests.Helpers;

namespace UKHO.ERPFacade.API.FunctionalTests.FunctionalTests
{
    [TestFixture]
    public class WebhookScenarios
    {
        private WebhookEndpoint Webhook { get; set; }
        private DirectoryInfo _dir;
        private readonly ADAuthTokenProvider _authToken = new ADAuthTokenProvider();
        public static Boolean noRole = false;

        [SetUp]
        public void Setup()
        {
            Webhook = new WebhookEndpoint();
            _dir = Directory.GetParent(Environment.CurrentDirectory).Parent.Parent;
        }

        [Test, Order(0)]
        [TestCase(TestName = "WhenValidEventInNewEncContentPublishedEventOptions_ThenWebhookReturns200OkResponse")]
        public async Task WhenValidEventInNewEncContentPublishedEventOptions_ThenWebhookReturns200OkResponse()
        {
            var response = await Webhook.OptionWebhookResponseAsync(await _authToken.GetAzureADToken(false));
            response.StatusCode.Should().Be((System.Net.HttpStatusCode)200);
        }

        [Test, Order(0)]
        [TestCase(TestName = "WhenValidEventInNewEncContentPublishedEventReceivedWithValidToken_ThenWebhookReturns200OkResponse")]
        public async Task WhenValidEventInNewEncContentPublishedEventReceivedWithValidToken_ThenWebhookReturns200OkResponse()
        {
            string filePath = Path.Combine(_dir.FullName, WebhookEndpoint.config.testConfig.PayloadFolder, WebhookEndpoint.config.testConfig.WebhookPayloadFileName);

            var response = await Webhook.PostWebhookResponseAsync(filePath, await _authToken.GetAzureADToken(false));
            response.StatusCode.Should().Be((System.Net.HttpStatusCode)200);
        }

        [Test, Order(1)]
        [TestCase(TestName = "WhenValidEventInNewEncContentPublishedEventReceivedWithInvalidToken_ThenWebhookReturns401UnAuthorizedResponse")]
        public async Task WhenValidEventInNewEncContentPublishedEventReceivedWithInvalidToken_ThenWebhookReturns401UnAuthorizedResponse()
        {
            string filePath = Path.Combine(_dir.FullName, WebhookEndpoint.config.testConfig.PayloadFolder, WebhookEndpoint.config.testConfig.WebhookPayloadFileName);

            var response = await Webhook.PostWebhookResponseAsync(filePath, "invalidToken_abcd");
            response.StatusCode.Should().Be((System.Net.HttpStatusCode)401);
        }

        [Test, Order(1)]
        [TestCase(TestName = "WhenValidEventInNewEncContentPublishedEventReceivedWithTokenHavingNoRole_ThenWebhookReturns403ForbiddenResponse")]
        public async Task WhenValidEventInNewEncContentPublishedEventReceivedWithTokenHavingNoRole_ThenWebhookReturns403ForbiddenResponse()
        {
            string filePath = Path.Combine(_dir.FullName, WebhookEndpoint.config.testConfig.PayloadFolder, WebhookEndpoint.config.testConfig.WebhookPayloadFileName);
            var response = await Webhook.PostWebhookResponseAsync(filePath, await _authToken.GetAzureADToken(true));
            response.StatusCode.Should().Be((System.Net.HttpStatusCode)403);
        }
    }
}
