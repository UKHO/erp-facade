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

        }
        [Test]
        public void WhenValidEventInnewenccontentpublishedeventoptions_ThenWebhookReturns200OkResponse()
        {
            var response = webhook.GetWebhookOptionResponse();
            Assert.That((int)response.StatusCode, Is.EqualTo(200));

        }
        [Test]
        public void WhenValidEventInNewEncContentPublishedEventReceived_ThenWebhookReturn200OkResponse()
        {
            var response = webhook.PostWebhookResponseFile();
            Assert.That((int)response.StatusCode, Is.EqualTo(200));

        }

    }
}
