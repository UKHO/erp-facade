using FluentAssertions;
using NUnit.Framework;
using RestSharp;
using UKHO.ERPFacade.API.FunctionalTests.Auth;
using UKHO.ERPFacade.API.FunctionalTests.Service;
using UKHO.ERPFacade.Common.Constants;

namespace UKHO.ERPFacade.API.FunctionalTests.FunctionalTests
{

    [Ignore("It was originally built for ADDS Increment 3")]
    [TestFixture]
    public class LicenceUpdatedWebhookScenario
    {
        private LicenceUpdatedEndpoint _licenceUpdatedWebhookEndpoint;
        private AuthTokenProvider _authTokenProvider;

        private readonly string _projectDir = Path.GetFullPath(Path.Combine(Environment.CurrentDirectory));

        [SetUp]
        public void Setup()
        {
            _licenceUpdatedWebhookEndpoint = new LicenceUpdatedEndpoint();
            _authTokenProvider = new AuthTokenProvider();
        }

        [Test(Description = "WhenValidLUEventInLicenceUpdatedPublishedEventReceivedOptions_ThenWebhookReturns200OkResponse"), Order(0)]
        public async Task WhenValidLUEventInLicenceUpdatedPublishedEventReceivedOptions_ThenWebhookReturns200OkResponse()
        {
            RestResponse response = await _licenceUpdatedWebhookEndpoint.OptionLicenceUpdatedWebhookResponseAsync(await _authTokenProvider.GetAzureADToken(false));
            response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
        }

        [Test(Description = "WhenValidLUEventInLicenceUpdatedPublishedEventReceivedOptionsReceivedWithInvalidToken_ThenWebhookReturns401UnauthorizedResponse"), Order(1)]
        public async Task WhenValidLUEventInLicenceUpdatedPublishedEventReceivedOptionsReceivedWithInvalidToken_ThenWebhookReturns401UnauthorizedResponse()
        {
            RestResponse response = await _licenceUpdatedWebhookEndpoint.OptionLicenceUpdatedWebhookResponseAsync("invalidToken_q234");
            response.StatusCode.Should().Be(System.Net.HttpStatusCode.Unauthorized);
        }

        [Test(Description = "WhenValidLUEventInLicenceUpdatedPublishedEventReceivedOptionsReceivedWithNoRole_ThenWebhookReturns403ForbiddenResponse"), Order(3)]
        public async Task WhenValidLUEventInLicenceUpdatedPublishedEventReceivedOptionsReceivedWithNoRole_ThenWebhookReturns403ForbiddenResponse()
        {
            RestResponse response = await _licenceUpdatedWebhookEndpoint.OptionLicenceUpdatedWebhookResponseAsync(await _authTokenProvider.GetAzureADToken(true));
            response.StatusCode.Should().Be(System.Net.HttpStatusCode.Forbidden);
        }

        [Test, Order(0)]
        [TestCase("LU01_ValidInput.json", TestName = "WhenValidLUEventInLicenceUpdatedPublishedEventReceivedPostReceivedWithValidToken_ThenWebhookReturns200OkResponse")]
        public async Task WhenValidLUEventInLicenceUpdatedPublishedEventReceivedPostReceivedWithValidToken_ThenWebhookReturns200OkResponse(string payloadFileName)
        {
            string filePath = Path.Combine(_projectDir, EventPayloadFiles.PayloadFolder, EventPayloadFiles.LicenceUpdatedPayloadTestData, payloadFileName);
            RestResponse response = await _licenceUpdatedWebhookEndpoint.PostLicenceUpdatedWebhookResponseAsync(filePath, await _authTokenProvider.GetAzureADToken(false));
            response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
        }

        [Test, Order(1)]
        [TestCase("LU01_ValidInput.json", TestName = "WhenValidLUEventInLicenceUpdatedPublishedEventReceivedPostReceivedWithInvalidToken_ThenWebhookReturns401UnauthorizedResponse")]
        public async Task WhenValidLUEventInLicenceUpdatedPublishedEventReceivedPostReceivedWithInvalidToken_ThenWebhookReturns401UnauthorizedResponse(string payloadFileName)
        {
            string filePath = Path.Combine(_projectDir, EventPayloadFiles.PayloadFolder, EventPayloadFiles.LicenceUpdatedPayloadTestData, payloadFileName);
            RestResponse response = await _licenceUpdatedWebhookEndpoint.PostLicenceUpdatedWebhookResponseAsync(filePath, "invalidToken_abcd");
            response.StatusCode.Should().Be(System.Net.HttpStatusCode.Unauthorized);
        }

