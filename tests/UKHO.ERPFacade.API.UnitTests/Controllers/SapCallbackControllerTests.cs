using System.Threading.Tasks;
using Azure.Data.Tables;
using FakeItEasy;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using UKHO.ERPFacade.API.Controllers;
using UKHO.ERPFacade.API.Services;
using UKHO.ERPFacade.API.Services.EventPublishingService;
using UKHO.ERPFacade.Common.Operations.IO.Azure;

namespace UKHO.ERPFacade.API.UnitTests.Controllers
{
    [TestFixture]
    public class SapCallbackControllerTests
    {
        private IHttpContextAccessor _fakeHttpContextAccessor;
        private ILogger<SapCallbackController> _fakeLogger;
        private IAzureTableReaderWriter _fakeAzureTableReaderWriter;
        private ISapCallBackService _fakeSapCallBackService;
        private IS100UnitOfSaleUpdatedEventPublishingService _fakeS100UnitOfSaleUpdatedEventPublishingService;
        private SapCallbackController _fakeSapCallbackController;

        [SetUp]
        public void Setup()
        {
            _fakeHttpContextAccessor = A.Fake<IHttpContextAccessor>();
            _fakeLogger = A.Fake<ILogger<SapCallbackController>>();
            _fakeAzureTableReaderWriter = A.Fake<IAzureTableReaderWriter>();
            _fakeSapCallBackService = A.Fake<ISapCallBackService>();
            _fakeS100UnitOfSaleUpdatedEventPublishingService = A.Fake<IS100UnitOfSaleUpdatedEventPublishingService>();

            _fakeSapCallbackController = new SapCallbackController(_fakeHttpContextAccessor, _fakeLogger, _fakeSapCallBackService, _fakeS100UnitOfSaleUpdatedEventPublishingService, _fakeAzureTableReaderWriter);
        }

        [Test]
        public async Task WhenValidCorrelationIdPassedInPayload_ThenSapCallbackReturns200OkResponse()
        {
            var fakeSapCallBackJson = JObject.Parse(@"{""correlationId"":""123""}");

            A.CallTo(() => _fakeSapCallBackService.IsValidCallback(A<string>.Ignored)).Returns(true);

            var result = (OkResult)await _fakeSapCallbackController.S100SapCallBack(fakeSapCallBackJson);

            result.StatusCode.Should().Be(200);
        }

        [Test]
        public async Task WhenInValidCorrelationIdPassedInPayload_ThenSapCallbackReturns404NotFoundResponse()
        {
            A.CallTo(() => _fakeAzureTableReaderWriter.GetEntityAsync(A<string>.Ignored, A<string>.Ignored))!.Returns((Task<TableEntity>)null);

            var fakeSapCallBackJson = JObject.Parse(@"{""correlationId"":""123""}");

            var result = (NotFoundObjectResult)await _fakeSapCallbackController.S100SapCallBack(fakeSapCallBackJson);

            result.StatusCode.Should().Be(404);
        }

        [Test]
        public async Task WhenEmptyCorrelationIdPassedInPayload_ThenSapCallbackReturns400BadRequestResponse()
        {
            A.CallTo(() => _fakeAzureTableReaderWriter.GetEntityAsync(A<string>.Ignored, A<string>.Ignored))!.Returns((Task<TableEntity>)null);

            var fakeSapCallBackJson = JObject.Parse(@"{""correlationId"":""""}");

            var result = (BadRequestObjectResult)await _fakeSapCallbackController.S100SapCallBack(fakeSapCallBackJson);

            result.StatusCode.Should().Be(400);
        }

        [Test]
        public async Task WhenNullCorrelationIdPassedInPayload_ThenSapCallbackReturns400BadRequestResponse()
        {
            A.CallTo(() => _fakeAzureTableReaderWriter.GetEntityAsync(A<string>.Ignored, A<string>.Ignored))!.Returns((Task<TableEntity>)null);

            var fakeSapCallBackJson = JObject.Parse(@"{""correlationId"": null}");

            var result = (BadRequestObjectResult)await _fakeSapCallbackController.S100SapCallBack(fakeSapCallBackJson);

            result.StatusCode.Should().Be(400);
        }

        [Test]
        public async Task WhenValidCorrelationIdPassedInPayloadButBlobIsNotExists_ThenSapCallbackReturns404NotFoundResponse()
        {
            var fakeSapCallBackJson = JObject.Parse(@"{""correlationId"":""123""}");

            A.CallTo(() => _fakeSapCallBackService.IsValidCallback(A<string>.Ignored))!.Returns(false);

            var result = (NotFoundObjectResult)await _fakeSapCallbackController.S100SapCallBack(fakeSapCallBackJson);

            result.StatusCode.Should().Be(404);
        }
    }
}
