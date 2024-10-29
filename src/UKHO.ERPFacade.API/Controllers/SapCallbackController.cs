﻿using Microsoft.AspNetCore.Mvc;

namespace UKHO.ERPFacade.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SapCallbackController : BaseController<SapCallbackController>
    {
        public SapCallbackController(IHttpContextAccessor contextAccessor) : base(contextAccessor)
        {
        }
    }
}