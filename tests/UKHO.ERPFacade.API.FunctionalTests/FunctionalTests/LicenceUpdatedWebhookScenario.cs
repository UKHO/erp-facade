﻿using FluentAssertions;
using NUnit.Framework;
using RestSharp;
using UKHO.ERPFacade.API.FunctionalTests.Configuration;
using UKHO.ERPFacade.API.FunctionalTests.Helpers;
using UKHO.ERPFacade.API.FunctionalTests.Service;

namespace UKHO.ERPFacade.API.FunctionalTests.FunctionalTests
{
    [TestFixture]
    public class LicenceUpdatedWebhookScenario
    {
        private LicenceUpdatedEndpoint _LUpdatedWebhookEndpoint { get; set; }
        private readonly ADAuthTokenProvider _authToken = new();
        //private readonly string _projectDir = Path.GetFullPath(Path.Combine(Environment.CurrentDirectory));
        //for local
        private readonly string _projectDir = Path.GetFullPath(Path.Combine(Environment.CurrentDirectory, "..\\..\\.."));

        [SetUp]
        public void Setup()
        {
            _LUpdatedWebhookEndpoint = new LicenceUpdatedEndpoint();
        }

        [Test(Description = "WhenValidLUEventInLicenceUpdatedPublishedEventReceivedOptions_ThenWebhookReturns200OkResponse"), Order(0)]
        public async Task WhenValidLUEventInLicenceUpdatedPublishedEventReceivedOptions_ThenWebhookReturns200OkResponse()
        {
            var response = await _LUpdatedWebhookEndpoint.OptionLicenceUpdatedWebhookResponseAsync(await _authToken.GetAzureADToken(false));
            response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
        }

        [Test(Description = "WhenValidLUEventInLicenceUpdatedPublishedEventReceivedOptionsReceivedWithInvalidToken_ThenWebhookReturns401UnauthorizedResponse"), Order(1)]
        public async Task WhenValidLUEventInLicenceUpdatedPublishedEventReceivedOptionsReceivedWithInvalidToken_ThenWebhookReturns401UnauthorizedResponse()
        {
            var response = await _LUpdatedWebhookEndpoint.OptionLicenceUpdatedWebhookResponseAsync("invalidToken_q234");
            response.StatusCode.Should().Be(System.Net.HttpStatusCode.Unauthorized);
        }

        [Test(Description = "WhenValidLUEventInLicenceUpdatedPublishedEventReceivedOptionsReceivedWithNoRole_ThenWebhookReturns403ForbiddenResponse"), Order(3)]
        public async Task WhenValidLUEventInLicenceUpdatedPublishedEventReceivedOptionsReceivedWithNoRole_ThenWebhookReturns403ForbiddenResponse()
        {
            var response = await _LUpdatedWebhookEndpoint.OptionLicenceUpdatedWebhookResponseAsync(await _authToken.GetAzureADToken(true));
            response.StatusCode.Should().Be(System.Net.HttpStatusCode.Forbidden);
        }

        [Test, Order(0)]
        [TestCase("ID01_ValidLUJsonFile.json", TestName = "WhenValidLUEventInLicenceUpdatedPublishedEventReceivedPostReceivedWithValidToken_ThenWebhookReturns200OkResponse")]
        public async Task WhenValidLUEventInLicenceUpdatedPublishedEventReceivedPostReceivedWithValidToken_ThenWebhookReturns200OkResponse(string payloadFileName)
        {
            string filePath = Path.Combine(_projectDir, Config.TestConfig.PayloadFolder, "LicenceUpdatedPayloadTestData", payloadFileName);
            var response = await _LUpdatedWebhookEndpoint.PostLicenceUpdatedWebhookResponseAsync(filePath, await _authToken.GetAzureADToken(false));
            response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
        }

        [Test, Order(1)]
        [TestCase("ID01_ValidLUJsonFile.json", TestName = "WhenValidLUEventInLicenceUpdatedPublishedEventReceivedPostReceivedWithInvalidToken_ThenWebhookReturns401UnauthorizedResponse")]
        public async Task WhenValidLUEventInLicenceUpdatedPublishedEventReceivedPostReceivedWithInvalidToken_ThenWebhookReturns401UnauthorizedResponse(string payloadFileName)
        {
            string filePath = Path.Combine(_projectDir, Config.TestConfig.PayloadFolder, "LicenceUpdatedPayloadTestData", payloadFileName);
            var response = await _LUpdatedWebhookEndpoint.PostLicenceUpdatedWebhookResponseAsync(filePath, "invalidToken_abcd");
            response.StatusCode.Should().Be(System.Net.HttpStatusCode.Unauthorized);
        }

