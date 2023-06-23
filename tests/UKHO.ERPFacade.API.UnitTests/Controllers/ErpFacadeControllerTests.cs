using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FakeItEasy;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using UKHO.ERPFacade.API.Controllers;
using UKHO.ERPFacade.Common.Exceptions;
using UKHO.ERPFacade.Common.Infrastructure.EventService;
using UKHO.ERPFacade.Common.Infrastructure.EventService.EventProvider;
using UKHO.ERPFacade.Common.IO.Azure;
using UKHO.ERPFacade.Common.Logging;
using UKHO.ERPFacade.Common.Models;
using UKHO.ERPFacade.Common.Services;
using IJsonHelper = UKHO.ERPFacade.Common.IO.IJsonHelper;

namespace UKHO.ERPFacade.API.UnitTests.Controllers
{
    public class ErpFacadeControllerTests
    {
        private IHttpContextAccessor _fakeHttpContextAccessor;
        private ILogger<ErpFacadeController> _fakeLogger;
        private IAzureTableReaderWriter _fakeAzureTableReaderWriter;
        private IAzureBlobEventWriter _fakeAzureBlobEventWriter;
        private IErpFacadeService _fakeErpFacadeService;
        private IJsonHelper _fakeJsonHelper;
        private IEventPublisher _fakeEventPublisher;
        private ICloudEventFactory _fakeCloudEventFactory;

        private ErpFacadeController _fakeErpFacadeController;

        [SetUp]
        public void Setup()
        {
            _fakeHttpContextAccessor = A.Fake<IHttpContextAccessor>();
            _fakeLogger = A.Fake<ILogger<ErpFacadeController>>();
            _fakeAzureTableReaderWriter = A.Fake<IAzureTableReaderWriter>();
            _fakeAzureBlobEventWriter = A.Fake<IAzureBlobEventWriter>();
            _fakeErpFacadeService = A.Fake<IErpFacadeService>();
            _fakeJsonHelper = A.Fake<IJsonHelper>();
            _fakeEventPublisher = A.Fake<IEventPublisher>();
            _fakeCloudEventFactory = A.Fake<ICloudEventFactory>();


            _fakeErpFacadeController = new ErpFacadeController(_fakeHttpContextAccessor,
                                                           _fakeLogger,
                                                           _fakeAzureTableReaderWriter,
                                                           _fakeAzureBlobEventWriter,
                                                           _fakeErpFacadeService,
                                                           _fakeJsonHelper,
                                                           _fakeEventPublisher,
                                                           _fakeCloudEventFactory);
        }

        #region Data

