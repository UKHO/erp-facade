using System;
using System.Collections.Generic;

using FakeItEasy;
using Microsoft.Extensions.Logging;
using NUnit.Framework;
using UKHO.ERPFacade.Common.Permit_Decryption;
using Microsoft.Extensions.Configuration;
using System.IO;
using System.Xml;
using UKHO.ERPFacade.Common.Infrastructure;
using FluentAssertions;
using WireMock.Org.Abstractions;
using UKHO.ERPFacade.Common.Exceptions;

namespace UKHO.ERPFacade.Common.UnitTests.Permit_Decryption
{
    [TestFixture]
    public class PermitDecryptionTests
    {
        private ILogger<PermitDecryption> _fakeLogger;
        private  IConfiguration _config;
        private PermitDecryption _fakePermitDecryption;

        [SetUp]
        public void Setup()
        {
            _fakeLogger = A.Fake<ILogger<PermitDecryption>>();
            _config = InitConfiguration();
            _fakePermitDecryption = new PermitDecryption(_config,_fakeLogger);
        }

        private IConfiguration InitConfiguration()
        {
            var builder = new ConfigurationBuilder();
            builder.AddJsonFile("./appsettings.json");
            return builder.Build();
        }

        [Test]
        public void GetPermitKeysTest()
        {
           var keys= _fakePermitDecryption.GetPermitKeys("GB10016020240301366373A1A297FB3C80CD3130D2AF5D27516F45D960CAD9EB");

            keys.ActiveKey.Should().Be("D9F7832D87");
            keys.NextKey.Should().Be("D9F7832D88");
        }

        [Test]
        public void GetPermitKeysWithIncorrectPermitStringTest()
        {
            Assert.Throws<ERPFacadeException>(() => _fakePermitDecryption.GetPermitKeys("GB"));
        }

        [Test]
        public void GetPermitKeysWithEmptyPermitStringTest()
        {
            var keys = _fakePermitDecryption.GetPermitKeys("");
            keys.Should().Be(null);
           
        }
    }
}
