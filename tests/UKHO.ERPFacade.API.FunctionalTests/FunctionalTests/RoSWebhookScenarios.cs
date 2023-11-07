using FluentAssertions;
using NUnit.Framework;
using RestSharp;
using UKHO.ERPFacade.API.FunctionalTests.Configuration;
using UKHO.ERPFacade.API.FunctionalTests.Helpers;
using UKHO.ERPFacade.API.FunctionalTests.Model;
using UKHO.ERPFacade.API.FunctionalTests.Service;

namespace UKHO.ERPFacade.API.FunctionalTests.FunctionalTests
{
    [TestFixture]
    public class RoSWebhookScenarios
    {
        private RoSWebhookEndpoint _RosWebhookEndpoint { get; set; }
        private readonly ADAuthTokenProvider _authToken = new();

        private readonly string _projectDir = Path.GetFullPath(Path.Combine(Environment.CurrentDirectory));
        //for local
        //private readonly string _projectDir = Path.GetFullPath(Path.Combine(Environment.CurrentDirectory, "..\\..\\.."));

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
        [TestCase("RoS01_ValidRoSMainHolding.json", TestName = "WhenValidRoSEventInRecordOfSalePublishedEventPostReceivedWithValidToken_ThenWebhookReturns200OkResponse")]
        public async Task WhenValidRoSEventInRecordOfSalePublishedEventPostReceivedWithValidToken_ThenWebhookReturns200OkResponse(string payloadFileName)
        {
            string filePath = Path.Combine(_projectDir, Config.TestConfig.PayloadFolder, "RoSPayloadTestData", payloadFileName);
            var response = await _RosWebhookEndpoint.PostWebhookResponseAsync(filePath, await _authToken.GetAzureADToken(false));
            response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
        }

        [Test, Order(1)]
        [TestCase("RoS01_ValidRoSMainHolding.json", TestName = "WhenValidRoSEventInRecordOfSalePublishedEventPostReceivedWithInvalidToken_ThenWebhookReturns401UnauthorizedResponse")]
        public async Task WhenValidRoSEventInRecordOfSalePublishedEventPostReceivedWithInvalidToken_ThenWebhookReturns401UnauthorizedResponse(string payloadFileName)
        {
            string filePath = Path.Combine(_projectDir, Config.TestConfig.PayloadFolder, "RoSPayloadTestData", payloadFileName);
            var response = await _RosWebhookEndpoint.PostWebhookResponseAsync(filePath, "invalidToken_abcd");
            response.StatusCode.Should().Be(System.Net.HttpStatusCode.Unauthorized);
        }

        [Test, Order(3)]
        [TestCase("RoS01_ValidRoSMainHolding.json", TestName = "WhenValidRoSEventInRecordOfSalePublishedEventPostReceivedWithInvalidToken_ThenWebhookReturns403ForbiddenResponse")]
        public async Task WhenValidRoSEventInRecordOfSalePublishedEventPostReceivedWithInvalidToken_ThenWebhookReturns403ForbiddenResponse(string payloadFileName)
        {
            string filePath = Path.Combine(_projectDir, Config.TestConfig.PayloadFolder, "RoSPayloadTestData", payloadFileName);
            var response = await _RosWebhookEndpoint.PostWebhookResponseAsync(filePath, await _authToken.GetAzureADToken(true));
            response.StatusCode.Should().Be(System.Net.HttpStatusCode.Forbidden);
        }

        [Test, Order(2)]
        [TestCase("LTC03_InValidLTCwithNoCorrelationID.json", TestName = "WhenValidRoSMigrateNewLicencePublishedEventReceivedWithInvalidPayload_ThenWebhookReturns400BadRequestResponse")]
        [TestCase("LTC05_InValidLTCwithNoCorrelationID.json", TestName = "WhenValidRoSMigrateExistingLicencePublishedEventReceivedWithInvalidPayload_ThenWebhookReturns400BadRequestResponse")]
        [TestCase("RoS11_InValidRoSwithNoCorrelationID.json", TestName = "WhenInValidRoSEventInRecordOfSalePublishedEventPostReceived_ThenWebhookReturns400BadRequestResponse")]
        public async Task WhenInValidRoSEventInRecordOfSalePublishedEventPostReceived_ThenWebhookReturns400BadRequestResponse(string payloadFileName)
        {
            string filePath = Path.Combine(_projectDir, Config.TestConfig.PayloadFolder, "RoSPayloadTestData", payloadFileName);
            var response = await _RosWebhookEndpoint.PostWebhookResponseAsync("Bad Request", filePath, await _authToken.GetAzureADToken(false));
            response.StatusCode.Should().Be(System.Net.HttpStatusCode.BadRequest);
        }