        private readonly string encContentPublishedJson = "{\r\n    \"specversion\": \"1.0\",\r\n    \"type\": \"uk.gov.ukho.encpublishing.enccontentpublished.v2\",\r\n    \"source\": \"https://encpublishing.ukho.gov.uk\",\r\n    \"id\": \"2f03a25f-28b3-46ea-b009-5943250a9a41\",\r\n    \"time\": \"2020-10-13T12:08:03.4880776Z\",\r\n    \"_COMMENT\": \"A comma separated list of products\",\r\n    \"subject\": \"MX545010\",\r\n    \"datacontenttype\": \"application/json\",\r\n    \"data\": {\r\n        \"correlationId\": \"123ce4a4-1d62-4f56-b359-59e178d333333\",\r\n        \"products\": [\r\n            {\r\n                \"productType\": \"ENC S57\",\r\n                \"dataSetName\": \"MX545010.001\",\r\n                \"productName\": \"MX545010\",\r\n                \"title\": \"ISla Clarion\",\r\n                \"scale\": 90000,\r\n                \"usageBand\": 5,\r\n                \"editionNumber\": 1,\r\n                \"updateNumber\": 0,\r\n                \"mayAffectHoldings\": true,\r\n                \"contentChanged\": true,\r\n                \"permit\": \"permitString\",\r\n                \"providerName\": \"IC-ENC\",\r\n                \"size\": \"medium\",\r\n                \"_ENUM\": [\r\n                    \"large\",\r\n                    \"medium\",\r\n                    \"small\"\r\n                ],\r\n                \"agency\": \"MX\",\r\n                \"bundle\": [\r\n                    {\r\n                        \"bundleType\": \"DVD\",\r\n                        \"_ENUM\": [\r\n                            \"DVD\"\r\n                        ],\r\n                        \"location\": \"M1;B1\"\r\n                    }\r\n                ],\r\n                \"status\": {\r\n                    \"statusName\": \"New Edition\",\r\n                    \"_ENUM\": [\r\n                        \"New Edition\",\r\n                        \"Re-issue\",\r\n                        \"Update\",\r\n                        \"Cancellation Update\",\r\n                        \"Withdrawn\",\r\n                        \"Suspended\"\r\n                    ],\r\n                    \"statusDate\": \"2023-03-03T04:30:00+05:30\",\r\n                    \"isNewCell\": true,\r\n                    \"_COMMENT\": \"A cell new to the service\"\r\n                },\r\n                \"replaces\": [],\r\n                \"replacedBy\": [],\r\n                \"additionalCoverage\": [],\r\n                \"geographicLimit\": {\r\n                    \"boundingBox\": {\r\n                        \"northLimit\": 24.0,\r\n                        \"southLimit\": 22.0,\r\n                        \"eastLimit\": 120.0,\r\n                        \"westLimit\": 119.0\r\n                    },\r\n                    \"polygons\": [\r\n                        {\r\n                            \"polygon\": [\r\n                                {\r\n                                    \"latitude\": 24,\r\n                                    \"longitude\": 121.5\r\n                                },\r\n                                {\r\n                                    \"latitude\": 121,\r\n                                    \"longitude\": 56\r\n                                },\r\n                                {\r\n                                    \"latitude\": 45,\r\n                                    \"longitude\": 78\r\n                                },\r\n                                {\r\n                                    \"latitude\": 119,\r\n                                    \"longitude\": 121.5\r\n                                }\r\n                            ]\r\n                        },\r\n                        {\r\n                            \"polygon\": [\r\n                                {\r\n                                    \"latitude\": 24,\r\n                                    \"longitude\": 121.5\r\n                                },\r\n                                {\r\n                                    \"latitude\": 121,\r\n                                    \"longitude\": 56\r\n                                },\r\n                                {\r\n                                    \"latitude\": 45,\r\n                                    \"longitude\": 78\r\n                                },\r\n                                {\r\n                                    \"latitude\": 119,\r\n                                    \"longitude\": 121.5\r\n                                }\r\n                            ]\r\n                        }\r\n                    ]\r\n                },\r\n                \"_COMMENT\": \"The units in which product is included, including any 1-1 unit\",\r\n                \"inUnitsOfSale\": [\r\n                    \"MX545010\"\r\n                ],\r\n                \"s63\": {\r\n                    \"name\": \"XXXXXXXX.001\",\r\n                    \"hash\": \"5160204288db0a543850da177ec637b71ed55946e7f3843e6180b1906ee4f4ea\",\r\n                    \"location\": \"8198201e-78ce-4af8-9145-ad68ba0472e2\",\r\n                    \"fileSize\": \"4500\",\r\n                    \"compression\": true,\r\n                    \"s57Crc\": \"5C06E104\"\r\n                },\r\n                \"signature\": {\r\n                    \"name\": \"XXXXXXXX.001\",\r\n                    \"hash\": \"fd0c9e5f95c0b664a1a27c2ff98812c648ebd113b117f5639f74536c397fbac9\",\r\n                    \"location\": \"0ecf2f38-a876-4d77-bd0e-0d901d3a0e73\",\r\n                    \"fileSize\": \"2500\"\r\n                },\r\n                \"ancillaryFiles\": [\r\n                    {\r\n                        \"name\": \"GBXXXXXXXX_04.TXT\",\r\n                        \"hash\": \"d030b93ad8a0801e6955e4f52599f6200d01310155882a1b80eec78c9b93662b\",\r\n                        \"location\": \"2a29932e-dcb8-4e3a-b8a2-b3cfc335ede5\",\r\n                        \"fileSize\": \"1240\"\r\n                    },\r\n                    {\r\n                        \"name\": \"GBXXXXXXXX_01.TXT\",\r\n                        \"hash\": \"bb8042082bd1d37236801837585fa0df5e96097fb8d2281b41888af2b23ceb0b\",\r\n                        \"location\": \"1ad0f9c3-8c93-495a-99a1-06a36410faa9\",\r\n                        \"fileSize\": \"1360\"\r\n                    },\r\n                    {\r\n                        \"name\": \"GBXXXXXXXX_01.TXT\",\r\n                        \"hash\": \"81470666f387a9035f6b33f59fb9bbf0872e9c296fecee58c4e919d6a1d87ab6\",\r\n                        \"location\": \"eb443aad-394c-4eb0-b391-415a261605a1\",\r\n                        \"fileSize\": \"1360\"\r\n                    }\r\n                ]\r\n            }\r\n        ],\r\n        \"_COMMENT\": \"Prices for all units in event will be included, including Cancelled Cell\",\r\n        \"unitsOfSale\": [\r\n            {\r\n                \"unitName\": \"MX545010\",\r\n                \"title\": \"Isla Clarion\",\r\n                \"unitType\": \"AVCS Units Coastal\",\r\n                \"status\": \"ForSale\",\r\n                \"_ENUM\": [\r\n                    \"ForSale\",\r\n                    \"NotForSale\"\r\n                ],\r\n                \"_COMMENT\": \"BoundingBox or polygon or both or neither\",\r\n                \"geographicLimit\": {\r\n                    \"boundingBox\": {\r\n                        \"northLimit\": 24.146815,\r\n                        \"southLimit\": 22.581615,\r\n                        \"eastLimit\": 120.349635,\r\n                        \"westLimit\": 119.39142\r\n                    },\r\n                    \"polygons\": []\r\n                },\r\n                \"compositionChanges\": {\r\n                    \"addProducts\": [\r\n                        \"MX545010\"\r\n                    ],\r\n                    \"removeProducts\": []\r\n                }\r\n            },\r\n            {\r\n                \"unitName\": \"ECXXXX\",\r\n                \"title\": \"ECDIS Folio Some Title\",\r\n                \"unitType\": \"AVCS ECDIS Folios\",\r\n                \"status\": \"ForSale\",\r\n                \"geographicLimit\": {\r\n                    \"boundingBox\": {},\r\n                    \"polygons\": [\r\n                        {\r\n                            \"polygon\": [\r\n                                {\r\n                                    \"latitude\": 22.0,\r\n                                    \"longitude\": 120.0\r\n                                },\r\n                                {\r\n                                    \"latitude\": 22.0,\r\n                                    \"longitude\": 120\r\n                                },\r\n                                {\r\n                                    \"latitude\": 22.0,\r\n                                    \"longitude\": 120\r\n                                },\r\n                                {\r\n                                    \"latitude\": 22.0,\r\n                                    \"longitude\": 121.0\r\n                                }\r\n                            ]\r\n                        }\r\n                    ]\r\n                },\r\n                \"compositionChanges\": {\r\n                    \"addProducts\": [\r\n                        \"MX545010\"\r\n                    ],\r\n                    \"removeProducts\": []\r\n                }\r\n            },\r\n            {\r\n                \"unitName\": \"AVXXXX\",\r\n                \"title\": \"AVCS Online Folio Some Title\",\r\n                \"unitType\": \"AVCS Online Folio\",\r\n                \"status\": \"ForSale\",\r\n                \"geographicLimit\": {\r\n                    \"boundingBox\": {},\r\n                    \"polygons\": [\r\n                        {\r\n                            \"polygon\": [\r\n                                {\r\n                                    \"latitude\": 22.0,\r\n                                    \"longitude\": 120.0\r\n                                },\r\n                                {\r\n                                    \"latitude\": 22.0,\r\n                                    \"longitude\": 120\r\n                                },\r\n                                {\r\n                                    \"latitude\": 22.0,\r\n                                    \"longitude\": 120\r\n                                },\r\n                                {\r\n                                    \"latitude\": 22.0,\r\n                                    \"longitude\": 121.0\r\n                                }\r\n                            ]\r\n                        }\r\n                    ]\r\n                },\r\n                \"compositionChanges\": {\r\n                    \"addProducts\": [\r\n                        \"MX545010\"\r\n                    ],\r\n                    \"removeProducts\": []\r\n                }\r\n            },\r\n            {\r\n                \"unitName\": \"PAYSF\",\r\n                \"title\": \"World Folio\",\r\n                \"unitType\": \"AVCS Folio Transit\",\r\n                \"status\": \"ForSale\",\r\n                \"geographicLimit\": {\r\n                    \"boundingBox\": {},\r\n                    \"polygons\": [\r\n                        {\r\n                            \"polygon\": [\r\n                                {\r\n                                    \"latitude\": 89,\r\n                                    \"longitude\": -179.995\r\n                                },\r\n                                {\r\n                                    \"latitude\": 89,\r\n                                    \"longitude\": 0\r\n                                },\r\n                                {\r\n                                    \"latitude\": 89,\r\n                                    \"longitude\": 179.995\r\n                                },\r\n                                {\r\n                                    \"latitude\": -89,\r\n                                    \"longitude\": 179.995\r\n                                },\r\n                                {\r\n                                    \"latitude\": -89,\r\n                                    \"longitude\": 0\r\n                                },\r\n                                {\r\n                                    \"latitude\": -89,\r\n                                    \"longitude\": -179.995\r\n                                },\r\n                                {\r\n                                    \"latitude\": 89,\r\n                                    \"longitude\": -179.995\r\n                                }\r\n                            ]\r\n                        }\r\n                    ]\r\n                },\r\n                \"compositionChanges\": {\r\n                    \"addProducts\": [\r\n                        \"MX545010\"\r\n                    ],\r\n                    \"removeProducts\": []\r\n                }\r\n            }\r\n        ]\r\n    }\r\n}";

