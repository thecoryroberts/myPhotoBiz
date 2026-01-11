
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MyPhotoBiz.Models;

namespace MyPhotoBiz.Data
{
    public static class SeedData
    {
        public static async Task SeedRolesAsync(RoleManager<IdentityRole> roleManager, ILogger logger)
        {
            string[] roles =
            [
                Enums.Roles.SuperAdmin.ToString(),
                Enums.Roles.Admin.ToString(),
                Enums.Roles.Photographer.ToString(),
                Enums.Roles.Client.ToString(),
                Enums.Roles.Guest.ToString()
            ];

            // Fetch all existing roles once to avoid repeated queries
            var existingRoles = new HashSet<string>(
                (await roleManager.Roles.ToListAsync()).Select(r => r.Name!),
                StringComparer.OrdinalIgnoreCase
            );

            foreach (var role in roles)
            {
                if (!existingRoles.Contains(role))
                {
                    var r = new IdentityRole(role);
                    var res = await roleManager.CreateAsync(r);
                    if (!res.Succeeded)
                    {
                        logger.LogWarning("Failed creating role {Role}: {Errors}", role, string.Join(';', res.Errors.Select(e => e.Description)));
                    }
                    else
                    {
                        logger.LogInformation("Created role {Role}", role);
                    }
                }
            }
        }

        public static async Task SeedSuperAdminAsync(UserManager<ApplicationUser> userManager, IConfiguration configuration, ILogger logger)
        {
            // Fetch all existing users once (more efficient than multiple FindByEmailAsync calls)
            var existingUsers = new Dictionary<string, ApplicationUser>(StringComparer.OrdinalIgnoreCase);
            foreach (var user in await userManager.Users.ToListAsync())
            {
                if (!string.IsNullOrEmpty(user.Email))
                {
                    existingUsers[user.Email] = user;
                }
            }

            // Fetch all user roles mapping once
            var userRoles = new Dictionary<string, HashSet<string>>();
            foreach (var user in existingUsers.Values)
            {
                userRoles[user.Id] = [..await userManager.GetRolesAsync(user)];
            }

            // Helper to create or ensure user exists and is in roles
            async Task EnsureUserAsync(string cfgPrefix, ApplicationUser template, string[] rolesToAssign)
            {
                var email = configuration[$"{cfgPrefix}:Email"] ?? configuration[$"{cfgPrefix}_EMAIL"];
                var userName = configuration[$"{cfgPrefix}:UserName"] ?? configuration[$"{cfgPrefix}_USERNAME"] ?? email ?? template.UserName;
                var password = configuration[$"{cfgPrefix}:Password"] ?? configuration[$"{cfgPrefix}_PASSWORD"];

                // Log the password with explicit character enumeration to see if there are hidden chars
                var chars = password?.Select(c => $"{c}({(int)c})") ?? [];
                var charList = string.Join(",", chars);
                
                logger.LogInformation("EnsureUserAsync: {Prefix} - Password chars: {CharList}", cfgPrefix, charList);
                logger.LogInformation("EnsureUserAsync called for {Prefix}: Email={Email}, UserName={UserName}, PasswordLength={PasswordLength}", 
                    cfgPrefix, email, userName, password?.Length ?? 0);

                if (string.IsNullOrEmpty(email))
                {
                    logger.LogInformation("Skipping seed user because no email configured for {Prefix}", cfgPrefix);
                    return;
                }

                if (existingUsers.TryGetValue(email, out var existing))
                {
                    logger.LogInformation("Seed user {Email} already exists with UserName={UserName}", email, existing.UserName);
                    var currentRoles = userRoles[existing.Id];
                    var missingRoles = rolesToAssign.Where(r => !currentRoles.Contains(r)).ToList();
                    
                    if (missingRoles.Count > 0)
                    {
                        await userManager.AddToRolesAsync(existing, missingRoles);
                        logger.LogInformation("Added existing user {Email} to {RoleCount} role(s)", email, missingRoles.Count);
                    }
                    return;
                }

                template.Email = email;
                template.UserName = userName;

                if (string.IsNullOrEmpty(password))
                {
                    password = Helpers.PasswordGenerator.GenerateSecurePassword();
                    logger.LogWarning("No password provided for seed user {Email}; generated a secure password. Rotate after first login.", email);
                }

                logger.LogInformation("Creating new seed user {Email} with UserName={UserName}, PasswordLength={PasswordLength}, Password=[{Password}]", 
                    email, userName, password.Length, password);

                var createResult = await userManager.CreateAsync(template, password);
                if (!createResult.Succeeded)
                {
                    logger.LogError("Failed to create seed user {Email}: {Errors}", email, string.Join(';', createResult.Errors.Select(e => e.Description)));
                    return;
                }

                // Batch assign all roles at once instead of per-role
                await userManager.AddToRolesAsync(template, rolesToAssign);
                logger.LogInformation("Created seed user {Email} with {RoleCount} role(s)", email, rolesToAssign.Length);
            }

            // Primary admin
            var primaryAdminTemplate = new ApplicationUser
            {
                FirstName = "Cory",
                LastName = "Roberts",
                EmailConfirmed = true,
                PhoneNumberConfirmed = true,
                IsPhotographer = true
            };
            await EnsureUserAsync("Seed:PrimaryAdmin", primaryAdminTemplate, [Enums.Roles.SuperAdmin.ToString(), Enums.Roles.Photographer.ToString(), Enums.Roles.Client.ToString(), Enums.Roles.Guest.ToString()]);
        }
    }
}