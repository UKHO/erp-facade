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
        [TestCase("NewCell.JSON", TestName = "WhenICallTheWebhookWithNewCellScenario_ThenWebhookReturns200Response")]
        [TestCase("CancellationAndReplacement.JSON", TestName = "WhenICallTheWebhookWithCancellationAndReplacementScenario_ThenWebhookReturns200Response")]
        [TestCase("NewCellAndEdition.JSON", TestName = "WhenICallTheWebhookWithNewCellAndEditionScenario_ThenWebhookReturns200Response")]
        [TestCase("SimpleUpdate.JSON", TestName = "WhenICallTheWebhookWithSimpleUpdateScenario_ThenWebhookReturns200Response")]
        [TestCase("SupplierChange.JSON", TestName = "WhenICallTheWebhookWithSupplierChangeScenario_ThenWebhookReturns200Response")]
        [TestCase("SupplierDefinedReleasability.JSON", TestName = "WhenICallTheWebhookWithSupplierDefinedReleasabilityScenario_ThenWebhookReturns200Response")]
        [TestCase("SupplierDefinedUnitChange.JSON", TestName = "WhenICallTheWebhookWithSupplierDefinedUnitChangeScenario_ThenWebhookReturns200Response")]

        public async Task WhenValidS100DataContentPublishedEventReceivedWithValidToken_ThenWebhookReturns200OkResponse(string payload)
        {
            string filePath = Path.Combine(_projectDir, Config.TestConfig.PayloadFolder, EventPayloadFiles.S100WebhookPayloadFolder, payload);
            string generatedXmlFolder = Path.Combine(_projectDir, Config.TestConfig.GeneratedXmlFolder);
            RestResponse response = await _webhookEndpoint.PostWebhookResponseAsyncForXml(filePath, generatedXmlFolder, await _authToken.GetAzureADToken(false));
            response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
        }
    }
}