        private readonly string jsonString = "\"[{\\\"corrid\\\":\\\"123ce4a4-1d62-4f56-b359-59e178d333333\\\",\\\"org\\\":\\\"UKHO\\\",\\\"productname\\\":\\\"PAYSF\\\",\\\"duration\\\":\\\"12\\\",\\\"effectivedate\\\":\\\"20230427\\\",\\\"effectivetime\\\":\\\"101454\\\",\\\"price\\\":\\\"156.00 \\\",\\\"currency\\\":\\\"USD\\\",\\\"futuredate\\\":\\\"20230527\\\",\\\"futuretime\\\":\\\"000001\\\",\\\"futureprice\\\":\\\"180.00\\\",\\\"futurecurr\\\":\\\"USD\\\",\\\"reqdate\\\":\\\"20230328\\\",\\\"reqtime\\\":\\\"160000\\\"},{\\\"corrid\\\":\\\"123ce4a4-1d62-4f56-b359-59e178d333333\\\",\\\"org\\\":\\\"UKHO\\\",\\\"productname\\\":\\\"PAYSF\\\",\\\"duration\\\":\\\"3\\\",\\\"effectivedate\\\":\\\"20230427\\\",\\\"effectivetime\\\":\\\"101454\\\",\\\"price\\\":\\\"156.00 \\\",\\\"currency\\\":\\\"USD\\\",\\\"futuredate\\\":\\\"\\\",\\\"futuretime\\\":\\\"\\\",\\\"futureprice\\\":\\\"N/A\\\",\\\"futurecurr\\\":\\\"\\\",\\\"reqdate\\\":\\\"20230328\\\",\\\"reqtime\\\":\\\"160000\\\"},{\\\"corrid\\\":\\\"123ce4a4-1d62-4f56-b359-59e178d333333\\\",\\\"org\\\":\\\"UKHO\\\",\\\"productname\\\":\\\"AVCSO\\\",\\\"duration\\\":\\\"12\\\",\\\"effectivedate\\\":\\\"20230427\\\",\\\"effectivetime\\\":\\\"101454\\\",\\\"price\\\":\\\"156.00 \\\",\\\"currency\\\":\\\"USD\\\",\\\"futuredate\\\":\\\"\\\",\\\"futuretime\\\":\\\"\\\",\\\"futureprice\\\":\\\"N/A\\\",\\\"futurecurr\\\":\\\"\\\",\\\"reqdate\\\":\\\"20230328\\\",\\\"reqtime\\\":\\\"160000\\\"},{\\\"corrid\\\":\\\"123ce4a4-1d62-4f56-b359-59e178d333333\\\",\\\"org\\\":\\\"UKHO\\\",\\\"productname\\\":\\\"MX545010\\\",\\\"duration\\\":\\\"12\\\",\\\"effectivedate\\\":\\\"20230427\\\",\\\"effectivetime\\\":\\\"101454\\\",\\\"price\\\":\\\"35.60 \\\",\\\"currency\\\":\\\"USD\\\",\\\"futuredate\\\":\\\"\\\",\\\"futuretime\\\":\\\"\\\",\\\"futureprice\\\":\\\"N/A\\\",\\\"futurecurr\\\":\\\"\\\",\\\"reqdate\\\":\\\"20230328\\\",\\\"reqtime\\\":\\\"160000\\\"},{\\\"corrid\\\":\\\"123ce4a4-1d62-4f56-b359-59e178d333333\\\",\\\"org\\\":\\\"UKHO\\\",\\\"productname\\\":\\\"MX545010\\\",\\\"duration\\\":\\\"9\\\",\\\"effectivedate\\\":\\\"20230427\\\",\\\"effectivetime\\\":\\\"101454\\\",\\\"price\\\":\\\"32.04 \\\",\\\"currency\\\":\\\"USD\\\",\\\"futuredate\\\":\\\"\\\",\\\"futuretime\\\":\\\"\\\",\\\"futureprice\\\":\\\"N/A\\\",\\\"futurecurr\\\":\\\"\\\",\\\"reqdate\\\":\\\"20230328\\\",\\\"reqtime\\\":\\\"160000\\\"},{\\\"corrid\\\":\\\"123ce4a4-1d62-4f56-b359-59e178d333333\\\",\\\"org\\\":\\\"UKHO\\\",\\\"productname\\\":\\\"MX545010\\\",\\\"duration\\\":\\\"6\\\",\\\"effectivedate\\\":\\\"20230427\\\",\\\"effectivetime\\\":\\\"101454\\\",\\\"price\\\":\\\"21.36 \\\",\\\"currency\\\":\\\"USD\\\",\\\"futuredate\\\":\\\"\\\",\\\"futuretime\\\":\\\"\\\",\\\"futureprice\\\":\\\"N/A\\\",\\\"futurecurr\\\":\\\"\\\",\\\"reqdate\\\":\\\"20230328\\\",\\\"reqtime\\\":\\\"160000\\\"},{\\\"corrid\\\":\\\"123ce4a4-1d62-4f56-b359-59e178d333333\\\",\\\"org\\\":\\\"UKHO\\\",\\\"productname\\\":\\\"MX545010\\\",\\\"duration\\\":\\\"3\\\",\\\"effectivedate\\\":\\\"20230427\\\",\\\"effectivetime\\\":\\\"101454\\\",\\\"price\\\":\\\"10.68 \\\",\\\"currency\\\":\\\"USD\\\",\\\"futuredate\\\":\\\"\\\",\\\"futuretime\\\":\\\"\\\",\\\"futureprice\\\":\\\"N/A\\\",\\\"futurecurr\\\":\\\"\\\",\\\"reqdate\\\":\\\"20230328\\\",\\\"reqtime\\\":\\\"160000\\\"}]\"";

