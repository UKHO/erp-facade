using System.Net.Http;
using System.Threading.Tasks;
using FakeItEasy;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using NUnit.Framework;
using UKHO.ERPFacade.Common.Logging;
using UKHO.ERPFacade.Common.Policies;

namespace UKHO.ERPFacade.API.UnitTests.Policies
{
    public class RetryPolicyProviderTests
    {
        private ILogger<RetryPolicyProvider> _fakeLogger;
        private RetryPolicyProvider _fakeRetryPolicyProvider;

        [SetUp]
        public void Setup()
        {
            _fakeLogger = A.Fake<ILogger<RetryPolicyProvider>>();
            _fakeRetryPolicyProvider = new RetryPolicyProvider(_fakeLogger);
        }

        [Test]
        public async Task WhenValidInputsProvided_ThenCreateRetrypolicySuccessfully()
        {
            var httpResponseMessage = new HttpResponseMessage(System.Net.HttpStatusCode.InternalServerError);
            var policy = _fakeRetryPolicyProvider.GetRetryPolicy("TestService", EventIds.RetryAttemptForEnterpriseEventServiceEvent, 3, 2);
            await policy.ExecuteAsync(() => Task.FromResult(httpResponseMessage));

            httpResponseMessage.Should().BeOfType<HttpResponseMessage>();
        }
    }
}
