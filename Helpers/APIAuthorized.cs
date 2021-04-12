using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace ChatAPI.Helpers
{
    public class APIAuthorized : ActionFilterAttribute
    {
        string functionName;
        public APIAuthorized(string function = "")
        {
            functionName = function;
        }
        public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            if (!CheckAuthen(context.HttpContext))
            {
                context.Result = new JsonResult(new { HttpStatusCode.Unauthorized });
            }
            else
            {
                // next() calls the action method.  
                var resultContext = await next();
            }
        }

        private bool CheckAuthen(HttpContext context)
        {
            string authHeader = context.Request.Headers["Authorization"];
            if (CheckLoginWithToken(context, authHeader)) return true; else return false;
        }

        private bool CheckLoginWithToken(HttpContext context, string Token)
        {
            var jwt = context.RequestServices.GetService(typeof(ITokensJwt)) as TokensJwt;
            var _token = jwt.ValidateToken(Token);
            if (_token == null)
                return false;
            return true;
        }
    }
}
