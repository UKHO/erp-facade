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
using UKHO.ERPFacade.API.Helpers;
using UKHO.ERPFacade.API.Models;
using UKHO.ERPFacade.Common.IO;

namespace UKHO.ERPFacade.API.UnitTests.Helpers
{
    [TestFixture]
    public class SapMessageBuilderTests
    {
        private ILogger<SapMessageBuilder> _fakeLogger;
        private IXmlHelper _fakeXmlHelper;
        private IFileSystemHelper _fakeFileSystemHelper;
        private IOptions<SapActionConfiguration> _fakeSapActionConfig;
        private IOptions<ActionNumberConfiguration> _fakeActionNumberConfig;

        private SapMessageBuilder _fakeSapMessageBuilder;

        [SetUp]
        public void Setup()
        {
            _fakeLogger = A.Fake<ILogger<SapMessageBuilder>>();
            _fakeXmlHelper = A.Fake<IXmlHelper>();
            _fakeFileSystemHelper = A.Fake<IFileSystemHelper>();
            _fakeSapActionConfig = Options.Create(InitConfiguration().GetSection("SapActionConfiguration").Get<SapActionConfiguration>())!;
            _fakeActionNumberConfig = Options.Create(InitConfiguration().GetSection("ActionNumberConfiguration").Get<ActionNumberConfiguration>())!;
            _fakeSapMessageBuilder = new SapMessageBuilder(_fakeLogger, _fakeXmlHelper, _fakeFileSystemHelper, _fakeSapActionConfig, _fakeActionNumberConfig);
        }

        private IConfiguration InitConfiguration()
        {
            var config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory() + @"/ConfigurationFiles")
                .AddJsonFile("ActionNumbers.json")
                .AddJsonFile("SapActions.json")
                .AddEnvironmentVariables()
                .Build();

            return config;
        }

        [Test]
        public void Test1()
        {
            Assert.Pass();
        }
    }
}
