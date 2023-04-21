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
        private readonly ADAuthTokenProvider _authToken=new ADAuthTokenProvider();
        public static Boolean noRole=false;

        [SetUp]
        public void Setup()
        {
            Webhook = new WebhookEndpoint();            
            _dir = Directory.GetParent(Environment.CurrentDirectory).Parent.Parent;
        }
        [Test, Order(0)]
        public async Task WhenValidEventInnewenccontentpublishedeventoptions_ThenWebhookReturns200OkResponse()
        {
            var response = await Webhook.OptionWebhookResponseAsync();
            response.StatusCode.Should().Be((System.Net.HttpStatusCode)200);
        }

        [Test, Order(0)]
        public async Task WhenValidEventInNewEncContentPublishedEventReceived_ThenWebhookReturns200OkResponse()
        {
            string filePath = Path.Combine(_dir.FullName, Webhook.config.testConfig.PayloadFolder, Webhook.config.testConfig.WebhookPayloadFileName);
            
            var response = await Webhook.PostWebhookResponseAsync(filePath);
            response.StatusCode.Should().Be((System.Net.HttpStatusCode)200);
        }
        [Test, Order(0)]
        public async Task WhenValidEventInNewEncContentPublishedEventReceivedWithValidToken_ThenWebhookReturns200OkResponse()
        {
            string filePath = Path.Combine(_dir.FullName, Webhook.config.testConfig.PayloadFolder, Webhook.config.testConfig.WebhookPayloadFileName);
            //var validToken = await _authToken.GetAzureADToken(false);
            var response = await Webhook.PostWebhookResponseAsync(filePath, await _authToken.GetAzureADToken(false));
            response.StatusCode.Should().Be((System.Net.HttpStatusCode)200);
        }

        [Test, Order(1)]
        public async Task WhenValidEventInNewEncContentPublishedEventReceivedWithInvalidToken_ThenWebhookReturns401UnAuthorizedResponse()
        {
            string filePath = Path.Combine(_dir.FullName, Webhook.config.testConfig.PayloadFolder, Webhook.config.testConfig.WebhookPayloadFileName);
            
            var response = await Webhook.PostWebhookResponseAsync(filePath, "abcd");
            response.StatusCode.Should().Be((System.Net.HttpStatusCode)401);
        }

        [Test, Order(1)]
        public async Task WhenValidEventInNewEncContentPublishedEventReceivedWithTokenHavingNoRole_ThenWebhookReturns403ForbiddenResponse()
        {
            string filePath = Path.Combine(_dir.FullName, Webhook.config.testConfig.PayloadFolder, Webhook.config.testConfig.WebhookPayloadFileName);
            noRole = true;
            var response = await Webhook.PostWebhookResponseAsync(filePath, await _authToken.GetAzureADToken(true));
            response.StatusCode.Should().Be((System.Net.HttpStatusCode)403);
            noRole = false;
        }
    }
}
