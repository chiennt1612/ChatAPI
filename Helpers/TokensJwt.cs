using ChatAPI.Models;
using ChatAPI.Models.DTO.SocketObject;
using IdentityModel;
using IdentityModel.Client;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.Unicode;
using System.Threading.Tasks;

namespace ChatAPI.Helpers
{
    public static class TypeConverterExtension
    {
        public static byte[] ToByteArray(this string value) =>
               Convert.FromBase64String(value);

        public static string FromByteArray(this byte[] value) =>
               Convert.ToBase64String(value);
    }

    public class UserToken
    {
        public string UserId { get; set; }
        public string Username { get; set; }
        public string EmailAddress { get; set; }
        public string Token { get; set; }
    }
    public class UserModel
    {
        public string UserId { get; set; }
        public string Username { get; set; }
        public string EmailAddress { get; set; }
        public string Password { get; set; }
    }
    public class TokensJwt : ITokensJwt
    {
        IJwtBearer jwtBearer;
        //private RSA rsaEnc;
        //private RSA rsaDec;

        private async Task FileWrite(string path, byte[] key)
        {
            var f = File.Create(path);
            var buffer = Encoding.UTF8.GetBytes(key.FromByteArray());
            await f.WriteAsync(buffer, 0, buffer.Length);
            f.Close();
        }
        private async Task<byte[]> GetKey(bool IsPrivateKey = true)
        {
            byte[] key = null;
            string path = Directory.GetCurrentDirectory() + "//" + (IsPrivateKey ? "private.pem" : "public.pem");
            try
            {
                if (File.Exists(path))
                {
                    key = (await File.ReadAllTextAsync(path)).ToByteArray();
                }
            }
            catch
            {
                key = null;
            }
            
            if (key == null)
            {
                var @base = new RSACryptoServiceProvider(4096);
                key = (IsPrivateKey ? @base.ExportRSAPrivateKey() : @base.ExportRSAPublicKey());

                path = Directory.GetCurrentDirectory() + "//private.pem";
                await FileWrite(path, @base.ExportRSAPrivateKey());

                path = Directory.GetCurrentDirectory() + "//public.pem";
                await FileWrite(path, @base.ExportRSAPublicKey());
            }
            return key;
        }
        public TokensJwt(IJwtBearer _jwtBearer)
        {
            jwtBearer = _jwtBearer;
            //rsaEnc = RSA.Create();
            //rsaDec = RSA.Create();
            //var @base = new RSACryptoServiceProvider(4096);
            //rsaEnc.ImportRSAPrivateKey(@base.ExportRSAPrivateKey(), out _);
            //rsaDec.ImportRSAPublicKey(@base.ExportRSAPublicKey(), out _);
        }
        public string GenerateToken(UserModel user)
        {
            var claims = new[] {
                new Claim(JwtRegisteredClaimNames.Sub, user.Username),
                new Claim(JwtRegisteredClaimNames.Email, user.EmailAddress),
                new Claim(JwtRegisteredClaimNames.Sid, user.UserId.ToString(), ClaimValueTypes.String),
                new Claim(JwtRegisteredClaimNames.Iat, DateTimeOffset.Now.ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };
            var now = DateTime.UtcNow;
            SigningCredentials signingCredentials = new SigningCredentials(new SymmetricSecurityKey(Encoding.ASCII.GetBytes(jwtBearer.SecurityKey)), SecurityAlgorithms.HmacSha256);
            var jwtSecurityToken = new JwtSecurityToken(
                issuer: jwtBearer.Issuer,
                audience: jwtBearer.Audience,
                claims: claims,
                notBefore: now,
                expires: now.AddDays(1),
                signingCredentials: signingCredentials
            );

            return String.Format("Bearer {0}", (new JwtSecurityTokenHandler().WriteToken(jwtSecurityToken)));// DecryptorProvider.Encrypt(new JwtSecurityTokenHandler().WriteToken(jwtSecurityToken), jwtBearer.SecurityKey));
        }
        public UserToken ValidateToken(string token)
        {
            try
            {
                string _token;
                if (token.StartsWith("Basic "))
                    _token = token.Substring("Basic ".Length);// DecryptorProvider.Decrypt(token.Substring("Basic ".Length), jwtBearer.SecurityKey);
                else if (token.StartsWith("Bearer "))
                    _token = token.Substring("Bearer ".Length); // DecryptorProvider.Decrypt(token.Substring("Bearer ".Length), jwtBearer.SecurityKey);
                else
                    _token = token;// DecryptorProvider.Decrypt(token, jwtBearer.SecurityKey);

                var tokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.ASCII.GetBytes(jwtBearer.SecurityKey);
                tokenHandler.ValidateToken(_token, new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),

                    ValidateIssuer = true,
                    ValidIssuer = jwtBearer.Issuer,

                    ValidateAudience = true,
                    ValidAudience = jwtBearer.Audience,

                    ValidateLifetime = true,
                    // set clockskew to zero so tokens expire exactly at token expiration time (instead of 5 minutes later)
                    ClockSkew = TimeSpan.Zero
                }, out SecurityToken validatedToken);

                var jwtToken = (JwtSecurityToken)validatedToken;
                var a = new UserToken()
                {
                    UserId = jwtToken.Claims.First(x => x.Type == JwtRegisteredClaimNames.Sid).Value,
                    Username = jwtToken.Claims.First(x => x.Type == JwtRegisteredClaimNames.Sub).Value,
                    EmailAddress = jwtToken.Claims.First(x => x.Type == JwtRegisteredClaimNames.Email).Value,
                    Token = token
                };

                // return account id from JWT token if validation successful
                return a;
            }
            catch (Exception ex)
            {
                // return null if validation fails
                return null;
            }
        }

