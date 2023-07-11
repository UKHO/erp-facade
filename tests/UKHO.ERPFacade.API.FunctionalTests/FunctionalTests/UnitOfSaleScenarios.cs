using FluentAssertions;
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
        //for local Execution
        //readonly string _projectDir = Path.GetFullPath(Path.Combine(Environment.CurrentDirectory, "..\\..\\.."));

        [OneTimeSetUp]
        public void Setup()
        {
            _unitOfSale = new UnitOfSaleEndpoint(Config.TestConfig.ErpFacadeConfiguration.BaseUrl);
            _webhook = new WebhookEndpoint();
        }
        
        [Test(Description = "WhenValidEventReceivedWithValidToken_ThenUoSReturns200OkResponse"), Order(0)]
        public async Task WhenValidEventReceivedWithValidToken_ThenUoSReturns200OkResponse()
        {
            string filePathWebhook = Path.Combine(_projectDir, Config.TestConfig.PayloadFolder, Config.TestConfig.WebhookPayloadFileName);
            string generatedXMLFolder = Path.Combine(_projectDir, Config.TestConfig.GeneratedXMLFolder);
            string webhookToken = await _authToken.GetAzureADToken(false);
            string filePath = Path.Combine(_projectDir, Config.TestConfig.PayloadFolder, Config.TestConfig.UoSPayloadFileName);
            var response = await _unitOfSale.PostUoSResponseAsync(filePathWebhook, generatedXMLFolder, webhookToken, filePath, Config.TestConfig.SharedKeyConfiguration.Key);
            response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
        }
        
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
        
        [Test, Order(0)]
        //UoS scenario based testing
        [TestCase("ID1_WebhookPayload.JSON", "UoS1_Pricing.JSON", TestName = "WhenReceivePriceInfoForAllUoSSentToSAP_UoSReturn200OkResponse")]
        public async Task WhenReceivePriceInfoForAllUoSSentToSAP_UoSReturn200OkResponse(string webhookPayloadJsonFileName, string UoSPayloadFileName)
        {
            string filePathWebhook = Path.Combine(_projectDir, Config.TestConfig.PayloadFolder, Config.TestConfig.WebhookPayloadFileName);
            string generatedXMLFolder = Path.Combine(_projectDir, Config.TestConfig.GeneratedXMLFolder);
            Console.WriteLine("Scenario: WhenReceivePriceInfoForAllUoSSentToSAP_UoSReturn200OkResponse" + "\n");
            string filePathUOS = Path.Combine(_projectDir, Config.TestConfig.PayloadFolder, UoSPayloadFileName);
            string generatedJSONFolder = Path.Combine(_projectDir, Config.TestConfig.GeneratedJSONFolder);
            string webhookToken = await _authToken.GetAzureADToken(false);
            string uOSKey = Config.TestConfig.SharedKeyConfiguration.Key;
            var response = await _unitOfSale.PostWebHookAndUoSResponseAsyncWithJSON(filePathWebhook, filePathUOS, generatedXMLFolder, generatedJSONFolder, webhookToken, uOSKey);
            response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
        }
        
        [Test, Order(0)]
        //UoS scenario based testing
        [TestCase("ID1_WebhookPayload.JSON", "UoS3_ProductMissingUOS.JSON", TestName = "WhenPriceInfoIsBlankFromSAP_ThenPriceSectionIsEmptyInFinalUoS_UoSReturn200OkResponse")]
        public async Task WhenPriceInfoIsBlankFromSAP_ThenPriceSectionIsEmptyInFinalUoS_UoSReturn200OkResponse(string webhookPayloadJsonFileName, string UoSPayloadProductBlankFileName)
        {
            string filePathWebhook = Path.Combine(_projectDir, Config.TestConfig.PayloadFolder, Config.TestConfig.WebhookPayloadFileName);
            string generatedXMLFolder = Path.Combine(_projectDir, Config.TestConfig.GeneratedXMLFolder);
            Console.WriteLine("Scenario: WhenPriceInfoIsBlankFromSAP_ThenPriceSectionIsEmptyInFinalUoS_UoSReturn200OkResponse" + "\n");
            string filePathUOS = Path.Combine(_projectDir, Config.TestConfig.PayloadFolder, UoSPayloadProductBlankFileName);
            string generatedJSONFolder = Path.Combine(_projectDir, Config.TestConfig.GeneratedJSONFolder);
            string webhookToken = await _authToken.GetAzureADToken(false);
            string uOSKey = Config.TestConfig.SharedKeyConfiguration.Key;
            var response = await _unitOfSale.PostWebHookAndUoSResponseAsyncWithNullProduct(filePathWebhook, filePathUOS, generatedXMLFolder, generatedJSONFolder, webhookToken, uOSKey);
            response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
        }
       
        [Test, Order(0)]
        //UoS scenario based testing
        [TestCase("ID1_WebhookPayload.JSON", "UoS2_ProductISNullUOS.JSON", TestName = "WhenPriceInfoIsNullFromSAP_ThenPriceSectionIsEmptyInFinalUoS_UoSReturn200OkResponse")]
        public async Task WhenPriceInfoIsNullFromSAP_ThenPriceSectionIsEmptyInFinalUoS_UoSReturn200OkResponse(string webhookPayloadJsonFileName, string UoSPayloadProductNullFileName)
        {
            string filePathWebhook = Path.Combine(_projectDir, Config.TestConfig.PayloadFolder, Config.TestConfig.WebhookPayloadFileName);
            string generatedXMLFolder = Path.Combine(_projectDir, Config.TestConfig.GeneratedXMLFolder);
            Console.WriteLine("Scenario: WhenPriceInfoIsNullFromSAP_ThenPriceSectionIsEmptyInFinalUoS_UoSReturn200OkResponse" + "\n");
            string filePathUOS = Path.Combine(_projectDir, Config.TestConfig.PayloadFolder, UoSPayloadProductNullFileName);
            string generatedJSONFolder = Path.Combine(_projectDir, Config.TestConfig.GeneratedJSONFolder);
            string webhookToken = await _authToken.GetAzureADToken(false);
            string uOSKey = Config.TestConfig.SharedKeyConfiguration.Key;
            var response = await _unitOfSale.PostWebHookAndUoSResponseAsyncWithNullProduct(filePathWebhook, filePathUOS, generatedXMLFolder, generatedJSONFolder, webhookToken, uOSKey);
            response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
        }

        [Test, Order(2)]
        //UoS scenario based testing
        [TestCase("ID1_WebhookPayload.JSON", "UoS1_Pricing.JSON", TestName = "WhenNoCorrIdinUoSrequest_UoSReturn400BadrequestResponse")]
        public async Task WhenNoCorrIdinUoSrequest_UoSReturn400BadrequestResponse(string webhookPayloadJsonFileName, string UoSPayloadProductNullFileName)
        {
            string filePathWebhook = Path.Combine(_projectDir, Config.TestConfig.PayloadFolder, Config.TestConfig.WebhookPayloadFileName);
            string generatedXMLFolder = Path.Combine(_projectDir, Config.TestConfig.GeneratedXMLFolder);
            Console.WriteLine("Scenario: WhenNoCorrIdinUoSrequest_UoSReturn400BadrequestResponse" + "\n");
            string filePathUOS = Path.Combine(_projectDir, Config.TestConfig.PayloadFolder, UoSPayloadProductNullFileName);
            string generatedJSONFolder = Path.Combine(_projectDir, Config.TestConfig.GeneratedJSONFolder);
            string webhookToken = await _authToken.GetAzureADToken(false);
            string uOSKey = Config.TestConfig.SharedKeyConfiguration.Key;
            var response = await _unitOfSale.PostWebHookAndUoSNoCorrIdResponse400BadRequest(filePathWebhook, filePathUOS, generatedXMLFolder, generatedJSONFolder, webhookToken, uOSKey);
            response.StatusCode.Should().Be(System.Net.HttpStatusCode.BadRequest);
        }

        [Test, Order(2)]
        //UoS scenario based testing
        [TestCase("ID1_WebhookPayload.JSON", "UoS1_Pricing.JSON", TestName = "WhenInvalidCorrIdinUoSrequest_UoSReturn404NotFoundResponse")]
        public async Task WhenInvalidCorrIdinUoSrequest_UoSReturn404NotFoundResponse(string webhookPayloadJsonFileName, string UoSPayloadProductNullFileName)
        {
            string filePathWebhook = Path.Combine(_projectDir, Config.TestConfig.PayloadFolder, Config.TestConfig.WebhookPayloadFileName);
            string generatedXMLFolder = Path.Combine(_projectDir, Config.TestConfig.GeneratedXMLFolder);
            Console.WriteLine("Scenario: WhenInvalidCorrIdinUoSrequest_UoSReturn404NotFoundResponse" + "\n");
            string filePathUOS = Path.Combine(_projectDir, Config.TestConfig.PayloadFolder, UoSPayloadProductNullFileName);
            string generatedJSONFolder = Path.Combine(_projectDir, Config.TestConfig.GeneratedJSONFolder);
            string webhookToken = await _authToken.GetAzureADToken(false);
            string uOSKey = Config.TestConfig.SharedKeyConfiguration.Key;
            var response = await _unitOfSale.PostWebHookAndUoSInvalidCorrIdResponse404NotFound(filePathWebhook, filePathUOS, generatedXMLFolder, generatedJSONFolder, webhookToken, uOSKey);
            response.StatusCode.Should().Be(System.Net.HttpStatusCode.NotFound);
        }

        [Test, Order(0)]
        //UoS scenario based testing
        [TestCase("ID1_WebhookPayload.JSON", "UoS1_Pricing.JSON", TestName = "WhenValidCorrIdPassed_UoSReturn200OkResponse")]
        [TestCase("ID1_WebhookPayload.JSON", "UoS_4_FutureDateFieldBlank.JSON", TestName = "WhenValidCorrIdWithFutureDateBlank_UoSReturn200OkResponse")]
        [TestCase("ID1_WebhookPayload.JSON", "UoS_5_MultiProductSameDuration.JSON", TestName = "WhenValidCorrIdMultiProdSameDuration_UoSReturn200OkResponse")]
        [TestCase("ID1_WebhookPayload.JSON", "UoS_6_MultiProductMultiDuration.JSON", TestName = "WhenValidCorrIdMultiProdMultiDuration_UoSReturn200OkResponse")]
        [TestCase("ID1_WebhookPayload.JSON", "UoS_7_SameEffectiveSameFutureDate.JSON", TestName = "WhenSameEffectiveAndFutureDate_UoSReturn200OkResponse")]
        [TestCase("ID1_WebhookPayload.JSON", "UoS_8_DiffEffectiveSameFutureDate.JSON", TestName = "WhenDiffEffectiveAndSameFutureDate_UoSReturn200OkResponse")]
        [TestCase("ID1_WebhookPayload.JSON", "UoS_9_SameEffectiveDiffFutureDate.JSON", TestName = "WhenSameEffectiveAndDiffFutureDate_UoSReturn200OkResponse")]
        [TestCase("ID1_WebhookPayload.JSON", "UoS_10_DiffntEffectiveAndDiffFutureDate.JSON", TestName = "WhenDiffEffectiveAndDiffFutureDate_UoSReturn200OkResponse")]

        public async Task WhenValidCorrIdPassed_UoSReturn200OkResponse(string webhookPayloadJsonFileName, string UoSPayloadFileName)
        {
            string filePathWebhook = Path.Combine(_projectDir, Config.TestConfig.PayloadFolder, Config.TestConfig.WebhookPayloadFileName);
            string generatedXMLFolder = Path.Combine(_projectDir, Config.TestConfig.GeneratedXMLFolder);
            Console.WriteLine("Scenario:" + UoSPayloadFileName + "\n");
            string filePathUOS = Path.Combine(_projectDir, Config.TestConfig.PayloadFolder, UoSPayloadFileName);
            string generatedJSONFolder = Path.Combine(_projectDir, Config.TestConfig.GeneratedJSONFolder);
            string webhookToken = await _authToken.GetAzureADToken(false);
            string uOSKey = Config.TestConfig.SharedKeyConfiguration.Key;
            var response = await _unitOfSale.PostWebHookAndUoSResponse200OK(filePathWebhook, filePathUOS, generatedXMLFolder, generatedJSONFolder, webhookToken, uOSKey);
            response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
        }

        [Test, Order(0)]
        [TestCase("UoS_12_SAPPricing_PAYSF12Months.JSON", TestName = "WhenPAYSFwith12MonthDuration_UoSAlteredPAYSFPricesFinalJson")]
        public async Task WhenPAYSFwith12MonthDuration_UoSAlteredPAYSFPricesFinalJson(string UoSPayloadFileName)
        {
            string filePathWebhook = Path.Combine(_projectDir, Config.TestConfig.PayloadFolder, Config.TestConfig.WebhookPayloadFileName);
            string generatedXMLFolder = Path.Combine(_projectDir, Config.TestConfig.GeneratedXMLFolder);
            Console.WriteLine("Scenario:" + UoSPayloadFileName + "\n");
            string filePathUOS = Path.Combine(_projectDir, Config.TestConfig.PayloadFolder, UoSPayloadFileName);
            string generatedJSONFolder = Path.Combine(_projectDir, Config.TestConfig.GeneratedJSONFolder);
            string webhookToken = await _authToken.GetAzureADToken(false);
            string uOSKey = Config.TestConfig.SharedKeyConfiguration.Key;
            var response = await _unitOfSale.PostWebHookAndUoSResponse200OKPAYSF12Months(filePathWebhook, filePathUOS, generatedXMLFolder, generatedJSONFolder, webhookToken, uOSKey);
            Assert.That(response, Is.True, "PAYSF Price Info for 12 months {0} are displayed");
        }

    }
}

