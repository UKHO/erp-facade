using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using UKHO.ERPFacade.API.Controllers;

namespace UKHO.ERPFacade.API.UnitTests.Controllers
{
    public class WebhookControllerTests
    {
        private WebhookController privateWebhookController;

        [SetUp]
        public void Setup()
        {
            privateWebhookController = new WebhookController();
        }

        [Test]
        public void TestDoesWebhookReturns200OkResponseWhenValidHeaderRequestedInEncContentPublishedOptions()
        {
            //Arrange
            var webHookRequestOrigin = "http://localhost";
            var httpContext = new DefaultHttpContext();
            httpContext.Request.Headers["WebHook-Request-Origin"] = webHookRequestOrigin;
            privateWebhookController.ControllerContext.HttpContext = httpContext;

            // Act
            var result = privateWebhookController.EncContentPublishedOptions() as OkObjectResult;

            // Assert
            Assert.AreEqual(StatusCodes.Status200OK, result.StatusCode);
            Assert.AreEqual("*", privateWebhookController.Response.Headers["WebHook-Allowed-Rate"].ToString());
            Assert.AreEqual(webHookRequestOrigin, privateWebhookController.Response.Headers["WebHook-Allowed-Origin"].ToString());
        }

        [Test]
        public async Task TestDoesWebhookReturns200OkResponseWhenValidInputInEncContentPublished()
        {
            //Arrange
            var httpContext = new DefaultHttpContext();
            privateWebhookController.ControllerContext.HttpContext = httpContext;

            // Act
            var result = await privateWebhookController.EncContentPublished(new JObject()) as OkObjectResult;

            //Assert
            Assert.AreEqual(StatusCodes.Status200OK, result.StatusCode);
        }
    }
}