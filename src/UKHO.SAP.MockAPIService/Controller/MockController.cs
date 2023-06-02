using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Mvc;
using UKHO.SAP.MockAPIService.Enums;
using UKHO.SAP.MockAPIService.Services;

namespace UKHO.SAP.MockAPIService.Controller
{
    [ExcludeFromCodeCoverage]
    [Route("api/[controller]")]
    [ApiController]
    public class MockController : ControllerBase
    {
        private readonly MockService _mockService;

        public MockController(MockService mockService)
        {
            _mockService = mockService;
        }

        [HttpPost]
        [Route("/api/ConfigureTestCase/{testCase}")]
        public virtual async Task<IActionResult> ConfigureTestCase(TestCase testCase)
        {
            _mockService.UpdateTestCase(testCase.ToString());

            await Task.CompletedTask;
            return new OkObjectResult(StatusCodes.Status200OK);
        }

        [HttpGet]
        [Route("/api/ConfigureTestCase")]
        public virtual async Task<IActionResult> GetCurrentTestCase()
        {
            string currentTestCase = _mockService.GetCurrentTestCase();

            await Task.CompletedTask;
            return new OkObjectResult(currentTestCase);
        }
    }
}