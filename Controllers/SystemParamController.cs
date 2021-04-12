using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ChatAPI.Helpers;
using ChatAPI.Models.DTO.SystemParam;
using ChatAPI.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace ChatAPI.Controllers
{
    [ApiController]
    public class SystemParamController : ControllerBase
    {
        ISystemParamService paramService;
        private readonly ILogger<SystemParamController> _logger;
        public SystemParamController(ISystemParamService _paramService, ILogger<SystemParamController> logger)
        {
            paramService = _paramService;
            _logger = logger;
            _logger.LogInformation("Test controller");
        }

        [Route("api/systemparam/getall")]
        [APIAuthorized("systemparam_view")]
        [HttpGet]
        public async Task<ActionResult<SystemParamListResultDTO>> GetAsync(int Page, int PageSize)
        {
            _logger.LogInformation("Get danh sách");
            return await paramService.GetAsync(Page, PageSize);
        }

        [Route("api/systemparam/getlist")]
        [HttpPost]
        [APIAuthorized("systemparam_view")]
        public async Task<ActionResult<SystemParamListResultDTO>> GetAsync(SystemParamFilterDTO _filter)
        {
            _logger.LogInformation("Get danh sách");
            return await paramService.GetAsync(_filter);
        }

        [Route("api/systemparam/getbyid")]
        [HttpGet]
        [APIAuthorized("systemparam_view")]
        public async Task<ActionResult<SystemParamDTO>> GetAsync(string id)
        {
            _logger.LogInformation("Get 1 bản ghi");
            return await paramService.GetAsync(id);
        }

        [Route("api/systemparam/Create")]
        [HttpPost]
        [APIAuthorized("systemparam_create")]
        public async Task<ActionResult<SystemParamResultDTO>> CreateAsync(SystemParamDTO book)
        {
            return await paramService.CreateAsync(book);
        }

        [Route("api/systemparam/Update")]
        [HttpPost]
        [APIAuthorized("systemparam_update")]
        public async Task<ActionResult<SystemParamResultDTO>> UpdateAsync(string id, SystemParamDTO book)
        {
            return await paramService.UpdateAsync(id, book);
        }

        [Route("api/systemparam/Removebyid")]
        [HttpPost]
        [APIAuthorized("systemparam_remove")]
        public async Task<ActionResult<SystemParamResultDTO>> RemoveAsync(string id)
        {
            return await paramService.RemoveAsync(id);
        }

        [Route("api/systemparam/Remove")]
        [HttpPost]
        [APIAuthorized("systemparam_remove")]
        public async Task<ActionResult<SystemParamResultDTO>> RemoveAsync(SystemParamDTO book)
        {
            return await paramService.RemoveAsync( book);
        }
    }
}
