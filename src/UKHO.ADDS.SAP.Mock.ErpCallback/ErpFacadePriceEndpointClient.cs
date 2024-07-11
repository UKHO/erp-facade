using System.Net.Http.Headers;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using UKHO.ADDS.SAP.Mock.Common.Models;

namespace UKHO.ADDS.SAP.Mock.ErpCallback;

public class ErpFacadePriceEndpointClient
{
    private readonly string _erpFacadeBaseUri;
    private readonly string _priceEndPointUri;
    private readonly string _sharedKey;
    private readonly int _callBackDelay;
    private readonly HttpClient _httpClient;

    private CustomDummyData _dummyData;
    
    public ErpFacadePriceEndpointClient(IConfiguration config, CustomDummyData dummyData, HttpClient httpClient)
    {
        _erpFacadeBaseUri = config["ErpFacadeConfig:ErpFacadeBaseUri"] ?? throw new InvalidOperationException($"could not find ErpFacadeBaseUri in config");
        _priceEndPointUri = config["ErpFacadeConfig:ErpFacadePriceEndpoint"] ?? throw new InvalidOperationException($"could not find ErpFacadePriceEndpoint in config");
        _sharedKey = config["ErpFacadeConfig:SharedKey"] ?? throw new InvalidOperationException($"could not find SharedKey in config");
        _callBackDelay = int.Parse(config["ErpFacadeConfig:MockSapPriceCallbackDelayMs"] ?? throw new InvalidOperationException($"could not find MockSapPriceCallbackDelayMs in config"));
        _dummyData = dummyData;
        _httpClient = httpClient;
    }
    
    public async Task<HttpResponseMessage> SimulateCallBackFromSap(IM_MATINFO matinfo)
    {
        var priceList = CreateDummyPricingInformation(matinfo);
        var priceJson = JsonSerializer.Serialize(priceList);
        
        Console.WriteLine($"Entering SAP to ERP facade callback simulation, delaying {_callBackDelay} ms...");
        await Task.Delay(_callBackDelay);
        Console.WriteLine("Delay over, calling ERP facade");

        try
        {
            var content = new StringContent(priceJson);
            content.Headers.ContentType = new MediaTypeWithQualityHeaderValue("application/json");
            var uriWithKey = new Uri($"{_erpFacadeBaseUri}/{_priceEndPointUri}?key={_sharedKey}", UriKind.Absolute);
            var response = await _httpClient.PostAsync(uriWithKey,content);
            var responseContent = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"ERP response was {responseContent}");
            return response;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
        
    }

    private List<PriceInformation> CreateDummyPricingInformation(IM_MATINFO matInfo)
    {
        var priceInfo = new List<PriceInformation>();
        var productNames = matInfo.ACTIONITEMS.Select(i => i.PRODUCTNAME)
            .Where(name => !string.IsNullOrWhiteSpace(name))
            .Distinct();
        if (_dummyData.DefaultProductDummy is null || _dummyData.CustomProductDummies is null)
        {
            throw new InvalidOperationException("Dummy data from CustomDummyData not set before trying to create price information");
        }
        
        if (_dummyData.CustomProductDummies.Any(dummy => string.IsNullOrWhiteSpace(dummy.Name)))
        {
            throw new InvalidOperationException($"Dummy data from CustomDummyData is missing product names");
        }
        
        foreach (var productName in productNames)
        {
            if (_dummyData.CustomProductDummies.Select(d => d.Name!.ToUpperInvariant()).Contains(productName.ToUpperInvariant()))
                priceInfo.AddRange( CreateDurationsUsingMaterialInfo(matInfo, _dummyData.CustomProductDummies
                    .First(d => d.Name!.Equals(productName, StringComparison.InvariantCultureIgnoreCase)).Durations, productName));
            else
                priceInfo.AddRange(CreateDurationsUsingMaterialInfo(matInfo, _dummyData.DefaultProductDummy.Durations, productName));
        }
        
        return priceInfo;
    }

    private static List<PriceInformation> CreateDurationsUsingMaterialInfo(IM_MATINFO matInfo, string[]? productDurations, string productName)
    {
        var priceInfo = new List<PriceInformation>();
        priceInfo.AddRange((productDurations ?? throw new ArgumentNullException(nameof(productDurations))).Select(duration => new PriceInformation()
        {
            Corrid = matInfo.CORRID,
            Org = matInfo.ORG,
            ProductName = productName,
            Duration = duration,
            EffectiveDate = DateTime.Today.AddMonths(-2).ToString("yyyyMMdd"),
            EffectiveTime = DateTime.Now.AddMonths(-2).ToString("HHmmss"),
            Price = "1.00",
            Currency = "USD",
            FutureDate = DateTime.Today.AddMonths(4).ToString("yyyyMMdd"),
            FutureTime = DateTime.Now.AddMonths(4).ToString("HHmmss"),
            FuturePrice = "2.00",
            FutureCurr = "USD",
            ReqDate = DateTime.Today.ToString("yyyyMMdd"),
            ReqTime = DateTime.Now.ToString("HHmmss")
        }));
        return priceInfo;
    }
}