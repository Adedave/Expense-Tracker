using ExpenseTracker.Data.Domain.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;

namespace ExpenseTracker.Web
{
    public class CreateAdmin
    {
        public static async Task CreateAdminAccount(
            IServiceProvider serviceProvider, IConfiguration configuration)
        {
            string email = "", password = "";
            if (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Production")
            {
                 email = Environment.GetEnvironmentVariable("SUPERADMIN_EMAIL");
                password = Environment.GetEnvironmentVariable("SUPERADMIN_PASSWORD");

            }
            else if (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development")
            {
                
                email = configuration["Data:AdminUser:Email"];
                password = configuration["Data:AdminUser:Password"];
            }
            UserManager<AppUser> userManager =
            serviceProvider.GetRequiredService<UserManager<AppUser>>();
            RoleManager<IdentityRole> roleManager =
            serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            string username = "Admin";
            string role = "Admin";

            if (await userManager.FindByNameAsync(username) == null)
            {
                if (await roleManager.FindByNameAsync(role) == null)
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                    await roleManager.CreateAsync(new IdentityRole("Users"));
                }
                AppUser user = new AppUser
                {
                    UserName = username,
                    Email = email
                };
                IdentityResult result = await userManager
                .CreateAsync(user, password);
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(user, role);
                }
            }
        }
    }
}
