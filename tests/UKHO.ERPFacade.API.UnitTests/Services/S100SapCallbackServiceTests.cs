using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Azure.Data.Tables;
using FakeItEasy;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using NUnit.Framework;
using UKHO.ERPFacade.API.Services;
using UKHO.ERPFacade.API.Services.EventPublishingServices;
using UKHO.ERPFacade.Common.Constants;
using UKHO.ERPFacade.Common.Enums;
using UKHO.ERPFacade.Common.Exceptions;
using UKHO.ERPFacade.Common.Logging;
using UKHO.ERPFacade.Common.Models;
using UKHO.ERPFacade.Common.Models.CloudEvents;
using UKHO.ERPFacade.Common.Operations.IO.Azure;

namespace UKHO.ERPFacade.API.UnitTests.Services;

[TestFixture]
public class S100SapCallbackServiceTests
{
    private IAzureBlobReaderWriter _fakeAzureBlobReaderWriter;
    private IAzureTableReaderWriter _fakeAzureTableReaderWriter;
    private ILogger<S100SapCallBackService> _fakeLogger;
    private IS100UnitOfSaleUpdatedEventPublishingService _fakeS100UnitOfSaleUpdatedEventPublishingService;
    private S100SapCallBackService _fakeSapCallbackService;
    private string _fakeCorrelationId;

    [SetUp]
    public void Setup()
    {
        _fakeCorrelationId = "123";
        _fakeAzureTableReaderWriter = A.Fake<IAzureTableReaderWriter>();
        _fakeAzureBlobReaderWriter = A.Fake<IAzureBlobReaderWriter>();
        _fakeLogger = A.Fake<ILogger<S100SapCallBackService>>();
        _fakeS100UnitOfSaleUpdatedEventPublishingService = A.Fake<IS100UnitOfSaleUpdatedEventPublishingService>();
        _fakeSapCallbackService = new S100SapCallBackService(_fakeAzureBlobReaderWriter, _fakeAzureTableReaderWriter, _fakeLogger, _fakeS100UnitOfSaleUpdatedEventPublishingService);
    }

    [Test]
    public async Task WhenGetEntityAsyncReturnsNull_ThenReturnFalse()
    {
        A.CallTo(() => _fakeAzureTableReaderWriter.GetEntityAsync(PartitionKeys.S100PartitionKey, _fakeCorrelationId))
         .Returns<TableEntity>(null);

        var result = await _fakeSapCallbackService.IsValidCallbackAsync(_fakeCorrelationId);
        result.Should().BeFalse();
    }

