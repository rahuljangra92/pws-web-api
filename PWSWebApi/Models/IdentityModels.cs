using System.Security.Claims;
using System.Threading.Tasks;
//using Microsoft.AspNet.Identity;
//using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace PWSWebApi.Models
{
    // You can add profile data for the user by adding more properties to your ApplicationUser class, please visit http://go.microsoft.com/fwlink/?LinkID=317594 to learn more.
    public class ApplicationUser : IdentityUser
    {
        // Your ApplicationUser class remains unchanged
    }

    //public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    //{
    //    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    //    {
    //    }

    //    // No need for the Create method anymore

    //    // No need for the OnConfiguring method anymore
    //}
}