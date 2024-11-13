using FluentAssertions;
using Newtonsoft.Json;
using NUnit.Framework;
using RestSharp;
using UKHO.ERPFacade.API.FunctionalTests.Auth;
using UKHO.ERPFacade.API.FunctionalTests.Model;
using UKHO.ERPFacade.API.FunctionalTests.Service;
using UKHO.ERPFacade.Common.Constants;

namespace UKHO.ERPFacade.API.FunctionalTests.FunctionalTests
{
    [Ignore("It was originally built for ADDS Increment 3")]
    [TestFixture]
    public class RoSWebhookScenarios
    {
        private RoSWebhookEndpoint _rosWebhookEndpoint;
        private AuthTokenProvider _authTokenProvider;

        private readonly string _projectDir = Path.GetFullPath(Path.Combine(Environment.CurrentDirectory));

        [SetUp]
        public void Setup()
        {
            _rosWebhookEndpoint = new RoSWebhookEndpoint();
            _authTokenProvider = new AuthTokenProvider();
        }

        [Test(Description = "WhenValidRoSEventInRecordOfSalePublishedEventReceivedOptions_ThenWebhookReturns200OkResponse"), Order(0)]
        public async Task WhenValidRoSEventInRecordOfSalePublishedEventReceivedOptions_ThenWebhookReturns200OkResponse()
        {
            var response = await _rosWebhookEndpoint.OptionRosWebhookResponseAsync(await _authTokenProvider.GetAzureADToken(false));
            response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
        }

        [Test(Description = "WhenValidRoSEventInRecordOfSalePublishedEventOptionsReceivedWithInvalidToken_ThenWebhookReturns401UnauthorizedResponse"), Order(1)]
        public async Task WhenValidRoSEventInRecordOfSalePublishedEventOptionsReceivedWithInvalidToken_ThenWebhookReturns401UnauthorizedResponse()
        {
            var response = await _rosWebhookEndpoint.OptionRosWebhookResponseAsync("invalidToken_q234");
            response.StatusCode.Should().Be(System.Net.HttpStatusCode.Unauthorized);
        }

        [Test(Description = "WhenValidRoSEventInRecordOfSalePublishedEventOptionsReceivedWithNoRole_ThenWebhookReturns403ForbiddenResponse"), Order(3)]
        public async Task WhenValidRoSEventInRecordOfSalePublishedEventOptionsReceivedWithNoRole_ThenWebhookReturns403ForbiddenResponse()
        {
            var response = await _rosWebhookEndpoint.OptionRosWebhookResponseAsync(await _authTokenProvider.GetAzureADToken(true));
            response.StatusCode.Should().Be(System.Net.HttpStatusCode.Forbidden);
        }

        [Test, Order(0)]
        [TestCase("RoS01_ValidRoSMainHolding.json", TestName = "WhenValidRoSEventInRecordOfSalePublishedEventPostReceivedWithValidToken_ThenWebhookReturns200OkResponse")]
        public async Task WhenValidRoSEventInRecordOfSalePublishedEventPostReceivedWithValidToken_ThenWebhookReturns200OkResponse(string payloadFileName)
        {
            string filePath = Path.Combine(_projectDir, EventPayloadFiles.PayloadFolder, EventPayloadFiles.RosPayloadTestDataFolder, payloadFileName);
            var response = await _rosWebhookEndpoint.PostWebhookResponseAsync(filePath, await _authTokenProvider.GetAzureADToken(false));
            response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
        }

