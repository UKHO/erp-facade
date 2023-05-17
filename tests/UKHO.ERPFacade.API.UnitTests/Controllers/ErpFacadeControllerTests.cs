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
using UKHO.ERPFacade.API.Helpers;
using UKHO.ERPFacade.API.Models;
using UKHO.ERPFacade.API.Services;
using UKHO.ERPFacade.Common.IO;
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
        private IERPFacadeService _fakeErpFacadeService;
        private IScenarioBuilder _fakeScenarioBuilder;



        private ErpFacadeController _fakeErpFacadeController;

        [SetUp]
        public void Setup()
        {
            _fakeHttpContextAccessor = A.Fake<IHttpContextAccessor>();
            _fakeLogger = A.Fake<ILogger<ErpFacadeController>>();
            _fakeAzureTableReaderWriter = A.Fake<IAzureTableReaderWriter>();
            _fakeAzureBlobEventWriter = A.Fake<IAzureBlobEventWriter>();
            _fakeErpFacadeService = A.Fake<IERPFacadeService>();
            _fakeScenarioBuilder = A.Fake<IScenarioBuilder>();

            _fakeErpFacadeController = new ErpFacadeController(_fakeHttpContextAccessor,
                                                           _fakeLogger,
                                                           _fakeAzureTableReaderWriter,
                                                           _fakeAzureBlobEventWriter,
                                                           _fakeErpFacadeService,
                                                           _fakeScenarioBuilder
                                                           );
        }

        [Test]
        public async Task WhenValidRequestReceived_ThenErpFacadeReturns200OkResponse()
        {
            var fakeSapEventJson = JArray.Parse(@"[{""corrid"":""123""}]");

            A.CallTo(() => _fakeAzureBlobEventWriter.CheckIfContainerExists(A<string>.Ignored)).Returns(true);

            var result = (OkObjectResult)await _fakeErpFacadeController.PostPriceInformation(fakeSapEventJson);
            result.StatusCode.Should().Be(200);

            A.CallTo(() => _fakeAzureTableReaderWriter.UpdateResponseTimeEntity(A<string>.Ignored)).MustHaveHappened();
            A.CallTo(() => _fakeAzureBlobEventWriter.CheckIfContainerExists(A<string>.Ignored)).MustHaveHappened();

            A.CallTo(_fakeLogger).Where(call => call.Method.Name == "Log"
             && call.GetArgument<LogLevel>(0) == LogLevel.Information
             && call.GetArgument<EventId>(1) == EventIds.BlobExistsInAzure.ToEventId()
             && call.GetArgument<IEnumerable<KeyValuePair<string, object>>>(2)!.ToDictionary(c => c.Key, c => c.Value)["{OriginalFormat}"].ToString() == "Blob exists in the Azure Storage for the correlation ID received from SAP event.").MustHaveHappenedOnceExactly();
        }

        [Test]
        public async Task WhenCorrIdIsMissingInRequest_ThenErpFacadeReturns400BadRequestResponse()
        {
            var fakeSapEventJson = JArray.Parse(@"[{""corrid"":""""}]");

            var result = (BadRequestObjectResult)await _fakeErpFacadeController.PostPriceInformation(fakeSapEventJson);

            result.StatusCode.Should().Be(400);

            A.CallTo(() => _fakeAzureTableReaderWriter.UpdateResponseTimeEntity(A<string>.Ignored)).MustNotHaveHappened();
            A.CallTo(() => _fakeAzureBlobEventWriter.CheckIfContainerExists(A<string>.Ignored)).MustNotHaveHappened();

            A.CallTo(_fakeLogger).Where(call => call.Method.Name == "Log"
             && call.GetArgument<LogLevel>(0) == LogLevel.Warning
             && call.GetArgument<EventId>(1) == EventIds.CorrIdMissingInSAPEvent.ToEventId()
             && call.GetArgument<IEnumerable<KeyValuePair<string, object>>>(2)!.ToDictionary(c => c.Key, c => c.Value)["{OriginalFormat}"].ToString() == "Correlation Id is missing in the event received from the SAP.").MustHaveHappenedOnceExactly();
        }

        [Test]
        public async Task WhenInvalidCorrIdInRequest_ThenErpFacadeReturns404NotFoundResponse()
        {
            var fakeSapEventJson = JArray.Parse(@"[{""corrid"":""123""}]");

            A.CallTo(() => _fakeAzureBlobEventWriter.CheckIfContainerExists(A<string>.Ignored)).Returns(false);

            var result = (NotFoundObjectResult)await _fakeErpFacadeController.PostPriceInformation(fakeSapEventJson);

            result.StatusCode.Should().Be(404);

            A.CallTo(() => _fakeAzureTableReaderWriter.UpdateResponseTimeEntity(A<string>.Ignored)).MustHaveHappened();
            A.CallTo(() => _fakeAzureBlobEventWriter.CheckIfContainerExists(A<string>.Ignored)).MustHaveHappened();

            A.CallTo(_fakeLogger).Where(call => call.Method.Name == "Log"
             && call.GetArgument<LogLevel>(0) == LogLevel.Error
             && call.GetArgument<EventId>(1) == EventIds.BlobNotFoundInAzure.ToEventId()
             && call.GetArgument<IEnumerable<KeyValuePair<string, object>>>(2)!.ToDictionary(c => c.Key, c => c.Value)["{OriginalFormat}"].ToString() == "Blob does not exist in the Azure Storage for the correlation ID received from SAP event.").MustHaveHappenedOnceExactly();
        }


        [Test]
        public async Task WhenEESEventReponseJsonSizeGreaterThanOneMb_ThenErpFacadeLogWarningEventSizeExceedsLimit() {
           //Arrange
            var fakeSapEventJson = JArray.Parse(@"[{""corrid"":""123""}]");

           /* List<PriceInformationEvent> fakePriceInformationEvent = new()
            {
                new PriceInformationEvent()
                {
                    InUnitOfSales = new() { "Fake" },
                    IsCellReplaced = false,
                    Product = new(),
                    ScenarioType = ScenarioType.NewCell,
                    UnitOfSales = new()
                }
            };*/


            //Act
            A.CallTo(() => _fakeAzureBlobEventWriter.CheckIfContainerExists(A<string>.Ignored)).Returns(true);

            var result = (OkObjectResult)await _fakeErpFacadeController.Post(fakeSapEventJson);
            result.StatusCode.Should().Be(200);

            A.CallTo(() => _fakeAzureTableReaderWriter.UpdateResponseTimeEntity(A<string>.Ignored)).MustHaveHappened();
             A.CallTo(() => _fakeAzureBlobEventWriter.CheckIfContainerExists(A<string>.Ignored)).MustHaveHappened();

            A.CallTo(_fakeLogger).Where(call => call.Method.Name == "Log"
            && call.GetArgument<LogLevel>(0) == LogLevel.Information
            && call.GetArgument<EventId>(1) == EventIds.BlobExistsInAzure.ToEventId()
            && call.GetArgument<IEnumerable<KeyValuePair<string, object>>>(2)!.ToDictionary(c => c.Key, c => c.Value)["{OriginalFormat}"].ToString() == "Blob exists in the Azure Storage for the trace ID received from SAP event.").MustHaveHappenedOnceExactly();

           // A.CallTo(() => _fakeScenarioBuilder.BuildScenariosPayment(A<PriceInformationEvent>.Ignored)).Returns(fakePriceInformationEvent);

            A.CallTo(() => _fakeAzureBlobEventWriter.DownloadEvent(A<string>.Ignored, A<string>.Ignored)).MustHaveHappened();

            A.CallTo(() => _fakeErpFacadeService.BuildUnitOfSalePricePayload(A<List<PriceInformationEvent>>.Ignored)); //What to return here

            //A.CallTo(() => _fakeErpFacadeService.BuildEESEventWithPriceInformation(A<List<UnitsOfSalePrices>>.Ignored, A<string>.Ignored)).MustHaveHappened();
            A.CallTo(() => _fakeErpFacadeService.BuildEESEventWithPriceInformation(A<List<UnitsOfSalePrices>>.Ignored, A<string>.Ignored)).Returns(new JObject());

            //int payloadSize = new byte[1048576];
            
            var eventSize = A.CallTo(() => CommonHelper.GetEventSize(A<JObject>.Ignored)).Returns(20000000);

            //A.CallTo(() => _fakeLogger.LogWarning("Payload size is greater than 1MB"));

            A.CallTo(_fakeLogger).Where(call => call.Method.Name == "Log"
           && call.GetArgument<LogLevel>(0) == LogLevel.Warning
           && call.GetArgument<EventId>(1) == EventIds.BlobExistsInAzure.ToEventId()
           && call.GetArgument<IEnumerable<KeyValuePair<string, object>>>(2)!.ToDictionary(c => c.Key, c => c.Value)["{OriginalFormat}"].ToString() == "payload size is greater than 1 mb.").MustHaveHappenedOnceExactly();

        }
    }
}