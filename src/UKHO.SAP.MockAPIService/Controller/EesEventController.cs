using System.Text;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UKHO.ERPFacade.Common.Infrastructure.EventService.EventProvider;
using UKHO.ERPFacade.Common.IO.Azure;
using UKHO.ERPFacade.MockAPIService.Models;
using UKHO.SAP.MockAPIService.Enums;
using UKHO.SAP.MockAPIService.Services;

namespace UKHO.SAP.MockAPIService.Controller
{
    [Route("api/[controller]")]
    [ApiController]
    public class EesEventController : ControllerBase
    {
        private readonly IAzureBlobEventWriter _azureBlobEventWriter;
        private readonly IConfiguration _configuration;
        private readonly MockService _mockService;

        private const string TraceIdKey = "data.traceId";
        private const string RequestFormat = "json";

        public EesEventController(IAzureBlobEventWriter azureBlobEventWriter, IConfiguration configuration, MockService mockService)
        {
            _azureBlobEventWriter = azureBlobEventWriter;
            _mockService = mockService;
            _configuration = configuration;
        }

        [HttpPost]
        [Route("/api/events")]
        public virtual async Task<IActionResult> Post()
        {
            using var stream = new MemoryStream();
            await Request.Body.CopyToAsync(stream);
            var byteArray = stream.ToArray();

            var unitOfSalePriceEvent = JsonConvert.DeserializeObject<CloudEvent<UnitOfSalePriceEvent>>(Encoding.UTF8.GetString(byteArray));

            JObject unitsOfSaleUpdatedEventPayloadJson = JObject.Parse(JsonConvert.SerializeObject(unitOfSalePriceEvent));
            JObject encEventPayloadJson = JObject.Parse(JsonConvert.SerializeObject(unitOfSalePriceEvent.Data));

            var traceId = encEventPayloadJson.SelectToken(TraceIdKey)?.Value<string>();

            await _azureBlobEventWriter.UploadEvent(unitsOfSaleUpdatedEventPayloadJson.ToString(), traceId!, traceId + "_ees." + RequestFormat);

            if (bool.Parse(_configuration["IsFTRunning"]))
            {
                string currentTestCase = _mockService.GetCurrentTestCase();

                if (currentTestCase == TestCase.EESInternalServerError401.ToString())
                {
                    _mockService.CleanUp();
                    return new UnauthorizedObjectResult(StatusCodes.Status401Unauthorized);
                }
            }

            return new OkObjectResult(StatusCodes.Status200OK);
        }
    }
}