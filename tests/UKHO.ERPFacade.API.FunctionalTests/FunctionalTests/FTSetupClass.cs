using NUnit.Framework;
using UKHO.ERPFacade.API.FunctionalTests.Helpers;

namespace UKHO.ERPFacade.API.FunctionalTests.FunctionalTests
{
    
    [SetUpFixture]
    internal class FTSetupClass
    {
        
        [OneTimeSetUp]
        public void Setup()
        {
            Config.ConfigSetup();
        }
    }
}
