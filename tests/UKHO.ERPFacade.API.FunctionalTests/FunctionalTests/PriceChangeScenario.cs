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
        //private readonly string _projectDir = Path.GetFullPath(Path.Combine(Environment.CurrentDirectory, "..\\..\\.."));

        [OneTimeSetUp]
        public void Setup()
        {
            _priceChange = new PriceChangeEndpoint(Config.TestConfig.ErpFacadeConfiguration.BaseUrl);
        }

        [Category("DevEnvFT")]
        [Category("QAEnvFT")]
        [Test(Description = "WhenValidPriceChangeEventReceivedWithValidToken_ThenPriceChangeReturns200OkResponse"), Order(0)]
        public async Task WhenValidPriceChangeEventReceivedWithValidToken_ThenPriceChangeReturns200OkResponse()
        {
            string filePath = Path.Combine(_projectDir, Config.TestConfig.PayloadFolder, Config.TestConfig.PriceChangePayloadFileName);
            var response = await _priceChange.PostPriceChangeResponseAsync(filePath, Config.TestConfig.SharedKeyConfiguration.Key);
            response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);

        }
        [Category("DevEnvFT")]
        [Category("QAEnvFT")]
        [Test(Description = "WhenValidPriceChangeEventReceivedWithInvalidToken_ThenPriceChangeReturns401UnAuthorizedResponse"), Order(1)]
        public async Task WhenValidPriceChangeEventReceivedWithInvalidToken_ThenPriceChangeReturns401UnAuthorizedResponse()
        {
            string filePath = Path.Combine(_projectDir, Config.TestConfig.PayloadFolder, Config.TestConfig.PriceChangePayloadFileName);

            var response = await _priceChange.PostPriceChangeResponseAsync(filePath, "invalidToken_abcd");
            response.StatusCode.Should().Be(System.Net.HttpStatusCode.Unauthorized);

        }


    }
}
