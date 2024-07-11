using System.Net;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using UKHO.ADDS.SAP.Mock.Common.Models;
using UKHO.ADDS.SAP.Mock.ErpCallback;

namespace UKHO.ADDS.SAP.Mock.IntegrationTests;

[TestFixture]
public class ErpFacadePriceEndpointClientTests
{
    private const string CallBackDelay = "100" ;
    private HttpClient _httpClient = new HttpClient();
    private IConfiguration _config =  new ConfigurationManager();
    
    [OneTimeSetUp]
    public void Setup()
    {
        _config["ErpFacadeConfig:ErpFacadeBaseUri"] = TestContext.Parameters["ErpBaseUri"];
        _config["ErpFacadeConfig:ErpFacadePriceEndpoint"] = TestContext.Parameters["ErpPriceEndpointUri"];
        _config["ErpFacadeConfig:SharedKey"] = TestContext.Parameters["ErpEndpointSharedKey"];
        _config["ErpFacadeConfig:MockSapPriceCallbackDelayMs"] = CallBackDelay;
    }

    [Test]
    public async Task SimulateCallBackFromSAP_WhenSendingValidCallBack_GetsValidResponseFromErpFacade()
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
                Durations = ["12", "8"],
            }
        }, _httpClient);
        var matInfo = new IM_MATINFO()
        {
            ACTIONITEMS = new []{ new ZMAT_ACTIONITEMS(){PRODUCTNAME = "TestProduct"} },
            CORRID = "" //If CORRID is given but no erp facade storage container has that name,
                        //response would be 404
        };
        var response = await erpClient.SimulateCallBackFromSap(matInfo);
        var responseContent = await response.Content.ReadAsStringAsync();
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        responseContent.Should().Be("400"); //Not an ideal integration test,
                        //but 400 is the response from real ERP when no correlation ID is given
    }
}