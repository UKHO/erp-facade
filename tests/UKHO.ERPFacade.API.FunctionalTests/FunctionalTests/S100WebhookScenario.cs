using FluentAssertions;
using NUnit.Framework;
using UKHO.ERPFacade.API.FunctionalTests.Auth;
using UKHO.ERPFacade.API.FunctionalTests.Modifiers;
using UKHO.ERPFacade.API.FunctionalTests.Operations;
using UKHO.ERPFacade.API.FunctionalTests.Service;
using UKHO.ERPFacade.API.FunctionalTests.Validators;
using UKHO.ERPFacade.Common.Constants;

namespace UKHO.ERPFacade.API.FunctionalTests.FunctionalTests
{
    [TestFixture]
    public class S100WebhookScenarios
    {
        private AuthTokenProvider _authTokenProvider;
        private WebhookEndpoint _webhookEndpoint;
        private AzureBlobReaderWriter _azureBlobReaderWriter;
        private AzureTableReaderWriter _azureTableReaderWriter;
        private S100JsonValidator _s100JsonValidator;
        private readonly string _projectDir = Path.GetFullPath(Path.Combine(Environment.CurrentDirectory));

        [SetUp]
        public void Setup()
        {
            _authTokenProvider = new AuthTokenProvider();
            _webhookEndpoint = new WebhookEndpoint();
            _azureBlobReaderWriter = new AzureBlobReaderWriter();
            _azureTableReaderWriter = new AzureTableReaderWriter();
            _s100JsonValidator = new S100JsonValidator();
        }

        [Test]
        [TestCase("NewCell.JSON", TestName = "WhenICallTheWebhookWithNewCellScenario_ThenWebhookReturns200Response")]
        [TestCase("CancellationAndReplacement.JSON", TestName = "WhenICallTheWebhookWithCancellationAndReplacementScenario_ThenWebhookReturns200Response")]
        [TestCase("NewCellAndEdition.JSON", TestName = "WhenICallTheWebhookWithNewCellAndEditionScenario_ThenChangeProductAndChangeUnitOfSaleActionsAreNotGenerated")]
        [TestCase("SimpleUpdate.JSON", TestName = "WhenICallTheWebhookWithSimpleUpdateScenario_ThenWebhookReturns200Response")]
        [TestCase("SupplierChange.JSON", TestName = "WhenICallTheWebhookWithSupplierChangeScenario_ThenWebhookReturns200Response")]
        [TestCase("SupplierDefinedReleasability.JSON", TestName = "WhenICallTheWebhookWithSupplierDefinedReleasabilityScenario_ThenWebhookReturns200Response")]
        [TestCase("SupplierDefinedUnitChange.JSON", TestName = "WhenICallTheWebhookWithSupplierDefinedUnitChangeScenario_ThenWebhookReturns200Response")]
        [TestCase("SupplierDefinedUnitChangeV2.JSON", TestName = "WhenICallTheWebhookWithSupplierDefinedUnitChangeV2Scenario_ThenWebhookReturns200Response")]
        [TestCase("Suspend.JSON", TestName = "WhenICallTheWebhookWithSuspendScenario_ThenWebhookReturns200Response")]
        [TestCase("Withdrawn.JSON", TestName = "WhenICallTheWebhookWithWithdrawnScenario_ThenWebhookReturns200Response")]
        [TestCase("NewCellWithUnitOfSaleTypeIsFolio.JSON", TestName = "VerifyCreateUnitOfSaleActionNotGeneratedWhenUnitOfSaleTypeIsNotUnit")]
        public async Task WhenValidS100DataContentPublishedEventReceivedWithValidToken_ThenWebhookReturns200OkResponse(string jsonPayloadFileName)
        {
            var jsonPayloadFilePath = Path.Combine(_projectDir, EventPayloadFiles.PayloadFolder, EventPayloadFiles.S100PayloadFolder, jsonPayloadFileName);
            var xmlPayloadFilePath = jsonPayloadFilePath.Replace(EventPayloadFiles.PayloadFolder, EventPayloadFiles.ErpFacadeExpectedXmlFolder)
                                               .Replace(EventPayloadFiles.S100PayloadFolder, EventPayloadFiles.S100ExpectedXmlFiles)
                                               .Replace(".JSON", ".xml");

            var requestBody = await File.ReadAllTextAsync(jsonPayloadFilePath);
            requestBody = JsonModifier.UpdateTime(requestBody);
            (requestBody, var correlationId) = JsonModifier.UpdateCorrelationId(requestBody);

            Console.WriteLine("Scenario: " + jsonPayloadFileName + "\n" + "CorrelationId: " + correlationId + "\n");

            var response = await _webhookEndpoint.PostWebhookResponseAsync(requestBody, await _authTokenProvider.GetAzureADTokenAsync(false));

            response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);

            var generatedXmlFilePath = _azureBlobReaderWriter.DownloadContainerFile(Path.Combine(_projectDir, EventPayloadFiles.GeneratedXmlFolder), correlationId, ".xml");

            Assert.That(S100XmlValidator.VerifyXmlAttributes(generatedXmlFilePath, xmlPayloadFilePath, correlationId));
        }

