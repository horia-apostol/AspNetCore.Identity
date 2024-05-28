using AspNet8Identity.Misc;
using Microsoft.AspNetCore.Identity;

namespace AspNet8Identity.Data
{
    internal sealed class DbInitializer(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
    {
        private readonly UserManager<ApplicationUser> _userManager = userManager;
        private readonly RoleManager<IdentityRole> _roleManager = roleManager;

        public async Task SeedRolesAsync()
        {
            if (!await _roleManager.RoleExistsAsync(Roles.Admin))
            {
                await _roleManager.CreateAsync(new IdentityRole(Roles.Admin));
            }

            if (!await _roleManager.RoleExistsAsync(Roles.User))
            {
                await _roleManager.CreateAsync(new IdentityRole(Roles.User));
            }
        }

        public async Task SeedAdminAsync()
        {
            if (await _userManager.FindByEmailAsync(AdminCredentials.Email) == null)
            {
                var user = new ApplicationUser
                {
                    Email = AdminCredentials.Email,
                    EmailConfirmed = true
                };

                var result = await _userManager.CreateAsync(user, AdminCredentials.Password);
                if (result.Succeeded)
                {
                    await _userManager.AddToRoleAsync(user, Roles.Admin);
                }
            }
        }
    }
}
