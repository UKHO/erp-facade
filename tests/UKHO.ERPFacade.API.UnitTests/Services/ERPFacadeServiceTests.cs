using FakeItEasy;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using UKHO.ERPFacade.API.Models;
using UKHO.ERPFacade.API.Services;
using UKHO.ERPFacade.Common.Logging;

namespace UKHO.ERPFacade.API.UnitTests.Services
{
    [TestFixture]
    public class ERPFacadeServiceTests
    {
        private ILogger<ErpFacadeService> _fakeLogger;

        private ErpFacadeService _fakeERPFacadeService;

        [SetUp]
        public void Setup()
        {
            _fakeLogger = A.Fake<ILogger<ErpFacadeService>>();
            _fakeERPFacadeService = new ErpFacadeService(_fakeLogger);
        }

        #region Data

        private string encContentPublishedJson = "\"{\\\"specversion\\\":\\\"1.0\\\",\\\"type\\\":\\\"uk.gov.ukho.encpublishing.enccontentpublished.v2\\\",\\\"source\\\":\\\"https://encpublishing.ukho.gov.uk\\\",\\\"id\\\":\\\"2f03a25f-28b3-46ea-b009-5943250a9a41\\\",\\\"time\\\":\\\"2020-10-13T12:08:03.4880776Z\\\",\\\"_COMMENT\\\":\\\"A comma separated list of products\\\",\\\"subject\\\":\\\"MX545010\\\",\\\"datacontenttype\\\":\\\"application/json\\\",\\\"data\\\":{\\\"traceId\\\":\\\"367ce4a4-1d62-4f56-b359-59e178d77321\\\",\\\"products\\\":[{\\\"productType\\\":\\\"ENC S57\\\",\\\"dataSetName\\\":\\\"MX545010.001\\\",\\\"productName\\\":\\\"MX545010\\\",\\\"title\\\":\\\"Isla Clarion\\\",\\\"scale\\\":90000,\\\"usageBand\\\":5,\\\"editionNumber\\\":1,\\\"updateNumber\\\":0,\\\"mayAffectHoldings\\\":true,\\\"contentChanged\\\":true,\\\"permit\\\":\\\"permitString\\\",\\\"providerCode\\\":1,\\\"providerDesc\\\":\\\"ICE\\\",\\\"size\\\":\\\"medium\\\",\\\"_ENUM\\\":[\\\"large\\\",\\\"medium\\\",\\\"small\\\"],\\\"agency\\\":\\\"MX\\\",\\\"bundle\\\":[{\\\"bundleType\\\":\\\"DVD\\\",\\\"_ENUM\\\":[\\\"DVD\\\"],\\\"location\\\":\\\"M1;B1\\\"}],\\\"status\\\":{\\\"statusName\\\":\\\"New Edition\\\",\\\"_ENUM\\\":[\\\"New Edition\\\",\\\"Re-issue\\\",\\\"Update\\\",\\\"Cancellation Update\\\",\\\"Withdrawn\\\",\\\"Suspended\\\"],\\\"statusDate\\\":\\\"2023-03-03T04:30:00+05:30\\\",\\\"isNewCell\\\":false,\\\"_COMMENT\\\":\\\"A cell new to the service\\\"},\\\"replaces\\\":[],\\\"replacedBy\\\":[],\\\"additionalCoverage\\\":[],\\\"geographicLimit\\\":{\\\"boundingBox\\\":{\\\"northLimit\\\":24.0,\\\"southLimit\\\":22.0,\\\"eastLimit\\\":120.0,\\\"westLimit\\\":119.0},\\\"polygons\\\":[{\\\"polygon\\\":[{\\\"latitude\\\":24,\\\"longitude\\\":121.5},{\\\"latitude\\\":121,\\\"longitude\\\":56},{\\\"latitude\\\":45,\\\"longitude\\\":78},{\\\"latitude\\\":119,\\\"longitude\\\":121.5}]},{\\\"polygon\\\":[{\\\"latitude\\\":24,\\\"longitude\\\":121.5},{\\\"latitude\\\":121,\\\"longitude\\\":56},{\\\"latitude\\\":45,\\\"longitude\\\":78},{\\\"latitude\\\":119,\\\"longitude\\\":121.5}]}]},\\\"_COMMENT\\\":\\\"The units in which product is included, including any 1-1 unit\\\",\\\"inUnitsOfSale\\\":[\\\"MX545010\\\",\\\"AVCSO\\\",\\\"PAYSF\\\"],\\\"s63\\\":{\\\"name\\\":\\\"XXXXXXXX.001\\\",\\\"hash\\\":\\\"5160204288db0a543850da177ec637b71ed55946e7f3843e6180b1906ee4f4ea\\\",\\\"location\\\":\\\"8198201e-78ce-4af8-9145-ad68ba0472e2\\\",\\\"fileSize\\\":\\\"4500\\\",\\\"compression\\\":true,\\\"s57Crc\\\":\\\"5C06E104\\\"},\\\"signature\\\":{\\\"name\\\":\\\"XXXXXXXX.001\\\",\\\"hash\\\":\\\"fd0c9e5f95c0b664a1a27c2ff98812c648ebd113b117f5639f74536c397fbac9\\\",\\\"location\\\":\\\"0ecf2f38-a876-4d77-bd0e-0d901d3a0e73\\\",\\\"fileSize\\\":\\\"2500\\\"},\\\"ancillaryFiles\\\":[{\\\"name\\\":\\\"GBXXXXXXXX_04.TXT\\\",\\\"hash\\\":\\\"d030b93ad8a0801e6955e4f52599f6200d01310155882a1b80eec78c9b93662b\\\",\\\"location\\\":\\\"2a29932e-dcb8-4e3a-b8a2-b3cfc335ede5\\\",\\\"fileSize\\\":\\\"1240\\\"},{\\\"name\\\":\\\"GBXXXXXXXX_01.TXT\\\",\\\"hash\\\":\\\"bb8042082bd1d37236801837585fa0df5e96097fb8d2281b41888af2b23ceb0b\\\",\\\"location\\\":\\\"1ad0f9c3-8c93-495a-99a1-06a36410faa9\\\",\\\"fileSize\\\":\\\"1360\\\"},{\\\"name\\\":\\\"GBXXXXXXXX_01.TXT\\\",\\\"hash\\\":\\\"81470666f387a9035f6b33f59fb9bbf0872e9c296fecee58c4e919d6a1d87ab6\\\",\\\"location\\\":\\\"eb443aad-394c-4eb0-b391-415a261605a1\\\",\\\"fileSize\\\":\\\"1360\\\"}]}],\\\"_COMMENT\\\":\\\"Prices for all units in event will be included, including Cancelled Cell\\\",\\\"unitsOfSale\\\":[{\\\"unitName\\\":\\\"MX545010\\\",\\\"title\\\":\\\"Isla Clarion\\\",\\\"unitType\\\":\\\"AVCS Units Coastal\\\",\\\"unitSize\\\":\\\"large\\\",\\\"status\\\":\\\"ForSale\\\",\\\"_ENUM\\\":[\\\"ForSale\\\",\\\"NotForSale\\\"],\\\"isNewUnitOfSale\\\":true,\\\"_COMMENT\\\":\\\"BoundingBox or polygon or both or neither\\\",\\\"geographicLimit\\\":{\\\"boundingBox\\\":{\\\"northLimit\\\":24.146815,\\\"southLimit\\\":22.581615,\\\"eastLimit\\\":120.349635,\\\"westLimit\\\":119.39142},\\\"polygons\\\":[]},\\\"compositionChanges\\\":{\\\"addProducts\\\":[\\\"MX545010\\\"],\\\"removeProducts\\\":[]}},{\\\"unitName\\\":\\\"AVCSO\\\",\\\"title\\\":\\\"AVCS Online Folio Some Title\\\",\\\"unitSize\\\":\\\"large\\\",\\\"unitType\\\":\\\"AVCS Online Folio\\\",\\\"status\\\":\\\"ForSale\\\",\\\"isNewUnitOfSale\\\":false,\\\"geographicLimit\\\":{\\\"boundingBox\\\":{},\\\"polygons\\\":[{\\\"polygon\\\":[{\\\"latitude\\\":22.0,\\\"longitude\\\":120.0},{\\\"latitude\\\":22.0,\\\"longitude\\\":120},{\\\"latitude\\\":22.0,\\\"longitude\\\":120},{\\\"latitude\\\":22.0,\\\"longitude\\\":121.0}]}]},\\\"compositionChanges\\\":{\\\"addProducts\\\":[\\\"MX545010\\\"],\\\"removeProducts\\\":[]}},{\\\"unitName\\\":\\\"PAYSF\\\",\\\"title\\\":\\\"World Folio\\\",\\\"unitType\\\":\\\"AVCS Folio Transit\\\",\\\"unitSize\\\":\\\"large\\\",\\\"status\\\":\\\"ForSale\\\",\\\"isNewUnitOfSale\\\":false,\\\"geographicLimit\\\":{\\\"boundingBox\\\":{},\\\"polygons\\\":[{\\\"polygon\\\":[{\\\"latitude\\\":89,\\\"longitude\\\":-179.995},{\\\"latitude\\\":89,\\\"longitude\\\":0},{\\\"latitude\\\":89,\\\"longitude\\\":179.995},{\\\"latitude\\\":-89,\\\"longitude\\\":179.995},{\\\"latitude\\\":-89,\\\"longitude\\\":0},{\\\"latitude\\\":-89,\\\"longitude\\\":-179.995},{\\\"latitude\\\":89,\\\"longitude\\\":-179.995}]}]},\\\"compositionChanges\\\":{\\\"addProducts\\\":[\\\"MX545010\\\"],\\\"removeProducts\\\":[]}}]}}\"";

