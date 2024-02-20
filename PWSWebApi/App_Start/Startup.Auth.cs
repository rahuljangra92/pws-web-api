/* Added by CTA: OAuth is now handled by using the public void ConfigureServices(IServiceCollection services) method in the Startup.cs class. The basic process is to use services.AddAuthentication(options => and then set a series of options. We can chain unto that the actual OAuth settings call services.AddOAuth("Auth_Service_here_such_as_GitHub_Canvas...", options =>. Also remember to add a call to IApplicationBuilder.UseAuthentication() in your public void Configure(IApplicationBuilder app, IHostingEnvironment env) method. Please ensure this call comes before setting up your routes. */
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.Owin.Security.Cookies;
using Microsoft.Owin.Security.Google;
using Microsoft.Owin.Security.OAuth;
using PWSWebApi.Providers;
using PWSWebApi.Models;
using System.Web.Http;
using System.Web.Http.Cors;
using Microsoft.AspNetCore.Owin;
using Microsoft.AspNetCore.Builder;

namespace PWSWebApi
{
    public partial class Startup
    {
        public static OAuthAuthorizationServerOptions OAuthOptions { get; private set; }

        public static string PublicClientId { get; private set; }

        // For more information on configuring authentication, please visit http://go.microsoft.com/fwlink/?LinkId=301864
        public void ConfigureAuth(IApplicationBuilder app)
        {
            // Configure the db context and user manager to use a single instance per request
            /* Added by CTA: Please replace CreatePerOwinContext<T>(System.Func<T>) and add a new ConfigureServices method: public void ConfigureServices(IServiceCollection services) { Register your service here instead of using CreatePerOwinContext }. For example, app.CreatePerOwinContext(ApplicationDbContext.Create); would become: services.AddDbContext<ApplicationDbContext>(options => options.UseSqlServer(config.GetConnectionString("DefaultConnection")));  */
            app.CreatePerOwinContext(ApplicationDbContext.Create);
            /* Added by CTA: Please replace CreatePerOwinContext<T>(System.Func<Microsoft.AspNet.Identity.Owin.IdentityFactoryOptions<T>, Microsoft.Owin.IOwinContext, T>) and add a new ConfigureServices method: public void ConfigureServices(IServiceCollection services) { Register your service here instead of using CreatePerOwinContext }. For example, app.CreatePerOwinContext(ApplicationDbContext.Create); would become: services.AddDbContext<ApplicationDbContext>(options => options.UseSqlServer(config.GetConnectionString("DefaultConnection")));  */
            app.CreatePerOwinContext<ApplicationUserManager>(ApplicationUserManager.Create);
            // Enable the application to use a cookie to store information for the signed in user
            // and to use a cookie to temporarily store information about a user logging in with a third party login provider
            app.UseCookieAuthentication(new CookieAuthenticationOptions());
            app.UseExternalSignInCookie(DefaultAuthenticationTypes.ExternalCookie);
            // Configure the application for OAuth based flow
            PublicClientId = "self";
            OAuthOptions = new OAuthAuthorizationServerOptions
            {
                TokenEndpointPath = new PathString("/Token"),
                Provider = new ApplicationOAuthProvider(PublicClientId),
                AuthorizeEndpointPath = new PathString("/api/Account/ExternalLogin"),
                AccessTokenExpireTimeSpan = TimeSpan.FromDays(14),
                // In production mode set AllowInsecureHttp = false
                AllowInsecureHttp = true
            };
            // Enable the application to use bearer tokens to authenticate users
            app.UseOAuthBearerTokens(OAuthOptions);
            // Uncomment the following lines to enable logging in with third party login providers
            //app.UseMicrosoftAccountAuthentication(
            //    clientId: "",
            //    clientSecret: "");
            //app.UseTwitterAuthentication(
            //    consumerKey: "",
            //    consumerSecret: "");
            //app.UseFacebookAuthentication(
            //    appId: "",
            //    appSecret: "");
            //app.UseGoogleAuthentication(new GoogleOAuth2AuthenticationOptions()
            //{
            //    ClientId = "",
            //    ClientSecret = ""
            //});
            //ConfigureAuth(app);
            //HttpConfiguration config = new HttpConfiguration();
            //WebApiConfig.Register(config);
            //var cors = new EnableCorsAttribute(
            //origins: "http://abc.com",
            //headers: "*",
            //methods: "*");
            app.UseCors(Microsoft.Owin.Cors.CorsOptions.AllowAll);
        //app.UseWebApi(config);
        }
    }
}