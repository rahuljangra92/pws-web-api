using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Threading.Tasks;
using System.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using PWSWebApi.Models;
using HttpGetAttribute = Microsoft.AspNetCore.Mvc.HttpGetAttribute;
using System.Linq;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.Extensions.DependencyInjection;
using UserLoginInfo = Microsoft.AspNetCore.Identity.UserLoginInfo;
using IdentityResult = Microsoft.AspNetCore.Identity.IdentityResult;


namespace PWSWebApi.Controllers
{
    #region Actual Code
    //[Authorize]
    [Route("api/[controller]")]
    public class AccountController : Controller
    {

        private readonly IHttpContextAccessor httpContextAccessor;
        private readonly IConfiguration configuration;
        private readonly UserManager<IdentityUser> userManager;
        private readonly SignInManager<IdentityUser> signInManager;
        private readonly IOptions<IdentityOptions> identityOptions;
        private readonly int loginAttemptCount;
        private readonly int passwordExpiryDays;
        private const string LocalLoginProvider = "Local";


        public AccountController(
           IHttpContextAccessor httpContextAccessor,
            IConfiguration configuration,
            Microsoft.AspNetCore.Identity.UserManager<IdentityUser> userManager,
            SignInManager<IdentityUser> signInManager,
            IOptions<IdentityOptions> identityOptions)

        {
            this.httpContextAccessor = httpContextAccessor;
            this.configuration = configuration;
            this.userManager = userManager;
            this.signInManager = signInManager;
            this.identityOptions = identityOptions;
        }


        //public ApplicationUserManager UserManager
        //{
        //    get
        //    {
        //        return _userManager ?? Request.GetOwinContext().GetUserManager<ApplicationUserManager>();
        //    }
        //    private set
        //    {
        //        _userManager = value;
        //    }
        //}

        public ISecureDataFormat<AuthenticationTicket> AccessTokenFormat { get; private set; }

        //GET api/Account/UserInfo

        [HttpGet]
        public UserInfoViewModel GetUserInfo()
        {
            var externalLogin = ExternalLoginData.FromIdentity(User as ClaimsPrincipal);

            return new UserInfoViewModel
            {
                Email = User.Identity.Name,
                HasRegistered = externalLogin == null,
                LoginProvider = externalLogin != null ? externalLogin.LoginProvider : null
            };
        }
        // POST api/Account/Logout

        [HttpGet("Logout")]
        public IActionResult Logout()
        {
            HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return Ok();
        }

        [HttpGet("ManageInfo")]
        public async Task<ManageInfoViewModel> GetManageInfo(string returnUrl, bool generateState = false)
        {
            var user = await userManager.GetUserAsync(User);

            if (user == null)
            {
                return null;
            }

            var logins = (await userManager.GetLoginsAsync(user))
                .Select(linkedAccount => new UserLoginInfoViewModel
                {
                    LoginProvider = linkedAccount.LoginProvider,
                    ProviderKey = linkedAccount.ProviderKey
                })
                .ToList();

            if (user.PasswordHash != null)
            {
                logins.Add(new UserLoginInfoViewModel
                {
                    LoginProvider = LocalLoginProvider,
                    ProviderKey = user.UserName,
                });
            }

            return new ManageInfoViewModel
            {
                LocalLoginProvider = LocalLoginProvider,
                Email = user.UserName,
                Logins = logins,
                ExternalLoginProviders = await GetExternalLogins(returnUrl, generateState)
            };
        }

        [HttpPost("ChangePassword")]
        public async Task<IActionResult> ChangePassword(ChangePasswordBindingModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var user = await userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound();
            }

            IdentityResult result = await userManager.ChangePasswordAsync(user, model.OldPassword, model.NewPassword);

            if (!result.Succeeded)
            {
                return GetErrorResult(result);
            }

            return Ok();
        }

        [HttpPost("SetPassword")]
        public async Task<IActionResult> SetPassword(SetPasswordBindingModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var user = await userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound("User not found");
            }

            IdentityResult result = await userManager.AddPasswordAsync(user, model.NewPassword);

            if (!result.Succeeded)
            {
                return GetErrorResult(result);
            }

