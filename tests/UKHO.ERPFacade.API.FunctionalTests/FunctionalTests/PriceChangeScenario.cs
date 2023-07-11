using FluentAssertions;
using NUnit.Framework;
using UKHO.ERPFacade.API.FunctionalTests.Helpers;

namespace UKHO.ERPFacade.API.FunctionalTests.FunctionalTests
{
    [TestFixture]
    public class PriceChangeScenarios
    {

        private PriceChangeEndpoint _priceChange { get; set; }
        private readonly ADAuthTokenProvider _authToken = new();
        public static bool noRole = false;
        //for pipeline
        private readonly string _projectDir = Path.GetFullPath(Path.Combine(Environment.CurrentDirectory));
        //for local
       // private readonly string _projectDir = Path.GetFullPath(Path.Combine(Environment.CurrentDirectory, "..\\..\\.."));

        [OneTimeSetUp]
        public void Setup()
        {
            _priceChange = new PriceChangeEndpoint(Config.TestConfig.ErpFacadeConfiguration.BaseUrl);
        }

        [Test(Description = "WhenValidPriceChangeEventReceivedWithValidToken_ThenPriceChangeReturns200OkResponse"), Order(0)]
        public async Task WhenValidPriceChangeEventReceivedWithValidToken_ThenPriceChangeReturns200OkResponse()
        {
            string filePath = Path.Combine(_projectDir, Config.TestConfig.PayloadFolder, Config.TestConfig.PriceChangePayloadFileName);
            var response = await _priceChange.PostPriceChangeResponseAsync(filePath,Config.TestConfig.SharedKeyConfiguration.Key);
            response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);

        }

        [Test(Description = "WhenValidPriceChangeEventReceivedWithInvalidToken_ThenPriceChangeReturns401UnAuthorizedResponse"), Order(1)]
        public async Task WhenValidPriceChangeEventReceivedWithInvalidToken_ThenPriceChangeReturns401UnAuthorizedResponse()
        {
            string filePath = Path.Combine(_projectDir, Config.TestConfig.PayloadFolder, Config.TestConfig.PriceChangePayloadFileName);

            var response = await _priceChange.PostPriceChangeResponseAsync(filePath, "invalidToken_abcd");
            response.StatusCode.Should().Be(System.Net.HttpStatusCode.Unauthorized);

        }
        
        [Test, Order(0)]
        [TestCase("PC1_MultipleProduct.JSON", TestName = "WhenValidEventReceivedWithValidTokenandMultipleProduct_ThenPriceChangeReturn200OkResponse")]
        [TestCase("PC2_FutureDateBlank.JSON", TestName = "WhenValidEventReceivedWithValidTokenandFutureDateBlank_ThenPriceChangeReturn200OkResponse")]
        [TestCase("PC3_MultipleProductDifferentDuration.JSON", TestName = "WhenValidEventReceivedWithValidTokenandMultipleProductDifferentDuration_ThenPriceChangeReturn200OkResponse")]
        [TestCase("PC4_MultipleDurationSameEffectiveAndFutureDate.JSON", TestName = "WhenValidEventReceivedWithValidTokenandMultipleDurationSameEffectiveAndFutureDate_ThenPriceChangeReturn200OkResponse")]
        [TestCase("PC5_MultipleDurationDifferentEffectiveAndFutureDate.JSON", TestName = "WhenValidEventReceivedWithValidTokenandMultipleDurationDifferentEffectiveAndFutureDate_ThenPriceChangeReturn200OkResponse")]
        [TestCase("PC6_MultipleDurationSameEffectiveDifferentFutureDate.JSON", TestName = "WhenValidEventReceivedWithValidTokenandMultipleDurationSameEffectiveDifferentFutureDate_ThenPriceChangeReturn200OkResponse")]
        public async Task WhenValidEventReceivedWithValidTokenand_ThenPriceChangeReturn200OkResponse(string payloadJsonFileName)
        {
            string filePath = Path.Combine(_projectDir, Config.TestConfig.PayloadFolder, payloadJsonFileName);
            string generatedProductJsonFolder = Path.Combine(_projectDir, Config.TestConfig.GeneratedProductJsonFolder);
            var response = await _priceChange.PostPriceChangeResponseAsyncForJSON(filePath, generatedProductJsonFolder, Config.TestConfig.SharedKeyConfiguration.Key);
            response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);

        }
        
        [Test, Order(3)]
        [TestCase("PC7_ExistingCorrId.JSON", TestName = "WhenValidEventReceivedWithValidTokenandExistingCorelationID_ThenPriceChangeReturn500InternalServerErrorResponse")]
        public async Task WhenValidEventReceivedWithValidTokenandExistingCorelationID_ThenPriceChangeReturn500InternalServerErrorResponse(string payloadJsonFileName)
        {
            string filePath = Path.Combine(_projectDir, Config.TestConfig.PayloadFolder, payloadJsonFileName);
            string generatedProductJsonFolder = Path.Combine(_projectDir, Config.TestConfig.GeneratedProductJsonFolder);
            var response = await _priceChange.PostPriceChangeResponseAsyncForJSON(filePath, generatedProductJsonFolder, Config.TestConfig.SharedKeyConfiguration.Key);
            response.StatusCode.Should().Be(System.Net.HttpStatusCode.InternalServerError);

        }

        [Test, Order(0)]
        [TestCase("PC1_MultipleProduct.JSON", TestName = "WhenPAYSFwith12MonthDuration_PriceChangeAlteredPAYSFPrices")]
        public async Task WhenPAYSFwith12MonthDuration_PriceChangeAlteredPAYSFPrices(string payloadJsonFileName)
        {
            string filePath = Path.Combine(_projectDir, Config.TestConfig.PayloadFolder, payloadJsonFileName);
            string generatedProductJsonFolder = Path.Combine(_projectDir, Config.TestConfig.GeneratedProductJsonFolder);
            var response = await _priceChange.PostPriceChangeResponse200OKPAYSF12Months(filePath, generatedProductJsonFolder, Config.TestConfig.SharedKeyConfiguration.Key);
            //response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
             Assert.That(response, Is.True, "PAYSF Price Info for 12 months {0} are displayed");
        }



    }
}