        #endregion Data

        [Test]
        public async Task WhenValidRequestReceived_ThenPostPriceInformationReturns200OkResponse()
        {
            var fakePriceInformationJson = JArray.Parse(@"[{""corrid"":""123""}]");

            A.CallTo(() => _fakeAzureBlobEventWriter.CheckIfContainerExists(A<string>.Ignored)).Returns(true);
            A.CallTo(() => _fakeAzureBlobEventWriter.DownloadEvent(A<string>.Ignored, A<string>.Ignored)).Returns(encContentPublishedJson);

            var result = (OkObjectResult)await _fakeErpFacadeController.PostPriceInformation(fakePriceInformationJson);
            result.StatusCode.Should().Be(200);

            A.CallTo(() => _fakeAzureTableReaderWriter.UpdateResponseTimeEntity(A<string>.Ignored)).MustHaveHappened();
            A.CallTo(() => _fakeAzureBlobEventWriter.CheckIfContainerExists(A<string>.Ignored)).MustHaveHappened();

            A.CallTo(_fakeLogger).Where(call => call.Method.Name == "Log"
            && call.GetArgument<LogLevel>(0) == LogLevel.Information
            && call.GetArgument<EventId>(1) == EventIds.ERPFacadeToSAPRequestFound.ToEventId()
            && call.GetArgument<IEnumerable<KeyValuePair<string, object>>>(2)!.ToDictionary(c => c.Key, c => c.Value)["{OriginalFormat}"].ToString() == "Valid SAP callback.").MustHaveHappenedOnceExactly();
        }

