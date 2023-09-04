using FluentAssertions;
using NUnit.Framework;
using RestSharp;
using UKHO.ERPFacade.API.FunctionalTests.Configuration;
using UKHO.ERPFacade.API.FunctionalTests.Helpers;
using UKHO.ERPFacade.API.FunctionalTests.Service;

namespace UKHO.ERPFacade.API.FunctionalTests.FunctionalTests
{
    [TestFixture]
    public class RoSWebhookScenarios
    {
        private RoSWebhookEndpoint _RosWebhookEndpoint { get; set; }
        private readonly ADAuthTokenProvider _authToken = new();
        //private readonly string _projectDir = Path.GetFullPath(Path.Combine(Environment.CurrentDirectory));
        //for local
        private readonly string _projectDir = Path.GetFullPath(Path.Combine(Environment.CurrentDirectory, "..\\..\\.."));

        [SetUp]
        public void Setup()
        {
            _RosWebhookEndpoint = new RoSWebhookEndpoint();
        }

        [Test(Description = "WhenValidRoSEventInRecordOfSalePublishedEventReceivedOptions_ThenWebhookReturns200OkResponse"), Order(0)]
        public async Task WhenValidRoSEventInRecordOfSalePublishedEventReceivedOptions_ThenWebhookReturns200OkResponse()
        {
            var response = await _RosWebhookEndpoint.OptionRosWebhookResponseAsync(await _authToken.GetAzureADToken(false));
            response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
        }

        [Test(Description = "WhenValidRoSEventInRecordOfSalePublishedEventOptionsReceivedWithInvalidToken_ThenWebhookReturns401UnauthorizedResponse"), Order(1)]
        public async Task WhenValidRoSEventInRecordOfSalePublishedEventOptionsReceivedWithInvalidToken_ThenWebhookReturns401UnauthorizedResponse()
        {
            var response = await _RosWebhookEndpoint.OptionRosWebhookResponseAsync("invalidToken_q234");
            response.StatusCode.Should().Be(System.Net.HttpStatusCode.Unauthorized);
        }

        [Test(Description = "WhenValidRoSEventInRecordOfSalePublishedEventOptionsReceivedWithNoRole_ThenWebhookReturns403ForbiddenResponse"), Order(3)]
        public async Task WhenValidRoSEventInRecordOfSalePublishedEventOptionsReceivedWithNoRole_ThenWebhookReturns403ForbiddenResponse()
        {
            var response = await _RosWebhookEndpoint.OptionRosWebhookResponseAsync(await _authToken.GetAzureADToken(true));
            response.StatusCode.Should().Be(System.Net.HttpStatusCode.Forbidden);
        }

        [Test, Order(0)]
        [TestCase("ID01_GenericJson.json", TestName = "WhenValidRoSEventInRecordOfSalePublishedEventPostReceivedWithValidToken_ThenWebhookReturns200OkResponse")]
        public async Task WhenValidRoSEventInRecordOfSalePublishedEventPostReceivedWithValidToken_ThenWebhookReturns200OkResponse(string payloadFileName)
        {
            string filePath = Path.Combine(_projectDir, Config.TestConfig.PayloadFolder, "RoSPayloadTestData", payloadFileName);
            var response = await _RosWebhookEndpoint.PostWebhookResponseAsync(filePath, await _authToken.GetAzureADToken(false));
            response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
        }

        [Test, Order(1)]
        [TestCase("ID01_GenericJson.json", TestName = "WhenValidRoSEventInRecordOfSalePublishedEventPostReceivedWithInvalidToken_ThenWebhookReturns401UnauthorizedResponse")]
        public async Task WhenValidRoSEventInRecordOfSalePublishedEventPostReceivedWithInvalidToken_ThenWebhookReturns401UnauthorizedResponse(string payloadFileName)
        {
            string filePath = Path.Combine(_projectDir, Config.TestConfig.PayloadFolder, "RoSPayloadTestData", payloadFileName);
            var response = await _RosWebhookEndpoint.PostWebhookResponseAsync(filePath, "invalidToken_abcd");
            response.StatusCode.Should().Be(System.Net.HttpStatusCode.Unauthorized);
        }

        [Test, Order(3)]
        [TestCase("ID01_GenericJson.json", TestName = "WhenValidRoSEventInRecordOfSalePublishedEventPostReceivedWithInvalidToken_ThenWebhookReturns403ForbiddenResponse")]
        public async Task WhenValidRoSEventInRecordOfSalePublishedEventPostReceivedWithInvalidToken_ThenWebhookReturns403ForbiddenResponse(string payloadFileName)
        {
            string filePath = Path.Combine(_projectDir, Config.TestConfig.PayloadFolder, "RoSPayloadTestData", payloadFileName);
            var response = await _RosWebhookEndpoint.PostWebhookResponseAsync(filePath, await _authToken.GetAzureADToken(true));
            response.StatusCode.Should().Be(System.Net.HttpStatusCode.Forbidden);
        }

        [Test, Order(2)]
        [TestCase("ID03_InvalidRoSJsonFile.json", TestName = "WhenInValidRoSEventInRecordOfSalePublishedEventPostReceived_ThenWebhookReturns400BadRequestResponse")]
        public async Task WhenInValidRoSEventInRecordOfSalePublishedEventPostReceived_ThenWebhookReturns400BadRequestResponse(string payloadFileName)
        {
            string filePath = Path.Combine(_projectDir, Config.TestConfig.PayloadFolder, "RoSPayloadTestData", payloadFileName);
            var response = await _RosWebhookEndpoint.PostWebhookResponseAsync("Bad Request", filePath, await _authToken.GetAzureADToken(false));
            response.StatusCode.Should().Be(System.Net.HttpStatusCode.BadRequest);
        }

        [Test, Order(2)]
        [TestCase("ID02_UnSupportedPayloadType.xml", TestName = "WhenInvalidRoSEventInRecordOfSalePublishedEventPostReceivedWithInvalidPayloadType_ThenWebhookReturns415UnsupportedMediaResponse")]
        public async Task WhenInvalidRoSEventInRecordOfSalePublishedEventPostReceivedWithInvalidPayloadType_ThenWebhookReturns415UnsupportedMediaResponse(string payloadFileName)
        {
            string filePath = Path.Combine(_projectDir, Config.TestConfig.PayloadFolder, "RoSPayloadTestData", payloadFileName);
            var response = await _RosWebhookEndpoint.PostWebhookResponseAsync("Unsupported Media Type", filePath, await _authToken.GetAzureADToken(false));
            response.StatusCode.Should().Be(System.Net.HttpStatusCode.UnsupportedMediaType);
        }
        [TestCase("RoS04_ValidRoSJsonFile.json", TestName = "WhenValidRoSEventInRecordOfSalePublishedEventReceivedPostReceivedWithValidPayload_ThenWebhookReturns200OkResponse")]
        public async Task WhenValidRoSEventInRecordOfSalePublishedEventReceivedPostReceivedWithValidPayload_ThenWebhookReturns200OkResponse(string payloadJsonFileName)
        {
            Console.WriteLine("Scenario:" + payloadJsonFileName + "\n");
            string filePath = Path.Combine(_projectDir, Config.TestConfig.PayloadFolder, "RoSPayloadTestData", payloadJsonFileName);
            string generatedXmlFolder = Path.Combine(_projectDir, Config.TestConfig.GeneratedXMLFolder, "RoSPayloadTestData");
            RestResponse response = await _RosWebhookEndpoint.PostRoSWebhookResponseAsyncForXML(filePath, generatedXmlFolder, await _authToken.GetAzureADToken(false));
            response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
        }
    }
}
