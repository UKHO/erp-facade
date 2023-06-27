﻿using FluentAssertions;
using NUnit.Framework;
using UKHO.ERPFacade.API.FunctionalTests.Helpers;

namespace UKHO.ERPFacade.API.FunctionalTests.FunctionalTests
{
    [TestFixture]
    public class UnitOfSaleScenarios
    {

        private UnitOfSaleEndpoint _unitOfSale { get; set; }
        private WebhookEndpoint _webhook { get; set; }
        private readonly ADAuthTokenProvider _authToken = new();
        public static bool noRole = false;
        //for pipeline
        private readonly string _projectDir = Path.GetFullPath(Path.Combine(Environment.CurrentDirectory));
        //for local
        //private readonly string _projectDir = Path.GetFullPath(Path.Combine(Environment.CurrentDirectory, "..\\..\\.."));

        [OneTimeSetUp]
        public void Setup()
        {
            _unitOfSale = new UnitOfSaleEndpoint(Config.TestConfig.ErpFacadeConfiguration.BaseUrl);
            _webhook = new WebhookEndpoint();
        }

        [Category("DevEnvFT")]
        [Category("QAEnvFT")]
        [Test(Description = "WhenValidEventReceivedWithValidToken_ThenUoSReturns200OkResponse"), Order(0)]
        public async Task WhenValidEventReceivedWithValidToken_ThenUoSReturns200OkResponse()
        {
            string filePathWebhook = Path.Combine(_projectDir, Config.TestConfig.PayloadFolder, Config.TestConfig.WebhookPayloadFileName);
            string generatedXMLFolder = Path.Combine(_projectDir, Config.TestConfig.GeneratedXMLFolder);
            string webhookToken = await _authToken.GetAzureADToken(false);
            string filePath = Path.Combine(_projectDir, Config.TestConfig.PayloadFolder, Config.TestConfig.UoSPayloadFileName);
            var response = await _unitOfSale.PostUoSResponseAsync(filePathWebhook, generatedXMLFolder, webhookToken, filePath, await _authToken.GetAzureADToken(false, "UnitOfSale"));
            response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);

        }
        [Category("DevEnvFT")]
        [Category("QAEnvFT")]
        [Test(Description = "WhenValidEventReceivedWithInvalidToken_ThenUoSReturns401UnAuthorizedResponse"), Order(2)]
        public async Task WhenValidEventReceivedWithInvalidToken_ThenUoSReturns401UnAuthorizedResponse()
        {
            string filePath = Path.Combine(_projectDir, Config.TestConfig.PayloadFolder, Config.TestConfig.UoSPayloadFileName);
            string filePathWebhook = Path.Combine(_projectDir, Config.TestConfig.PayloadFolder, Config.TestConfig.WebhookPayloadFileName);
            string generatedXMLFolder = Path.Combine(_projectDir, Config.TestConfig.GeneratedXMLFolder);
            string webhookToken = await _authToken.GetAzureADToken(false);
            var response = await _unitOfSale.PostUoSResponseAsync(filePathWebhook, generatedXMLFolder, webhookToken, filePath, "invalidToken_abcd");
            response.StatusCode.Should().Be(System.Net.HttpStatusCode.Unauthorized);

        }
        [Category("DevEnvFT")]
        [Category("QAEnvFT")]
        [Test(Description = "WhenValidEventReceivedWithTokenHavingNoRole_ThenUoSReturns403ForbiddenResponse"), Order(3)]
        public async Task WhenValidEventReceivedWithInvalidToken_ThenUoSReturns403UnAuthorizedResponse()
        {
            string filePath = Path.Combine(_projectDir, Config.TestConfig.PayloadFolder, Config.TestConfig.UoSPayloadFileName);
            string filePathWebhook = Path.Combine(_projectDir, Config.TestConfig.PayloadFolder, Config.TestConfig.WebhookPayloadFileName);
            string generatedXMLFolder = Path.Combine(_projectDir, Config.TestConfig.GeneratedXMLFolder);
            string webhookToken = await _authToken.GetAzureADToken(false);
            var response = await _unitOfSale.PostUoSResponseAsync(filePathWebhook, generatedXMLFolder, webhookToken, filePath, await _authToken.GetAzureADToken(true));
            response.StatusCode.Should().Be(System.Net.HttpStatusCode.Forbidden);

        }

