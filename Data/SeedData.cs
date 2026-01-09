
using Microsoft.AspNetCore.Identity;
using MyPhotoBiz.Models;

namespace MyPhotoBiz.Data
{
    public static class SeedData
    {
        public static async Task Initialize(IServiceProvider serviceProvider)
        {
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();

            // Create roles
            string[] roles = { "Photographer", "Client" };
            foreach (string role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                }
            }
        }

        public static async Task SeedRolesAsync(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            //Seed Roles
            string[] roles = {
                Enums.Roles.SuperAdmin.ToString(),
                Enums.Roles.Admin.ToString(),
                Enums.Roles.Photographer.ToString(),
                Enums.Roles.Client.ToString(),
                Enums.Roles.Guest.ToString()
            };

            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                }
            }
        }
        public static async Task SeedSuperAdminAsync(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            // Seed Primary SuperAdmin
            var primaryAdmin = new ApplicationUser
            {
                UserName = "thecoryroberts",
                Email = "mail@thecoryroberts.com",
                FirstName = "Cory",
                LastName = "Roberts",
                EmailConfirmed = true,
                PhoneNumberConfirmed = true,
                IsPhotographer = true,
            };
            if (userManager.Users.All(u => u.Email != primaryAdmin.Email))
            {
                var user = await userManager.FindByEmailAsync(primaryAdmin.Email);
                if (user == null)
                {
                    await userManager.CreateAsync(primaryAdmin, "Harpoon121");
                    await userManager.AddToRoleAsync(primaryAdmin, Enums.Roles.SuperAdmin.ToString());
                    await userManager.AddToRoleAsync(primaryAdmin, Enums.Roles.Photographer.ToString());
                    await userManager.AddToRoleAsync(primaryAdmin, Enums.Roles.Client.ToString());
                    await userManager.AddToRoleAsync(primaryAdmin, Enums.Roles.Guest.ToString());
                }
            }

            // Seed Secondary SuperAdmin
            var defaultUser = new ApplicationUser
            {
                UserName = "superadmin",
                Email = "help@coryroberts.net",
                FirstName = "Super",
                LastName = "Admin",
                EmailConfirmed = true,
                PhoneNumberConfirmed = true,
                IsPhotographer = true,
            };
            if (userManager.Users.All(u => u.Email != defaultUser.Email))
            {
                var user = await userManager.FindByEmailAsync(defaultUser.Email);
                if (user == null)
                {
                    await userManager.CreateAsync(defaultUser, "Harpoon12!");
                    await userManager.AddToRoleAsync(defaultUser, Enums.Roles.SuperAdmin.ToString());
                    await userManager.AddToRoleAsync(defaultUser, Enums.Roles.Photographer.ToString());
                    await userManager.AddToRoleAsync(defaultUser, Enums.Roles.Client.ToString());
                    await userManager.AddToRoleAsync(defaultUser, Enums.Roles.Guest.ToString());
                }
            }

            // Seed PhotoBiz Admin
            var photoBizAdmin = new ApplicationUser
            {
                UserName = "photobizadmin",
                Email = "superadmin@photobiz.com",
                FirstName = "PhotoBiz",
                LastName = "Admin",
                EmailConfirmed = true,
                PhoneNumberConfirmed = true,
                IsPhotographer = true,
            };
            if (userManager.Users.All(u => u.Email != photoBizAdmin.Email))
            {
                var user = await userManager.FindByEmailAsync(photoBizAdmin.Email);
                if (user == null)
                {
                    await userManager.CreateAsync(photoBizAdmin, "Harpoon1234!!!!");
                    await userManager.AddToRoleAsync(photoBizAdmin, Enums.Roles.Admin.ToString());
                    await userManager.AddToRoleAsync(photoBizAdmin, Enums.Roles.Photographer.ToString());
                    await userManager.AddToRoleAsync(photoBizAdmin, Enums.Roles.Client.ToString());
                }
            }
        }
    }
}