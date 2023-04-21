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

        [SetUp]
        public void Setup()
        {
            Webhook = new WebhookEndpoint();
            _dir = Directory.GetParent(Environment.CurrentDirectory).Parent.Parent;
        }
        [Test]
        public async Task WhenValidEventInnewenccontentpublishedeventoptions_ThenWebhookReturns200OkResponse()
        {
            var response = await Webhook.OptionWebhookResponseAsync();
            response.StatusCode.Should().Be((System.Net.HttpStatusCode)200);
        }

        [Test]
        public async Task WhenValidEventInNewEncContentPublishedEventReceived_ThenWebhookReturns200OkResponse()
        {
            string filePath = Path.Combine(_dir.FullName, Webhook.config.testConfig.PayloadFolder, Webhook.config.testConfig.WebhookPayloadFileName);

            var response = await Webhook.PostWebhookResponseAsync(filePath);
            response.StatusCode.Should().Be((System.Net.HttpStatusCode)200);
        }
    }
}
