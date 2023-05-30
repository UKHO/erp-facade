﻿using Azure.Storage;
using FluentAssertions;
using Microsoft.Extensions.Configuration;

using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UKHO.ERPFacade.API.FunctionalTests.Helpers;
using UKHO.ERPFacade.API.FunctionalTests.Model;

namespace UKHO.ERPFacade.API.FunctionalTests.FunctionalTests
{
    [TestFixture]
    public class UnitOfSaleScenarios
    {

        private UnitOfSaleEndpoint _unitOfSale { get; set; }
        public static Config config;
        private readonly ADAuthTokenProvider _authToken = new();
        public static bool noRole = false;
        //for pipeline
        //private readonly string _projectDir = Path.GetFullPath(Path.Combine(Environment.CurrentDirectory));
        //for local
        private readonly string _projectDir = Path.GetFullPath(Path.Combine(Environment.CurrentDirectory, "..\\..\\.."));

        [OneTimeSetUp]
        public void Setup()
        {
            config = new();
            _unitOfSale = new UnitOfSaleEndpoint(config.TestConfig.ErpFacadeConfiguration.BaseUrl);
        }
       
        [Test(Description = "WhenValidEventReceivedWithValidToken_ThenUoSReturns200OkResponse"), Order(0)]
        public async Task WhenValidEventReceivedWithValidToken_ThenUoSReturns200OkResponse()
        {
            string filePath = Path.Combine(_projectDir, config.TestConfig.PayloadFolder, config.TestConfig.UoSPayloadFileName);            
            var response = await _unitOfSale.PostUoSResponseAsync(filePath, await _authToken.GetAzureADToken(false, "UnitOfSale"));
           response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);

        }
        [Test(Description = "WhenValidEventReceivedWithInvalidToken_ThenUoSReturns401UnAuthorizedResponse"), Order(1)]
        public async Task WhenValidEventReceivedWithInvalidToken_ThenUoSReturns401UnAuthorizedResponse()
        {
            string filePath = Path.Combine(_projectDir, config.TestConfig.PayloadFolder, config.TestConfig.UoSPayloadFileName);

            var response = await _unitOfSale.PostUoSResponseAsync(filePath, "invalidToken_abcd");
            response.StatusCode.Should().Be(System.Net.HttpStatusCode.Unauthorized);

        }
        [Test(Description = "WhenValidEventReceivedWithTokenHavingNoRole_ThenUoSReturns403ForbiddenResponse"), Order(2)]
        public async Task WhenValidEventReceivedWithInvalidToken_ThenUoSReturns403UnAuthorizedResponse()
        {
            string filePath = Path.Combine(_projectDir, config.TestConfig.PayloadFolder, config.TestConfig.UoSPayloadFileName);

            var response = await _unitOfSale.PostUoSResponseAsync(filePath, await _authToken.GetAzureADToken(true));
            response.StatusCode.Should().Be(System.Net.HttpStatusCode.Forbidden);

        }

    }
}