        [Test, Order(1)]
        [TestCase("RoS01_ValidRoSMainHolding.json", TestName = "WhenValidRoSEventInRecordOfSalePublishedEventPostReceivedWithInvalidToken_ThenWebhookReturns401UnauthorizedResponse")]
        public async Task WhenValidRoSEventInRecordOfSalePublishedEventPostReceivedWithInvalidToken_ThenWebhookReturns401UnauthorizedResponse(string payloadFileName)
        {
            string filePath = Path.Combine(_projectDir, EventPayloadFiles.PayloadFolder, EventPayloadFiles.RosPayloadTestDataFolder, payloadFileName);
            var response = await _rosWebhookEndpoint.PostWebhookResponseAsync(filePath, "invalidToken_abcd");
            response.StatusCode.Should().Be(System.Net.HttpStatusCode.Unauthorized);
        }

        [Test, Order(3)]
        [TestCase("RoS01_ValidRoSMainHolding.json", TestName = "WhenValidRoSEventInRecordOfSalePublishedEventPostReceivedWithInvalidToken_ThenWebhookReturns403ForbiddenResponse")]
        public async Task WhenValidRoSEventInRecordOfSalePublishedEventPostReceivedWithInvalidToken_ThenWebhookReturns403ForbiddenResponse(string payloadFileName)
        {
            string filePath = Path.Combine(_projectDir, EventPayloadFiles.PayloadFolder, EventPayloadFiles.RosPayloadTestDataFolder, payloadFileName);
            var response = await _rosWebhookEndpoint.PostWebhookResponseAsync(filePath, await _authTokenProvider.GetAzureADToken(true));
            response.StatusCode.Should().Be(System.Net.HttpStatusCode.Forbidden);
        }

        [Test, Order(2)]
        [TestCase("LTC03_InValidLTCwithNoCorrelationID.json", TestName = "WhenValidRoSMigrateNewLicencePublishedEventReceivedWithInvalidPayload_ThenWebhookReturns400BadRequestResponse")]
        [TestCase("LTC05_InValidLTCwithNoCorrelationID.json", TestName = "WhenValidRoSMigrateExistingLicencePublishedEventReceivedWithInvalidPayload_ThenWebhookReturns400BadRequestResponse")]
        [TestCase("LTC09_InvalidLTCwithNoCorrelationID.json", TestName = "WhenValidRoSConvertTrialToFullLicencePublishedEventReceivedWithInvalidPayload_ThenWebhookReturns400BadRequestResponse")]
        [TestCase("RoS11_InValidRoSwithNoCorrelationID.json", TestName = "WhenInValidRoSEventInRecordOfSalePublishedEventPostReceived_ThenWebhookReturns400BadRequestResponse")]
        public async Task WhenInValidRoSEventInRecordOfSalePublishedEventPostReceived_ThenWebhookReturns400BadRequestResponse(string payloadFileName)
        {
            string filePath = Path.Combine(_projectDir, EventPayloadFiles.PayloadFolder, EventPayloadFiles.RosPayloadTestDataFolder, payloadFileName);
            var response = await _rosWebhookEndpoint.PostWebhookResponseAsync("Bad Request", filePath, await _authTokenProvider.GetAzureADToken(false));
            response.StatusCode.Should().Be(System.Net.HttpStatusCode.BadRequest);
        }

        [Test, Order(2)]
        [TestCase("RoS12_UnSupportedPayloadType.xml", TestName = "WhenInvalidRoSEventInRecordOfSalePublishedEventPostReceivedWithInvalidPayloadType_ThenWebhookReturns415UnsupportedMediaResponse")]
        public async Task WhenInvalidRoSEventInRecordOfSalePublishedEventPostReceivedWithInvalidPayloadType_ThenWebhookReturns415UnsupportedMediaResponse(string payloadFileName)
        {
            string filePath = Path.Combine(_projectDir, EventPayloadFiles.PayloadFolder, EventPayloadFiles.RosPayloadTestDataFolder, payloadFileName);
            var response = await _rosWebhookEndpoint.PostWebhookResponseAsync("Unsupported Media Type", filePath, await _authTokenProvider.GetAzureADToken(false));
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
            string filePath = Path.Combine(_projectDir, EventPayloadFiles.PayloadFolder, EventPayloadFiles.RosPayloadTestDataFolder, payloadJsonFileName);
            RestResponse response = await _rosWebhookEndpoint.PostWebhookResponseAsync(filePath, await _authTokenProvider.GetAzureADToken(false));
            response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
        }

