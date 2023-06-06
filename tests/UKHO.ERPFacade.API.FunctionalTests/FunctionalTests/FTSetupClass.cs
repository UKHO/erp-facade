using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
