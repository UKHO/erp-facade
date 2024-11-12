using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;

namespace UKHO.ERPFacade.API.FunctionalTests.Configuration
{
    public class TestFixtureBase
    {
        private readonly ServiceProvider _serviceProvider;

        protected ServiceProvider GetServiceProvider()
        {
            return _serviceProvider;
        }

        public TestFixtureBase()
        {
            _serviceProvider = TestServiceConfiguration.ConfigureServices();
        }

        [OneTimeTearDown]
        public void Cleanup()
        {
            _serviceProvider?.Dispose();
        }
    }
}