        public async Task<string> GenerateTokenRSAAsync(UserModel user)
        {
            var claims = new[] {
                new Claim(JwtRegisteredClaimNames.Sub, user.Username),
                new Claim(JwtRegisteredClaimNames.Email, user.EmailAddress),
                new Claim(JwtRegisteredClaimNames.Sid, user.UserId.ToString(), ClaimValueTypes.String),
                new Claim(JwtRegisteredClaimNames.Iat, DateTimeOffset.Now.ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };
            var now = DateTime.UtcNow;
            var privateKey = await GetKey();
            RSA rsa = RSA.Create();
            rsa.ImportRSAPrivateKey(privateKey, out _);
            var signingCredentials = new SigningCredentials(new RsaSecurityKey(rsa), SecurityAlgorithms.RsaSha256)
            {
                CryptoProviderFactory = new CryptoProviderFactory { CacheSignatureProviders = false }
            };
            var jwtSecurityToken = new JwtSecurityToken(
                issuer: jwtBearer.Issuer,
                audience: jwtBearer.Audience,
                claims: claims,
                notBefore: now,
                expires: now.AddMinutes(30),
                signingCredentials: signingCredentials
            );

            return String.Format("Bearer {0}", (new JwtSecurityTokenHandler().WriteToken(jwtSecurityToken)));// DecryptorProvider.Encrypt(new JwtSecurityTokenHandler().WriteToken(jwtSecurityToken), jwtBearer.SecurityKey));
        }

        public async Task<UserToken> ValidateTokenRSAAsync(string token)
        {
            //var publicKey = await Task.Run(() =>
            //{
            //    return File.ReadAllText(Directory.GetCurrentDirectory() + "//public.pem").ToByteArray();
            //});

            try
            {
                var publicKey = await GetKey(false);
                RSA rsa = RSA.Create();
                rsa.ImportRSAPublicKey(publicKey, out _);

                string _token;
                if (token.StartsWith("Basic "))
                    _token = token.Substring("Basic ".Length);// DecryptorProvider.Decrypt(token.Substring("Basic ".Length), jwtBearer.SecurityKey);
                else if (token.StartsWith("Bearer "))
                    _token = token.Substring("Bearer ".Length); // DecryptorProvider.Decrypt(token.Substring("Bearer ".Length), jwtBearer.SecurityKey);
                else
                    _token = token;// DecryptorProvider.Decrypt(token, jwtBearer.SecurityKey);

                var tokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.ASCII.GetBytes(jwtBearer.SecurityKey);
                tokenHandler.ValidateToken(_token, new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new RsaSecurityKey(rsa),

                    ValidateIssuer = true,
                    ValidIssuer = jwtBearer.Issuer,

                    ValidateAudience = true,
                    ValidAudience = jwtBearer.Audience,

                    ValidateLifetime = true,
                    // set clockskew to zero so tokens expire exactly at token expiration time (instead of 5 minutes later)
                    ClockSkew = TimeSpan.Zero
                }, out SecurityToken validatedToken);

                var jwtToken = (JwtSecurityToken)validatedToken;
                var a = new UserToken()
                {
                    UserId = jwtToken.Claims.First(x => x.Type == JwtRegisteredClaimNames.Sid).Value,
                    Username = jwtToken.Claims.First(x => x.Type == JwtRegisteredClaimNames.Sub).Value,
                    EmailAddress = jwtToken.Claims.First(x => x.Type == JwtRegisteredClaimNames.Email).Value,
                    Token = token
                };

                // return account id from JWT token if validation successful
                return a;
            }
            catch (Exception ex)
            {
                // return null if validation fails
                return null;
            }
        }