        [Test]
        public async Task WhenCorrelationIdIsMissingInRequest_ThenErpFacadeReturns400BadRequestResponse()
        {
            var fakePriceInformationJson = JArray.Parse(@"[{""corrid"":""""}]");

            var result = (BadRequestObjectResult)await _fakeErpFacadeController.PostPriceInformation(fakePriceInformationJson);

            result.StatusCode.Should().Be(400);

            A.CallTo(() => _fakeAzureTableReaderWriter.UpdateResponseTimeEntity(A<string>.Ignored)).MustNotHaveHappened();
            A.CallTo(() => _fakeAzureBlobEventWriter.CheckIfContainerExists(A<string>.Ignored)).MustNotHaveHappened();

            A.CallTo(_fakeLogger).Where(call => call.Method.Name == "Log"
             && call.GetArgument<LogLevel>(0) == LogLevel.Warning
             && call.GetArgument<EventId>(1) == EventIds.CorrelationIdMissingInSAPPriceInformationPayload.ToEventId()
             && call.GetArgument<IEnumerable<KeyValuePair<string, object>>>(2)!.ToDictionary(c => c.Key, c => c.Value)["{OriginalFormat}"].ToString() == "CorrelationId is missing in price information payload recieved from SAP.").MustHaveHappenedOnceExactly();
        }

        [Test]
        public async Task WhenInvalidCorrelationIdInRequest_ThenErpFacadeReturns404NotFoundResponse()
        {
            var fakePriceInformationJson = JArray.Parse(@"[{""corrid"":""123""}]");

            A.CallTo(() => _fakeAzureBlobEventWriter.CheckIfContainerExists(A<string>.Ignored)).Returns(false);

            var result = (NotFoundObjectResult)await _fakeErpFacadeController.PostPriceInformation(fakePriceInformationJson);

            result.StatusCode.Should().Be(404);

            A.CallTo(() => _fakeAzureTableReaderWriter.UpdateResponseTimeEntity(A<string>.Ignored)).MustHaveHappened();
            A.CallTo(() => _fakeAzureBlobEventWriter.CheckIfContainerExists(A<string>.Ignored)).MustHaveHappened();

            A.CallTo(_fakeLogger).Where(call => call.Method.Name == "Log"
            && call.GetArgument<LogLevel>(0) == LogLevel.Error
            && call.GetArgument<EventId>(1) == EventIds.ERPFacadeToSAPRequestNotFound.ToEventId()
            && call.GetArgument<IEnumerable<KeyValuePair<string, object>>>(2)!.ToDictionary(c => c.Key, c => c.Value)["{OriginalFormat}"].ToString() == "Invalid SAP callback. Request from ERP Facade to SAP not found.").MustHaveHappenedOnceExactly();
        }

        [Test]
        public async Task WhenUnitsOfSaleUpdatedEventPayloadJsonSizeIsLessThanOrEqualToOneMB_ThenErpFacadeReturns200OkResponse()
        {
            var requestJson = JArray.Parse(JsonConvert.DeserializeObject(jsonString).ToString()!);
            var unitsOfSalePricesList = GetUnitsOfSalePriceList();
            var eesPriceEventPayload = GetUnitsOfSaleUpdatedEventPayloadData();

            A.CallTo(() => _fakeAzureBlobEventWriter.CheckIfContainerExists(A<string>.Ignored)).Returns(true);
            A.CallTo(() => _fakeAzureBlobEventWriter.DownloadEvent(A<string>.Ignored, A<string>.Ignored)).Returns(encContentPublishedJson);

            A.CallTo(() => _fakeErpFacadeService.MapAndBuildUnitsOfSalePrices(A<List<PriceInformation>>.Ignored, A<List<string>>.Ignored)).Returns(unitsOfSalePricesList);

            A.CallTo(() => _fakeErpFacadeService.BuildUnitsOfSaleUpdatedEventPayload(A<List<UnitsOfSalePrices>>.Ignored, A<string>.Ignored)).Returns(eesPriceEventPayload);

            var result = (OkObjectResult)await _fakeErpFacadeController.PostPriceInformation(requestJson);
            result.StatusCode.Should().Be(200);

            A.CallTo(() => _fakeAzureTableReaderWriter.UpdateResponseTimeEntity(A<string>.Ignored)).MustHaveHappened();
            A.CallTo(() => _fakeAzureBlobEventWriter.CheckIfContainerExists(A<string>.Ignored)).MustHaveHappened();
            A.CallTo(() => _fakeAzureBlobEventWriter.DownloadEvent(A<string>.Ignored, A<string>.Ignored)).MustHaveHappened();

            A.CallTo(_fakeLogger).Where(call => call.Method.Name == "Log"
            && call.GetArgument<LogLevel>(0) == LogLevel.Information
            && call.GetArgument<EventId>(1) == EventIds.SapUnitsOfSalePriceInformationPayloadReceived.ToEventId()
            && call.GetArgument<IEnumerable<KeyValuePair<string, object>>>(2)!.ToDictionary(c => c.Key, c => c.Value)["{OriginalFormat}"].ToString() == "UnitsOfSale price information payload received from SAP.").MustHaveHappenedOnceExactly();

            A.CallTo(_fakeLogger).Where(call => call.Method.Name == "Log"
            && call.GetArgument<LogLevel>(0) == LogLevel.Information
            && call.GetArgument<EventId>(1) == EventIds.ERPFacadeToSAPRequestFound.ToEventId()
            && call.GetArgument<IEnumerable<KeyValuePair<string, object>>>(2)!.ToDictionary(c => c.Key, c => c.Value)["{OriginalFormat}"].ToString() == "Valid SAP callback.").MustHaveHappenedOnceExactly();

            A.CallTo(_fakeLogger).Where(call => call.Method.Name == "Log"
            && call.GetArgument<LogLevel>(0) == LogLevel.Information
            && call.GetArgument<EventId>(1) == EventIds.DownloadEncEventPayloadStarted.ToEventId()
            && call.GetArgument<IEnumerable<KeyValuePair<string, object>>>(2)!.ToDictionary(c => c.Key, c => c.Value)["{OriginalFormat}"].ToString() == "Downloading the ENC event payload from azure blob storage.").MustHaveHappenedOnceExactly();

            A.CallTo(_fakeLogger).Where(call => call.Method.Name == "Log"
            && call.GetArgument<LogLevel>(0) == LogLevel.Information
            && call.GetArgument<EventId>(1) == EventIds.DownloadEncEventPayloadCompleted.ToEventId()
            && call.GetArgument<IEnumerable<KeyValuePair<string, object>>>(2)!.ToDictionary(c => c.Key, c => c.Value)["{OriginalFormat}"].ToString() == "ENC event payload is downloaded from azure blob storage successfully.").MustHaveHappenedOnceExactly();
        }

        [Test]
        public void WhenUnitsOfSalePriceInformationIsMissingFromPriceInformationJson_ThenErpFacadeThrowsExceptionNoDataFoundInSAPPriceInformationPayload()
        {
            var fakePriceInformationJson = JArray.Parse(@"[{""corrid"":""123ce4a4-1d62-4f56-b359-59e178d333333"",""org"":""UKHO"",""productname"":""""}]");

            A.CallTo(() => _fakeAzureBlobEventWriter.CheckIfContainerExists(A<string>.Ignored)).Returns(true);

            A.CallTo(() => _fakeErpFacadeService.MapAndBuildUnitsOfSalePrices(A<List<PriceInformation>>.Ignored, A<List<string>>.Ignored)).MustNotHaveHappened();

            A.CallTo(() => _fakeErpFacadeService.BuildUnitsOfSaleUpdatedEventPayload(A<List<UnitsOfSalePrices>>.Ignored, A<string>.Ignored)).MustNotHaveHappened();

            Assert.ThrowsAsync<ERPFacadeException>(() => _fakeErpFacadeController.PostPriceInformation(fakePriceInformationJson));

            A.CallTo(() => _fakeAzureTableReaderWriter.UpdateResponseTimeEntity(A<string>.Ignored)).MustHaveHappened();
            A.CallTo(() => _fakeAzureBlobEventWriter.CheckIfContainerExists(A<string>.Ignored)).MustHaveHappened();
            A.CallTo(() => _fakeAzureBlobEventWriter.DownloadEvent(A<string>.Ignored, A<string>.Ignored)).MustNotHaveHappened();

            A.CallTo(_fakeLogger).Where(call => call.Method.Name == "Log"
            && call.GetArgument<LogLevel>(0) == LogLevel.Information
            && call.GetArgument<EventId>(1) == EventIds.SapUnitsOfSalePriceInformationPayloadReceived.ToEventId()
            && call.GetArgument<IEnumerable<KeyValuePair<string, object>>>(2)!.ToDictionary(c => c.Key, c => c.Value)["{OriginalFormat}"].ToString() == "UnitsOfSale price information payload received from SAP.").MustHaveHappenedOnceExactly();

            A.CallTo(_fakeLogger).Where(call => call.Method.Name == "Log"
            && call.GetArgument<LogLevel>(0) == LogLevel.Information
            && call.GetArgument<EventId>(1) == EventIds.ERPFacadeToSAPRequestFound.ToEventId()
            && call.GetArgument<IEnumerable<KeyValuePair<string, object>>>(2)!.ToDictionary(c => c.Key, c => c.Value)["{OriginalFormat}"].ToString() == "Valid SAP callback.").MustHaveHappenedOnceExactly();


            A.CallTo(_fakeLogger).Where(call => call.Method.Name == "Log"
            && call.GetArgument<LogLevel>(0) == LogLevel.Error
            && call.GetArgument<EventId>(1) == EventIds.NoDataFoundInSAPPriceInformationPayload.ToEventId()
            && call.GetArgument<IEnumerable<KeyValuePair<string, object>>>(2)!.ToDictionary(c => c.Key, c => c.Value)["{OriginalFormat}"].ToString() == "No data found in SAP price information payload.").MustHaveHappenedOnceExactly();
        }

