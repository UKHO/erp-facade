using FakeItEasy;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UKHO.ERPFacade.API.Controllers;
using UKHO.ERPFacade.Common.Infrastructure.EventService;
using UKHO.ERPFacade.Common.IO.Azure;
using UKHO.ERPFacade.Common.Logging;

namespace UKHO.ERPFacade.API.UnitTests.Controllers
{
    public class ErpFacadeControllerTests
    {
        private IHttpContextAccessor _fakeHttpContextAccessor;
        private ILogger<ErpFacadeController> _fakeLogger;
        private IAzureTableReaderWriter _fakeAzureTableReaderWriter;
        private IAzureBlobEventWriter _fakeAzureBlobEventWriter;
        private IEventPublisher _fakeEventPublisher;

        private ErpFacadeController _fakeErpFacadeController;

        [SetUp]
        public void Setup()
        {
            _fakeHttpContextAccessor = A.Fake<IHttpContextAccessor>();
            _fakeLogger = A.Fake<ILogger<ErpFacadeController>>();
            _fakeAzureTableReaderWriter = A.Fake<IAzureTableReaderWriter>();
            _fakeAzureBlobEventWriter = A.Fake<IAzureBlobEventWriter>();
            _fakeEventPublisher = A.Fake<IEventPublisher>();

            _fakeErpFacadeController = new ErpFacadeController(_fakeHttpContextAccessor,
                                                           _fakeLogger,
                                                           _fakeAzureTableReaderWriter,
                                                           _fakeAzureBlobEventWriter,
                                                           _fakeEventPublisher);
        }

        [Test]
        public async Task WhenValidRequestReceived_ThenErpFacadeReturns200OkResponse()
        {
            var fakeSapEventJson = JObject.Parse(@"{""data"":{""traceId"":""123""}}");

            A.CallTo(() => _fakeAzureBlobEventWriter.CheckIfContainerExists(A<string>.Ignored)).Returns(true);

            var result = (OkObjectResult)await _fakeErpFacadeController.Post(fakeSapEventJson);
            result.StatusCode.Should().Be(200);

            A.CallTo(() => _fakeAzureTableReaderWriter.UpdateResponseTimeEntity(A<string>.Ignored)).MustHaveHappened();
            A.CallTo(() => _fakeAzureBlobEventWriter.CheckIfContainerExists(A<string>.Ignored)).MustHaveHappened();

            A.CallTo(_fakeLogger).Where(call => call.Method.Name == "Log"
             && call.GetArgument<LogLevel>(0) == LogLevel.Information
             && call.GetArgument<EventId>(1) == EventIds.BlobExistsInAzure.ToEventId()
             && call.GetArgument<IEnumerable<KeyValuePair<string, object>>>(2)!.ToDictionary(c => c.Key, c => c.Value)["{OriginalFormat}"].ToString() == "Blob exists in the Azure Storage for the trace ID received from SAP event.").MustHaveHappenedOnceExactly();
        }

        [Test]
        public async Task WhenTraceIdIsMissingInRequest_ThenErpFacadeReturnsReturns400BadRequestResponse()
        {
            var fakeSapEventJson = JObject.Parse(@"{""data"":{""corId"":""123""}}");

            var result = (BadRequestObjectResult)await _fakeErpFacadeController.Post(fakeSapEventJson);

            result.StatusCode.Should().Be(400);

            A.CallTo(() => _fakeAzureTableReaderWriter.UpdateResponseTimeEntity(A<string>.Ignored)).MustNotHaveHappened();
            A.CallTo(() => _fakeAzureBlobEventWriter.CheckIfContainerExists(A<string>.Ignored)).MustNotHaveHappened();

            A.CallTo(_fakeLogger).Where(call => call.Method.Name == "Log"
             && call.GetArgument<LogLevel>(0) == LogLevel.Warning
             && call.GetArgument<EventId>(1) == EventIds.TraceIdMissingInSAPEvent.ToEventId()
             && call.GetArgument<IEnumerable<KeyValuePair<string, object>>>(2)!.ToDictionary(c => c.Key, c => c.Value)["{OriginalFormat}"].ToString() == "TraceId is missing in the event received from the SAP.").MustHaveHappenedOnceExactly();
        }

        [Test]
        public async Task WhenInvalidTraceIdInRequest_ThenErpFacadeReturns404NotFoundResponse()
        {
            var fakeSapEventJson = JObject.Parse(@"{""data"":{""traceId"":""123""}}");

            A.CallTo(() => _fakeAzureBlobEventWriter.CheckIfContainerExists(A<string>.Ignored)).Returns(false);

            var result = (NotFoundObjectResult)await _fakeErpFacadeController.Post(fakeSapEventJson);

            result.StatusCode.Should().Be(404);

            A.CallTo(() => _fakeAzureTableReaderWriter.UpdateResponseTimeEntity(A<string>.Ignored)).MustHaveHappened();
            A.CallTo(() => _fakeAzureBlobEventWriter.CheckIfContainerExists(A<string>.Ignored)).MustHaveHappened();

            A.CallTo(_fakeLogger).Where(call => call.Method.Name == "Log"
             && call.GetArgument<LogLevel>(0) == LogLevel.Error
             && call.GetArgument<EventId>(1) == EventIds.BlobNotFoundInAzure.ToEventId()
             && call.GetArgument<IEnumerable<KeyValuePair<string, object>>>(2)!.ToDictionary(c => c.Key, c => c.Value)["{OriginalFormat}"].ToString() == "Blob does not exist in the Azure Storage for the trace ID received from SAP event.").MustHaveHappenedOnceExactly();
        }
    }
}