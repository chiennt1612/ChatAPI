using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ChatAPI.Helpers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace ChatAPI.Controllers
{
    [ApiController]
    public class TokenController : ControllerBase
    {
        ITokensJwt tokensJwt;
        private readonly ILogger<TokenController> _logger;
        public TokenController (ITokensJwt _tokensJwt, ILogger<TokenController> logger)
        {
            tokensJwt = _tokensJwt;
            _logger = logger;
        }

        [Route("api/token/generate")]
        [HttpPost]
        public ActionResult<UserToken> GenerateToken([FromBody] UserModel user)
        {
            _logger.LogInformation("GenerateToken");
            return new UserToken() { 
                UserId = user.UserId,
                Username = user.Username,
                EmailAddress = user.EmailAddress,
                Token = tokensJwt.GenerateToken(user)
            };
        }

        [Route("api/token/validate")]
        [HttpGet]
        public ActionResult<UserToken> ValidateToken(string Token)
        {
            _logger.LogInformation("ValidateToken");
            return tokensJwt.ValidateToken(Token);
        }

        [Route("api/token/generatersa")]
        [HttpPost]
        public async Task<ActionResult<UserToken>> GenerateTokenRSAAsync([FromBody] UserModel user)
        {
            _logger.LogInformation("GenerateTokenRSA");
            return new UserToken()
            {
                UserId = user.UserId,
                Username = user.Username,
                EmailAddress = user.EmailAddress,
                Token = await tokensJwt.GenerateTokenRSAAsync(user)
            };
        }

        [Route("api/token/validatersa")]
        [HttpGet]
        public async Task<ActionResult<UserToken>> ValidateTokenRSAAsync(string Token)
        {
            _logger.LogInformation("ValidateTokenRSA");
            return await tokensJwt.ValidateTokenRSAAsync(Token);
        }
    }    
}
