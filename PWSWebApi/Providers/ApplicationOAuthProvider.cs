/* Added by CTA: OAuth is now handled by using the public void ConfigureServices(IServiceCollection services) method in the Startup.cs class. The basic process is to use services.AddAuthentication(options => and then set a series of options. We can chain unto that the actual OAuth settings call services.AddOAuth("Auth_Service_here_such_as_GitHub_Canvas...", options =>. Also remember to add a call to IApplicationBuilder.UseAuthentication() in your public void Configure(IApplicationBuilder app, IHostingEnvironment env) method. Please ensure this call comes before setting up your routes. */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.Owin.Security.Cookies;
using PWSWebApi.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authentication;

namespace PWSWebApi.Providers
{
    public class ApplicationOAuthProvider : OAuthAuthorizationServerProvider
    {
        private readonly string _publicClientId;
        public ApplicationOAuthProvider(string publicClientId)
        {
            if (publicClientId == null)
            {
                throw new ArgumentNullException("publicClientId");
            }

            _publicClientId = publicClientId;
        }

        public override async Task GrantResourceOwnerCredentials(OAuthGrantResourceOwnerCredentialsContext context)
        {
            var userManager = context.OwinContext.GetUserManager<ApplicationUserManager>();
            ApplicationUser user = await userManager.FindAsync(context.UserName, context.Password);
            if (user == null)
            {
                context.SetError("invalid_grant", "The user name or password is incorrect.");
                return;
            }

            ClaimsIdentity oAuthIdentity = await user.GenerateUserIdentityAsync(userManager, OAuthDefaults.AuthenticationType);
            ClaimsIdentity cookiesIdentity = await user.GenerateUserIdentityAsync(userManager, CookieAuthenticationDefaults.AuthenticationType);
            AuthenticationProperties properties = CreateProperties(user.UserName);
            AuthenticationTicket ticket = /* Added by CTA: Please use a ClaimsPrincipal object to wrap your ClaimsIdentity parameter to pass to this method, you can optionally include an AuthenticationProperties object and must include a scheme. */
            new AuthenticationTicket(oAuthIdentity, properties);
            context.Validated(ticket);
            /* Added by CTA: Please use a ClaimsPrincipal object to wrap your list of ClaimsIdentity to pass it to this method as a parameter, you can also include a scheme and an AuthenticationProperties object as well. */
            await context.Request.Context.Authentication.SignInAsync(cookiesIdentity);
        }

        public override Task TokenEndpoint(OAuthTokenEndpointContext context)
        {
            foreach (KeyValuePair<string, string> property in context.Properties.Dictionary)
            {
                context.AdditionalResponseParameters.Add(property.Key, property.Value);
            }

            return Task.FromResult<object>(null);
        }

        public override Task ValidateClientAuthentication(OAuthValidateClientAuthenticationContext context)
        {
            // Resource owner password credentials does not provide a client ID.
            if (context.ClientId == null)
            {
                context.Validated();
            }

            return Task.FromResult<object>(null);
        }

        public override Task ValidateClientRedirectUri(OAuthValidateClientRedirectUriContext context)
        {
            if (context.ClientId == _publicClientId)
            {
                Uri expectedRootUri = new Uri(context.Request.Uri, "/");
                if (expectedRootUri.AbsoluteUri == context.RedirectUri)
                {
                    context.Validated();
                }
            }

            return Task.FromResult<object>(null);
        }

        public static AuthenticationProperties CreateProperties(string userName)
        {
            IDictionary<string, string> data = new Dictionary<string, string>
            {
                {
                    "userName",
                    userName
                }
            };
            return new AuthenticationProperties(data);
        }
    }
}