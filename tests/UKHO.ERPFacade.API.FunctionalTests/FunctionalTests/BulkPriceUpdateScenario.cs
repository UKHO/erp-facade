using FluentAssertions;
using NUnit.Framework;
using UKHO.ERPFacade.API.FunctionalTests.Helpers;

namespace UKHO.ERPFacade.API.FunctionalTests.FunctionalTests
{
    [TestFixture]
    public class BulkPriceUpdateScenarios
    {

        private BulkPriceUpdateEndpoint _bulkPriceUpdate { get; set; }
        public static Config config;
        private readonly ADAuthTokenProvider _authToken = new();
        public static bool noRole = false;
        //for pipeline
        private readonly string _projectDir = Path.GetFullPath(Path.Combine(Environment.CurrentDirectory));
        //for local
        //private readonly string _projectDir = Path.GetFullPath(Path.Combine(Environment.CurrentDirectory, "..\\..\\.."));

        [OneTimeSetUp]
        public void Setup()
        {
            config = new();
            _bulkPriceUpdate = new BulkPriceUpdateEndpoint(config.TestConfig.ErpFacadeConfiguration.BaseUrl);
        }
       
        [Test(Description = "WhenValidEventReceivedWithValidToken_ThenBPUpdateReturns200OkResponse"), Order(0)]
        public async Task WhenValidEventReceivedWithValidToken_ThenBPUpdateReturns200OkResponse()
        {
            string filePath = Path.Combine(_projectDir, config.TestConfig.PayloadFolder, config.TestConfig.BPUpdatePayloadFileName);
            var response = await _bulkPriceUpdate.PostBPUpdateResponseAsync(filePath, await _authToken.GetAzureADToken(false, "BulkPriceUpdate"));
            response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);

        }
        [Test(Description = "WhenValidEventReceivedWithInvalidToken_ThenBPUpdateReturns401UnAuthorizedResponse"), Order(1)]
        public async Task WhenValidEventReceivedWithInvalidToken_ThenBPUpdateReturns401UnAuthorizedResponse()
        {
            string filePath = Path.Combine(_projectDir, config.TestConfig.PayloadFolder, config.TestConfig.BPUpdatePayloadFileName);

            var response = await _bulkPriceUpdate.PostBPUpdateResponseAsync(filePath, "invalidToken_abcd");
            response.StatusCode.Should().Be(System.Net.HttpStatusCode.Unauthorized);

        }
        [Test(Description = "WhenValidEventReceivedWithTokenHavingNoRole_ThenBPUpdateReturns403ForbiddenResponse"), Order(2)]
        public async Task WhenValidEventReceivedWithInvalidToken_ThenBPUpdateReturns403UnAuthorizedResponse()
        {
            string filePath = Path.Combine(_projectDir, config.TestConfig.PayloadFolder, config.TestConfig.BPUpdatePayloadFileName);

            var response = await _bulkPriceUpdate.PostBPUpdateResponseAsync(filePath, await _authToken.GetAzureADToken(true));
            response.StatusCode.Should().Be(System.Net.HttpStatusCode.Forbidden);

        }

    }
}