    [Test]
    public async Task WhenValidCorrelationIdIsProvidedInPayload_ThenPublishEventSuccessfully()
    {
        Result result = new Result(true, "");

        A.CallTo(() => _fakeS100UnitOfSaleUpdatedEventPublishingService.PublishEvent(A<BaseCloudEvent>.Ignored, A<string>.Ignored)).Returns(result);

        await _fakeSapCallbackService.ProcessSapCallback(_fakeCorrelationId);

        A.CallTo(_fakeLogger).Where(call => call.Method.Name == "Log"
                                            && call.GetArgument<LogLevel>(0) == LogLevel.Information
                                            && call.GetArgument<EventId>(1) == EventIds.ValidS100SapCallback.ToEventId()
                                            && call.GetArgument<IEnumerable<KeyValuePair<string, object>>>(2)!.ToDictionary(c => c.Key, c => c.Value)["{OriginalFormat}"].ToString() == "Processing of valid S-100 SAP callback request started.").MustHaveHappenedOnceExactly();

        A.CallTo(_fakeLogger).Where(call => call.Method.Name == "Log"
                                            && call.GetArgument<LogLevel>(0) == LogLevel.Information
                                            && call.GetArgument<EventId>(1) == EventIds.DownloadS100UnitOfSaleUpdatedEventIsStarted.ToEventId()
                                            && call.GetArgument<IEnumerable<KeyValuePair<string, object>>>(2)!.ToDictionary(c => c.Key, c => c.Value)["{OriginalFormat}"].ToString() == "Download S-100 Unit Of Sale Updated Event from blob container is started.").MustHaveHappenedOnceExactly();

        A.CallTo(_fakeLogger).Where(call => call.Method.Name == "Log"
                                            && call.GetArgument<LogLevel>(0) == LogLevel.Information
                                            && call.GetArgument<EventId>(1) == EventIds.DownloadS100UnitOfSaleUpdatedEventIsCompleted.ToEventId()
                                            && call.GetArgument<IEnumerable<KeyValuePair<string, object>>>(2)!.ToDictionary(c => c.Key, c => c.Value)["{OriginalFormat}"].ToString() == "Download S-100 Unit Of Sale Updated Event from blob container is completed.").MustHaveHappenedOnceExactly();

        A.CallTo(_fakeLogger).Where(call => call.Method.Name == "Log"
                                            && call.GetArgument<LogLevel>(0) == LogLevel.Information
                                            && call.GetArgument<EventId>(1) == EventIds.PublishingUnitOfSaleUpdatedEventToEesStarted.ToEventId()
                                            && call.GetArgument<IEnumerable<KeyValuePair<string, object>>>(2)!.ToDictionary(c => c.Key, c => c.Value)["{OriginalFormat}"].ToString() == "The publishing unit of sale updated event to EES is started.").MustHaveHappenedOnceExactly();

        A.CallTo(_fakeLogger).Where(call => call.Method.Name == "Log"
                                            && call.GetArgument<LogLevel>(0) == LogLevel.Information
                                            && call.GetArgument<EventId>(1) == EventIds.UnitOfSaleUpdatedEventPublished.ToEventId()
                                            && call.GetArgument<IEnumerable<KeyValuePair<string, object>>>(2)!.ToDictionary(c => c.Key, c => c.Value)["{OriginalFormat}"].ToString() == "The unit of sale updated event published to EES successfully.").MustHaveHappenedOnceExactly();

        A.CallTo(_fakeLogger).Where(call => call.Method.Name == "Log"
                                            && call.GetArgument<LogLevel>(0) == LogLevel.Information
                                            && call.GetArgument<EventId>(1) == EventIds.S100DataContentPublishedEventTableEntryUpdated.ToEventId()
                                            && call.GetArgument<IEnumerable<KeyValuePair<string, object>>>(2)!.ToDictionary(c => c.Key, c => c.Value)["{OriginalFormat}"].ToString() == "Status and event published date time for S-100 data content published event is updated successfully.").MustHaveHappenedOnceExactly();
    }

