using NUnit.Framework;
using UKHO.ERPFacade.API.FunctionalTests.Helpers;

namespace UKHO.ERPFacade.API.FunctionalTests.FunctionalTests
{
    [TestFixture]
    public class GetPriceInfoScenarios
    {
        [Test]
        public void WhenValidEventInGetPriceInfo_ThenReturns200OkResponse()
        {
            var webhook = new GetPriceInfo();

            var response = webhook.GetPriceInfoResponse();

            Assert.That((int)response.StatusCode, Is.EqualTo(200));

        }

    }
}
