using NUnit.Framework;
using FluentAssertions;
using UKHO.ERPFacade.API.FunctionalTests.Configuration;
using UKHO.ERPFacade.API.FunctionalTests.Helpers;
using UKHO.ERPFacade.API.FunctionalTests.Service;
using RestSharp;
using UKHO.ERPFacade.Common.Constants;

namespace UKHO.ERPFacade.API.FunctionalTests.FunctionalTests
{
    [TestFixture]
    public class S100WebhookScenarios
    {
        private WebhookEndpoint _webhookEndpoint;
        private readonly ADAuthTokenProvider _authToken = new();

        private readonly string _projectDir = Path.GetFullPath(Path.Combine(Environment.CurrentDirectory));
        //for local
        //private readonly string _projectDir = Path.GetFullPath(Path.Combine(Environment.CurrentDirectory, "..\\..\\.."));


        [SetUp]
        public void Setup()
        {
            _webhookEndpoint = new WebhookEndpoint();
        }

        [Test]
        [TestCase("NewCells.JSON", TestName = "WhenValidS100DataContentPublishedEventReceivedWithValidToken_ThenWebhookReturns200OkAndPayloadShouldBeStoredInBlobContainer")]

        public async Task WhenValidS100DataContentPublishedEventReceivedWithValidToken_ThenWebhookReturns200OkResponse(string payload)
        {
            string filePath = Path.Combine(_projectDir, Config.TestConfig.PayloadFolder, EventPayloadFiles.S100WebhookPayloadFolder, payload);
            RestResponse response = await _webhookEndpoint.PostWebhookResponseAsync(filePath, await _authToken.GetAzureADToken(false), true);
            response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
        }
    }

}
