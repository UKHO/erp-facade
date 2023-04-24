using FluentAssertions;
using NUnit.Framework;
using UKHO.ERPFacade.API.FunctionalTests.Helpers;

namespace UKHO.ERPFacade.API.FunctionalTests.FunctionalTests
{
    [TestFixture]
    public  class WebhookScenarios
    {

        private WebhookEndpoint webhook { get; set; }
        private TestConfiguration TestConfig { get; set; }

        private string filePathWebhook;

        [SetUp]
        public void Setup()
        {
            webhook = new WebhookEndpoint();
            TestConfig = new TestConfiguration();
            DirectoryInfo dir = Directory.GetParent(Environment.CurrentDirectory).Parent.Parent;
            filePathWebhook = Path.Combine(dir.FullName, TestConfig.PayloadFolder, TestConfig.WebhookPayloadFileName);
        }
        [Test]
        public void WhenValidEventInnewenccontentpublishedeventoptions_ThenWebhookReturns200OkResponse()
        {
            var response = webhook.OptionWebhookResponseAsync().Result;
            response.StatusCode.Should().Be((System.Net.HttpStatusCode)200);
        }
        
        [Test]
        public void WhenValidEventInNewEncContentPublishedEventReceived_ThenWebhookReturns200OkResponse()
        {
            var response = webhook.PostWebhookResponseAsync(filePathWebhook).Result;
            response.StatusCode.Should().Be((System.Net.HttpStatusCode)200);
            

        }
        //checking access

    }
}
