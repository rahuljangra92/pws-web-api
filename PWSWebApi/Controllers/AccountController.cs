/* Added by CTA: OAuth is now handled by using the public void ConfigureServices(IServiceCollection services) method in the Startup.cs class. The basic process is to use services.AddAuthentication(options => and then set a series of options. We can chain unto that the actual OAuth settings call services.AddOAuth("Auth_Service_here_such_as_GitHub_Canvas...", options =>. Also remember to add a call to IApplicationBuilder.UseAuthentication() in your public void Configure(IApplicationBuilder app, IHostingEnvironment env) method. Please ensure this call comes before setting up your routes. */
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.DependencyInjection;
//using Microsoft.AspNet.Identity;
//using Microsoft.AspNet.Identity.EntityFramework;
//using Microsoft.Owin.Security.Cookies;
using PWSWebApi.Models;
//using PWSWebApi.Providers;
//using PWSWebApi.Results;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Threading.Tasks;


namespace PWSWebApi.Controllers
{
    [Authorize]
    [Route("api/Account")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private const string LocalLoginProvider = "Local";
        private ApplicationUserManager _userManager;
        public AccountController()
        {
        }

        public AccountController(ApplicationUserManager userManager, ISecureDataFormat<AuthenticationTicket> accessTokenFormat)
        {
            UserManager = userManager;
            AccessTokenFormat = accessTokenFormat;
        }

        public ApplicationUserManager UserManager
        {
            get
            {
                //return _userManager ?? Request.GetOwinContext().GetUserManager<ApplicationUserManager>();
                return _userManager ?? HttpContext.RequestServices.GetService<ApplicationUserManager>();
            }

            private set
            {
                _userManager = value;
            }
        }

        public ISecureDataFormat<AuthenticationTicket> AccessTokenFormat { get; private set; }

        // GET api/Account/UserInfo
        //[HostAuthentication(DefaultAuthenticationTypes.ExternalBearer)]
        [Authorize(AuthenticationSchemes = "Bearer")]
        [Route("UserInfo")]
        public UserInfoViewModel GetUserInfo()
        {
            ExternalLoginData externalLogin = ExternalLoginData.FromIdentity(User.Identity as ClaimsIdentity);
            return new UserInfoViewModel
            {
                Email = User.Identity.Name,
                HasRegistered = externalLogin == null,
                LoginProvider = externalLogin != null ? externalLogin.LoginProvider : null
            };
        }

        // POST api/Account/Logout
        [Route("Logout")]
        public async Task<IActionResult> Logout()
        {
            /* Added by CTA: You can only pass in a single scheme as a parameter and you can also include an AuthenticationProperties object as well. */
            await HttpContext.SignOutAsync();
            return Ok();
        }

        // GET api/Account/ManageInfo?returnUrl=%2F&generateState=true
        [Route("ManageInfo")]
        public async Task<ManageInfoViewModel> GetManageInfo(string returnUrl, bool generateState = false)
        {
            // IdentityUser user = await UserManager.FindByIdAsync(User.Identity.GetUserId());
            string userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            ApplicationUser user = await UserManager.FindByIdAsync(userId);
            if (user == null)
            {
                return null;
            }

            List<UserLoginInfoViewModel> logins = new List<UserLoginInfoViewModel>();
            //foreach (IdentityUserLogin linkedAccount in user.Logins)
            //{
            //    logins.Add(new UserLoginInfoViewModel { LoginProvider = linkedAccount.LoginProvider, ProviderKey = linkedAccount.ProviderKey });
            //}
            foreach (var linkedAccount in await UserManager.GetLoginsAsync(user))
            {
                logins.Add(new UserLoginInfoViewModel
                {
                    LoginProvider = linkedAccount.LoginProvider,
                    ProviderKey = linkedAccount.ProviderKey
                });
            }

            if (user.PasswordHash != null)
            {
                logins.Add(new UserLoginInfoViewModel { LoginProvider = LocalLoginProvider, ProviderKey = user.UserName, });
            }

            return new ManageInfoViewModel
            {
                LocalLoginProvider = LocalLoginProvider,
                Email = user.UserName,
                Logins = logins,
                //ExternalLoginProviders = GetExternalLogins(returnUrl, generateState)
            };
        }

        // POST api/Account/ChangePassword
        [Route("ChangePassword")]
        public async Task<IActionResult> ChangePassword(ChangePasswordBindingModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            string userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            ApplicationUser user = await UserManager.FindByIdAsync(userId);
            IdentityResult result = await UserManager.ChangePasswordAsync(user, model.OldPassword, model.NewPassword);
            if (!result.Succeeded)
            {
                return GetErrorResult(result);
            }

            return Ok();
        }

        // POST api/Account/SetPassword
        [Route("SetPassword")]
        public async Task<IActionResult> SetPassword(SetPasswordBindingModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            string userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            ApplicationUser user = await UserManager.FindByIdAsync(userId);
            IdentityResult result = await UserManager.AddPasswordAsync(user, model.NewPassword);
            if (!result.Succeeded)
            {
                return GetErrorResult(result);
            }

            return Ok();
        }

        // POST api/Account/AddExternalLogin
        [Route("AddExternalLogin")]
        public async Task<IActionResult> AddExternalLogin(AddExternalLoginBindingModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            /* Added by CTA: You can only pass in a single scheme as a parameter and you can also include an AuthenticationProperties object as well. */
            //await Authentication.SignOutAsync();
            await HttpContext.SignOutAsync();

            //AuthenticationTicket ticket = AccessTokenFormat.Unprotect(model.ExternalAccessToken);
            //if (ticket == null || (ticket.Properties != null && ticket.Properties.ExpiresUtc.HasValue && ticket.Properties.ExpiresUtc.Value < DateTimeOffset.UtcNow))
            //{
            //    return BadRequest("External login failure.");
            //}

            //ExternalLoginData externalData = ExternalLoginData.FromIdentity(ticket.Identity);

            //if (externalData == null)
            //{
            //    return BadRequest("The external login is already associated with an account.");
            //}
            //string userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            //ApplicationUser user = await UserManager.FindByIdAsync(userId);
            //IdentityResult result = await UserManager.AddLoginAsync(user, new UserLoginInfo(externalData.LoginProvider, externalData.ProviderKey, externalData.UserName));
            //if (!result.Succeeded)
            //{
            //    return GetErrorResult(result);
            //}

            return Ok();
        }

        // POST api/Account/RemoveLogin
        [Route("RemoveLogin")]
        public async Task<IActionResult> RemoveLogin(RemoveLoginBindingModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            string userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            ApplicationUser user = await UserManager.FindByIdAsync(userId);
            IdentityResult result;
            if (model.LoginProvider == LocalLoginProvider)
            {
                result = await UserManager.RemovePasswordAsync(user);
            }
            else
            {
                result = await UserManager.RemoveLoginAsync(user, model.LoginProvider, model.ProviderKey);
            }

            if (!result.Succeeded)
            {
                return GetErrorResult(result);
            }

            return Ok();
        }

        //// GET api/Account/ExternalLogin
        //[OverrideAuthentication]
        ////[HostAuthentication(DefaultAuthenticationTypes.ExternalCookie)]
        //[Authorize(AuthenticationSchemes = "Cookie")]
        //[AllowAnonymous]
        //[Route("ExternalLogin", Name = "ExternalLogin")]
        //public async Task<IActionResult> GetExternalLogin(string provider, string error = null)
        //{
        //    if (error != null)
        //    {
        //        return Redirect(Url.Content("~/") + "#error=" + Uri.EscapeDataString(error));
        //    }

        //    if (!User.Identity.IsAuthenticated)
        //    {
        //        return new ChallengeResult(provider, this);
        //    }

        //    ExternalLoginData externalLogin = ExternalLoginData.FromIdentity(User.Identity as ClaimsIdentity);
        //    if (externalLogin == null)
        //    {
        //        return StatusCode(500);
        //    }

        //    if (externalLogin.LoginProvider != provider)
        //    {
        //        /* Added by CTA: You can only pass in a single scheme as a parameter and you can also include an AuthenticationProperties object as well. */
        //        await Authentication.SignOutAsync();
        //        return new ChallengeResult(provider, this);
        //    }

        //    ApplicationUser user = await UserManager.FindByLoginAsync(externalLogin.LoginProvider, externalLogin.ProviderKey);
        //    bool hasRegistered = user != null;
        //    if (hasRegistered)
        //    {
        //        /* Added by CTA: You can only pass in a single scheme as a parameter and you can also include an AuthenticationProperties object as well. */
        //        await Authentication.SignOutAsync();
        //        ClaimsIdentity oAuthIdentity = await user.GenerateUserIdentityAsync(UserManager, OAuthDefaults.AuthenticationType);
        //        ClaimsIdentity cookieIdentity = await user.GenerateUserIdentityAsync(UserManager, CookieAuthenticationDefaults.AuthenticationType);
        //        AuthenticationProperties properties = ApplicationOAuthProvider.CreateProperties(user.UserName);
        //        /* Added by CTA: Please use a ClaimsPrincipal object to wrap your list of ClaimsIdentity to pass it to this method as a parameter, you can also include a scheme and an AuthenticationProperties object as well. */
        //        await Authentication.SignInAsync(properties, oAuthIdentity, cookieIdentity);
        //    }
        //    else
        //    {
        //        IEnumerable<Claim> claims = externalLogin.GetClaims();
        //        ClaimsIdentity identity = new ClaimsIdentity(claims, OAuthDefaults.AuthenticationType);
        //        /* Added by CTA: Please use a ClaimsPrincipal object to wrap your list of ClaimsIdentity to pass it to this method as a parameter, you can also include a scheme and an AuthenticationProperties object as well. */
        //        await Authentication.SignInAsync(identity);
        //    }

        //    return Ok();
        //}

        //// GET api/Account/ExternalLogins?returnUrl=%2F&generateState=true
        //[AllowAnonymous]
        //[Route("ExternalLogins")]
        //public IEnumerable<ExternalLoginViewModel> GetExternalLogins(string returnUrl, bool generateState = false)
        //{
        //    IEnumerable<AuthenticationScheme> descriptions = /* Added by CTA: You must declare a Microsoft.AspNetCore.Identity.SignInManager<TUser> object and access the GetExternalAuthenticationSchemesAsync method to replace this invocation. */
        //    Authentication.GetExternalAuthenticationTypes();
        //    List<ExternalLoginViewModel> logins = new List<ExternalLoginViewModel>();
        //    string state;
        //    if (generateState)
        //    {
        //        const int strengthInBits = 256;
        //        state = RandomOAuthStateGenerator.Generate(strengthInBits);
        //    }
        //    else
        //    {
        //        state = null;
        //    }

        //    foreach (AuthenticationDescription description in descriptions)
        //    {
        //        ExternalLoginViewModel login = new ExternalLoginViewModel
        //        {
        //            Name = description.Caption,
        //            Url = Url.Route("ExternalLogin", new { provider = description.AuthenticationType, response_type = "token", client_id = Startup.PublicClientId, redirect_uri = new Uri(Request.RequestUri, returnUrl).AbsoluteUri, state = state }),
        //            State = state
        //        };
        //        logins.Add(login);
        //    }

        //    return logins;
        //}



        //// POST api/Account/Register
        //[AllowAnonymous]
        //[Route("Register")]
        //public async Task<IActionResult> Register(RegisterBindingModel model)
        //{
        //    if (!ModelState.IsValid)
        //    {
        //        return BadRequest(ModelState);
        //    }

        //    var user = new ApplicationUser()
        //    {
        //        UserName = model.Email,
        //        Email = model.Email
        //    };
        //    IdentityResult result = await UserManager.CreateAsync(user, model.Password);
        //    if (!result.Succeeded)
        //    {
        //        return GetErrorResult(result);
        //    }

        //    return Ok();
        //}

        //// POST api/Account/RegisterExternal
        //[OverrideAuthentication]
        //[HostAuthentication(DefaultAuthenticationTypes.ExternalBearer)]
        //[Route("RegisterExternal")]
        //public async Task<IActionResult> RegisterExternal(RegisterExternalBindingModel model)
        //{
        //    if (!ModelState.IsValid)
        //    {
        //        return BadRequest(ModelState);
        //    }

        //    var info = await /* Added by CTA: You must declare a Microsoft.AspNetCore.Identity.SignInManager<TUser> object and access the GetExternalLoginInfoAsync method to replace this invocation. */
        //    Authentication.GetExternalLoginInfoAsync();
        //    if (info == null)
        //    {
        //        return StatusCode(500);
        //    }

        //    var user = new ApplicationUser()
        //    {
        //        UserName = model.Email,
        //        Email = model.Email
        //    };
        //    IdentityResult result = await UserManager.CreateAsync(user);
        //    if (!result.Succeeded)
        //    {
        //        return GetErrorResult(result);
        //    }

        //    result = await UserManager.AddLoginAsync(user.Id, info.Login);
        //    if (!result.Succeeded)
        //    {
        //        return GetErrorResult(result);
        //    }

        //    return Ok();
        //}

        ////protected override void Dispose(bool disposing)
        ////{
        ////    if (disposing && _userManager != null)
        ////    {
        ////        _userManager.Dispose();
        ////        _userManager = null;
        ////    }

        ////    base.Dispose(disposing);
        ////}

        //#region Helpers
        //private IAuthenticationManager Authentication
        //{
        //    get
        //    {
        //        return Request.GetOwinContext().Authentication;
        //    }
        //}

        private IActionResult GetErrorResult(IdentityResult result)
        {
            if (result == null)
            {
                return StatusCode(500);
            }

            if (!result.Succeeded)
            {
                if (result.Errors != null)
                {
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError("", error.Description);
                    }
                }

                if (ModelState.IsValid)
                {
                    // No ModelState errors are available to send, so just return an empty BadRequest.
                    return BadRequest();
                }

                return BadRequest(ModelState);
            }

            return null;
        }