        private string jsonString = "\"[{\\\"corrid\\\":\\\"367ce4a4-1d62-4f56-b359-59e178d77321\\\",\\\"org\\\":\\\"UKHO\\\",\\\"productname\\\":\\\"PAYSF\\\",\\\"duration\\\":\\\"12\\\",\\\"effectivedate\\\":\\\"20230427\\\",\\\"effectivetime\\\":\\\"101454\\\",\\\"price\\\":\\\"156.00 \\\",\\\"currency\\\":\\\"USD\\\",\\\"futuredate\\\":\\\"20230527\\\",\\\"futuretime\\\":\\\"000001\\\",\\\"futureprice\\\":\\\"180.00\\\",\\\"futurecurr\\\":\\\"USD\\\",\\\"reqdate\\\":\\\"20230328\\\",\\\"reqtime\\\":\\\"160000\\\"},{\\\"corrid\\\":\\\"367ce4a4-1d62-4f56-b359-59e178d77321\\\",\\\"org\\\":\\\"UKHO\\\",\\\"productname\\\":\\\"PAYSF\\\",\\\"duration\\\":\\\"3\\\",\\\"effectivedate\\\":\\\"20230427\\\",\\\"effectivetime\\\":\\\"101454\\\",\\\"price\\\":\\\"156.00 \\\",\\\"currency\\\":\\\"USD\\\",\\\"futuredate\\\":\\\"\\\",\\\"futuretime\\\":\\\"\\\",\\\"futureprice\\\":\\\"N/A\\\",\\\"futurecurr\\\":\\\"\\\",\\\"reqdate\\\":\\\"20230328\\\",\\\"reqtime\\\":\\\"160000\\\"},{\\\"corrid\\\":\\\"367ce4a4-1d62-4f56-b359-59e178d77321\\\",\\\"org\\\":\\\"UKHO\\\",\\\"productname\\\":\\\"AVCSO\\\",\\\"duration\\\":\\\"12\\\",\\\"effectivedate\\\":\\\"20230427\\\",\\\"effectivetime\\\":\\\"101454\\\",\\\"price\\\":\\\"156.00 \\\",\\\"currency\\\":\\\"USD\\\",\\\"futuredate\\\":\\\"\\\",\\\"futuretime\\\":\\\"\\\",\\\"futureprice\\\":\\\"N/A\\\",\\\"futurecurr\\\":\\\"\\\",\\\"reqdate\\\":\\\"20230328\\\",\\\"reqtime\\\":\\\"160000\\\"},{\\\"corrid\\\":\\\"367ce4a4-1d62-4f56-b359-59e178d77321\\\",\\\"org\\\":\\\"UKHO\\\",\\\"productname\\\":\\\"MX545010\\\",\\\"duration\\\":\\\"12\\\",\\\"effectivedate\\\":\\\"20230427\\\",\\\"effectivetime\\\":\\\"101454\\\",\\\"price\\\":\\\"35.60 \\\",\\\"currency\\\":\\\"USD\\\",\\\"futuredate\\\":\\\"\\\",\\\"futuretime\\\":\\\"\\\",\\\"futureprice\\\":\\\"N/A\\\",\\\"futurecurr\\\":\\\"\\\",\\\"reqdate\\\":\\\"20230328\\\",\\\"reqtime\\\":\\\"160000\\\"},{\\\"corrid\\\":\\\"367ce4a4-1d62-4f56-b359-59e178d77321\\\",\\\"org\\\":\\\"UKHO\\\",\\\"productname\\\":\\\"MX545010\\\",\\\"duration\\\":\\\"9\\\",\\\"effectivedate\\\":\\\"20230427\\\",\\\"effectivetime\\\":\\\"101454\\\",\\\"price\\\":\\\"32.04 \\\",\\\"currency\\\":\\\"USD\\\",\\\"futuredate\\\":\\\"\\\",\\\"futuretime\\\":\\\"\\\",\\\"futureprice\\\":\\\"N/A\\\",\\\"futurecurr\\\":\\\"\\\",\\\"reqdate\\\":\\\"20230328\\\",\\\"reqtime\\\":\\\"160000\\\"},{\\\"corrid\\\":\\\"367ce4a4-1d62-4f56-b359-59e178d77321\\\",\\\"org\\\":\\\"UKHO\\\",\\\"productname\\\":\\\"MX545010\\\",\\\"duration\\\":\\\"6\\\",\\\"effectivedate\\\":\\\"20230427\\\",\\\"effectivetime\\\":\\\"101454\\\",\\\"price\\\":\\\"21.36 \\\",\\\"currency\\\":\\\"USD\\\",\\\"futuredate\\\":\\\"\\\",\\\"futuretime\\\":\\\"\\\",\\\"futureprice\\\":\\\"N/A\\\",\\\"futurecurr\\\":\\\"\\\",\\\"reqdate\\\":\\\"20230328\\\",\\\"reqtime\\\":\\\"160000\\\"},{\\\"corrid\\\":\\\"367ce4a4-1d62-4f56-b359-59e178d77321\\\",\\\"org\\\":\\\"UKHO\\\",\\\"productname\\\":\\\"MX545010\\\",\\\"duration\\\":\\\"3\\\",\\\"effectivedate\\\":\\\"20230427\\\",\\\"effectivetime\\\":\\\"101454\\\",\\\"price\\\":\\\"10.68 \\\",\\\"currency\\\":\\\"USD\\\",\\\"futuredate\\\":\\\"\\\",\\\"futuretime\\\":\\\"\\\",\\\"futureprice\\\":\\\"N/A\\\",\\\"futurecurr\\\":\\\"\\\",\\\"reqdate\\\":\\\"20230328\\\",\\\"reqtime\\\":\\\"160000\\\"}]\"";

