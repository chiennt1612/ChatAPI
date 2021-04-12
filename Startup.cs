using ChatAPI.Models;
using ChatAPI.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using ChatAPI.Websocket;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using IdentityModel.AspNetCore.AccessTokenValidation;
using System.Text;
using System;
using ChatAPI.Helpers;
using IdentityModel;
using IdentityModel.Client;
using System.Net.Http;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using System.IdentityModel.Tokens.Jwt;

namespace ChatAPI
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // requires using Microsoft.Extensions.Options
            services.Configure<JwtBearer>(Configuration.GetSection(nameof(JwtBearer)));
            services.AddSingleton<IJwtBearer>(sp => sp.GetRequiredService<IOptions<JwtBearer>>().Value);

            services.Configure<DBSetting>(Configuration.GetSection(nameof(DBSetting)));
            services.AddSingleton<IDBSetting>(sp => sp.GetRequiredService<IOptions<DBSetting>>().Value);

            services.AddRazorPages();

            services.AddSingleton<ITokensJwt, TokensJwt>();
            services.AddSingleton<IGroupService, GroupService>();
            services.AddSingleton<ISystemParamService, SystemParamService>();
            services.AddSingleton<IBlacklistService, BlacklistService>();
            services.AddSingleton<IMessageService, MessageService>();
            services.AddSingleton<IReportUserService, ReportUserService>();

            services.AddSingleton<IWebsocketHandler, WebsocketHandler>();

            JwtSecurityTokenHandler.DefaultMapInboundClaims = false;

            services.AddControllers();

            services.AddHttpClient();

            services.AddSingleton<IDiscoveryCache>(r =>
            {
                var factory = r.GetRequiredService<IHttpClientFactory>();
                return new DiscoveryCache(Constants.Authority, () => factory.CreateClient());
            });

            services.AddCors();
            
            services.AddAuthentication(options =>
            {
                options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = "oidc";
            })
                .AddCookie(options =>
                {
                    options.Cookie.Name = "chatapi";
                })
                .AddOpenIdConnect("oidc", options =>
                {
                    options.Authority = Constants.Authority;
                    options.ClientId = "mvc1";
                    options.ClientSecret = "secretchatapi";
                    options.ResponseType = "code";
                    options.UsePkce = true;

                    //options.Scope.Add("api1");

                    //options.SaveTokens = true;

                    options.RequireHttpsMetadata = false;

                    //options.ClientId = "chat.api";
                    //options.ClientSecret = "secret.chat.api";

                    //// code flow + PKCE (PKCE is turned on by default)
                    //options.ResponseType = "code";
                    options.UsePkce = true;

                    options.Scope.Clear();
                    options.Scope.Add("openid");
                    options.Scope.Add("profile");
                    options.Scope.Add("email");
                    options.Scope.Add("address");
                    options.Scope.Add("phone");
                    //options.Scope.Add("offline_access");
                    options.Scope.Add("api2");

                    // not mapped by default
                    options.ClaimActions.MapJsonKey("website", "website");

                    // keeps id_token smaller
                    options.GetClaimsFromUserInfoEndpoint = true;
                    options.SaveTokens = true;

                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        NameClaimType = JwtClaimTypes.Name,
                        RoleClaimType = JwtClaimTypes.Role,
                    };
                });

            //services.AddDistributedMemoryCache();
            //services.AddAuthentication("token")
            //     JWT tokens
            //    .AddJwtBearer("token", options =>
            //    {
            //        options.Authority = Constants.Authority;
            //        options.Audience = "resource1";
            //        options.TokenValidationParameters.ValidTypes = new[] { "at+jwt" };
            //        if token does not contain a dot, it is a reference token
            //        options.ForwardDefaultSelector = Selector.ForwardReferenceToken("introspection");
            //    })
            //     reference tokens
            //    .AddOAuth2Introspection("introspection", options =>
            //    {
            //        options.Authority = Constants.Authority;

            //        options.ClientId = "chat.api";
            //        options.ClientSecret = "secret.chat.api";
            //    });


            //services.AddScopeTransformation();

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo { Title = "My API", Version = "v1" });
                //c.OperationFilter<AuthorizationHeaderParameterOperationFilter>();
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthentication();

            app.UseAuthorization();

            // Enable middleware to serve generated Swagger as a JSON endpoint.
            app.UseSwagger();

            // Enable middleware to serve swagger-ui (HTML, JS, CSS, etc.), 
            // specifying the Swagger JSON endpoint.
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1");
            });

            app.UseWebSockets();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}")
                .RequireAuthorization(); 
            });
        }
    }
}
