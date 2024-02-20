using System.Threading.Tasks;
//using Microsoft.AspNet.Identity;
//using Microsoft.AspNet.Identity.EntityFramework;
using PWSWebApi.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Owin;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Collections.Generic;
using System;
using Microsoft.AspNetCore.DataProtection;

namespace PWSWebApi
{
    // Configure the application user manager used in this application. UserManager is defined in ASP.NET Identity and is used by the application.
    public class ApplicationUserManager : UserManager<ApplicationUser>
    {
        public ApplicationUserManager(
         IUserStore<ApplicationUser> store,
         IOptions<IdentityOptions> optionsAccessor,
         IPasswordHasher<ApplicationUser> passwordHasher,
         IEnumerable<IUserValidator<ApplicationUser>> userValidators,
         IEnumerable<IPasswordValidator<ApplicationUser>> passwordValidators,
         ILookupNormalizer keyNormalizer,
         IdentityErrorDescriber errors,
         IServiceProvider services,
         ILogger<UserManager<ApplicationUser>> logger,
         IHttpContextAccessor contextAccessor,
         IDataProtectionProvider dataProtectionProvider)
         : base(
               store,
               optionsAccessor,
               passwordHasher,
               userValidators,
               passwordValidators,
               keyNormalizer,
               errors,
               services,
               logger)
        {
        }

        //public static ApplicationUserManager Create(IdentityFactoryOptions<ApplicationUserManager> options, HttpContext context)
        //{
        //    var manager = new ApplicationUserManager(new UserStore<ApplicationUser>(context.Get<ApplicationDbContext>()));
        //    // Configure validation logic for usernames
        //    manager.UserValidator = new UserValidator<ApplicationUser>(manager)
        //    {
        //        AllowOnlyAlphanumericUserNames = false,
        //        RequireUniqueEmail = true
        //    };
        //    // Configure validation logic for passwords
        //    manager.PasswordValidator = new PasswordValidator
        //    {
        //        RequiredLength = 6,
        //        RequireNonLetterOrDigit = true,
        //        RequireDigit = true,
        //        RequireLowercase = true,
        //        RequireUppercase = true,
        //    };
        //    var dataProtectionProvider = options.DataProtectionProvider;
        //    if (dataProtectionProvider != null)
        //    {
        //        manager.UserTokenProvider = new DataProtectorTokenProvider<ApplicationUser>(dataProtectionProvider.CreateProtector("ASP.NET Identity"));
        //    }

        //    return manager;
        //}
    }
}