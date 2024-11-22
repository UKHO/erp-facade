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
        private AzureTableReaderWriter _azureTableReaderWriter;
        private JsonValidator _jsonValidator;

        private readonly string _projectDir = Path.GetFullPath(Path.Combine(Environment.CurrentDirectory));

        [SetUp]
        public void Setup()
        {
            _authTokenProvider = new AuthTokenProvider();
            _webhookEndpoint = new WebhookEndpoint();
            _azureBlobReaderWriter = new AzureBlobReaderWriter();
            _azureTableReaderWriter = new AzureTableReaderWriter();
            _jsonValidator = new JsonValidator();
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

        [Test]

        public async Task WhenValidS100DataContentPublishedEventReceivedWithValidToken_ThenWebhookAndSAPEndPointReturns200OkResponse()
        {
            string correlationId = null;

            string jsonPayloadFilePath = Path.Combine(_projectDir, EventPayloadFiles.PayloadFolder, EventPayloadFiles.S100PayloadFolder, "NewCell.JSON");
            string expectedFilePath = Path.Combine(_projectDir, EventPayloadFiles.GeneratedJsonFolder);

            string requestBody = await File.ReadAllTextAsync(jsonPayloadFilePath);
            requestBody = JsonModifier.UpdateTime(requestBody);
            (requestBody, correlationId) = JsonModifier.UpdateCorrelationId(requestBody);

            RestResponse response = await _webhookEndpoint.PostWebhookResponseAsync(requestBody, await _authTokenProvider.GetAzureADToken(false));

            response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);

            await Task.Delay(15000); //This delay has been added to ensure SAP callback is completed.

            expectedFilePath = _azureBlobReaderWriter.DownloadContainerFile(expectedFilePath, correlationId, ".json", EventPayloadFiles.S100UnitOfSaleUpdatedEvent);
            Assert.That(await _jsonValidator.VerifyUnitOfSaleEvent(expectedFilePath));
            Assert.That(_azureTableReaderWriter.GetSapStatus(correlationId).Equals("Complete"));
        }

        [Test]

        public async Task WhenValidS100DataContentPublishedEventReceived500InternalServerErrorFromSapCallbackEndpoint_ThenWebhookEndPointReturns500InternalServerErrorResponse()
        {
            string jsonPayloadFilePath = Path.Combine(_projectDir, EventPayloadFiles.PayloadFolder, EventPayloadFiles.S100PayloadFolder, "SAP500InternalServerError.JSON");

            string requestBody = await File.ReadAllTextAsync(jsonPayloadFilePath);
            requestBody = JsonModifier.UpdateTime(requestBody);

            RestResponse response = await _webhookEndpoint.PostWebhookResponseAsync(requestBody, await _authTokenProvider.GetAzureADToken(false));

            response.StatusCode.Should().Be(System.Net.HttpStatusCode.InternalServerError);
        }
    }
}
