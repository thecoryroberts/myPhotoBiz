
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MyPhotoBiz.Models;
using MyPhotoBiz.Enums;

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
                userRoles[user.Id] = [.. await userManager.GetRolesAsync(user)];
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

        public static class Permissions
        {
            public const string ViewDashboard = "permissions.dashboard.view";
            public const string ManageUsers = "permissions.users.manage";
            public const string ManageRoles = "permissions.roles.manage";
            public const string ManageClients = "permissions.clients.manage";
            public const string ManagePhotoShoots = "permissions.photoshoots.manage";
            public const string ManageAlbums = "permissions.albums.manage";
            public const string ManagePhotos = "permissions.photos.manage";
            public const string ManageInvoices = "permissions.invoices.manage";
            public const string ManageBookings = "permissions.bookings.manage";
            public const string ManagePackages = "permissions.packages.manage";
            public const string ManageGalleries = "permissions.galleries.manage";

            public static IEnumerable<string> All()
            {
                yield return ViewDashboard;
                yield return ManageUsers;
                yield return ManageRoles;
                yield return ManageClients;
                yield return ManagePhotoShoots;
                yield return ManageAlbums;
                yield return ManagePhotos;
                yield return ManageInvoices;
                yield return ManageBookings;
                yield return ManagePackages;
                yield return ManageGalleries;
            }
        }

        public static async Task SeedPermissionsAsync(ApplicationDbContext context, RoleManager<IdentityRole> roleManager, ILogger logger)
        {
            logger.LogInformation("Seeding permissions...");

            var allPermissions = Permissions.All().ToList();
            var existingPermissions = await context.Permissions.Select(p => p.Name).ToListAsync();

            var newPermissions = allPermissions.Except(existingPermissions).Select(p => new Permission { Name = p });
            if (newPermissions.Any())
            {
                await context.Permissions.AddRangeAsync(newPermissions);
                await context.SaveChangesAsync();
                logger.LogInformation("Created {Count} new permissions.", newPermissions.Count());
            }

            var rolePermissions = new Dictionary<string, List<string>>
            {
                [Enums.Roles.SuperAdmin.ToString()] = allPermissions,
                [Enums.Roles.Admin.ToString()] =
                [
                    Permissions.ViewDashboard,
                    Permissions.ManageClients,
                    Permissions.ManagePhotoShoots,
                    Permissions.ManageAlbums,
                    Permissions.ManagePhotos,
                    Permissions.ManageInvoices,
                    Permissions.ManageBookings,
                    Permissions.ManagePackages,
                    Permissions.ManageGalleries
                ],
                [Enums.Roles.Photographer.ToString()] =
                [
                    Permissions.ViewDashboard,
                    Permissions.ManagePhotoShoots,
                    Permissions.ManageAlbums,
                    Permissions.ManagePhotos,
                    Permissions.ManageGalleries
                ],
                [Enums.Roles.Client.ToString()] = []
            };

            foreach (var rolePermission in rolePermissions)
            {
                var role = await roleManager.FindByNameAsync(rolePermission.Key);
                if (role == null)
                {
                    logger.LogWarning("Role {RoleName} not found, cannot assign permissions.", rolePermission.Key);
                    continue;
                }

                var currentPermissions = await context.RolePermissions
                    .Where(rp => rp.RoleId == role.Id)
                    .Select(rp => rp.Permission)
                    .ToListAsync();

                var permissionsToAssign = rolePermission.Value.Except(currentPermissions).ToList();
                if (permissionsToAssign.Any())
                {
                    var newRolePermissions = permissionsToAssign.Select(p => new RolePermission { RoleId = role.Id, Permission = p });
                    await context.RolePermissions.AddRangeAsync(newRolePermissions);
                    logger.LogInformation("Assigned {Count} new permissions to role {RoleName}.", permissionsToAssign.Count, role.Name);
                }
            }
            await context.SaveChangesAsync();
        }

        public static async Task SeedDummyDataAsync(UserManager<ApplicationUser> userManager, ILogger logger)
        {
            logger.LogInformation("Seeding dummy user data for development...");

            var dummyUsers = new[]
            {
                new { FirstName = "Alice", LastName = "Lens", Email = "alice@photobiz.com", Role = Enums.Roles.Photographer.ToString() },
                new { FirstName = "Bob", LastName = "Shutter", Email = "bob@photobiz.com", Role = Enums.Roles.Photographer.ToString() },
                new { FirstName = "Charlie", LastName = "Customer", Email = "charlie@client.com", Role = Enums.Roles.Client.ToString() },
                new { FirstName = "Diana", LastName = "Dreamer", Email = "diana@client.com", Role = Enums.Roles.Client.ToString() },
                new { FirstName = "Evan", LastName = "Event", Email = "evan@client.com", Role = Enums.Roles.Client.ToString() }
            };

            foreach (var u in dummyUsers)
            {
                if (await userManager.FindByEmailAsync(u.Email) == null)
                {
                    var user = new ApplicationUser
                    {
                        UserName = u.Email,
                        Email = u.Email,
                        FirstName = u.FirstName,
                        LastName = u.LastName,
                        EmailConfirmed = true,
                        PhoneNumberConfirmed = true,
                        IsPhotographer = u.Role == Enums.Roles.Photographer.ToString()
                    };

                    var result = await userManager.CreateAsync(user, "DummyPass123!");
                    if (result.Succeeded)
                    {
                        await userManager.AddToRoleAsync(user, u.Role);
                        logger.LogInformation("Created dummy user {Email} ({Role})", u.Email, u.Role);
                    }
                    else
                    {
                        logger.LogWarning("Failed to create dummy user {Email}: {Errors}", u.Email, string.Join("; ", result.Errors.Select(e => e.Description)));
                    }
                }
            }
        }

        public static async Task SeedDummyDomainDataAsync(ApplicationDbContext context, UserManager<ApplicationUser> userManager, ILogger logger)
        {
            if (await context.ServicePackages.AnyAsync())
            {
                logger.LogInformation("Dummy domain data (packages, photoshoots, etc.) already exists. Skipping seed.");
                return;
            }

            logger.LogInformation("Seeding dummy domain data...");

            var photographerUser = await userManager.FindByEmailAsync("alice@photobiz.com");
            var clientUser = await userManager.FindByEmailAsync("charlie@client.com");
            var dianaClientUser = await userManager.FindByEmailAsync("diana@client.com");
            var evanClientUser = await userManager.FindByEmailAsync("evan@client.com");

            if (photographerUser == null || clientUser == null || dianaClientUser == null || evanClientUser == null)
            {
                logger.LogWarning("Could not find all dummy users to seed related data. Make sure SeedDummyDataAsync has run first.");
                return;
            }

            var photographerProfile = await context.PhotographerProfiles.FirstOrDefaultAsync(p => p.UserId == photographerUser.Id) ?? new PhotographerProfile { UserId = photographerUser.Id, Bio = "An amazing photographer." };
            if (photographerProfile.Id == 0) context.PhotographerProfiles.Add(photographerProfile);

            var clientProfile = await context.ClientProfiles.FirstOrDefaultAsync(p => p.UserId == clientUser.Id) ?? new ClientProfile { UserId = clientUser.Id, Notes = "A valued client." };
            if (clientProfile.Id == 0) context.ClientProfiles.Add(clientProfile);

            var dianaClientProfile = await context.ClientProfiles.FirstOrDefaultAsync(p => p.UserId == dianaClientUser.Id) ?? new ClientProfile { UserId = dianaClientUser.Id, Notes = "A prospective client for portraits." };
            if (dianaClientProfile.Id == 0) context.ClientProfiles.Add(dianaClientProfile);

            var evanClientProfile = await context.ClientProfiles.FirstOrDefaultAsync(p => p.UserId == evanClientUser.Id) ?? new ClientProfile { UserId = evanClientUser.Id, Notes = "A corporate client." };
            if (evanClientProfile.Id == 0) context.ClientProfiles.Add(evanClientProfile);

            await context.SaveChangesAsync();

            var weddingPackage = new ServicePackage { Name = "Full Day Wedding", BasePrice = 3000, DurationHours = 8, Description = "Full day coverage for your special day.", Category = "Weddings", IsActive = true };
            var portraitPackage = new ServicePackage { Name = "Portrait Session", BasePrice = 400, DurationHours = 1.5m, Description = "1.5 hour portrait session.", Category = "Portraits", IsActive = true };
            context.ServicePackages.AddRange(weddingPackage, portraitPackage);

            var photoShoot = new PhotoShoot
            {
                Title = "Charlie's Wedding",
                ScheduledDate = DateTime.UtcNow.AddDays(-30),
                EndTime = DateTime.UtcNow.AddDays(-30).AddHours(4),
                ClientProfileId = clientProfile.Id,
                PhotographerProfileId = photographerProfile.Id,
                Status = PhotoShootStatus.Completed,
                Location = "City Hall"
            };
            context.PhotoShoots.Add(photoShoot);

            var album = new Album
            {
                Name = "Wedding Highlights",
                PhotoShoot = photoShoot,
                ClientProfileId = clientProfile.Id
            };
            context.Albums.Add(album);

            var photos = new List<Photo>();
            for (int i = 1; i <= 10; i++)
            {
                photos.Add(new Photo
                {
                    FileName = $"wedding_highlight_{i}.jpg",
                    Album = album,
                    ClientProfileId = clientProfile.Id,
                    UploadDate = DateTime.UtcNow.AddDays(-29),
                    FilePath = $"/uploads/dummy/wedding_highlight_{i}.jpg",
                    ThumbnailPath = $"/uploads/dummy/thumbnails/wedding_highlight_{i}.jpg"
                });
            }
            context.Photos.AddRange(photos);

            var gallery = new Gallery
            {
                Name = "Charlie's Wedding Gallery",
                Description = "A gallery of wedding highlights",
                ExpiryDate = photoShoot.ScheduledDate.AddMonths(6),
                IsActive = true,
                Albums = new List<Album> { album }
            };
            context.Galleries.Add(gallery);

            var invoice = new Invoice
            {
                ClientProfileId = clientProfile.Id,
                PhotoShootId = photoShoot.Id,
                DueDate = photoShoot.ScheduledDate.AddDays(31),
                Amount = weddingPackage.BasePrice,
                Status = InvoiceStatus.Paid,
                Notes = "Invoice for Charlie's Wedding package.",
                InvoiceItems = new List<InvoiceItem> { new InvoiceItem { Description = weddingPackage.Name, Quantity = 1, UnitPrice = weddingPackage.BasePrice } }
            };
            context.Invoices.Add(invoice);

            var contract = new Contract
            {
                Title = "Contract for Charlie's Wedding",
                Content = "This is a sample signed contract for the wedding photography services, outlining deliverables, payment schedule, and usage rights.",
                ClientProfileId = clientProfile.Id,
                PhotoShootId = photoShoot.Id,
                Status = ContractStatus.Signed,
                SignedDate = photoShoot.ScheduledDate.AddDays(-5),
                CreatedDate = photoShoot.ScheduledDate.AddDays(-10)
            };
            context.Contracts.Add(contract);

            var pendingBooking = new BookingRequest
            {
                ClientProfileId = dianaClientProfile.Id,
                ServicePackageId = portraitPackage.Id,
                EventType = "Family Portraits",
                PreferredDate = DateTime.UtcNow.AddDays(14),
                PreferredStartTime = new TimeSpan(14, 0, 0),
                Location = "City Park",
                Status = BookingStatus.Pending,
                EstimatedDurationHours = portraitPackage.DurationHours,
                EstimatedPrice = portraitPackage.BasePrice,
                SpecialRequirements = "Wants photos with the dog."
            };
            context.BookingRequests.Add(pendingBooking);

            var confirmedBooking = new BookingRequest
            {
                ClientProfileId = evanClientProfile.Id,
                PhotographerProfileId = photographerProfile.Id,
                EventType = "Corporate Headshots",
                PreferredDate = DateTime.UtcNow.AddDays(45),
                PreferredStartTime = new TimeSpan(10, 0, 0),
                Location = "Client's Office",
                Status = BookingStatus.Confirmed,
                ConfirmedDate = DateTime.UtcNow.AddDays(-2),
                EstimatedDurationHours = 4,
                EstimatedPrice = 1500,
                AdminNotes = "Confirmed by phone call. 50% deposit invoice sent separately."
            };
            context.BookingRequests.Add(confirmedBooking);

            await context.SaveChangesAsync();
            logger.LogInformation("Successfully seeded dummy packages, a photoshoot, an album with photos, a gallery, an invoice, a contract, and booking requests.");
        }
    }
}