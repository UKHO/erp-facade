using FakeItEasy;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UKHO.ERPFacade.API.Controllers;
using UKHO.ERPFacade.API.Helpers;
using UKHO.ERPFacade.API.Models;

namespace UKHO.ERPFacade.API.UnitTests.Helpers
{
    [TestFixture]
    public class ScenarioBuilderTests
    {
        private ILogger<ScenarioBuilder> _fakeLogger;
        private IOptions<ScenarioRuleConfiguration> _fakeScenarioRuleConfig;
        private ScenarioBuilder _fakeScenarioBuilder;


        [SetUp]
        public void Setup()
        {
            _fakeLogger = A.Fake<ILogger<ScenarioBuilder>>();
            _fakeScenarioRuleConfig = Options.Create(InitConfiguration().GetSection("ScenarioRuleConfiguration").Get<ScenarioRuleConfiguration>())!;
            _fakeScenarioBuilder = new ScenarioBuilder(_fakeLogger, _fakeScenarioRuleConfig);
        }

        private IConfiguration InitConfiguration()
        {
            var config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory()+ @"/ConfigurationFiles")
               .AddJsonFile("ScenarioRules.json")
                .AddEnvironmentVariables()
                .Build();

            return config;
        }

    }
}
