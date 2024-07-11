using System.Runtime.CompilerServices;
using System.Text.Json;
using FakeItEasy;
using Microsoft.Extensions.Configuration;
using UKHO.ADDS.SAP.Mock.Common.Models;
using UKHO.ADDS.SAP.Mock.ErpCallback;

//For testable http client wrapper
[assembly: InternalsVisibleTo("DynamicProxyGenAssembly2")] 

namespace UKHO.ADDS.SAP.Mock.UnitTests;

public class ErpFacadePriceEndpointClientUnitTests
{
    private const string CallBackDelay = "100" ;
    private HttpClientWrapper _fakeHttpClient = null!;
    private IConfiguration _config =  new ConfigurationManager();
    
    [OneTimeSetUp]
    public void Setup()
    {
        _config["ErpFacadeConfig:ErpFacadeBaseUri"] = "http://TestBaseUri/";
        _config["ErpFacadeConfig:ErpFacadePriceEndpoint"] = "/TestUri";
        _config["ErpFacadeConfig:SharedKey"] = "TestKey";
        _config["ErpFacadeConfig:MockSapPriceCallbackDelayMs"] = CallBackDelay;
        _fakeHttpClient = A.Fake<HttpClientWrapper>();
    }
    
    [Test]
    public async Task SimulateCallBackFromSap_WhenCalledWithValidConfigAndDummyData_SendsValidPostRequestToTargetURI()
    {
        var erpClient = new ErpFacadePriceEndpointClient(_config, new CustomDummyData()
        {
            CustomProductDummies = [new CustomProductDummy()
            {
                Durations = ["12", "24"],
                Name = "testName"
            }],
            DefaultProductDummy = new()
            {
                Durations = ["12"],
            }
        }, _fakeHttpClient);
        var matInfo = new IM_MATINFO()
        {
            ACTIONITEMS = new []
            {
                new ZMAT_ACTIONITEMS{PRODUCTNAME = "testName"},
                new ZMAT_ACTIONITEMS{PRODUCTNAME = "unknownName"}
            },
            CORRID = "12345"
        };
        await erpClient.SimulateCallBackFromSap(matInfo);
        
        A.CallTo(() => _fakeHttpClient.SendAsync(
            A<HttpRequestMessage>.That.Matches(req => ValidatePricePostRequest(req)), 
            A<CancellationToken>.Ignored))
            .MustHaveHappenedOnceExactly();
    }

    private bool ValidatePricePostRequest(HttpRequestMessage req)
    {
        var postRequestPriceInfo = GetPostedPriceInfo(req.Content!.ReadAsStringAsync().Result);
        
        return postRequestPriceInfo?.First(p => p.ProductName == "testName").Corrid == "12345" &&
               postRequestPriceInfo.FindAll(p => p.ProductName == "testName").Single(p => p.Duration == "12").Corrid == "12345" &&
               postRequestPriceInfo.FindAll(p => p.ProductName == "testName").Single(p => p.Duration == "24").Corrid == "12345" &&
               postRequestPriceInfo.Single(p => p.ProductName == "unknownName").Duration == "12" &&
               req.Method == HttpMethod.Post &&
               req.RequestUri == new Uri($"{_config["ErpFacadeConfig:ErpFacadePriceEndpoint"]}?key={_config["ErpFacadeConfig:SharedKey"]}", UriKind.Relative);
    }

    private static List<PriceInformation>? GetPostedPriceInfo(string content)
    {
        return JsonSerializer.Deserialize<List<PriceInformation>>(content);
    }
}

internal abstract class HttpClientWrapper : HttpClient;