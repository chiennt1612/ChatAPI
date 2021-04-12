using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Net.WebSockets;
using System.Threading.Tasks;
using ChatAPI.Helpers;
using IdentityModel.Client;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ChatAPI.Controllers
{
    public class HomeController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IDiscoveryCache _discoveryCache;

        public HomeController(IHttpClientFactory httpClientFactory, IDiscoveryCache discoveryCache)
        {
            _httpClientFactory = httpClientFactory;
            _discoveryCache = discoveryCache;
        }

        [AllowAnonymous]
        public IActionResult Index() => View();

        public IActionResult Secure() => View();

        public async Task<IActionResult> Chat()
        {
            var a = new UserToken();
            foreach (var claim in User.Claims)
            {
                if (claim.Type == "email")
                    a.EmailAddress = claim.Value;
                else if (claim.Type == "sub")
                    a.UserId = claim.Value;
                else if (claim.Type == "name")
                    a.Username = claim.Value;
            }
            foreach (var prop in (await HttpContext.AuthenticateAsync()).Properties.Items)
            {
                if (prop.Key == ".Token.access_token")
                {
                    a.Token = prop.Value;
                }
            }
            return View("Chat", a);
        } 

        public IActionResult Logout() => SignOut("oidc");

        public async Task<IActionResult> RenewTokens()
        {
            var disco = await _discoveryCache.GetAsync();
            if (disco.IsError) throw new Exception(disco.Error);

            var rt = await HttpContext.GetTokenAsync("refresh_token");
            var tokenClient = _httpClientFactory.CreateClient();

            var tokenResult = await tokenClient.RequestRefreshTokenAsync(new RefreshTokenRequest
            {
                Address = disco.TokenEndpoint,

                ClientId = "chat.api",
                ClientSecret = "secret.chat.api",
                RefreshToken = rt
            });

            if (!tokenResult.IsError)
            {
                var oldIdToken = await HttpContext.GetTokenAsync("id_token");
                var newAccessToken = tokenResult.AccessToken;
                var newRefreshToken = tokenResult.RefreshToken;
                var expiresAt = DateTime.UtcNow + TimeSpan.FromSeconds(tokenResult.ExpiresIn);

                var info = await HttpContext.AuthenticateAsync("Cookies");

                info.Properties.UpdateTokenValue("refresh_token", newRefreshToken);
                info.Properties.UpdateTokenValue("access_token", newAccessToken);
                info.Properties.UpdateTokenValue("expires_at", expiresAt.ToString("o", CultureInfo.InvariantCulture));

                await HttpContext.SignInAsync("Cookies", info.Principal, info.Properties);
                return Redirect("~/Home/Secure");
            }

            ViewData["Error"] = tokenResult.Error;
            return View("Error");
        }
    }
}
