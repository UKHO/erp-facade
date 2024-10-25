using NUnit.Framework;
using UKHO.ERPFacade.API.FunctionalTests.Helpers;
using UKHO.ERPFacade.API.FunctionalTests.Service;

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
    }

}
