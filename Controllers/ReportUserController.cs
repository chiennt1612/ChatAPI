using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ChatAPI.Helpers;
using ChatAPI.Models.DTO.ReportUser;
using ChatAPI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace ChatAPI.Controllers
{
    [ApiController]
    public class ReportUserController : Controller
    {
        IReportUserService blacklist;
        private readonly ILogger<ReportUserController> _logger;
        public ReportUserController(IReportUserService _blacklist, ILogger<ReportUserController> logger)
        {
            blacklist = _blacklist;
            _logger = logger;
            _logger.LogInformation("Test controller");
        }

        [Route("api/reportuser/getall")]
        [APIAuthorized(("reportuser_view"))]
        [HttpGet]
        public async Task<ActionResult<ReportUserListResultDTO>> GetAsync(int Page, int PageSize)
        {
            _logger.LogInformation("Get danh sách");
            return await blacklist.GetAsync(Page, PageSize);
        }

        [Route("api/reportuser/getlist")]
        [HttpPost]
        [APIAuthorized("reportuser_view")]
        public async Task<ActionResult<ReportUserListResultDTO>> GetAsync(ReportUserFilterDTO _filter)
        {
            _logger.LogInformation("Get danh sách");
            return await blacklist.GetAsync(_filter);
        }

        [Route("api/reportuser/getbyid")]
        [HttpGet]
        [APIAuthorized("reportuser_view")]
        public async Task<ActionResult<ReportUserDTO>> GetAsync(string id)
        {
            _logger.LogInformation("Get 1 bản ghi");
            return await blacklist.GetAsync(id);
        }

        [Route("api/reportuser/Create")]
        [HttpPost]
        [APIAuthorized("reportuser_create")]
        public async Task<ActionResult<ReportUserResultDTO>> CreateAsync(ReportUserDTO book)
        {
            return await blacklist.CreateAsync(book);
        }

        [Route("api/reportuser/Update")]
        [HttpPost]
        [APIAuthorized("reportuser_update")]
        public async Task<ActionResult<ReportUserResultDTO>> UpdateAsync(string id, ReportUserDTO book)
        {
            return await blacklist.UpdateAsync(id, book);
        }

        [Route("api/reportuser/Removebyid")]
        [HttpPost]
        [APIAuthorized("reportuser_remove")]
        public async Task<ActionResult<ReportUserResultDTO>> RemoveAsync(string id)
        {
            return await blacklist.RemoveAsync(id);
        }

        [Route("api/reportuser/Remove")]
        [HttpPost]
        [APIAuthorized("reportuser_remove")]
        public async Task<ActionResult<ReportUserResultDTO>> RemoveAsync(ReportUserDTO book)
        {
            return await blacklist.RemoveAsync(book);
        }
    }
}
