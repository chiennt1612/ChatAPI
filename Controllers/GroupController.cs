using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ChatAPI.Helpers;
using ChatAPI.Models.DTO.Groupchat;
using ChatAPI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace ChatAPI.Controllers
{
    [ApiController]
    public class GroupController : Controller
    {
        IGroupService blacklist;
        private readonly ILogger<GroupController> _logger;
        public GroupController(IGroupService _blacklist, ILogger<GroupController> logger)
        {
            blacklist = _blacklist;
            _logger = logger;
            _logger.LogInformation("Test controller");
        }

        #region CMS-API
        [Route("api/groupchat/getall")]
        [APIAuthorized("groupchat_view")]
        [HttpGet]
        public async Task<ActionResult<GroupListResultDTO>> GetAsync(int Page, int PageSize)
        {
            _logger.LogInformation("Get danh sách");
            return await blacklist.GetAsync(Page, PageSize);
        }

        [Route("api/groupchat/getlist")]
        [HttpPost]
        [APIAuthorized("groupchat_view")]
        public async Task<ActionResult<GroupListResultDTO>> GetAsync(GroupFilterDTO _filter)
        {
            _logger.LogInformation("Get danh sách");
            return await blacklist.GetAsync(_filter);
        }

        [Route("api/groupchat/getbyid")]
        [HttpGet]
        [APIAuthorized("groupchat_view")]
        public async Task<ActionResult<GroupDTO>> GetAsync(string id)
        {
            _logger.LogInformation("Get 1 bản ghi");
            return await blacklist.GetAsync(id);
        }

        [Route("api/groupchat/Create")]
        [HttpPost]
        [APIAuthorized("groupchat_create")]
        public async Task<ActionResult<GroupResultDTO>> CreateAsync(GroupDTO book)
        {
            return await blacklist.CreateAsync(book);
        }

        [Route("api/groupchat/Update")]
        [HttpPost]
        [APIAuthorized("groupchat_update")]
        public async Task<ActionResult<GroupResultDTO>> UpdateAsync(string id, GroupDTO book)
        {
            return await blacklist.UpdateAsync(id, book);
        }

        [Route("api/groupchat/Removebyid")]
        [HttpPost]
        [APIAuthorized("groupchat_remove")]
        public async Task<ActionResult<GroupResultDTO>> RemoveAsync(string id)
        {
            return await blacklist.RemoveAsync(id);
        }

        [Route("api/groupchat/Remove")]
        [HttpPost]
        [APIAuthorized("groupchat_remove")]
        public async Task<ActionResult<GroupResultDTO>> RemoveAsync(GroupDTO book)
        {
            return await blacklist.RemoveAsync(book);
        }
        #endregion

        #region APP-API
        [Route("api/groupchat/creategroup2")]
        [HttpPost]
        [APIAuthorized]
        public async Task<ActionResult<GroupResultDTO>> CreateChat(Groupchat2DTO bookIn)
        {
            string Token = "abc"; string Username = "chiennt";
            return await blacklist.CreateChat(bookIn, Token, Username);
        }

        [Route("api/groupchat/creategroup")]
        [HttpPost]
        [APIAuthorized]
        public async Task<ActionResult<GroupResultDTO>> CreateGroupchat(GroupchatDTO bookIn)
        {
            string Token = "abc"; string Username = "chiennt";
            return await blacklist.CreateGroupchat(bookIn, Token, Username);
        }

        [Route("api/groupchat/searchgroup")]
        [HttpPost]
        [APIAuthorized]
        public async Task<ActionResult<GroupListResultDTO>> SearchGroupAsync(GroupSearchDTO _filter)
        {
            string Token = "abc"; string Username = "chiennt";
            return await blacklist.SearchGroupAsync(_filter, Token, Username);
        }

        [Route("api/groupchat/joingroup")]
        [HttpPost]
        [APIAuthorized]
        public async Task<ActionResult<GroupJoinResultDTO>> JoinGroupAsync(GroupJoinDTO bookIn)
        {
            string Token = "abc"; string Username = "chiennt";
            return await blacklist.JoinGroupAsync(bookIn, Token, Username);
        }

        [Route("api/groupchat/removegroup")]
        [HttpPost]
        [APIAuthorized]
        public async Task<ActionResult<GroupJoinResultDTO>> RemoveGroupAsync(GroupRemoveDTO bookIn)
        {
            string Token = "abc"; string Username = "chiennt";
            return await blacklist.RemoveGroupAsync(bookIn, Token, Username);
        }

        [Route("api/groupchat/leavegroup")]
        [HttpPost]
        [APIAuthorized]
        public async Task<ActionResult<GroupJoinResultDTO>> LeaveGroupAsync(GroupLeaveDTO bookIn)
        {
            string Token = "abc"; string Username = "chiennt";
            return await blacklist.LeaveGroupAsync(bookIn, Token, Username);
        }
        #endregion
    }
}
