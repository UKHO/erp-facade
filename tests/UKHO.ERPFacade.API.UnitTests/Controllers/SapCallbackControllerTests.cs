using System.Threading.Tasks;
using FakeItEasy;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using UKHO.ERPFacade.API.Controllers;
using UKHO.ERPFacade.API.Services.EventPublishingServices;
using UKHO.ERPFacade.Common.Models;
using UKHO.ERPFacade.Common.Models.CloudEvents;
using UKHO.ERPFacade.Services;

namespace UKHO.ERPFacade.API.UnitTests.Controllers
{
    [TestFixture]
    public class SapCallbackControllerTests
    {
        private IHttpContextAccessor _fakeHttpContextAccessor;
        private ILogger<SapCallbackController> _fakeLogger;
        private ISapCallbackService _fakeSapCallbackService;
        private SapCallbackController _fakeSapCallbackController;
        private IS100UnitOfSaleUpdatedEventPublishingService _fakeS100UnitOfSaleUpdatedEventPublishingService;

        [SetUp]
        public void Setup()
        {
            _fakeHttpContextAccessor = A.Fake<IHttpContextAccessor>();
            _fakeLogger = A.Fake<ILogger<SapCallbackController>>();
            _fakeSapCallbackService = A.Fake<ISapCallbackService>();
            _fakeS100UnitOfSaleUpdatedEventPublishingService = A.Fake<IS100UnitOfSaleUpdatedEventPublishingService>();
            _fakeSapCallbackController = new SapCallbackController(_fakeHttpContextAccessor, _fakeLogger, _fakeSapCallbackService, _fakeS100UnitOfSaleUpdatedEventPublishingService);
        }

        [Test]
        public async Task WhenValidHeaderRequestedInEventOptionsEndpoint_ThenWebhookReturns200OkResponse()
        {
            var fakeSapCallBackJson = JObject.Parse(@"{""correlationId"":""123""}");

            Result result = new (true,"");

            A.CallTo(() => _fakeSapCallbackService.IsValidCallbackAsync(A<string>.Ignored)).Returns(true);

            A.CallTo(() => _fakeS100UnitOfSaleUpdatedEventPublishingService.PublishEvent(A<BaseCloudEvent>.Ignored, A<string>.Ignored)).Returns(result);

            var response = (OkObjectResult)await _fakeSapCallbackController.S100SapCallback(fakeSapCallBackJson);

            response.StatusCode.Should().Be(200);
        }
    }
}