        [Test]
        public async Task WhenValidS100DataContentPublishedEventReceivedWithValidToken_ThenWebhookReturns200OkResponseAndCallbackEndpointPublishesEvent()
        {
            var jsonPayloadFilePath = Path.Combine(_projectDir, EventPayloadFiles.PayloadFolder, EventPayloadFiles.S100PayloadFolder, "NewCell.JSON");
            var expectedFilePath = Path.Combine(_projectDir, EventPayloadFiles.GeneratedJsonFolder);

            var requestBody = await File.ReadAllTextAsync(jsonPayloadFilePath);
            requestBody = JsonModifier.UpdateTime(requestBody);
            (requestBody, var correlationId) = JsonModifier.UpdateCorrelationId(requestBody);

            Console.WriteLine("Scenario: ERP Facade to SAP to EES event publish.\nCorrelationId: " + correlationId + "\n");

            var response = await _webhookEndpoint.PostWebhookResponseAsync(requestBody, await _authTokenProvider.GetAzureADTokenAsync(false));

            //Once the webhook endpoint returns 200 OK response, the SAP callback endpoint is called from wiremock.
            response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);

            //Delay is added to ensure SAP callback is completed from wiremock.
            await Task.Delay(30000);

            //Download the S100UnitOfSaleUpdatedEvent JSON file from the blob container.
            expectedFilePath = _azureBlobReaderWriter.DownloadContainerFile(expectedFilePath, correlationId, ".json", EventPayloadFiles.S100UnitOfSaleUpdatedEventFileName);

            Assert.That(await _s100JsonValidator.VerifyS100UnitOfSaleUpdatedEventJson(expectedFilePath));
            Assert.That(_azureTableReaderWriter.GetStatus(correlationId).Equals("Complete"));
        }

        [Test]
        public async Task WhenValidS100DataContentPublishedEventReceived500InternalServerErrorFromSapCallbackEndpoint_ThenWebhookEndPointReturns500InternalServerErrorResponse()
        {
            var jsonPayloadFilePath = Path.Combine(_projectDir, EventPayloadFiles.PayloadFolder, EventPayloadFiles.S100PayloadFolder, "SAP500InternalServerError.JSON");

            var requestBody = await File.ReadAllTextAsync(jsonPayloadFilePath);
            requestBody = JsonModifier.UpdateTime(requestBody);

            var response = await _webhookEndpoint.PostWebhookResponseAsync(requestBody, await _authTokenProvider.GetAzureADTokenAsync(false));

            response.StatusCode.Should().Be(System.Net.HttpStatusCode.InternalServerError);
        }

        [Test]
        public async Task Verify400BadRequestReceivedForInvalidS100DataContentPublishedEvent()
        {
            var jsonPayloadFilePath = Path.Combine(_projectDir, EventPayloadFiles.PayloadFolder, EventPayloadFiles.S100PayloadFolder, "NewCellV1.JSON");

            var requestBody = await File.ReadAllTextAsync(jsonPayloadFilePath);
            requestBody = JsonModifier.UpdateTime(requestBody);
            (requestBody, _) = JsonModifier.UpdateCorrelationId(requestBody);

            var response = await _webhookEndpoint.PostWebhookResponseAsync(requestBody, await _authTokenProvider.GetAzureADTokenAsync(false));

            response.StatusCode.Should().Be(System.Net.HttpStatusCode.BadRequest);

            Assert.That(response.Content!.Contains("Unknown event type received in payload."));
        }

        
        [TestCase("SimpleUpdateNoXmlGeneration.JSON", TestName = "WhenICallTheWebhookWithSimpleUpdateScenario_ThenWebhookReturns200ResponseAndXmlIsNotGenerated")]
        [TestCase("SuspendNoXmlGeneration.JSON", TestName = "WhenICallTheWebhookWithSuspendScenario_ThenWebhookReturns200ResponseAndXmlIsNotGenerated")]
        [TestCase("NewEditionNoXmlGeneration.JSON", TestName = "WhenICallTheWebhookWithNewEditionScenario_ThenWebhookReturns200ResponseAndXmlIsNotGenerated")]
        [TestCase("ReissueNoXmlGeneration.JSON", TestName = "WhenICallTheWebhookWithReissueScenario_ThenWebhookReturns200ResponseAndXmlIsNotGenerated")]
        public async Task WhenValidS100DataContentPublishedEventReceivedWithValidToken_ThenWebhookReturns200OkResponseAndXmlIsNotGenerated(string jsonPayloadFileName)
        {
            var jsonPayloadFilePath = Path.Combine(_projectDir, EventPayloadFiles.PayloadFolder, EventPayloadFiles.S100PayloadFolder, jsonPayloadFileName);
            var expectedFilePath = Path.Combine(_projectDir, EventPayloadFiles.GeneratedJsonFolder);

            var requestBody = await File.ReadAllTextAsync(jsonPayloadFilePath);
            requestBody = JsonModifier.UpdateTime(requestBody);
            (requestBody, var correlationId) = JsonModifier.UpdateCorrelationId(requestBody);

            Console.WriteLine("Scenario: " + jsonPayloadFileName + "\n" + "CorrelationId: " + correlationId + "\n");

            var response = await _webhookEndpoint.PostWebhookResponseAsync(requestBody, await _authTokenProvider.GetAzureADTokenAsync(false));

            response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);

            Assert.That(_azureTableReaderWriter.GetStatus(correlationId), Is.EqualTo("Complete"), $"SAP status is Incomplete for {correlationId}");

            var responseDateTime =  _azureTableReaderWriter.GetTableEntity(correlationId);
            responseDateTime.FirstOrDefault()!.ResponseDateTime.Should().BeNull();

            var generatedXmlFilePath = _azureBlobReaderWriter.DownloadContainerFile(Path.Combine(_projectDir, EventPayloadFiles.GeneratedXmlFolder), correlationId, ".xml");

            generatedXmlFilePath.Should().BeNullOrEmpty();
        }
    }
}
