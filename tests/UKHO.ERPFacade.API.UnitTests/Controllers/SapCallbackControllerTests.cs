using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FakeItEasy;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using UKHO.ERPFacade.API.Controllers;
using UKHO.ERPFacade.Common.Logging;
using UKHO.ERPFacade.Common.Models;
using UKHO.ERPFacade.Services;

namespace UKHO.ERPFacade.API.UnitTests.Controllers
{
    [TestFixture]
    public class SapCallbackControllerTests
    {
        private IHttpContextAccessor _fakeHttpContextAccessor;
        private ILogger<SapCallbackController> _fakeLogger;
        private IS100SapCallBackService _fakeSapCallbackService;
        private SapCallbackController _fakeSapCallbackController;

        [SetUp]
        public void Setup()
        {
            _fakeHttpContextAccessor = A.Fake<IHttpContextAccessor>();
            _fakeLogger = A.Fake<ILogger<SapCallbackController>>();
            _fakeSapCallbackService = A.Fake<IS100SapCallBackService>();
            _fakeSapCallbackController = new SapCallbackController(_fakeHttpContextAccessor, _fakeLogger, _fakeSapCallbackService);
        }

        [Test]
        public async Task WhenValidCorrelationIdIsProvidedInPayload_ThenSapCallbackReturns200OkResponse()
        {
            var fakeSapCallBackJson = JObject.Parse(@"{""correlationId"":""123""}");

            A.CallTo(() => _fakeSapCallbackService.IsValidCallbackAsync(A<string>.Ignored)).Returns(true);

            var response = (OkObjectResult)await _fakeSapCallbackController.S100SapCallback(fakeSapCallBackJson);

            A.CallTo(() => _fakeSapCallbackService.ProcessSapCallbackAsync(A<string>.Ignored)).MustHaveHappenedOnceOrMore();

            A.CallTo(_fakeLogger).Where(call => call.Method.Name == "Log"
                                                && call.GetArgument<LogLevel>(0) == LogLevel.Information
                                                && call.GetArgument<EventId>(1) == EventIds.S100SapCallbackPayloadReceived.ToEventId()
                                                && call.GetArgument<IEnumerable<KeyValuePair<string, object>>>(2)!.ToDictionary(c => c.Key, c => c.Value)["{OriginalFormat}"].ToString() == "S-100 SAP callback received.").MustHaveHappenedOnceExactly();

            response.StatusCode.Should().Be(200);
        }

        [Test]
        public async Task WhenEmptyCorrelationIdIsProvidedInPayload_ThenSapCallbackReturns400BadRequestResponse()
        {
            var fakeSapCallBackJson = JObject.Parse(@"{""correlationId"":""""}");

            A.CallTo(() => _fakeSapCallbackService.IsValidCallbackAsync(A<string>.Ignored)).Returns(true);

            var response = (BadRequestObjectResult)await _fakeSapCallbackController.S100SapCallback(fakeSapCallBackJson);

            A.CallTo(_fakeLogger).Where(call => call.Method.Name == "Log"
                                                && call.GetArgument<LogLevel>(0) == LogLevel.Information
                                                && call.GetArgument<EventId>(1) == EventIds.S100SapCallbackPayloadReceived.ToEventId()
                                                && call.GetArgument<IEnumerable<KeyValuePair<string, object>>>(2)!.ToDictionary(c => c.Key, c => c.Value)["{OriginalFormat}"].ToString() == "S-100 SAP callback received.").MustHaveHappenedOnceExactly();

            A.CallTo(_fakeLogger).Where(call => call.Method.Name == "Log"
                                                && call.GetArgument<LogLevel>(0) == LogLevel.Error
                                                && call.GetArgument<EventId>(1) == EventIds.CorrelationIdMissingInS100SapCallBack.ToEventId()
                                                && call.GetArgument<IEnumerable<KeyValuePair<string, object>>>(2)!.ToDictionary(c => c.Key, c => c.Value)["{OriginalFormat}"].ToString() == "CorrelationId is missing in S-100 SAP callback request.").MustHaveHappenedOnceExactly();

            response.StatusCode.Should().Be(400);
            var errors = (ErrorDescription)response.Value;
            errors.Errors.Single().Description.Should().Be("Correlation ID Not Found.");
        }

        [Test]
        public async Task WhenInValidCorrelationIdIsProvidedInPayload_ThenSapCallbackReturns404NotFoundResponse()
        {
            var fakeSapCallBackJson = JObject.Parse(@"{""correlationId"":""1234""}");

            A.CallTo(() => _fakeSapCallbackService.IsValidCallbackAsync(A<string>.Ignored)).Returns(false);

            var response = (NotFoundObjectResult)await _fakeSapCallbackController.S100SapCallback(fakeSapCallBackJson);

            A.CallTo(_fakeLogger).Where(call => call.Method.Name == "Log"
                                                && call.GetArgument<LogLevel>(0) == LogLevel.Information
                                                && call.GetArgument<EventId>(1) == EventIds.S100SapCallbackPayloadReceived.ToEventId()
                                                && call.GetArgument<IEnumerable<KeyValuePair<string, object>>>(2)!.ToDictionary(c => c.Key, c => c.Value)["{OriginalFormat}"].ToString() == "S-100 SAP callback received.").MustHaveHappenedOnceExactly();

            A.CallTo(_fakeLogger).Where(call => call.Method.Name == "Log"
                                                && call.GetArgument<LogLevel>(0) == LogLevel.Error
                                                && call.GetArgument<EventId>(1) == EventIds.InvalidS100SapCallback.ToEventId()
                                                && call.GetArgument<IEnumerable<KeyValuePair<string, object>>>(2)!.ToDictionary(c => c.Key, c => c.Value)["{OriginalFormat}"].ToString() == "Invalid S-100 SAP callback request. Requested correlationId not found.").MustHaveHappenedOnceExactly();

            response.StatusCode.Should().Be(404);
        }
    }
}