        [TestCase("LTC02_InvalidRoSMigrateNewLicence.json", TestName = "WhenValidRoSMigrateNewLicencePublishedEventReceivedWithInvalidPayload_ThenWebhookReturns500InternalServerError")]
        [TestCase("LTC06_InvalidRoSMigrateExistingLicence.json", TestName = "WhenValidRoSMigrateExistingLicencePublishedEventReceivedWithInvalidPayload_ThenWebhookReturns500InternalServerError")]
        [TestCase("LTC08_InvalidRoSConvertTrialToFullLicence.json", TestName = "WhenValidRoSConvertTrialToFullNewLicencePublishedEventReceivedWithInvalidPayload_ThenWebhookReturns500InternalServerError")]
        [TestCase("RoS09_InValidPayloadRoSNewLicence.json", TestName = "WhenInvalidRoSEventInRecordOfSalePublishedEventPostReceivedWithInvalidPayloadForNewLicense_ThenWebhookReturns500InternalServerError")]
        [TestCase("RoS10_InValidPayloadRoSMainHolding.json", TestName = "WhenInvalidRoSEventInRecordOfSalePublishedEventPostReceivedWithInvalidPayloadForManintainHolding_ThenWebhookReturns500InternalServerError")]
        public async Task WhenInvalidRoSEventInRecordOfSalePublishedEventPostReceivedWithInvalidPayload_ThenWebhookReturns500InternalServerError(string payloadJsonFileName)
        {
            Console.WriteLine("Scenario:" + payloadJsonFileName + "\n");
            string filePath = Path.Combine(_projectDir, EventPayloadFiles.PayloadFolder, EventPayloadFiles.RosPayloadTestDataFolder, payloadJsonFileName);
            RestResponse response = await _rosWebhookEndpoint.PostWebhookResponseAsync(filePath, await _authTokenProvider.GetAzureADToken(false));
            response.StatusCode.Should().Be(System.Net.HttpStatusCode.InternalServerError);
        }

        [TestCase("RoS13_ValidFirstPayloadForMerging.json", "RoS13_ValidLastPayloadForMerging.json", TestName = "WhenValidRoSEventsReceivedWithValidPayloadsForMerging_ThenWebhookReturns200OkResponseAndWebJobCreatesXMLPayload")]
        public async Task WhenValidRoSEventsReceivedWithValidPayloadsForMerging_ThenWebhookReturns200OkResponseAndWebJobCreatesXMLPayload(string firstEventPayloadJsonFileName, string lastEventPayloadJsonFileName)
        {
            Console.WriteLine("Scenario: Merging ROS Events - " + firstEventPayloadJsonFileName + " & " + lastEventPayloadJsonFileName + "\n");

            string generatedXmlFolder = Path.Combine(_projectDir, EventPayloadFiles.GeneratedXmlFolder, EventPayloadFiles.RosPayloadTestDataFolder);
            string correlationId = "ft-" + Guid.NewGuid();
            List<string> fileNames = new List<string> { firstEventPayloadJsonFileName, lastEventPayloadJsonFileName };
            List<JsonInputRoSWebhookEvent> listOfEventJsons = new();

            foreach (var filePath in fileNames.Select(fileName => Path.Combine(_projectDir, EventPayloadFiles.PayloadFolder, EventPayloadFiles.RosPayloadTestDataFolder, fileName)))
            {
                string requestBody;

                using (StreamReader streamReader = new(filePath))
                {
                    requestBody = await streamReader.ReadToEndAsync();
                }
                JsonInputRoSWebhookEvent eventPayloadJson = JsonConvert.DeserializeObject<JsonInputRoSWebhookEvent>(requestBody);
                listOfEventJsons.Add(eventPayloadJson);
            }

            //Send first event
            string firstEventPayloadJsonFilePath = Path.Combine(_projectDir, EventPayloadFiles.PayloadFolder, EventPayloadFiles.RosPayloadTestDataFolder, firstEventPayloadJsonFileName);
            RestResponse firstEventResponse = await _rosWebhookEndpoint.PostWebhookResponseAsyncForXml(firstEventPayloadJsonFilePath, correlationId, true, false, generatedXmlFolder, null, await _authTokenProvider.GetAzureADToken(false));

            //Assert if ROS webhook returns OK
            firstEventResponse.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);