        #region IdentityServerToken
        public async Task<LoginResultDTO> ValidateIdentityServerToken(LoginMessageDTO a)
        {
            try
            {
                var claims = await ValidateJwt(a.Token);

                if (claims.FindFirst("sub") == null && claims.FindFirst("sid") == null) throw new Exception("Invalid token");

                var nonce = claims.FindFirstValue("nonce");
                if (!String.IsNullOrWhiteSpace(nonce)) new LoginResultDTO() { ResponseStatus = false, Message = "LoginFail", Sender = a.Sender, Token = a.Token };

                var eventsJson = claims.FindFirst("events")?.Value;
                if (String.IsNullOrWhiteSpace(eventsJson)) new LoginResultDTO() { ResponseStatus = false, Message = "LoginFail", Sender = a.Sender, Token = a.Token };

                if (a.Sender != claims.FindFirst("sub").Value)
                    return new LoginResultDTO() { ResponseStatus = false, Message = "LoginFail", Sender = a.Sender, Token = a.Token };

                return new LoginResultDTO() { ResponseStatus = true, Message = "LoginSuccess", Sender = a.Sender, Token = a.Token };
            }
            catch (Exception ex)
            {
                return new LoginResultDTO() { ResponseStatus = false, Message = "LoginFail", Sender = a.Sender, Token = a.Token };
            }
            
        }
        private static async Task<ClaimsPrincipal> ValidateJwt(string jwt)
        {
            // read discovery document to find issuer and key material
            var client = new HttpClient();
            var disco = await client.GetDiscoveryDocumentAsync(Constants.Authority);

            var keys = new List<SecurityKey>();
            foreach (var webKey in disco.KeySet.Keys)
            {
                var key = new JsonWebKey()
                {
                    Kty = webKey.Kty,
                    Alg = webKey.Alg,
                    Kid = webKey.Kid,
                    X = webKey.X,
                    Y = webKey.Y,
                    Crv = webKey.Crv,
                    E = webKey.E,
                    N = webKey.N,
                };
                keys.Add(key);
            }

            var parameters = new TokenValidationParameters
            {
                ValidIssuer = disco.Issuer,
                ValidAudience = "https://localhost:5001/resources",
                IssuerSigningKeys = keys,

                NameClaimType = JwtClaimTypes.Name,
                RoleClaimType = JwtClaimTypes.Role
            };

            var handler = new JwtSecurityTokenHandler();
            handler.InboundClaimTypeMap.Clear();

            var user = handler.ValidateToken(jwt, parameters, out var _);
            //var cuser = user.FindFirst(ClaimTypes.NameIdentifier).Value;
            return user;
        }
        #endregion
    }

    public interface ITokensJwt
    {
        string GenerateToken(UserModel user);
        UserToken ValidateToken(string token);

        Task<string> GenerateTokenRSAAsync(UserModel user);
        Task<UserToken> ValidateTokenRSAAsync(string token);
        Task<LoginResultDTO> ValidateIdentityServerToken(LoginMessageDTO a);
    }
}