using FluentAssertions;
using NUnit.Framework;
using UKHO.ERPFacade.API.FunctionalTests.Helpers;

namespace UKHO.ERPFacade.API.FunctionalTests.FunctionalTests
{
    [TestFixture]
    public  class WebhookScenarios
    {
        private WebhookEndpoint _webhook { get; set; }
        private DirectoryInfo _dir;

        [SetUp]
        public void Setup()
        {
            _webhook = new WebhookEndpoint();
             _dir = Directory.GetParent(Environment.CurrentDirectory).Parent.Parent;
        }
        [Test]
        public async Task WhenValidEventInnewenccontentpublishedeventoptions_ThenWebhookReturns200OkResponse()
        {
            var response = await _webhook.OptionWebhookResponseAsync();
            response.StatusCode.Should().Be((System.Net.HttpStatusCode)200);
        }
        
        [Test]
        public async Task WhenValidEventInNewEncContentPublishedEventReceived_ThenWebhookReturns200OkResponse()
        {
            string filePath = Path.Combine(_dir.FullName, _webhook.config.testConfig.PayloadFolder, _webhook.config.testConfig.WebhookPayloadFileName);

            var response = await _webhook.PostWebhookResponseAsync(filePath);
            response.StatusCode.Should().Be((System.Net.HttpStatusCode)200);
        }
    }
}