        [Test, Order(3)]
        [TestCase("LU01_ValidInput.json", TestName = "WhenValidLUEventInLicenceUpdatedPublishedEventReceivedPostReceivedWithInvalidToken_ThenWebhookReturns403ForbiddenResponse")]
        public async Task WhenValidLUEventInLicenceUpdatedPublishedEventReceivedPostReceivedWithInvalidToken_ThenWebhookReturns403ForbiddenResponse(string payloadFileName)
        {
            string filePath = Path.Combine(_projectDir, EventPayloadFiles.PayloadFolder, EventPayloadFiles.LicenceUpdatedPayloadTestData, payloadFileName);
            RestResponse response = await _licenceUpdatedWebhookEndpoint.PostLicenceUpdatedWebhookResponseAsync(filePath, await _authTokenProvider.GetAzureADToken(true));
            response.StatusCode.Should().Be(System.Net.HttpStatusCode.Forbidden);
        }

        [Test, Order(2)]
        [TestCase("LU04_InvalidLUJsonFile.json", TestName = "WhenInValidLUEventInLicenceUpdatedPublishedEventReceivedPostReceived_ThenWebhookReturns400BadRequestResponse")]
        public async Task WhenInValidLUEventInLicenceUpdatedPublishedEventReceivedPostReceived_ThenWebhookReturns400BadRequestResponse(string payloadFileName)
        {
            string filePath = Path.Combine(_projectDir, EventPayloadFiles.PayloadFolder, EventPayloadFiles.LicenceUpdatedPayloadTestData, payloadFileName);
            RestResponse response = await _licenceUpdatedWebhookEndpoint.PostLicenceUpdatedWebhookResponseAsync("Bad Request", filePath, await _authTokenProvider.GetAzureADToken(false));
            response.StatusCode.Should().Be(System.Net.HttpStatusCode.BadRequest);
        }

        [Test, Order(2)]
        [TestCase("LU02_UnSupportedPayloadType.xml", TestName = "WhenInvalidLUEventInLicenceUpdatedPublishedEventReceivedPostReceivedWithInvalidPayloadType_ThenWebhookReturns415UnsupportedMediaResponse")]
        public async Task WhenInvalidLUEventInLicenceUpdatedPublishedEventReceivedPostReceivedWithInvalidPayloadType_ThenWebhookReturns415UnsupportedMediaResponse(string payloadFileName)
        {
            string filePath = Path.Combine(_projectDir, EventPayloadFiles.PayloadFolder, EventPayloadFiles.LicenceUpdatedPayloadTestData, payloadFileName);
            RestResponse response = await _licenceUpdatedWebhookEndpoint.PostLicenceUpdatedWebhookResponseAsync("Unsupported Media Type", filePath, await _authTokenProvider.GetAzureADToken(false));
            response.StatusCode.Should().Be(System.Net.HttpStatusCode.UnsupportedMediaType);
        }

        [Test, Order(0)]
        [TestCase("LU01_ValidInput.json", TestName = "WhenValidLUEventInLicenceUpdatedPublishedEventReceivedPostReceivedWithValidPayload_ThenWebhookReturns200OkResponse")]
        public async Task WhenValidLUEventInLicenceUpdatedPublishedEventReceivedPostReceivedWithValidPayload_ThenWebhookReturns200OkResponse(string payloadJsonFileName)
        {
            Console.WriteLine("Scenario:" + payloadJsonFileName + "\n");
            string filePath = Path.Combine(_projectDir, EventPayloadFiles.PayloadFolder, EventPayloadFiles.LicenceUpdatedPayloadTestData, payloadJsonFileName);
            string generatedXmlFolder = Path.Combine(_projectDir, EventPayloadFiles.GeneratedXmlFolder, EventPayloadFiles.LicenceUpdatedPayloadTestData);
            RestResponse response = await _licenceUpdatedWebhookEndpoint.PostLicenceUpdatedResponseAsyncForXML(filePath, generatedXmlFolder, await _authTokenProvider.GetAzureADToken(false));
            response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
        }

        [TestCase("LU03_InvalidLUJsonFile.json", TestName = "WhenValidLUEventInLicenceUpdatedPublishedEventReceivedPostReceivedWithInvalidPayload_ThenWebhookReturns500InternalServerErrorResponse")]
        public async Task WhenValidLUEventInLicenceUpdatedPublishedEventReceivedPostReceivedWithInvalidPayload_ThenWebhookReturns500InternalServerErrorResponse(string payloadFileName)
        {
            string filePath = Path.Combine(_projectDir, EventPayloadFiles.PayloadFolder, EventPayloadFiles.LicenceUpdatedPayloadTestData, payloadFileName);
            RestResponse response = await _licenceUpdatedWebhookEndpoint.PostLicenceUpdatedWebhookResponseAsync(filePath, await _authTokenProvider.GetAzureADToken(false));
            response.StatusCode.Should().Be(System.Net.HttpStatusCode.InternalServerError);
        }
    }
}
