using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ChatAPI.Helpers;
using ChatAPI.Models.DTO.Message;
using ChatAPI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace ChatAPI.Controllers
{
    [ApiController]
    public class MessageController : Controller
    {
        IMessageService message;
        private readonly ILogger<MessageController> _logger;
        public MessageController(IMessageService _message, ILogger<MessageController> logger)
        {
            message = _message;
            _logger = logger;
            _logger.LogInformation("Test controller");
        }

        #region CMS-API
        [Route("api/message/getall")]
        [APIAuthorized(("message_view"))]
        [HttpGet]
        public async Task<ActionResult<MessageListResultDTO>> GetAsync(int Page, int PageSize)
        {
            _logger.LogInformation("Get danh sách");
            return await message.GetAsync(Page, PageSize);
        }

        [Route("api/message/getlist")]
        [HttpPost]
        [APIAuthorized("message_view")]
        public async Task<ActionResult<MessageListResultDTO>> GetAsync(MessageFilterDTO _filter)
        {
            _logger.LogInformation("Get danh sách");
            return await message.GetAsync(_filter);
        }

        [Route("api/message/getbyid")]
        [HttpGet]
        [APIAuthorized("message_view")]
        public async Task<ActionResult<MessageListDTO>> GetAsync(string id)
        {
            _logger.LogInformation("Get 1 bản ghi");
            return await message.GetAsync(id);
        }

        [Route("api/message/Create")]
        [HttpPost]
        [APIAuthorized("message_create")]
        public async Task<ActionResult<MessageResultDTO>> CreateAsync(MessageDTO book)
        {
            return await message.CreateAsync(book);
        }

        [Route("api/message/Update")]
        [HttpPost]
        [APIAuthorized("message_update")]
        public async Task<ActionResult<MessageResultDTO>> UpdateAsync(string id, MessageDTO book)
        {
            return await message.UpdateAsync(id, book);
        }

        [Route("api/message/Removebyid")]
        [HttpPost]
        [APIAuthorized("message_remove")]
        public async Task<ActionResult<MessageResultDTO>> RemoveAsync(string id)
        {
            return await message.RemoveAsync(id);
        }

        [Route("api/message/Remove")]
        [HttpPost]
        [APIAuthorized("message_remove")]
        public async Task<ActionResult<MessageResultDTO>> RemoveAsync(MessageDTO book)
        {
            return await message.RemoveAsync(book);
        }
        #endregion

        #region APP-API
        [Route("api/message/create")]
        [HttpPost]
        [APIAuthorized]
        public async Task<MsgResultDTO> CreateAsync(MsgDTO bookIn)
        {
            string Token = "abc"; string Username = "chiennt";
            return await message.CreateAsync(bookIn, Token, Username);
        }

        [Route("api/message/update")]
        [HttpPost]
        [APIAuthorized]
        public async Task<MsgResultDTO> UpdateAsync(MsgDTO bookIn)
        {
            string Token = "abc"; string Username = "chiennt";
            return await message.UpdateAsync(bookIn, Token, Username);
        }

        [Route("api/message/read")]
        [HttpPost]
        [APIAuthorized]
        public async Task<MessageReadResultDTO> MesssageReadAsync(MessageReadDTO bookIn)
        {
            string Token = "abc"; string Username = "chiennt";
            return await message.MesssageReadAsync(bookIn, Token, Username);
        }

        [Route("api/message/delete")]
        [HttpPost]
        [APIAuthorized]
        public async Task<MessageDeleteResultDTO> MesssageDeleteAsync(MessageReadDTO bookIn)
        {
            string Token = "abc"; string Username = "chiennt";
            return await message.MesssageDeleteAsync(bookIn, Token, Username);
        }

        [Route("api/message/recall")]
        [HttpPost]
        [APIAuthorized]
        public async Task<MessageRecallResultDTO> MesssageRecallAsync(MessageReadDTO bookIn)
        {
            string Token = "abc"; string Username = "chiennt";
            return await message.MesssageRecallAsync(bookIn, Token, Username);
        }

        [Route("api/message/search")]
        [HttpPost]
        [APIAuthorized]
        public async Task<MessageListResultDTO> SearchMesssageAsync(MessageSearchDTO _filter)
        {
            string Token = "abc"; string Username = "chiennt";
            return await message.SearchMesssageAsync(_filter, Token, Username);
        }

        [Route("api/message/unread")]
        [HttpPost]
        [APIAuthorized]
        public async Task<MessageListResultDTO> UnreadMesssageAsync(MessageUnreadDTO _filter)
        {
            string Token = "abc"; string Username = "chiennt";
            return await message.UnreadMesssageAsync(_filter, Token, Username);
        }
        #endregion
    }
    }