        [Test, Order(2)]
        [TestCase("RoS12_UnSupportedPayloadType.xml", TestName = "WhenInvalidRoSEventInRecordOfSalePublishedEventPostReceivedWithInvalidPayloadType_ThenWebhookReturns415UnsupportedMediaResponse")]
        public async Task WhenInvalidRoSEventInRecordOfSalePublishedEventPostReceivedWithInvalidPayloadType_ThenWebhookReturns415UnsupportedMediaResponse(string payloadFileName)
        {
            string filePath = Path.Combine(_projectDir, Config.TestConfig.PayloadFolder, "RoSPayloadTestData", payloadFileName);
            var response = await _RosWebhookEndpoint.PostWebhookResponseAsync("Unsupported Media Type", filePath, await _authToken.GetAzureADToken(false));
            response.StatusCode.Should().Be(System.Net.HttpStatusCode.UnsupportedMediaType);
        }

        [TestCase("RoS01_ValidRoSMainHolding.json", TestName = "WhenValidRoSEventInRecordOfSalePublishedEventReceivedPostReceivedMaintainHoldingWithValidPayload_ThenWebhookReturns200OkResponse")]
        [TestCase("RoS02_ValidRoSMainHolding900UoS.json", TestName = "WhenValidRoSEventInRecordOfSalePublishedEventReceivedMaintainHoldingPostReceivedWithValidPayload900UoS_ThenWebhookReturns200OkResponse")]
        [TestCase("RoS03_ValidRoSMainHolding2000UoS.json", TestName = "WhenValidRoSEventInRecordOfSalePublishedEventReceivedMaintainHoldingPostReceivedWithValidPayload2000UoS_ThenWebhookReturns200OkResponse")]
        [TestCase("RoS04_ValidRoSMainHolding4000UoS.json", TestName = "WhenValidRoSEventInRecordOfSalePublishedEventReceivedMaintainHoldingePostReceivedWithValidPayload4000UoS_ThenWebhookReturns200OkResponse")]
        [TestCase("RoS05_ValidRoSNewLicence.json", TestName = "WhenValidRoSEventInRecordOfSalePublishedEventReceivedNewLicencePostReceivedWithValidPayload_ThenWebhookReturns200OkResponse")]
        [TestCase("RoS06_ValidRoSNewLicence900UoS.json", TestName = "WhenValidRoSEventInRecordOfSalePublishedEventReceivedNewLicencePostReceivedWithValidPayload900UoS_ThenWebhookReturns200OkResponse")]
        [TestCase("RoS07_ValidRoSNewLicence2000UoS.json", TestName = "WhenValidRoSEventInRecordOfSalePublishedEventReceivedNewLicencePostReceivedWithValidPayload2000UoS_ThenWebhookReturns200OkResponse")]
        [TestCase("RoS08_ValidRoSNewLicence4000UoS.json", TestName = "WhenValidRoSEventInRecordOfSalePublishedEventReceivedNewLicencePostReceivedWithValidPayload4000UoS_ThenWebhookReturns200OkResponse")]
        public async Task WhenValidRoSEventInRecordOfSalePublishedEventReceivedPostReceivedWithValidPayload_ThenWebhookReturns200OkResponse(string payloadJsonFileName)
        {
            Console.WriteLine("Scenario:" + payloadJsonFileName + "\n");
            string filePath = Path.Combine(_projectDir, Config.TestConfig.PayloadFolder, "RoSPayloadTestData", payloadJsonFileName);
            RestResponse response = await _RosWebhookEndpoint.PostWebhookResponseAsync(filePath, await _authToken.GetAzureADToken(false));
            response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
        }
        [TestCase("LTC02_InvalidRoSMigrateNewLicence.json", TestName = "WhenValidRoSMigrateNewLicencePublishedEventReceivedWithInvalidPayload_ThenWebhookReturns500InternalServerError")]
        [TestCase("LTC06_InvalidRoSMigrateExistingLicence.json", TestName = "WhenValidRoSMigrateExistingLicencePublishedEventReceivedWithInvalidPayload_ThenWebhookReturns500InternalServerError")]
        [TestCase("RoS09_InValidPayloadRoSNewLicence.json", TestName = "WhenInvalidRoSEventInRecordOfSalePublishedEventPostReceivedWithInvalidPayloadForNewLicense_ThenWebhookReturns500InternalServerError")]
        [TestCase("RoS10_InValidPayloadRoSMainHolding.json", TestName = "WhenInvalidRoSEventInRecordOfSalePublishedEventPostReceivedWithInvalidPayloadForManintainHolding_ThenWebhookReturns500InternalServerError")]
        public async Task WhenInvalidRoSEventInRecordOfSalePublishedEventPostReceivedWithInvalidPayload_ThenWebhookReturns500InternalServerError(string payloadJsonFileName)
        {
            Console.WriteLine("Scenario:" + payloadJsonFileName + "\n");
            string filePath = Path.Combine(_projectDir, Config.TestConfig.PayloadFolder, "RoSPayloadTestData", payloadJsonFileName);
            RestResponse response = await _RosWebhookEndpoint.PostWebhookResponseAsync(filePath, await _authToken.GetAzureADToken(false));
            response.StatusCode.Should().Be(System.Net.HttpStatusCode.InternalServerError);
        }
       
