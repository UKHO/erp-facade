using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UKHO.ERPFacade.API.FunctionalTests.Helpers;

namespace UKHO.ERPFacade.API.FunctionalTests.FunctionalTests
{
    [TestFixture]
    public  class WebhookScenarios
    {
        //private readonly object HttpStatusCodeOK=OK;

        [SetUp]
        public void Setup()
        {
        }
        [Test]
        public void WhenValidEventInNewEncContentPublishedEventReceived_ThenWebhookReturns200OkResponse()
        {
            var webhook = new WebhookEndpoint();
            
            var response = webhook.GetWebhookResponse();

            Assert.That((int)response.StatusCode, Is.EqualTo(200));

        }
        
    }
}