    [Test]
    public void WhenInValidCorrelationIdIsProvidedInPayload_ThenPublishEventIsFailed()
    {
        Result result = new Result(false, "Forbidden");

        A.CallTo(() => _fakeS100UnitOfSaleUpdatedEventPublishingService.PublishEvent(A<BaseCloudEvent>.Ignored, A<string>.Ignored)).Returns(result);

        Assert.ThrowsAsync<ERPFacadeException>(() => _fakeSapCallbackService.ProcessSapCallback(_fakeCorrelationId))
            .Message.Should().Be("Error occurred while publishing S-100 unit of sale updated event to EES.");

        A.CallTo(_fakeLogger).Where(call => call.Method.Name == "Log"
                                            && call.GetArgument<LogLevel>(0) == LogLevel.Information
                                            && call.GetArgument<EventId>(1) == EventIds.ValidS100SapCallback.ToEventId()
                                            && call.GetArgument<IEnumerable<KeyValuePair<string, object>>>(2)!.ToDictionary(c => c.Key, c => c.Value)["{OriginalFormat}"].ToString() == "Processing of valid S-100 SAP callback request started.").MustHaveHappenedOnceExactly();

        A.CallTo(_fakeLogger).Where(call => call.Method.Name == "Log"
                                            && call.GetArgument<LogLevel>(0) == LogLevel.Information
                                            && call.GetArgument<EventId>(1) == EventIds.DownloadS100UnitOfSaleUpdatedEventIsStarted.ToEventId()
                                            && call.GetArgument<IEnumerable<KeyValuePair<string, object>>>(2)!.ToDictionary(c => c.Key, c => c.Value)["{OriginalFormat}"].ToString() == "Download S-100 Unit Of Sale Updated Event from blob container is started.").MustHaveHappenedOnceExactly();

        A.CallTo(_fakeLogger).Where(call => call.Method.Name == "Log"
                                            && call.GetArgument<LogLevel>(0) == LogLevel.Information
                                            && call.GetArgument<EventId>(1) == EventIds.DownloadS100UnitOfSaleUpdatedEventIsCompleted.ToEventId()
                                            && call.GetArgument<IEnumerable<KeyValuePair<string, object>>>(2)!.ToDictionary(c => c.Key, c => c.Value)["{OriginalFormat}"].ToString() == "Download S-100 Unit Of Sale Updated Event from blob container is completed.").MustHaveHappenedOnceExactly();

        A.CallTo(_fakeLogger).Where(call => call.Method.Name == "Log"
                                            && call.GetArgument<LogLevel>(0) == LogLevel.Information
                                            && call.GetArgument<EventId>(1) == EventIds.PublishingUnitOfSaleUpdatedEventToEesStarted.ToEventId()
                                            && call.GetArgument<IEnumerable<KeyValuePair<string, object>>>(2)!.ToDictionary(c => c.Key, c => c.Value)["{OriginalFormat}"].ToString() == "The publishing unit of sale updated event to EES is started.").MustHaveHappenedOnceExactly();

        A.CallTo(_fakeLogger).Where(call => call.Method.Name == "Log"
                                            && call.GetArgument<LogLevel>(0) == LogLevel.Error
                                            && call.GetArgument<EventId>(1) == EventIds.ErrorOccurredWhilePublishingUnitOfSaleUpdatedEventToEes.ToEventId()
                                            && call.GetArgument<IEnumerable<KeyValuePair<string, object>>>(2)!.ToDictionary(c => c.Key, c => c.Value)["{OriginalFormat}"].ToString() == "Error occurred while publishing S-100 unit of sale updated event to EES.").MustHaveHappenedOnceExactly();
    }

    [Test]
    public async Task WhenProcessedSapCallback_ThenUpdateEntityWithResponseDateTimeAndEventPublishDateTime()
    {
        A.CallTo(() => _fakeAzureTableReaderWriter.UpdateEntityAsync(PartitionKeys.S100PartitionKey, _fakeCorrelationId, A<Dictionary<string, object>>.Ignored))
            .Returns(Task.FromResult(new object())); 

        A.CallTo(() => _fakeAzureBlobReaderWriter.DownloadEventAsync(EventPayloadFiles.S100DataEventFileName, _fakeCorrelationId.ToLower()))
            .Returns(Task.FromResult(JsonConvert.SerializeObject(new BaseCloudEvent())));

        A.CallTo(() => _fakeS100UnitOfSaleUpdatedEventPublishingService.PublishEvent(A<BaseCloudEvent>.Ignored, _fakeCorrelationId))
            .Returns(Task.FromResult(Result.Success()));

        await _fakeSapCallbackService.ProcessSapCallback(_fakeCorrelationId);

        A.CallTo(() => _fakeAzureTableReaderWriter.UpdateEntityAsync(
            PartitionKeys.S100PartitionKey,
            _fakeCorrelationId,
            A<Dictionary<string, object>>.That.Matches(d => d.ContainsKey("ResponseDateTime") && (DateTime)d["ResponseDateTime"] <= DateTime.UtcNow)))
            .MustHaveHappenedOnceExactly();

        A.CallTo(() => _fakeAzureTableReaderWriter.UpdateEntityAsync(
          PartitionKeys.S100PartitionKey,
          _fakeCorrelationId,
          A<Dictionary<string, object>>.That.Matches(d => d.ContainsKey("EventPublishedDateTime") && (DateTime)d["EventPublishedDateTime"] <= DateTime.UtcNow)))
          .MustHaveHappenedOnceExactly();

        A.CallTo(() => _fakeAzureTableReaderWriter.UpdateEntityAsync(
            PartitionKeys.S100PartitionKey,
            _fakeCorrelationId,
            A<Dictionary<string, object>>.That.Matches(d => d.ContainsKey("Status") && (string)d["Status"] == Status.Complete.ToString())))
            .MustHaveHappenedOnceExactly();
    }
}
