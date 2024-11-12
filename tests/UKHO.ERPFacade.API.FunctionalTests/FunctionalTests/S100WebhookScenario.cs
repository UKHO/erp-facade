using NUnit.Framework;
using FluentAssertions;
using UKHO.ERPFacade.API.FunctionalTests.Service;
using RestSharp;
using UKHO.ERPFacade.Common.Constants;
using UKHO.ERPFacade.API.FunctionalTests.Auth;
using UKHO.ERPFacade.API.FunctionalTests.Operations;
using UKHO.ERPFacade.API.FunctionalTests.Modifiers;
using UKHO.ERPFacade.API.FunctionalTests.Validators;

namespace UKHO.ERPFacade.API.FunctionalTests.FunctionalTests
{
    [TestFixture]
    public class S100WebhookScenarios
    {
        private AuthTokenProvider _authTokenProvider;
        private WebhookEndpoint _webhookEndpoint;
        private AzureBlobReaderWriter _azureBlobReaderWriter;

        private readonly string _projectDir = Path.GetFullPath(Path.Combine(Environment.CurrentDirectory));

        [SetUp]
        public void Setup()
        {
            _authTokenProvider = new AuthTokenProvider();
            _webhookEndpoint = new WebhookEndpoint();
            _azureBlobReaderWriter = new AzureBlobReaderWriter();
        }

        [Test]
        [TestCase("NewCell.JSON", TestName = "WhenICallTheWebhookWithNewCellScenario_ThenWebhookReturns200Response")]
        [TestCase("CancellationAndReplacement.JSON", TestName = "WhenICallTheWebhookWithCancellationAndReplacementScenario_ThenWebhookReturns200Response")]
        [TestCase("NewCellAndEdition.JSON", TestName = "WhenICallTheWebhookWithNewCellAndEditionScenario_ThenWebhookReturns200Response")]
        [TestCase("SimpleUpdate.JSON", TestName = "WhenICallTheWebhookWithSimpleUpdateScenario_ThenWebhookReturns200Response")]
        [TestCase("SupplierChange.JSON", TestName = "WhenICallTheWebhookWithSupplierChangeScenario_ThenWebhookReturns200Response")]
        [TestCase("SupplierDefinedReleasability.JSON", TestName = "WhenICallTheWebhookWithSupplierDefinedReleasabilityScenario_ThenWebhookReturns200Response")]
        [TestCase("SupplierDefinedUnitChange.JSON", TestName = "WhenICallTheWebhookWithSupplierDefinedUnitChangeScenario_ThenWebhookReturns200Response")]
        [TestCase("SupplierDefinedUnitChangeV2.JSON", TestName = "WhenICallTheWebhookWithSupplierDefinedUnitChangeV2Scenario_ThenWebhookReturns200Response")]
        [TestCase("Suspend.JSON", TestName = "WhenICallTheWebhookWithSuspendScenario_ThenWebhookReturns200Response")]
        [TestCase("Withdrawn.JSON", TestName = "WhenICallTheWebhookWithWithdrawnScenario_ThenWebhookReturns200Response")]

        public async Task WhenValidS100DataContentPublishedEventReceivedWithValidToken_ThenWebhookReturns200OkResponse(string jsonPayloadFileName)
        {
            string correlationId = null;

            string jsonPayloadFilePath = Path.Combine(_projectDir, EventPayloadFiles.PayloadFolder, EventPayloadFiles.S100PayloadFolder, jsonPayloadFileName);
            string xmlPayloadFilePath = jsonPayloadFilePath.Replace(EventPayloadFiles.PayloadFolder, EventPayloadFiles.ErpFacadeExpectedXmlFolder)
                                               .Replace(EventPayloadFiles.S100PayloadFolder, EventPayloadFiles.S100ExpectedXmlFiles)
                                               .Replace(".JSON", ".xml");

            string requestBody = await File.ReadAllTextAsync(jsonPayloadFilePath);
            requestBody = JsonModifier.UpdateTime(requestBody);
            (requestBody, correlationId) = JsonModifier.UpdateCorrelationId(requestBody);

            RestResponse response = await _webhookEndpoint.PostWebhookResponseAsync(requestBody, await _authTokenProvider.GetAzureADToken(false));

            response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);

            string generatedXmlFilePath = _azureBlobReaderWriter.DownloadContainerFile(Path.Combine(_projectDir, EventPayloadFiles.GeneratedXmlFolder), correlationId, ".xml");

            Assert.That(S100XmlValidator.VerifyXmlAttributes(generatedXmlFilePath, xmlPayloadFilePath, correlationId));
        }
    }
}
