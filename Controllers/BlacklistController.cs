using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ChatAPI.Helpers;
using ChatAPI.Models.DTO.Blacklist;
using ChatAPI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace ChatAPI.Controllers
{
    [ApiController]
    public class BlacklistController : Controller
    {
        IBlacklistService blacklist;
        private readonly ILogger<BlacklistController> _logger;
        public BlacklistController(IBlacklistService _blacklist, ILogger<BlacklistController> logger)
        {
            blacklist = _blacklist;
            _logger = logger;
            _logger.LogInformation("Test controller");
        }

        [Route("api/blacklist/getall")]
        [APIAuthorized(("blacklist_view"))]
        [HttpGet]
        public async Task<ActionResult<BlacklistListResultDTO>> GetAsync(int Page, int PageSize)
        {
            _logger.LogInformation("Get danh sách");
            return await blacklist.GetAsync(Page, PageSize);
        }

        [Route("api/blacklist/getlist")]
        [HttpPost]
        [APIAuthorized("blacklist_view")]
        public async Task<ActionResult<BlacklistListResultDTO>> GetAsync(BlacklistFilterDTO _filter)
        {
            _logger.LogInformation("Get danh sách");
            return await blacklist.GetAsync(_filter);
        }

        [Route("api/blacklist/getbyid")]
        [HttpGet]
        [APIAuthorized("blacklist_view")]
        public async Task<ActionResult<BlacklistDTO>> GetAsync(string id)
        {
            _logger.LogInformation("Get 1 bản ghi");
            return await blacklist.GetAsync(id);
        }

        [Route("api/blacklist/Create")]
        [HttpPost]
        [APIAuthorized("blacklist_create")]
        public async Task<ActionResult<BlacklistResultDTO>> CreateAsync(BlacklistDTO book)
        {
            return await blacklist.CreateAsync(book);
        }

        [Route("api/blacklist/Update")]
        [HttpPost]
        [APIAuthorized("blacklist_update")]
        public async Task<ActionResult<BlacklistResultDTO>> UpdateAsync(string id, BlacklistDTO book)
        {
            return await blacklist.UpdateAsync(id, book);
        }

        [Route("api/blacklist/Removebyid")]
        [HttpPost]
        [APIAuthorized("blacklist_remove")]
        public async Task<ActionResult<BlacklistResultDTO>> RemoveAsync(string id)
        {
            return await blacklist.RemoveAsync(id);
        }

        [Route("api/blacklist/Remove")]
        [HttpPost]
        [APIAuthorized("blacklist_remove")]
        public async Task<ActionResult<BlacklistResultDTO>> RemoveAsync(BlacklistDTO book)
        {
            return await blacklist.RemoveAsync(book);
        }
    }
}
