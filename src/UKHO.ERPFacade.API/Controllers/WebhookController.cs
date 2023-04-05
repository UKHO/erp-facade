using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using UKHO.ERPFacade.Common.Helpers;

namespace UKHO.ERPFacade.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class WebhookController : BaseController<WebhookController>
    {
        private readonly ILogger<WebhookController> _logger;
        private readonly IAzureTableStorageHelper _azureTableStorageHelper;
        private readonly IAzureBlobStorageHelper _azureBlobStorageHelper;

        public WebhookController(IHttpContextAccessor contextAccessor,
                                 ILogger<WebhookController> logger,
                                 IAzureTableStorageHelper azureTableStorageHelper,
                                 IAzureBlobStorageHelper azureBlobStorageHelper)
        : base(contextAccessor)
        {
            _logger = logger;
            _azureTableStorageHelper = azureTableStorageHelper;
            _azureBlobStorageHelper = azureBlobStorageHelper;
        }

        [HttpPost]
        [Route("/webhook/newenccontentpublishedeventreceived")]
        public virtual async Task<IActionResult> NewEncContentPublishedEventReceived([FromBody] JObject requestJson)
        {
            await _azureTableStorageHelper.UpsertEntity(requestJson, requestJson.SelectToken("data.traceId").Value<string>());
            await _azureBlobStorageHelper.UploadEvent(requestJson, requestJson.SelectToken("data.traceId").Value<string>());

            return new OkObjectResult(StatusCodes.Status200OK);
        }
    }
}
