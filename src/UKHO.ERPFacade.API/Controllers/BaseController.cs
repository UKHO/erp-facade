using Microsoft.AspNetCore.Mvc;
using System.Diagnostics.CodeAnalysis;
using UKHO.ERPFacade.API.Filters;

namespace UKHO.ERPFacade.API.Controllers
{
    [ExcludeFromCodeCoverage]
    [ApiController]
    public abstract class BaseController<T> : ControllerBase
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        protected new HttpContext HttpContext => _httpContextAccessor.HttpContext!;
        protected readonly ILogger<T> _logger;
        public const string InternalServerError = "Internal Server Error";

        protected BaseController(IHttpContextAccessor httpContextAccessor, ILogger<T> logger)
        {
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
        }

        protected string GetCurrentCorrelationId()
        {
            return _httpContextAccessor.HttpContext!.Request.Headers[CorrelationIdMiddleware.XCorrelationIdHeaderKey].FirstOrDefault()!;
        }
    }
}