        [Test, Order(3)]
        [TestCase("ID01_ValidLUJsonFile.json", TestName = "WhenValidLUEventInLicenceUpdatedPublishedEventReceivedPostReceivedWithInvalidToken_ThenWebhookReturns403ForbiddenResponse")]
        public async Task WhenValidLUEventInLicenceUpdatedPublishedEventReceivedPostReceivedWithInvalidToken_ThenWebhookReturns403ForbiddenResponse(string payloadFileName)
        {
            string filePath = Path.Combine(_projectDir, Config.TestConfig.PayloadFolder, "LicenceUpdatedPayloadTestData", payloadFileName);
            var response = await _LUpdatedWebhookEndpoint.PostLicenceUpdatedWebhookResponseAsync(filePath, await _authToken.GetAzureADToken(true));
            response.StatusCode.Should().Be(System.Net.HttpStatusCode.Forbidden);
        }

        [Test, Order(2)]
        [TestCase("ID03_InvalidLUJsonFile.json", TestName = "WhenInValidLUEventInLicenceUpdatedPublishedEventReceivedPostReceived_ThenWebhookReturns400BadRequestResponse")]
        public async Task WhenInValidLUEventInLicenceUpdatedPublishedEventReceivedPostReceived_ThenWebhookReturns400BadRequestResponse(string payloadFileName)
        {
            string filePath = Path.Combine(_projectDir, Config.TestConfig.PayloadFolder, "LicenceUpdatedPayloadTestData", payloadFileName);
            var response = await _LUpdatedWebhookEndpoint.PostLicenceUpdatedWebhookResponseAsync("Bad Request", filePath, await _authToken.GetAzureADToken(false));
            response.StatusCode.Should().Be(System.Net.HttpStatusCode.BadRequest);
        }

        [Test, Order(2)]
        [TestCase("ID02_UnSupportedPayloadType.xml", TestName = "WhenInvalidLUEventInLicenceUpdatedPublishedEventReceivedPostReceivedWithInvalidPayloadType_ThenWebhookReturns415UnsupportedMediaResponse")]
        public async Task WhenInvalidLUEventInLicenceUpdatedPublishedEventReceivedPostReceivedWithInvalidPayloadType_ThenWebhookReturns415UnsupportedMediaResponse(string payloadFileName)
        {
            string filePath = Path.Combine(_projectDir, Config.TestConfig.PayloadFolder, "LicenceUpdatedPayloadTestData", payloadFileName);
            var response = await _LUpdatedWebhookEndpoint.PostLicenceUpdatedWebhookResponseAsync("Unsupported Media Type", filePath, await _authToken.GetAzureADToken(false));
            response.StatusCode.Should().Be(System.Net.HttpStatusCode.UnsupportedMediaType);
        }

        [Test, Order(0)]
        [TestCase("LicenceUpdated.json", TestName = "WhenValidLUEventInLicenceUpdatedPublishedEventReceivedPostReceivedWithValidPayload_ThenWebhookReturns200OkResponse")]
        public async Task WhenValidLUEventInLicenceUpdatedPublishedEventReceivedPostReceivedWithValidPayload_ThenWebhookReturns200OkResponse(string payloadJsonFileName)
        {
            Console.WriteLine("Scenario:" + payloadJsonFileName + "\n");
            string filePath = Path.Combine(_projectDir, Config.TestConfig.PayloadFolder, Config.TestConfig.LicenceUpdate,  payloadJsonFileName);
            string generatedXMLFolder = Path.Combine(_projectDir, Config.TestConfig.GeneratedXMLFolder,Config.TestConfig.LicenceUpdate);
           RestResponse response = await _LUpdatedWebhookEndpoint.PostLicenceUpdatedResponseAsyncForXML(filePath, generatedXMLFolder, await _authToken.GetAzureADToken(false));
            response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
        }
    }
}
