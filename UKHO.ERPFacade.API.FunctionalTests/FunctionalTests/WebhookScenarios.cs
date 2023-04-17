using Newtonsoft.Json;
using NUnit.Framework;
using System.Reflection;
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
        public async Task SetupAsync()
        {
            webhook = new WebhookEndpoint();
            TestConfig = new TestConfiguration();
            DirectoryInfo dir = Directory.GetParent(Environment.CurrentDirectory).Parent.Parent;
            filePathWebhook = Path.Combine(dir.FullName, TestConfig.PayloadParentFolder, TestConfig.PayloadFolder, TestConfig.WebhookPayloadFileName);
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
            var response = webhook.PostWebhookResponseFileAsync(filePathWebhook).Result;
            Assert.That((int)response.StatusCode, Is.EqualTo(200));

        }

    }
}