        #endregion Data

        [Test]
        public void WhenPriceInformationDetailIsPassed_ThenReturnsUnitsOfSalePrices()
        {
            var priceInformationList = GetPriceInformationData();
            var result = _fakeERPFacadeService.MapAndBuildUnitsOfSalePrices(priceInformationList);

            result.Should().BeOfType<List<UnitsOfSalePrices>>();
        }

        [Test]
        public void WhenValidInformationIsPassed_ThenReturnsPriceEventPayload()
        {
            var unitsOfSalePricesList = GetUnitsOfSalePriceList();

            var existingEESJson = JsonConvert.DeserializeObject(encContentPublishedJson);
            var result = _fakeERPFacadeService.BuildUnitsOfSaleUpdatedEventPayload(unitsOfSalePricesList, existingEESJson.ToString()!);

            A.CallTo(_fakeLogger).Where(call => call.Method.Name == "Log"
         && call.GetArgument<LogLevel>(0) == LogLevel.Information
         && call.GetArgument<EventId>(1) == EventIds.AppendingUnitofSalePricesToEncEvent.ToEventId()
         && call.GetArgument<IEnumerable<KeyValuePair<string, object>>>(2)!.ToDictionary(c => c.Key, c => c.Value)["{OriginalFormat}"].ToString() == "Appending UnitofSale prices to ENC event.").MustHaveHappenedOnceExactly();

            A.CallTo(_fakeLogger).Where(call => call.Method.Name == "Log"
         && call.GetArgument<LogLevel>(0) == LogLevel.Information
         && call.GetArgument<EventId>(1) == EventIds.UnitsOfSaleUpdatedEventPayloadCreated.ToEventId()
         && call.GetArgument<IEnumerable<KeyValuePair<string, object>>>(2)!.ToDictionary(c => c.Key, c => c.Value)["{OriginalFormat}"].ToString() == "UnitofSale updated event payload created.").MustHaveHappenedOnceExactly();

            result.Should().BeOfType<UnitOfSaleUpdatedEventPayload>();
        }

        private List<PriceInformation> GetPriceInformationData()
        {
            var requestJson = JsonConvert.DeserializeObject(jsonString);
            var priceInformationList = JsonConvert.DeserializeObject<List<PriceInformation>>(requestJson.ToString()!);
            return priceInformationList!;
        }

        private List<UnitsOfSalePrices> GetUnitsOfSalePriceList()
        {
            var priceInformationList = GetPriceInformationData();
            var unitsOfSalePricesList = _fakeERPFacadeService.MapAndBuildUnitsOfSalePrices(priceInformationList);

            return unitsOfSalePricesList!;
        }
    }
}