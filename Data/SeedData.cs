
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

                logger.LogInformation("Creating new seed user {Email} with UserName={UserName}, PasswordLength={PasswordLength}",
                    email, userName, password.Length);

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

            var newPermissions = allPermissions.Except(existingPermissions).Select(p => new Permission { Name = p }).ToList();
            if (newPermissions.Count > 0)
            {
                await context.Permissions.AddRangeAsync(newPermissions);
                await context.SaveChangesAsync();
                logger.LogInformation("Created {Count} new permissions.", newPermissions.Count);
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

        public static async Task SeedQuestionnaireTemplatesAsync(ApplicationDbContext context, ILogger logger)
        {
            var existingNames = new HashSet<string>(
                await context.QuestionnaireTemplates
                    .Select(t => t.Name)
                    .ToListAsync(),
                StringComparer.OrdinalIgnoreCase
            );

            var templates = new List<QuestionnaireTemplate>
            {
                new QuestionnaireTemplate
                {
                    Name = "Client Intake Questionnaire",
                    Category = "Client Intake",
                    Description = "Required for all clients before contract to understand goals and fit.",
                    QuestionText = @"Client Intake Questionnaire (Required for all clients)

Purpose
- Establish who the client is, what they want, and whether you are a good fit.

When used
- Immediately after inquiry / before contract.

Client Information
- Full name:
- Email address:
- Phone number:
- Preferred contact method:
- Billing address:

Project Overview
- Type of shoot (wedding, portrait, branding, event, product, etc.):
- Intended usage (personal, commercial, advertising, social media, print):
- Target audience:
- Desired style (light and airy, moody, editorial, documentary, etc.):

Expectations
- What inspired you to book this shoot?
- What would make this shoot a success for you?
- Are there any photographers whose style you love?

Logistics
- Desired shoot date(s):
- Location(s):
- Indoor / outdoor:
- Flexibility on date/time:",
                    IsActive = true
                },
                new QuestionnaireTemplate
                {
                    Name = "Shoot Details Questionnaire",
                    Category = "Shoot Details",
                    Description = "Locks down session specifics after booking to avoid assumptions.",
                    QuestionText = @"Shoot Details Questionnaire (Session-Specific)

Purpose
- Lock down the details so nothing is assumed.

When used
- After booking, before the shoot.

Timing
- Start time:
- End time:
- Hard stop time (if any):
- Arrival buffer required?

Location
- Address(s):
- Parking instructions:
- Permit requirements:
- Weather backup plan:

People Involved
- Number of subjects:
- Names (important for events/weddings):
- Ages (especially for children):
- Any special needs or accessibility considerations:

Shot List / Priorities
- Must-have shots:
- Nice-to-have shots:
- Anything explicitly NOT wanted:",
                    IsActive = true
                },
                new QuestionnaireTemplate
                {
                    Name = "Branding / Commercial Photography Questionnaire",
                    Category = "Branding",
                    Description = "Ensures images serve business goals for brand and commercial shoots.",
                    QuestionText = @"Branding / Commercial Photography Questionnaire

Purpose
- Ensure images actually serve the client's business goals.

When used
- For brand, product, corporate, or marketing shoots.

Brand Identity
- Brand values (3 to 5 words):
- Brand personality (professional, bold, playful, luxury, etc.):
- Color palette / brand guidelines upload (link or file name):
- Logo upload (link or file name):

Usage Rights
- Where will images be used? (website, ads, billboards, packaging):
- Geographic reach:
- Duration of usage:

Visual Direction
- Reference images (links):
- Competitors you admire:
- Competitors you want to differentiate from:",
                    IsActive = true
                },
                new QuestionnaireTemplate
                {
                    Name = "Wedding Photography Questionnaire",
                    Category = "Wedding",
                    Description = "Prevents missed moments and timeline issues 4-8 weeks before the wedding.",
                    QuestionText = @"Wedding Photography Questionnaire

Purpose
- Prevent missed moments and timeline chaos.

When used
- 4 to 8 weeks before the wedding.

Couple Information
- Full legal names:
- Preferred names:
- Phone numbers for wedding day:

Timeline
- Getting ready location(s):
- Ceremony start time:
- Reception start time:
- Exit time:

Family Shot List
- Required family groupings:
- Sensitive family dynamics to be aware of:
- Who wrangles family for photos?

Special Moments
- First look?
- Private vows?
- Cultural or religious traditions?
- Surprise events planned?",
                    IsActive = true
                },
                new QuestionnaireTemplate
                {
                    Name = "Portrait / Family Session Questionnaire",
                    Category = "Portrait",
                    Description = "Helps clients feel prepared and confident for portrait and family sessions.",
                    QuestionText = @"Portrait / Family Session Questionnaire

Purpose
- Make clients feel prepared and confident.

When used
- Before portraits, family, maternity, or senior sessions.

Wardrobe
- Preferred color palette:
- Outfit coordination concerns:
- Any uniforms or sentimental items?

Comfort and Preferences
- Poses they love/hate:
- Side preferences:
- Insecurities to be mindful of:

Children / Pets
- Nap schedules:
- Favorite toys:
- Treat permissions:
- Safety concerns:",
                    IsActive = true
                },
                new QuestionnaireTemplate
                {
                    Name = "Model Release Questionnaire",
                    Category = "Release",
                    Description = "Legal protection and clarity on image usage.",
                    QuestionText = @"Model Release Questionnaire (Integrated or Standalone)

Purpose
- Legal protection and clarity on image usage.

When used
- Before or immediately after the shoot.

Consent
- Permission to photograph:
- Permission to edit:
- Permission to use images for marketing:

Usage Scope
- Website:
- Social media:
- Advertising:
- Portfolio:

Restrictions
- Any limitations on usage?
- Any platforms explicitly excluded?",
                    IsActive = true
                },
                new QuestionnaireTemplate
                {
                    Name = "Post-Shoot Feedback and Delivery Preferences",
                    Category = "Post-Shoot",
                    Description = "Improves future work and prevents revision disputes.",
                    QuestionText = @"Post-Shoot Feedback and Delivery Preferences

Purpose
- Improve future work and prevent revision disputes.

When used
- After gallery delivery.

Delivery
- Preferred delivery format:
- Print vs digital priorities:
- Album interest:

Feedback
- Favorite images:
- Least favorite images (optional):
- What could be improved?

Testimonials
- Permission to request testimonial:
- Permission to publish testimonial:",
                    IsActive = true
                },
                new QuestionnaireTemplate
                {
                    Name = "Invoice / Payment Confirmation Questionnaire",
                    Category = "Billing",
                    Description = "Reduces payment friction and disputes before invoicing or final delivery.",
                    QuestionText = @"Invoice / Payment Confirmation Questionnaire

Purpose
- Reduce payment friction and disputes.

When used
- Before invoicing or final delivery.

Billing Confirmation
- Confirm billing email:
- Confirm billing address:
- Purchase order number (if applicable):
- Tax-exempt status:
- Acknowledgement of payment terms:",
                    IsActive = true
                },
                new QuestionnaireTemplate
                {
                    Name = "Creative Control Acknowledgement",
                    Category = "Policy",
                    Description = "Protects artistic discretion and sets editing expectations.",
                    QuestionText = @"Creative Control Acknowledgement (Optional but Highly Professional)

Purpose
- Protects your artistic discretion.

Key Points
- Photographer retains creative control:
- Editing style consistency:
- No guarantees on specific poses unless agreed:",
                    IsActive = true
                },
                new QuestionnaireTemplate
                {
                    Name = "Weather and Rescheduling Policy Acknowledgement",
                    Category = "Policy",
                    Description = "Avoids disputes related to weather, rescheduling, and refunds.",
                    QuestionText = @"Weather and Rescheduling Policy Acknowledgement

Purpose
- Avoid 'but it was cloudy' arguments.

Key Points
- Weather conditions acceptable:
- Reschedule criteria:
- Refund vs credit terms:",
                    IsActive = true
                }
            };

            var templatesToAdd = templates
                .Where(t => !existingNames.Contains(t.Name))
                .ToList();

            if (templatesToAdd.Count == 0)
            {
                logger.LogInformation("Questionnaire Templates already exist. Skipping.");
                return;
            }

            await context.QuestionnaireTemplates.AddRangeAsync(templatesToAdd);
            await context.SaveChangesAsync();
            logger.LogInformation("Seeded {Count} Questionnaire Templates.", templatesToAdd.Count);
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