            //Send last event  - Additionally we need to send list of all json files to compare total unitofsales items with final XML payload
            string lastEventPayloadJsonFilePath = Path.Combine(_projectDir, EventPayloadFiles.PayloadFolder, EventPayloadFiles.RosPayloadTestDataFolder, lastEventPayloadJsonFileName);
            RestResponse lastEventResponse = await _rosWebhookEndpoint.PostWebhookResponseAsyncForXml(lastEventPayloadJsonFilePath, correlationId, false, true, generatedXmlFolder, listOfEventJsons, await _authTokenProvider.GetAzureADToken(false));

            //Assert if ROS webhook returns OK
            lastEventResponse.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
        }

        [TestCase("LTC01_RoSMigrateNewLicence.json", TestName = "WhenValidRoSMigrateNewLicencePublishedEventReceivedWithValidPayload_ThenWebhookReturns200OkResponse")]
        [TestCase("LTC04_RoSMigrateExistingLicence.json", TestName = "WhenValidRoSMigrateExistingLicencePublishedEventReceivedWithValidPayload_ThenWebhookReturns200OkResponse")]
        [TestCase("LTC07_RoSConvertTrialToFullLicence.json", TestName = "WhenValidRoSConvertTrialToFullLicencePublishedEventReceivedWithValidPayload_ThenWebhookReturns200OkResponse")]
        public async Task WhenValidRoSMigrateLicencePublishedEventReceivedWithValidPayload_ThenWebhookReturns200OkResponse(string firstEventPayloadJsonFileName)
        {
            Console.WriteLine("Scenario: Merging ROS Events - " + firstEventPayloadJsonFileName + " & " + "\n");

            string generatedXmlFolder = Path.Combine(_projectDir, EventPayloadFiles.GeneratedXmlFolder, EventPayloadFiles.RosPayloadTestDataFolder);
            string correlationId = "ft-" + Guid.NewGuid();
            List<string> fileNames = new List<string> { firstEventPayloadJsonFileName };
            List<JsonInputRoSWebhookEvent> listOfEventJsons = new();

            foreach (var filePath in fileNames.Select(fileName => Path.Combine(_projectDir, EventPayloadFiles.PayloadFolder, EventPayloadFiles.RosPayloadTestDataFolder, fileName)))
            {
                string requestBody;

                using (StreamReader streamReader = new(filePath))
                {
                    requestBody = await streamReader.ReadToEndAsync();
                }
                JsonInputRoSWebhookEvent eventPayloadJson = JsonConvert.DeserializeObject<JsonInputRoSWebhookEvent>(requestBody);
                listOfEventJsons.Add(eventPayloadJson);
            }

            string firstEventPayloadJsonFilePath = Path.Combine(_projectDir, EventPayloadFiles.PayloadFolder, EventPayloadFiles.RosPayloadTestDataFolder, firstEventPayloadJsonFileName);
            RestResponse firstEventResponse = await _rosWebhookEndpoint.PostWebhookResponseAsyncForXml(firstEventPayloadJsonFilePath, correlationId, true, true, generatedXmlFolder, listOfEventJsons, await _authTokenProvider.GetAzureADToken(false));
            firstEventResponse.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
        }
    }
}
