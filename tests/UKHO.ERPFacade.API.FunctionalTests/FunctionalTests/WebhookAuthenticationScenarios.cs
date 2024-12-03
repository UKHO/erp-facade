using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using NUnit.Framework;
using UKHO.ERPFacade.API.FunctionalTests.Auth;
using UKHO.ERPFacade.API.FunctionalTests.Configuration;
using UKHO.ERPFacade.API.FunctionalTests.Modifiers;
using UKHO.ERPFacade.API.FunctionalTests.Service;
using UKHO.ERPFacade.Common.Constants;
namespace UKHO.ERPFacade.API.FunctionalTests.FunctionalTests
{
    public class WebhookAuthenticationScenarios : TestFixtureBase
    {
        private readonly ErpFacadeConfiguration _erpFacadeConfiguration;
        private AuthTokenProvider _authTokenProvider;
        private WebhookEndpoint _webhookEndpoint;
        private readonly string _projectDir = Path.GetFullPath(Path.Combine(Environment.CurrentDirectory));

        public WebhookAuthenticationScenarios()
        {
            var serviceProvider = GetServiceProvider();
            _erpFacadeConfiguration = serviceProvider!.GetRequiredService<IOptions<ErpFacadeConfiguration>>().Value;
        }

        [SetUp]
        public void Setup()
        {
            _authTokenProvider = new AuthTokenProvider();
            _webhookEndpoint = new WebhookEndpoint();
        }

        [Test(Description = "WhenWebhookOptionsEndpointRequestedWithValidToken_ThenWebhookReturns200OkResponse"), Order(0)]
        public async Task WhenWebhookOptionsEndpointRequestedWithValidToken_ThenWebhookReturns200OkResponse()
        {
            var token = await _authTokenProvider.GetAzureADTokenAsync(false);
            var response = await _webhookEndpoint.OptionWebhookResponseAsync(token);
            response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
        }

        [Test(Description = "WhenWebhookOptionsEndpointRequestedWithInvalidToken_ThenWebhookReturns401UnauthorizedResponse"), Order(0)]
        public async Task WhenWebhookOptionsEndpointRequestedWithInvalidToken_ThenWebhookReturns401UnauthorizedResponse()
        {
            var response = await _webhookEndpoint.OptionWebhookResponseAsync("InvalidToken");
            response.StatusCode.Should().Be(System.Net.HttpStatusCode.Unauthorized);
        }

        [Test(Description = "WhenWebhookOptionsEndpointRequestedWithValidTokenWithNoRole_ThenWebhookReturns403ForbiddenResponse"), Order(0)]
        public async Task WhenWebhookOptionsEndpointRequestedWithValidTokenWithNoRole_ThenWebhookReturns403ForbiddenResponse()
        {
            var response = await _webhookEndpoint.OptionWebhookResponseAsync(await _authTokenProvider.GetAzureADTokenAsync(true));
            response.StatusCode.Should().Be(System.Net.HttpStatusCode.Forbidden);
        }

        [Test(Description = "WhenWebhookPostEndpointReceivesEventWithValidToken_ThenWebhookReturns200OkResponse"), Order(0)]
        public async Task WhenWebhookPostEndpointReceivesEventWithValidToken_ThenWebhookReturns200OkResponse()
        {
            string requestPayload = await File.ReadAllTextAsync(Path.Combine(_projectDir, EventPayloadFiles.PayloadFolder, EventPayloadFiles.S57PayloadFolder, EventPayloadFiles.WebhookPayloadFileName));
            requestPayload = JsonModifier.UpdatePermitField(requestPayload, _erpFacadeConfiguration.PermitWithSameKey.Permit);
            var response = await _webhookEndpoint.PostWebhookResponseAsync(requestPayload, await _authTokenProvider.GetAzureADTokenAsync(false));
            response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
        }

        [Test(Description = "WhenWebhookPostEndpointReceivesEventWithInvalidToken_ThenWebhookReturns401UnauthorizedResponse"), Order(2)]
        public async Task WhenWebhookPostEndpointReceivesEventWithInvalidToken_ThenWebhookReturns401UnauthorizedResponse()
        {
            string requestPayload = await File.ReadAllTextAsync(Path.Combine(_projectDir, EventPayloadFiles.PayloadFolder, EventPayloadFiles.S57PayloadFolder, EventPayloadFiles.WebhookPayloadFileName));
            var response = await _webhookEndpoint.PostWebhookResponseAsync(requestPayload, "InvalidToken");
            response.StatusCode.Should().Be(System.Net.HttpStatusCode.Unauthorized);
        }

        [Test(Description = "WhenWebhookPostEndpointReceivesEventWithValidTokenWithNoRole_ThenWebhookReturns403ForbiddenResponse"), Order(4)]
        public async Task WhenWebhookPostEndpointReceivesEventWithValidTokenWithNoRole_ThenWebhookReturns403ForbiddenResponse()
        {
            string requestPayload = await File.ReadAllTextAsync(Path.Combine(_projectDir, EventPayloadFiles.PayloadFolder, EventPayloadFiles.S57PayloadFolder, EventPayloadFiles.WebhookPayloadFileName));
            var response = await _webhookEndpoint.PostWebhookResponseAsync(requestPayload, await _authTokenProvider.GetAzureADTokenAsync(true));
            response.StatusCode.Should().Be(System.Net.HttpStatusCode.Forbidden);
        }

    }
}