        [Category("DevEnvFT")]
        [Category("QAEnvFT")]
        [Test, Order(0)]
        //UoS scenario based testing
        [TestCase("ID1_WebhookPayload.JSON", "UoS1_Pricing.JSON", TestName = "WhenReceivePriceInfoForAllUoSSentToSAP_UoSReturn200OkResponse")]
        public async Task WhenReceivePriceInfoForAllUoSSentToSAP_UoSReturn200OkResponse(string webhookPayloadJsonFileName, string UoSPayloadFileName)
        {
            string filePathWebhook = Path.Combine(_projectDir, Config.TestConfig.PayloadFolder, Config.TestConfig.WebhookPayloadFileName);
            string generatedXMLFolder = Path.Combine(_projectDir, Config.TestConfig.GeneratedXMLFolder);
            Console.WriteLine("Scenario:" + webhookPayloadJsonFileName + "\n");
            string filePathUOS = Path.Combine(_projectDir, Config.TestConfig.PayloadFolder, UoSPayloadFileName);
            string generatedJSONFolder = Path.Combine(_projectDir, Config.TestConfig.GeneratedJSONFolder);
            string webhookToken = await _authToken.GetAzureADToken(false);
            string uOSToken = await _authToken.GetAzureADToken(false, "UnitOfSale");
            var response = await _unitOfSale.PostWebHookAndUoSResponseAsyncWithJSON(filePathWebhook, filePathUOS, generatedXMLFolder, generatedJSONFolder, webhookToken, uOSToken);
            response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);

        }

        [Category("DevEnvFT")]
        [Category("QAEnvFT")]
        [Test, Order(0)]
        //UoS scenario based testing
        [TestCase("ID1_WebhookPayload.JSON", "UoS3_ProductMissingUOS.JSON", TestName = "WhenPriceInfoIsBlankFromSAP_ThenPriceSectionIsEmptyInFinalUoS_UoSReturn200OkResponse")]
        public async Task WhenPriceInfoIsBlankFromSAP_ThenPriceSectionIsEmptyInFinalUoS_UoSReturn200OkResponse(string webhookPayloadJsonFileName, string UoSPayloadProductBlankFileName)
        {
            string filePathWebhook = Path.Combine(_projectDir, Config.TestConfig.PayloadFolder, Config.TestConfig.WebhookPayloadFileName);
            string generatedXMLFolder = Path.Combine(_projectDir, Config.TestConfig.GeneratedXMLFolder);
            Console.WriteLine("Scenario:" + webhookPayloadJsonFileName + "\n");
            string filePathUOS = Path.Combine(_projectDir, Config.TestConfig.PayloadFolder, UoSPayloadProductBlankFileName);
            string generatedJSONFolder = Path.Combine(_projectDir, Config.TestConfig.GeneratedJSONFolder);
            string webhookToken = await _authToken.GetAzureADToken(false);
            string uOSToken = await _authToken.GetAzureADToken(false, "UnitOfSale");
            var response = await _unitOfSale.PostWebHookAndUoSResponseAsyncWithNullProduct(filePathWebhook, filePathUOS, generatedXMLFolder, generatedJSONFolder, webhookToken, uOSToken);
            response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);

        }

        [Category("DevEnvFT")]
        [Category("QAEnvFT")]
        [Test, Order(0)]
        //UoS scenario based testing
        [TestCase("ID1_WebhookPayload.JSON", "UoS2_ProductISNullUOS.JSON", TestName = "WhenPriceInfoIsNullFromSAP_ThenPriceSectionIsEmptyInFinalUoS_UoSReturn200OkResponse")]
        public async Task WhenPriceInfoIsNullFromSAP_ThenPriceSectionIsEmptyInFinalUoS_UoSReturn200OkResponse(string webhookPayloadJsonFileName, string UoSPayloadProductNullFileName)
        {
            string filePathWebhook = Path.Combine(_projectDir, Config.TestConfig.PayloadFolder, Config.TestConfig.WebhookPayloadFileName);
            string generatedXMLFolder = Path.Combine(_projectDir, Config.TestConfig.GeneratedXMLFolder);
            Console.WriteLine("Scenario:" + webhookPayloadJsonFileName + "\n");
            string filePathUOS = Path.Combine(_projectDir, Config.TestConfig.PayloadFolder, UoSPayloadProductNullFileName);
            string generatedJSONFolder = Path.Combine(_projectDir, Config.TestConfig.GeneratedJSONFolder);
            string webhookToken = await _authToken.GetAzureADToken(false);
            string uOSToken = await _authToken.GetAzureADToken(false, "UnitOfSale");
            var response = await _unitOfSale.PostWebHookAndUoSResponseAsyncWithNullProduct(filePathWebhook, filePathUOS, generatedXMLFolder, generatedJSONFolder, webhookToken, uOSToken);
            response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);

        }

    }
}