        [TestCase("RoS13_ValidFirstPayloadForMerging.json", "RoS13_ValidLastPayloadForMerging.json", TestName = "WhenValidRoSEventsReceivedWithValidPayloadsForMerging_ThenWebhookReturns200OkResponseAndWebJobCreatesXMLPayload")]
        public async Task WhenValidRoSEventsReceivedWithValidPayloadsForMerging_ThenWebhookReturns200OkResponseAndWebJobCreatesXMLPayload(string firstEventPayloadJsonFileName, string lastEventPayloadJsonFileName)
        {
            Console.WriteLine("Scenario: Merging ROS Events - " + firstEventPayloadJsonFileName + " & " + lastEventPayloadJsonFileName + "\n");

            string generatedCorrelationId = SAPXmlHelper.GenerateRandomCorrelationId();
            string generatedXmlFolder = Path.Combine(_projectDir, Config.TestConfig.GeneratedXMLFolder, "RoSPayloadTestData");
            List<JsonInputRoSWebhookEvent> listOfEventJsons = await JsonHelper.GetEventJsonListUsingFileNameAsync(new List<string> { firstEventPayloadJsonFileName, lastEventPayloadJsonFileName });

            //Send first event
            string firstEventPayloadJsonFilePath = Path.Combine(_projectDir, Config.TestConfig.PayloadFolder, "RoSPayloadTestData", firstEventPayloadJsonFileName);
            RestResponse firstEventResponse = await _RosWebhookEndpoint.PostWebhookResponseAsyncForXML(generatedCorrelationId, firstEventPayloadJsonFilePath, true, false, generatedXmlFolder, null, await _authToken.GetAzureADToken(false));

            //Assert if ROS webhook returns OK
            firstEventResponse.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);

            //Send last event  - Additionally we need to send list of all json files to compare total unitofsales items with final XML payload
            string lastEventPayloadJsonFilePath = Path.Combine(_projectDir, Config.TestConfig.PayloadFolder, "RoSPayloadTestData", lastEventPayloadJsonFileName);
            RestResponse lastEventResponse = await _RosWebhookEndpoint.PostWebhookResponseAsyncForXML(generatedCorrelationId, lastEventPayloadJsonFilePath, false, true, generatedXmlFolder, listOfEventJsons, await _authToken.GetAzureADToken(false));

            //Assert if ROS webhook returns OK
            lastEventResponse.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
        }

        [TestCase("LTC01_RoSMigrateNewLicence.json", TestName = "WhenValidRoSMigrateNewLicencePublishedEventReceivedWithValidPayload_ThenWebhookReturns200OkResponse")]
        [TestCase("LTC04_RoSMigrateExistingLicence.json", TestName = "WhenValidRoSMigrateExistingLicencePublishedEventReceivedWithValidPayload_ThenWebhookReturns200OkResponse")]
        public async Task WhenValidRoSMigrateLicencePublishedEventReceivedWithValidPayload_ThenWebhookReturns200OkResponse( string firstEventPayloadJsonFileName)
        {
            Console.WriteLine("Scenario: Merging ROS Events - " + firstEventPayloadJsonFileName + " & " + "\n");

            string generatedCorrelationId = SAPXmlHelper.GenerateRandomCorrelationId();
            string generatedXmlFolder = Path.Combine(_projectDir, Config.TestConfig.GeneratedXMLFolder, "RoSPayloadTestData");
            List<JsonInputRoSWebhookEvent> listOfEventJsons2 = await JsonHelper.GetEventJsonListUsingFileNameAsync(new List<string> { firstEventPayloadJsonFileName });
            string firstEventPayloadJsonFilePath = Path.Combine(_projectDir, Config.TestConfig.PayloadFolder, "RoSPayloadTestData", firstEventPayloadJsonFileName);
            RestResponse firstEventResponse = await _RosWebhookEndpoint.PostWebhookResponseAsyncForXML(generatedCorrelationId, firstEventPayloadJsonFilePath, true, true, generatedXmlFolder, listOfEventJsons2, await _authToken.GetAzureADToken(false));
            firstEventResponse.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);

        }
    }
}