        private class ExternalLoginData
        {
            public string LoginProvider { get; set; }

            public string ProviderKey { get; set; }

            public string UserName { get; set; }

            public IList<Claim> GetClaims()
            {
                IList<Claim> claims = new List<Claim>();
                claims.Add(new Claim(ClaimTypes.NameIdentifier, ProviderKey, null, LoginProvider));
                if (UserName != null)
                {
                    claims.Add(new Claim(ClaimTypes.Name, UserName, null, LoginProvider));
                }

                return claims;
            }

            public static ExternalLoginData FromIdentity(ClaimsIdentity identity)
            {
                if (identity == null)
                {
                    return null;
                }

                Claim providerKeyClaim = identity.FindFirst(ClaimTypes.NameIdentifier);
                if (providerKeyClaim == null || String.IsNullOrEmpty(providerKeyClaim.Issuer) || String.IsNullOrEmpty(providerKeyClaim.Value))
                {
                    return null;
                }

                if (providerKeyClaim.Issuer == ClaimsIdentity.DefaultIssuer)
                {
                    return null;
                }

                return new ExternalLoginData
                {
                    LoginProvider = providerKeyClaim.Issuer,
                    ProviderKey = providerKeyClaim.Value,
                    UserName = identity.Name
                };
            }
        }

        private static class RandomOAuthStateGenerator
        {
            private static RandomNumberGenerator _random = new RNGCryptoServiceProvider();
            public static string Generate(int strengthInBits)
            {
                const int bitsPerByte = 8;
                if (strengthInBits % bitsPerByte != 0)
                {
                    throw new ArgumentException("strengthInBits must be evenly divisible by 8.", "strengthInBits");
                }

                int strengthInBytes = strengthInBits / bitsPerByte;
                byte[] data = new byte[strengthInBytes];
                _random.GetBytes(data);
                return WebEncoders.Base64UrlEncode(data);
            }
        }
        //#endregion
    }
}