        [Test]
        public void WhenUnitsOfSaleUpdatedEventPayloadJsonSizeGreaterThanOneMb_ThenErpFacadeThrowsException()
        {
            var requestJson = JArray.Parse(JsonConvert.DeserializeObject(jsonString).ToString()!);
            var unitsOfSalePricesList = GetUnitsOfSalePriceList();
            var eesPriceEventPayload = GetUnitsOfSaleUpdatedEventPayloadData();

            A.CallTo(() => _fakeAzureBlobEventWriter.CheckIfContainerExists(A<string>.Ignored)).Returns(true);
            A.CallTo(() => _fakeAzureBlobEventWriter.DownloadEvent(A<string>.Ignored, A<string>.Ignored)).Returns(encContentPublishedJson);

            A.CallTo(() => _fakeJsonHelper.GetPayloadJsonSize(A<string>.Ignored)).Returns(2000000);

            A.CallTo(() => _fakeErpFacadeService.MapAndBuildUnitsOfSalePrices(A<List<PriceInformation>>.Ignored, A<List<string>>.Ignored)).Returns(unitsOfSalePricesList);

            A.CallTo(() => _fakeErpFacadeService.BuildUnitsOfSaleUpdatedEventPayload(A<List<UnitsOfSalePrices>>.Ignored, A<string>.Ignored)).Returns(eesPriceEventPayload);

            Assert.ThrowsAsync<ERPFacadeException>(() => _fakeErpFacadeController.PostPriceInformation(requestJson));

            A.CallTo(() => _fakeAzureTableReaderWriter.UpdateResponseTimeEntity(A<string>.Ignored)).MustHaveHappened();
            A.CallTo(() => _fakeAzureBlobEventWriter.CheckIfContainerExists(A<string>.Ignored)).MustHaveHappened();
            A.CallTo(() => _fakeAzureBlobEventWriter.DownloadEvent(A<string>.Ignored, A<string>.Ignored)).MustHaveHappened();

            A.CallTo(_fakeLogger).Where(call => call.Method.Name == "Log"
            && call.GetArgument<LogLevel>(0) == LogLevel.Information
            && call.GetArgument<EventId>(1) == EventIds.SapUnitsOfSalePriceInformationPayloadReceived.ToEventId()
            && call.GetArgument<IEnumerable<KeyValuePair<string, object>>>(2)!.ToDictionary(c => c.Key, c => c.Value)["{OriginalFormat}"].ToString() == "UnitsOfSale price information payload received from SAP.").MustHaveHappenedOnceExactly();

            A.CallTo(_fakeLogger).Where(call => call.Method.Name == "Log"
            && call.GetArgument<LogLevel>(0) == LogLevel.Information
            && call.GetArgument<EventId>(1) == EventIds.ERPFacadeToSAPRequestFound.ToEventId()
            && call.GetArgument<IEnumerable<KeyValuePair<string, object>>>(2)!.ToDictionary(c => c.Key, c => c.Value)["{OriginalFormat}"].ToString() == "Valid SAP callback.").MustHaveHappenedOnceExactly();

            A.CallTo(_fakeLogger).Where(call => call.Method.Name == "Log"
            && call.GetArgument<LogLevel>(0) == LogLevel.Information
            && call.GetArgument<EventId>(1) == EventIds.DownloadEncEventPayloadStarted.ToEventId()
            && call.GetArgument<IEnumerable<KeyValuePair<string, object>>>(2)!.ToDictionary(c => c.Key, c => c.Value)["{OriginalFormat}"].ToString() == "Downloading the ENC event payload from azure blob storage.").MustHaveHappenedOnceExactly();

            A.CallTo(_fakeLogger).Where(call => call.Method.Name == "Log"
            && call.GetArgument<LogLevel>(0) == LogLevel.Information
            && call.GetArgument<EventId>(1) == EventIds.DownloadEncEventPayloadCompleted.ToEventId()
            && call.GetArgument<IEnumerable<KeyValuePair<string, object>>>(2)!.ToDictionary(c => c.Key, c => c.Value)["{OriginalFormat}"].ToString() == "ENC event payload is downloaded from azure blob storage successfully.").MustHaveHappenedOnceExactly();

            A.CallTo(_fakeLogger).Where(call => call.Method.Name == "Log"
            && call.GetArgument<LogLevel>(0) == LogLevel.Error
            && call.GetArgument<EventId>(1) == EventIds.UnitsOfSaleUpdatedEventSizeLimit.ToEventId()
            && call.GetArgument<IEnumerable<KeyValuePair<string, object>>>(2)!.ToDictionary(c => c.Key, c => c.Value)["{OriginalFormat}"].ToString() == "UnitsOfSale price event exceeds the size limit of 1 MB.").MustHaveHappenedOnceExactly();
        }

