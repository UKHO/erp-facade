using System.Threading.Tasks;
using FakeItEasy;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using UKHO.ERPFacade.API.Controllers;

namespace UKHO.ERPFacade.API.UnitTests.Controllers
{
    [TestFixture]
    public class SapCallbackControllerTests
    {
        private IHttpContextAccessor _fakeHttpContextAccessor;
        private SapCallbackController _fakeSapCallbackController;

        [SetUp]
        public void Setup()
        {
            _fakeHttpContextAccessor = A.Fake<IHttpContextAccessor>();
            _fakeSapCallbackController = new SapCallbackController(_fakeHttpContextAccessor);
        }

        [Test]
        public async Task WhenValidHeaderRequestedInEventOptionsEndpoint_ThenWebhookReturns200OkResponse()
        {
            var fakeSapCallBackJson = JObject.Parse(@"{""data"":{""corId"":""123""}}");

            var result = (OkObjectResult)await _fakeSapCallbackController.S100ErpFacadeCallBack(fakeSapCallBackJson);

            result.StatusCode.Should().Be(200);
        }
    }
}
