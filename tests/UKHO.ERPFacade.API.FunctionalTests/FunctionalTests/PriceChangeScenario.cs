using FluentAssertions;
using NUnit.Framework;
using UKHO.ERPFacade.API.FunctionalTests.Configuration;
using UKHO.ERPFacade.API.FunctionalTests.Service;

namespace UKHO.ERPFacade.API.FunctionalTests.FunctionalTests
{
    [TestFixture]
    public class PriceChangeScenarios
    {
        private PriceChangeEndpoint _priceChange { get; set; }

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
            var response = await _priceChange.PostPriceChangeResponseAsync(filePath, Config.TestConfig.SharedKeyConfiguration.Key);
            response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
        }

        [Test(Description = "WhenValidPriceChangeEventReceivedWithInvalidToken_ThenPriceChangeReturns401UnAuthorizedResponse"), Order(9)]
        public async Task WhenValidPriceChangeEventReceivedWithInvalidToken_ThenPriceChangeReturns401UnAuthorizedResponse()
        {
            string filePath = Path.Combine(_projectDir, Config.TestConfig.PayloadFolder, Config.TestConfig.PriceChangePayloadFileName);

            var response = await _priceChange.PostPriceChangeResponseAsync(filePath, "invalidToken_abcd");
            response.StatusCode.Should().Be(System.Net.HttpStatusCode.Unauthorized);
        }

        [Test, Order(1)]
        [TestCase("PC1_MultipleProduct.JSON", TestName = "WhenMultipleProduct_ThenPriceChangeReturn200OkResponse")]
        public async Task WhenMultipleProduct_ThenPriceChangeReturn200OkResponse(string payloadJsonFileName)
        {
            string filePath = Path.Combine(_projectDir, Config.TestConfig.PayloadFolder, payloadJsonFileName);
            string generatedProductJsonFolder = Path.Combine(_projectDir, Config.TestConfig.GeneratedProductJsonFolder);

            var response = await _priceChange.PostPriceChangeResponseAsyncForJSON(filePath, generatedProductJsonFolder, Config.TestConfig.SharedKeyConfiguration.Key);
            response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
        }
        [Test, Order(2)]
        [TestCase("PC2_FutureDateBlank.JSON", TestName = "WhenFutureDateBlank_ThenPriceChangeReturn200OkResponse")]
        public async Task WhenFutureDateBlank_ThenPriceChangeReturn200OkResponse(string payloadJsonFileName)
        {
            string filePath = Path.Combine(_projectDir, Config.TestConfig.PayloadFolder, payloadJsonFileName);
            string generatedProductJsonFolder = Path.Combine(_projectDir, Config.TestConfig.GeneratedProductJsonFolder);
            var response = await _priceChange.PostPriceChangeResponseAsyncForJSON(filePath, generatedProductJsonFolder, Config.TestConfig.SharedKeyConfiguration.Key);
            response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);

        }
        [Test, Order(3)]
        [TestCase("PC3_MultipleProductDifferentDuration.JSON", TestName = "WhenMultipleProductDifferentDuration_ThenPriceChangeReturn200OkResponse")]
        public async Task WhenMultipleProductDifferentDuration_ThenPriceChangeReturn200OkResponse(string payloadJsonFileName)
        {
            string filePath = Path.Combine(_projectDir, Config.TestConfig.PayloadFolder, payloadJsonFileName);
            string generatedProductJsonFolder = Path.Combine(_projectDir, Config.TestConfig.GeneratedProductJsonFolder);
            var response = await _priceChange.PostPriceChangeResponseAsyncForJSON(filePath, generatedProductJsonFolder, Config.TestConfig.SharedKeyConfiguration.Key);
            response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);

        }
        [Test, Order(4)]
        [TestCase("PC4_MultipleDurationSameEffectiveAndFutureDate.JSON", TestName = "WhenMultipleDurationSameEffectiveAndFutureDate_ThenPriceChangeReturn200OkResponse")]
        public async Task WhenMultipleDurationSameEffectiveAndFutureDate_ThenPriceChangeReturn200OkResponse(string payloadJsonFileName)
        {
            string filePath = Path.Combine(_projectDir, Config.TestConfig.PayloadFolder, payloadJsonFileName);
            string generatedProductJsonFolder = Path.Combine(_projectDir, Config.TestConfig.GeneratedProductJsonFolder);

            var response = await _priceChange.PostPriceChangeResponseAsyncForJSON(filePath, generatedProductJsonFolder, Config.TestConfig.SharedKeyConfiguration.Key);
            response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);

        }
        [Test, Order(5)]
        [TestCase("PC5_MultipleDurationDifferentEffectiveAndFutureDate.JSON", TestName = "WhenMultipleDurationDifferentEffectiveAndFutureDate_ThenPriceChangeReturn200OkResponse")]
        public async Task WhenMultipleDurationDifferentEffectiveAndFutureDate_ThenPriceChangeReturn200OkResponse(string payloadJsonFileName)
        {
            string filePath = Path.Combine(_projectDir, Config.TestConfig.PayloadFolder, payloadJsonFileName);
            string generatedProductJsonFolder = Path.Combine(_projectDir, Config.TestConfig.GeneratedProductJsonFolder);
            var response = await _priceChange.PostPriceChangeResponseAsyncForJSON(filePath, generatedProductJsonFolder, Config.TestConfig.SharedKeyConfiguration.Key);
            response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);

        }
        [Test, Order(6)]
        [TestCase("PC6_MultipleDurationSameEffectiveDifferentFutureDate.JSON", TestName = "WhenMultipleDurationSameEffectiveDifferentFutureDate_ThenPriceChangeReturn200OkResponse")]
        public async Task WhenMultipleDurationSameEffectiveDifferentFutureDate_ThenPriceChangeReturn200OkResponse(string payloadJsonFileName)
        {
            string filePath = Path.Combine(_projectDir, Config.TestConfig.PayloadFolder, payloadJsonFileName);
            string generatedProductJsonFolder = Path.Combine(_projectDir, Config.TestConfig.GeneratedProductJsonFolder);
            var response = await _priceChange.PostPriceChangeResponseAsyncForJSON(filePath, generatedProductJsonFolder, Config.TestConfig.SharedKeyConfiguration.Key);
            response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
        }

        [Test, Order(8)]
        [TestCase("PC7_ExistingCorrId.JSON", TestName = "WhenExistingCorelationID_ThenPriceChangeReturn500InternalServerErrorResponse")]
        public async Task WhenExistingCorelationID_ThenPriceChangeReturn500InternalServerErrorResponse(string payloadJsonFileName)
        {
            string filePath = Path.Combine(_projectDir, Config.TestConfig.PayloadFolder, payloadJsonFileName);
            string generatedProductJsonFolder = Path.Combine(_projectDir, Config.TestConfig.GeneratedProductJsonFolder);
            var response = await _priceChange.PostPriceChangeResponseAsyncForJSON(filePath, generatedProductJsonFolder, Config.TestConfig.SharedKeyConfiguration.Key);
            response.StatusCode.Should().Be(System.Net.HttpStatusCode.InternalServerError);
        }

    }
}