        private List<PriceInformation> GetPriceInformationData()
        {
            var requestJson = JsonConvert.DeserializeObject(jsonString);
            var priceInformationList = JsonConvert.DeserializeObject<List<PriceInformation>>(requestJson.ToString()!);
            return priceInformationList!;
        }

        private List<string> GetUnitOfSaleData()
        {
            var encContentData = JsonConvert.DeserializeObject<EncEventPayload>(encContentPublishedJson.ToString());
            var unitOfSaleList = encContentData.Data.UnitsOfSales.Select(x => x.UnitName).ToList();
            return unitOfSaleList!;
        }

        private List<UnitsOfSalePrices> GetUnitsOfSalePriceList()
        {
            var priceInformationList = GetPriceInformationData();
            var unitOfSaleList = GetUnitOfSaleData();

            var unitsOfSalePricesList = _fakeErpFacadeService.MapAndBuildUnitsOfSalePrices(priceInformationList, unitOfSaleList);

            return unitsOfSalePricesList!;
        }

        private UnitOfSaleUpdatedEventPayload GetUnitsOfSaleUpdatedEventPayloadData()
        {
            var unitsOfSalePricesList = GetUnitsOfSalePriceList();
            var existingEESJson = JsonConvert.DeserializeObject(encContentPublishedJson);
            var eesPriceEventPayload = _fakeErpFacadeService.BuildUnitsOfSaleUpdatedEventPayload(unitsOfSalePricesList, existingEESJson.ToString()!);

            return eesPriceEventPayload;
        }

