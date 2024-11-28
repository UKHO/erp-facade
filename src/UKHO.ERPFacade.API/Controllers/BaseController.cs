using Microsoft.AspNetCore.Mvc;
using System.Diagnostics.CodeAnalysis;
using UKHO.ERPFacade.API.Filters;
using UKHO.ERPFacade.Common.Models;

namespace UKHO.ERPFacade.API.Controllers
{
    [ExcludeFromCodeCoverage]    
    public abstract class BaseController<T> : ControllerBase
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        protected new HttpContext HttpContext => _httpContextAccessor.HttpContext!;

        protected BaseController(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        protected string GetCurrentCorrelationId()
        {
            return _httpContextAccessor.HttpContext!.Request.Headers[CorrelationIdMiddleware.XCorrelationIdHeaderKey].FirstOrDefault()!;
        }

        protected IActionResult BuildBadRequestErrorResponse(List<Error> errors)
        {
            return new BadRequestObjectResult(new ErrorDescription
            {
                CorrelationId = GetCurrentCorrelationId(),
                Errors = errors,
            });
        }
    }
}
