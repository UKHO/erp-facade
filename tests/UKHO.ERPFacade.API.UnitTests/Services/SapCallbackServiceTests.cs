using System.Collections.Generic;
using System.Threading.Tasks;
using Azure.Data.Tables;
using FakeItEasy;
using FluentAssertions;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using UKHO.ERPFacade.API.Services;
using UKHO.ERPFacade.Common.Operations.IO.Azure;

namespace UKHO.ERPFacade.API.UnitTests.Services;

[TestFixture]
public class SapCallbackServiceTests
{
    private IAzureBlobReaderWriter _fakeAzureBlobReaderWriter;
    private IAzureTableReaderWriter _fakeAzureTableReaderWriter;
    private SapCallbackService _fakeSapCallbackService;
    private string _fakeCorrelationId;

    [SetUp]
    public void Setup()
    {
        _fakeCorrelationId = "123";
        _fakeAzureTableReaderWriter = A.Fake<IAzureTableReaderWriter>();
        _fakeAzureBlobReaderWriter = A.Fake<IAzureBlobReaderWriter>();
        _fakeSapCallbackService = new SapCallbackService(_fakeAzureBlobReaderWriter, _fakeAzureTableReaderWriter);
    }

    [Test]
    public async Task WhenValidCorrelationIdIsProvidedInPayload_ThenIsValidCallbackAsyncReturnsTrue()
    {
        A.CallTo(() => _fakeAzureTableReaderWriter.GetEntityAsync(A<string>.Ignored, A<string>.Ignored)).Returns(new TableEntity());

        var result = _fakeSapCallbackService.IsValidCallbackAsync(_fakeCorrelationId);

        result.Result.Should().Be(true);
    }

    [Test]
    public async Task WhenValidCorrelationIdIsProvidedInPayload_ThenGetEventPayloadReturnsPayload()
    {
        var fakeS100EventDataJson = JObject.Parse(@"{""Type"":""S100Event"", ""data"":{""correlationId"":""123""}}");
        A.CallTo(() => _fakeAzureBlobReaderWriter.DownloadEventAsync(A<string>.Ignored, A<string>.Ignored)).Returns(fakeS100EventDataJson.ToString());
        var result = await _fakeSapCallbackService.GetEventPayload(_fakeCorrelationId);
        var type = fakeS100EventDataJson.SelectToken("Type")?.Value<string>();
        result.Type.Should().Be(type);
    }

    [Test]
    public async Task WhenValidCorrelationIdIsProvidedInPayload_ThenResponseDateTimeUpdated()
    {
        await _fakeSapCallbackService.UpdateResponseDateTimeAsync(_fakeCorrelationId);
        A.CallTo(() => _fakeAzureTableReaderWriter.UpdateEntityAsync(A<string>.Ignored, A<string>.Ignored, A<Dictionary<string,object>>.Ignored)).MustHaveHappened();
    }

    [Test]
    public async Task WhenEventIsPublishedSuccessfully_ThenEventStatusAndEventPublishDateTimeEntityUpdated()
    {
        await _fakeSapCallbackService.UpdateEventStatusAndEventPublishDateTimeEntity(_fakeCorrelationId);
        A.CallTo(() => _fakeAzureTableReaderWriter.UpdateEntityAsync(A<string>.Ignored, A<string>.Ignored, A<Dictionary<string, object>>.Ignored)).MustHaveHappened();
    }
}