        [Test]
        public async Task WhenValidRequestReceived_ThenPostBulkPriceInformationReturns200OkResponse()
        {
            var fakeSapEventJson = JArray.Parse(@"[{""corrid"":""123"",""org"": ""UKHO""},{""corrid"":""123"",""org"": ""UKHO""}]");

            var result = (OkObjectResult)await _fakeErpFacadeController.PostBulkPriceInformation(fakeSapEventJson);
            result.StatusCode.Should().Be(200);

            A.CallTo(_fakeLogger).Where(call => call.Method.Name == "Log"
            && call.GetArgument<LogLevel>(0) == LogLevel.Information
             && call.GetArgument<EventId>(1) == EventIds.SapBulkPriceInformationPayloadReceived.ToEventId()
            && call.GetArgument<IEnumerable<KeyValuePair<string, object>>>(2)!.ToDictionary(c => c.Key, c => c.Value)["{OriginalFormat}"].ToString() == "Bulk price information payload received from SAP.").MustHaveHappenedOnceExactly();

            A.CallTo(_fakeLogger).Where(call => call.Method.Name == "Log"
            && call.GetArgument<LogLevel>(0) == LogLevel.Information
             && call.GetArgument<EventId>(1) == EventIds.StoreBulkPriceInformationEventInAzureTable.ToEventId()
            && call.GetArgument<IEnumerable<KeyValuePair<string, object>>>(2)!.ToDictionary(c => c.Key, c => c.Value)["{OriginalFormat}"].ToString() == "Storing the received Bulk price information event in azure table.").MustHaveHappenedOnceExactly();

            A.CallTo(_fakeLogger).Where(call => call.Method.Name == "Log"
            && call.GetArgument<LogLevel>(0) == LogLevel.Information
             && call.GetArgument<EventId>(1) == EventIds.StoreBulkPriceInformationEventInAzureTable.ToEventId()
            && call.GetArgument<IEnumerable<KeyValuePair<string, object>>>(2)!.ToDictionary(c => c.Key, c => c.Value)["{OriginalFormat}"].ToString() == "Storing the received Bulk price information event in azure table.").MustHaveHappenedOnceExactly();

            A.CallTo(_fakeLogger).Where(call => call.Method.Name == "Log"
            && call.GetArgument<LogLevel>(0) == LogLevel.Information
             && call.GetArgument<EventId>(1) == EventIds.UploadBulkPriceInformationEventInAzureBlob.ToEventId()
            && call.GetArgument<IEnumerable<KeyValuePair<string, object>>>(2)!.ToDictionary(c => c.Key, c => c.Value)["{OriginalFormat}"].ToString() == "Uploading the received Bulk price information event in blob storage.").MustHaveHappenedOnceExactly();

            A.CallTo(_fakeLogger).Where(call => call.Method.Name == "Log"
           && call.GetArgument<LogLevel>(0) == LogLevel.Information
            && call.GetArgument<EventId>(1) == EventIds.UploadedBulkPriceInformationEventInAzureBlob.ToEventId()
           && call.GetArgument<IEnumerable<KeyValuePair<string, object>>>(2)!.ToDictionary(c => c.Key, c => c.Value)["{OriginalFormat}"].ToString() == "Bulk price information event is uploaded in blob storage successfully.").MustHaveHappenedOnceExactly();


        }
    }
}