            return Ok();
        }       

        

        [HttpPost]
        public async Task<IActionResult> RemoveLogin([FromBody] RemoveLoginBindingModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            IdentityResult result;

            var user = await userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound("User not found");
            }

            if (model.LoginProvider == LocalLoginProvider)
            {
                result = await userManager.RemovePasswordAsync(user);
            }
            else
            {
                result = await userManager.RemoveLoginAsync(user,
                    model.LoginProvider, model.ProviderKey);
            }

            if (!result.Succeeded)
            {
                return GetErrorResult(result);
            }

            return Ok();
        }


        public Task<IEnumerable<ExternalLoginViewModel>> GetExternalLogins(string returnUrl, bool generateState = false)
        {
            var descriptions = new List<AuthenticationScheme>();

            foreach (var provider in HttpContext.RequestServices.GetRequiredService<IAuthenticationSchemeProvider>().GetAllSchemesAsync().Result)
            {
                descriptions.Add(new AuthenticationScheme(provider.Name, provider.DisplayName, provider.HandlerType));
            }

            var logins = new List<ExternalLoginViewModel>();
            string state = generateState ? RandomOAuthStateGenerator.Generate(256) : null;

            foreach (var description in descriptions)
            {
                var login = new ExternalLoginViewModel
                {
                    Name = description.DisplayName,
                    Url = Url.RouteUrl("ExternalLogin", new
                    {
                        provider = description.Name,
                        response_type = "token",
                        client_id = "",//Startup.PublicClientId,                        
                        redirect_uri = new Uri(Request.GetEncodedUrl(), Convert.ToBoolean(returnUrl)).AbsoluteUri,
                        state = state
                    }),
                    State = state
                };

                logins.Add(login);
            }

            return Task.FromResult<IEnumerable<ExternalLoginViewModel>>(logins);
        }

        [AllowAnonymous]
        [HttpPost("Register")]
        public async Task<IActionResult> Register(RegisterBindingModel model)
        {
            if (ModelState.IsValid)
            {
                var user = new IdentityUser { UserName = model.Email };
                var result = await userManager.CreateAsync(user, model.Password);

                if (result.Succeeded)
                {

                    await SignInAsync(user, isPersistent: false);
                }
                else
                {
                    GetErrorResult(result);
                }
            }
            return Ok();
        }
        private void AddErrors(Microsoft.AspNetCore.Identity.IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error.ToString());
            }
        }
        private async Task SignInAsync(IdentityUser user, bool isPersistent)
        {
            await signInManager.SignOutAsync();
            var signInResult = await userManager.CreateAsync(user);
            if (signInResult.Succeeded)
                await signInManager.SignInAsync(user, new AuthenticationProperties() { IsPersistent = isPersistent });
            else
                GetErrorResult(signInResult);
        }

        [HttpPost("RegisterExternal")]
        public async Task<IActionResult> RegisterExternal(RegisterExternalBindingModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var info = await HttpContext.AuthenticateAsync("Bearer");

            if (info == null)
            {
                return StatusCode(500, "Error retrieving external login information.");
            }

            var user = new IdentityUser { UserName = model.Email, Email = model.Email };

            var result = await userManager.CreateAsync(user);

            if (!result.Succeeded)
            {
                GetErrorResult(result);
            }
            var loginProvider = info.Principal.FindFirstValue(ClaimTypes.AuthenticationMethod);
            var providerKey = info.Principal.FindFirstValue(ClaimTypes.NameIdentifier);
            result = await userManager.AddLoginAsync(user, new UserLoginInfo(loginProvider, providerKey, loginProvider));

            if (!result.Succeeded)
            {
                GetErrorResult(result);
            }

            return Ok();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && userManager != null)
            {
                userManager.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Helpers
        //[Route("AddExternalLogin")]
        //public async Task<IHttpActionResult> AddExternalLogin(AddExternalLoginBindingModel model)
        //{
        //    if (!ModelState.IsValid)
        //    {
        //        return BadRequest(ModelState);
        //    }

        //    Authentication.SignOut(DefaultAuthenticationTypes.ExternalCookie);

        //    AuthenticationTicket ticket = AccessTokenFormat.Unprotect(model.ExternalAccessToken);

        //    if (ticket == null || ticket.Identity == null || (ticket.Properties != null
        //        && ticket.Properties.ExpiresUtc.HasValue
        //        && ticket.Properties.ExpiresUtc.Value < DateTimeOffset.UtcNow))
        //    {
        //        return BadRequest("External login failure.");
        //    }

        //    ExternalLoginData externalData = ExternalLoginData.FromIdentity(ticket.Identity);

        //    if (externalData == null)
        //    {
        //        return BadRequest("The external login is already associated with an account.");
        //    }

        //    IdentityResult result = await UserManager.AddLoginAsync(User.Identity.GetUserId(),
        //        new UserLoginInfo(externalData.LoginProvider, externalData.ProviderKey));

        //    if (!result.Succeeded)
        //    {
        //        return GetErrorResult(result);
        //    }

        //    return Ok();
        //}

        //private IAuthenticationManager Authentication
        //{
        //    get { return Request.GetOwinContext().Authentication; }
        //}

        private IActionResult GetErrorResult(IdentityResult result)
        {
            if (result == null)
            {
                return BadRequest("An error occurred while processing your request.");
            }

            if (!result.Succeeded)
            {
                if (result.Errors != null)
                {
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }
                }

                if (ModelState.IsValid)
                {
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

            public static ExternalLoginData FromIdentity(ClaimsPrincipal principal)
            {
                if (principal == null)
                {
                    return null;
                }

                Claim providerKeyClaim = principal.FindFirst(ClaimTypes.NameIdentifier);

                if (providerKeyClaim == null || String.IsNullOrEmpty(providerKeyClaim.Issuer)
                    || String.IsNullOrEmpty(providerKeyClaim.Value))
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
                    UserName = principal.FindFirstValue(ClaimTypes.Name)
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
                return HttpUtility.UrlEncode(data);
            }
        }

        #endregion
    }
    #endregion
}
