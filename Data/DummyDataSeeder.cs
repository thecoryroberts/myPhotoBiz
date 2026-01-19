using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MyPhotoBiz.Data;
using MyPhotoBiz.Enums;
using MyPhotoBiz.Models;

namespace MyPhotoBiz.Data
{
    public static class DummyDataSeeder
    {
        public static async Task SeedAsync(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var logger = scope.ServiceProvider.GetRequiredService<ILogger<ApplicationDbContext>>();

            try
            {
                // Check if data already exists
                if (await context.ServicePackages.AnyAsync())
                {
                    logger.LogInformation("Database already contains seed data. Skipping dummy data seeding.");
                    return;
                }

                logger.LogInformation("Seeding comprehensive dummy data...");

                // 1. Seed Roles
                await SeedRolesAsync(roleManager, logger);

                // 2. Seed Users (Admin, Photographers, Clients)
                var (adminUser, photographers, clientUsers) = await SeedUsersAsync(userManager, logger);

                // 3. Seed Photographer Profiles
                var photographerProfiles = await SeedPhotographerProfilesAsync(context, photographers, logger);

                // 4. Seed Client Profiles
                var clientProfiles = await SeedClientProfilesAsync(context, clientUsers, logger);

                // 5. Seed Badges
                var badges = await SeedBadgesAsync(context, logger);

                // 6. Seed Service Packages with Add-ons
                var servicePackages = await SeedServicePackagesAsync(context, logger);

                // 7. Seed Contract Templates
                var contractTemplates = await SeedContractTemplatesAsync(context, badges, logger);

                // 8. Seed Print Pricing
                await SeedPrintPricingAsync(context, logger);

                // 9. Seed Permissions
                await SeedPermissionsAsync(context, logger);

                // 10. Seed PhotoShoots
                var photoShoots = await SeedPhotoShootsAsync(context, clientProfiles, photographerProfiles, logger);

                // 11. Seed Albums and Photos
                await SeedAlbumsAndPhotosAsync(context, photoShoots, clientProfiles, logger);

                // 12. Seed Galleries
                var galleries = await SeedGalleriesAsync(context, logger);

                // 13. Seed Gallery Access
                await SeedGalleryAccessAsync(context, galleries, clientProfiles, logger);

                // 14. Seed Invoices with Items and Payments
                await SeedInvoicesAsync(context, clientProfiles, photoShoots, logger);

                // 15. Seed Contracts
                await SeedContractsAsync(context, clientProfiles, photoShoots, contractTemplates, logger);

                // 16. Seed Booking Requests
                await SeedBookingRequestsAsync(context, clientProfiles, photographerProfiles, servicePackages, logger);

                // 17. Seed Client Badges
                await SeedClientBadgesAsync(context, clientProfiles, badges, logger);

                // 18. Seed Notifications
                await SeedNotificationsAsync(context, clientUsers, logger);

                // 19. Seed Activities
                await SeedActivitiesAsync(context, adminUser, clientUsers, logger);

                // 20. Seed Tags
                await SeedTagsAsync(context, logger);

                logger.LogInformation("Comprehensive dummy data seeding completed successfully.");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred while seeding dummy data.");
                throw;
            }
        }

        private static async Task SeedRolesAsync(RoleManager<IdentityRole> roleManager, ILogger logger)
        {
            string[] roles = { "Admin", "Photographer", "Client", "Manager" };

            foreach (var roleName in roles)
            {
                if (!await roleManager.RoleExistsAsync(roleName))
                {
                    await roleManager.CreateAsync(new IdentityRole(roleName));
                }
            }
            logger.LogInformation("Seeded Roles.");
        }

        private static async Task<(ApplicationUser admin, List<ApplicationUser> photographers, List<ApplicationUser> clients)> SeedUsersAsync(
            UserManager<ApplicationUser> userManager, ILogger logger)
        {
            // Create Admin User
            var adminUser = await CreateUserIfNotExistsAsync(userManager, new ApplicationUser
            {
                UserName = "admin@myphotobiz.com",
                Email = "admin@myphotobiz.com",
                FirstName = "Admin",
                LastName = "User",
                EmailConfirmed = true,
                UserType = UserType.Admin,
                IsActive = true
            }, "Admin123!123", "Admin");

            if (adminUser == null)
                throw new InvalidOperationException("Failed to create admin user");

            // Create Photographer Users
            var photographers = new List<ApplicationUser>();
            var photographerData = new[]
            {
                ("john.photographer@myphotobiz.com", "John", "Smith"),
                ("sarah.photographer@myphotobiz.com", "Sarah", "Johnson"),
                ("mike.photographer@myphotobiz.com", "Mike", "Williams")
            };

            foreach (var (email, firstName, lastName) in photographerData)
            {
                var photographer = await CreateUserIfNotExistsAsync(userManager, new ApplicationUser
                {
                    UserName = email,
                    Email = email,
                    FirstName = firstName,
                    LastName = lastName,
                    EmailConfirmed = true,
                    UserType = UserType.Photographer,
                    IsActive = true
                }, "Photo123!123", "Photographer");

                if (photographer != null)
                    photographers.Add(photographer);
            }

            // Create Client Users
            var clients = new List<ApplicationUser>();
            var clientData = new[]
            {
                ("emily.client@example.com", "Emily", "Davis", "555-0101"),
                ("james.client@example.com", "James", "Brown", "555-0102"),
                ("olivia.client@example.com", "Olivia", "Miller", "555-0103"),
                ("william.client@example.com", "William", "Wilson", "555-0104"),
                ("sophia.client@example.com", "Sophia", "Moore", "555-0105"),
                ("benjamin.client@example.com", "Benjamin", "Taylor", "555-0106"),
                ("ava.client@example.com", "Ava", "Anderson", "555-0107"),
                ("lucas.client@example.com", "Lucas", "Thomas", "555-0108")
            };

            foreach (var (email, firstName, lastName, phone) in clientData)
            {
                var client = await CreateUserIfNotExistsAsync(userManager, new ApplicationUser
                {
                    UserName = email,
                    Email = email,
                    FirstName = firstName,
                    LastName = lastName,
                    PhoneNumber = phone,
                    EmailConfirmed = true,
                    UserType = UserType.Client,
                    IsActive = true
                }, "Client123!123", "Client");

                if (client != null)
                    clients.Add(client);
            }

            logger.LogInformation("Seeded {PhotographerCount} photographers and {ClientCount} clients.", photographers.Count, clients.Count);
            return (adminUser, photographers, clients);
        }

        private static async Task<ApplicationUser?> CreateUserIfNotExistsAsync(
            UserManager<ApplicationUser> userManager, ApplicationUser user, string password, string role)
        {
            var existingUser = await userManager.FindByEmailAsync(user.Email!);
            if (existingUser != null)
                return existingUser;

            var result = await userManager.CreateAsync(user, password);
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(user, role);
                // Re-fetch the user to ensure we have the database-generated ID
                var createdUser = await userManager.FindByEmailAsync(user.Email!);
                return createdUser;
            }
            else
            {
                // Log errors - user creation failed
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                throw new InvalidOperationException($"Failed to create user {user.Email}: {errors}");
            }
        }

        private static async Task<List<PhotographerProfile>> SeedPhotographerProfilesAsync(
            ApplicationDbContext context, List<ApplicationUser> photographers, ILogger logger)
        {
            // Check if profiles already exist
            if (await context.PhotographerProfiles.AnyAsync())
            {
                logger.LogInformation("Photographer Profiles already exist. Skipping.");
                return await context.PhotographerProfiles.ToListAsync();
            }

            var profiles = new List<PhotographerProfile>();
            var specialties = new[] { "Weddings, Portraits", "Events, Corporate", "Family, Newborn" };
            var bios = new[]
            {
                "Professional wedding and portrait photographer with 10+ years of experience.",
                "Specializing in corporate events and commercial photography.",
                "Passionate about capturing family moments and newborn photography."
            };

            for (int i = 0; i < photographers.Count; i++)
            {
                // Skip if user ID is empty
                if (string.IsNullOrEmpty(photographers[i].Id))
                {
                    logger.LogWarning("Photographer {Email} has no ID. Skipping profile creation.", photographers[i].Email);
                    continue;
                }

                // Verify user actually exists in database
                var userExists = await context.Users.AnyAsync(u => u.Id == photographers[i].Id);
                if (!userExists)
                {
                    logger.LogWarning("Photographer {Email} with ID {Id} not found in database. Skipping profile creation.", photographers[i].Email, photographers[i].Id);
                    continue;
                }

                // Check if profile already exists for this user
                if (await context.PhotographerProfiles.AnyAsync(p => p.UserId == photographers[i].Id))
                    continue;

                var profile = new PhotographerProfile
                {
                    UserId = photographers[i].Id,
                    Bio = bios[i % bios.Length],
                    Specialties = specialties[i % specialties.Length],
                    PortfolioUrl = $"https://portfolio.myphotobiz.com/{photographers[i].FirstName.ToLower()}",
                    HourlyRate = 150m + (i * 25),
                    IsAvailable = true
                };
                profiles.Add(profile);
            }

            if (profiles.Count > 0)
            {
                await context.PhotographerProfiles.AddRangeAsync(profiles);
                await context.SaveChangesAsync();
                logger.LogInformation("Seeded Photographer Profiles.");
            }
            return profiles;
        }

        private static async Task<List<ClientProfile>> SeedClientProfilesAsync(
            ApplicationDbContext context, List<ApplicationUser> clients, ILogger logger)
        {
            // Check if profiles already exist
            if (await context.ClientProfiles.AnyAsync())
            {
                logger.LogInformation("Client Profiles already exist. Skipping.");
                return await context.ClientProfiles.ToListAsync();
            }

            var profiles = new List<ClientProfile>();
            var categories = new[] { ClientCategory.Regular, ClientCategory.VIP, ClientCategory.Corporate, ClientCategory.Prospect };
            var referralSources = new[] { ReferralSource.Website, ReferralSource.WordOfMouth, ReferralSource.SocialMedia, ReferralSource.Referral };

            for (int i = 0; i < clients.Count; i++)
            {
                // Skip if user ID is empty
                if (string.IsNullOrEmpty(clients[i].Id))
                {
                    logger.LogWarning("Client {Email} has no ID. Skipping profile creation.", clients[i].Email);
                    continue;
                }

                // Verify user actually exists in database
                var userExists = await context.Users.AnyAsync(u => u.Id == clients[i].Id);
                if (!userExists)
                {
                    logger.LogWarning("Client {Email} with ID {Id} not found in database. Skipping profile creation.", clients[i].Email, clients[i].Id);
                    continue;
                }

                // Check if profile already exists for this user
                if (await context.ClientProfiles.AnyAsync(p => p.UserId == clients[i].Id))
                    continue;

                var profile = new ClientProfile
                {
                    UserId = clients[i].Id,
                    PhoneNumber = clients[i].PhoneNumber,
                    Address = $"{100 + i} Main Street, City, State 12345",
                    Notes = $"Client notes for {clients[i].FirstName}",
                    Status = ClientStatus.Active,
                    Category = categories[i % categories.Length],
                    ReferralSource = referralSources[i % referralSources.Length],
                    ContactPreference = ContactPreference.Email
                };
                profiles.Add(profile);
            }

            if (profiles.Count > 0)
            {
                await context.ClientProfiles.AddRangeAsync(profiles);
                await context.SaveChangesAsync();
                logger.LogInformation("Seeded Client Profiles.");
            }
            return profiles;
        }

        private static async Task<List<Badge>> SeedBadgesAsync(ApplicationDbContext context, ILogger logger)
        {
            var badges = new List<Badge>
            {
                new Badge { Name = "Contract Signed", Description = "Awarded when a client signs their first contract", Type = BadgeType.ContractSigned, Icon = "ti-signature", Color = "#28a745", IsActive = true },
                new Badge { Name = "VIP Client", Description = "Special status for valued clients", Type = BadgeType.VIPClient, Icon = "ti-star", Color = "#ffc107", IsActive = true },
                new Badge { Name = "First Session Complete", Description = "Completed first photo session", Type = BadgeType.FirstSession, Icon = "ti-camera", Color = "#17a2b8", IsActive = true },
                new Badge { Name = "Returning Client", Description = "Booked multiple sessions", Type = BadgeType.ReturningClient, Icon = "ti-heart", Color = "#e83e8c", IsActive = true },
                new Badge { Name = "Payment Complete", Description = "All invoices paid in full", Type = BadgeType.PaymentCompleted, Icon = "ti-check", Color = "#20c997", IsActive = true },
                new Badge { Name = "Model Release Signed", Description = "Signed model release form", Type = BadgeType.ReleaseFormSigned, Icon = "ti-file-check", Color = "#6f42c1", IsActive = true },
                new Badge { Name = "Early Bird", Description = "Booked well in advance", Type = BadgeType.EarlyBird, Icon = "ti-clock", Color = "#fd7e14", IsActive = true },
                new Badge { Name = "Referral Champion", Description = "Referred new clients", Type = BadgeType.ReferralSource, Icon = "ti-users", Color = "#007bff", IsActive = true }
            };

            await context.Badges.AddRangeAsync(badges);
            await context.SaveChangesAsync();
            logger.LogInformation("Seeded Badges.");
            return badges;
        }

        private static async Task<List<ServicePackage>> SeedServicePackagesAsync(ApplicationDbContext context, ILogger logger)
        {
            var packages = new List<ServicePackage>
            {
                new ServicePackage
                {
                    Name = "Wedding Essential",
                    Description = "Perfect for intimate weddings",
                    DetailedDescription = "6 hours of coverage, 300+ edited photos, online gallery, print release",
                    Category = "Wedding",
                    BasePrice = 2500m,
                    DurationHours = 6,
                    IncludedPhotos = 300,
                    IncludesDigitalGallery = true,
                    NumberOfLocations = 2,
                    DisplayOrder = 1,
                    IsActive = true,
                    IsFeatured = true,
                    IncludedFeatures = "Engagement Session,Online Gallery,Print Release,Second Photographer"
                },
                new ServicePackage
                {
                    Name = "Wedding Premium",
                    Description = "Complete wedding day coverage",
                    DetailedDescription = "10 hours of coverage, 500+ edited photos, premium album, engagement session",
                    Category = "Wedding",
                    BasePrice = 4500m,
                    DiscountedPrice = 3999m,
                    DurationHours = 10,
                    IncludedPhotos = 500,
                    IncludesDigitalGallery = true,
                    IncludesAlbum = true,
                    IncludesPrints = true,
                    NumberOfPrints = 20,
                    NumberOfLocations = 3,
                    DisplayOrder = 2,
                    IsActive = true,
                    IsFeatured = true,
                    IncludedFeatures = "Engagement Session,Premium Album,Canvas Print,Second Photographer,Same-Day Preview"
                },
                new ServicePackage
                {
                    Name = "Portrait Mini Session",
                    Description = "Quick 30-minute session",
                    Category = "Portrait",
                    BasePrice = 199m,
                    DurationHours = 0.5m,
                    IncludedPhotos = 15,
                    IncludesDigitalGallery = true,
                    NumberOfLocations = 1,
                    OutfitChanges = 1,
                    DisplayOrder = 3,
                    IsActive = true,
                    IncludedFeatures = "15 Edited Photos,Online Gallery,Print Release"
                },
                new ServicePackage
                {
                    Name = "Portrait Standard",
                    Description = "1-hour portrait session",
                    Category = "Portrait",
                    BasePrice = 350m,
                    DurationHours = 1,
                    IncludedPhotos = 40,
                    IncludesDigitalGallery = true,
                    NumberOfLocations = 1,
                    OutfitChanges = 2,
                    DisplayOrder = 4,
                    IsActive = true,
                    IncludedFeatures = "40 Edited Photos,Online Gallery,Print Release,Outfit Changes"
                },
                new ServicePackage
                {
                    Name = "Family Session",
                    Description = "Capture your family memories",
                    Category = "Family",
                    BasePrice = 450m,
                    DurationHours = 1.5m,
                    IncludedPhotos = 50,
                    IncludesDigitalGallery = true,
                    NumberOfLocations = 1,
                    DisplayOrder = 5,
                    IsActive = true,
                    IncludedFeatures = "50 Edited Photos,Online Gallery,Print Release,Multiple Groupings"
                },
                new ServicePackage
                {
                    Name = "Newborn Session",
                    Description = "Professional newborn photography",
                    Category = "Newborn",
                    BasePrice = 550m,
                    DurationHours = 3,
                    IncludedPhotos = 35,
                    IncludesDigitalGallery = true,
                    DisplayOrder = 6,
                    IsActive = true,
                    IncludedFeatures = "35 Edited Photos,Props Included,Parent Poses,Sibling Poses"
                },
                new ServicePackage
                {
                    Name = "Corporate Headshots",
                    Description = "Professional business portraits",
                    Category = "Corporate",
                    BasePrice = 250m,
                    DurationHours = 0.5m,
                    IncludedPhotos = 5,
                    IncludesDigitalGallery = true,
                    DisplayOrder = 7,
                    IsActive = true,
                    IncludedFeatures = "5 Retouched Images,Multiple Backgrounds,LinkedIn Ready"
                },
                new ServicePackage
                {
                    Name = "Event Coverage",
                    Description = "Corporate or private events",
                    Category = "Event",
                    BasePrice = 800m,
                    DurationHours = 4,
                    IncludedPhotos = 200,
                    IncludesDigitalGallery = true,
                    DisplayOrder = 8,
                    IsActive = true,
                    IncludedFeatures = "200+ Photos,Quick Turnaround,Online Gallery,Commercial License"
                }
            };

            await context.ServicePackages.AddRangeAsync(packages);
            await context.SaveChangesAsync();

            // Add Add-ons to packages
            var addOns = new List<PackageAddOn>
            {
                new PackageAddOn { ServicePackageId = packages[0].Id, Name = "Extra Hour", Description = "Additional hour of coverage", Price = 300m, Category = "Time", IsActive = true },
                new PackageAddOn { ServicePackageId = packages[0].Id, Name = "Second Photographer", Description = "Additional photographer for the day", Price = 500m, Category = "Service", IsActive = true },
                new PackageAddOn { ServicePackageId = packages[1].Id, Name = "Parent Album", Description = "Duplicate album for parents", Price = 350m, Category = "Albums", IsActive = true },
                new PackageAddOn { ServicePackageId = packages[1].Id, Name = "Canvas Print 16x20", Description = "Gallery-wrapped canvas", Price = 150m, Category = "Prints", IsActive = true },
                new PackageAddOn { Name = "Rush Editing", Description = "Expedited 48-hour turnaround", Price = 200m, Category = "Service", IsStandalone = true, IsActive = true },
                new PackageAddOn { Name = "Print Package (10 prints)", Description = "10 assorted prints up to 8x10", Price = 175m, Category = "Prints", IsStandalone = true, IsActive = true },
                new PackageAddOn { Name = "Additional Location", Description = "Extra location within 25 miles", Price = 100m, Category = "Location", IsStandalone = true, IsActive = true },
                new PackageAddOn { Name = "Hair & Makeup Referral", Description = "Professional styling coordination", Price = 0m, Category = "Service", IsStandalone = true, IsActive = true }
            };

            await context.PackageAddOns.AddRangeAsync(addOns);
            await context.SaveChangesAsync();
            logger.LogInformation("Seeded Service Packages and Add-ons.");
            return packages;
        }

        private static async Task<List<ContractTemplate>> SeedContractTemplatesAsync(
            ApplicationDbContext context, List<Badge> badges, ILogger logger)
        {
            var templates = new List<ContractTemplate>
            {
                new ContractTemplate
                {
                    Name = "Wedding Photography Contract",
                    Description = "Standard contract for wedding photography services",
                    Category = "Wedding",
                    ContentTemplate = @"<h1>Wedding Photography Contract</h1>
<p>This agreement is entered into between {{PhotographerName}} (""Photographer"") and {{ClientName}} (""Client"").</p>
<h2>Event Details</h2>
<p>Date: {{EventDate}}<br>Location: {{Location}}<br>Package: {{PackageName}}</p>
<h2>Terms and Conditions</h2>
<p>1. The Photographer agrees to provide photography services as described in the selected package.</p>
<p>2. A non-refundable retainer of 30% is required to secure the date.</p>
<p>3. Final payment is due 14 days before the event date.</p>
<h2>Signatures</h2>
<p>Client: ______________________ Date: ______</p>",
                    IsActive = true,
                    AwardBadgeOnSign = true,
                    BadgeToAwardId = badges.First(b => b.Type == BadgeType.ContractSigned).Id
                },
                new ContractTemplate
                {
                    Name = "Portrait Session Agreement",
                    Description = "Contract for portrait and family sessions",
                    Category = "Portrait",
                    ContentTemplate = @"<h1>Portrait Session Agreement</h1>
<p>Client: {{ClientName}}<br>Session Date: {{EventDate}}<br>Location: {{Location}}</p>
<h2>Session Details</h2>
<p>Package: {{PackageName}}<br>Duration: {{Duration}}</p>
<h2>Terms</h2>
<p>Full payment is required at time of booking. Rescheduling is allowed with 48-hour notice.</p>",
                    IsActive = true
                },
                new ContractTemplate
                {
                    Name = "Model Release Form",
                    Description = "Standard model release for commercial use",
                    Category = "Release",
                    ContentTemplate = @"<h1>Model Release Agreement</h1>
<p>I, {{ClientName}}, hereby grant {{PhotographerName}} permission to use my photographs for promotional purposes.</p>
<p>Signed: ______________________ Date: ______</p>",
                    IsActive = true,
                    AwardBadgeOnSign = true,
                    BadgeToAwardId = badges.First(b => b.Type == BadgeType.ReleaseFormSigned).Id
                },
                new ContractTemplate
                {
                    Name = "Event Photography Contract",
                    Description = "Contract for corporate and private events",
                    Category = "Event",
                    ContentTemplate = @"<h1>Event Photography Contract</h1>
<p>Event: {{EventName}}<br>Date: {{EventDate}}<br>Client: {{ClientName}}</p>
<h2>Services</h2>
<p>The photographer will provide coverage as agreed upon.</p>",
                    IsActive = true
                },
                new ContractTemplate
                {
                    Name = "Minor Model Release",
                    Description = "Release form for clients under 18",
                    Category = "Release",
                    ContentTemplate = @"<h1>Minor Model Release</h1>
<p>Parent/Guardian: {{GuardianName}}<br>Minor: {{ClientName}}</p>
<p>I hereby grant permission for photographs of my minor child to be used for promotional purposes.</p>",
                    IsActive = true
                }
            };

            await context.ContractTemplates.AddRangeAsync(templates);
            await context.SaveChangesAsync();
            logger.LogInformation("Seeded Contract Templates.");
            return templates;
        }

        private static async Task SeedPrintPricingAsync(ApplicationDbContext context, ILogger logger)
        {
            if (await context.PrintPricings.AnyAsync())
                return;

            var pricing = PrintPricing.GetDefaultPrices();
            // Clear IDs to let database generate them
            foreach (var p in pricing)
                p.Id = 0;

            await context.PrintPricings.AddRangeAsync(pricing);
            await context.SaveChangesAsync();
            logger.LogInformation("Seeded Print Pricing.");
        }

        private static async Task SeedPermissionsAsync(ApplicationDbContext context, ILogger logger)
        {
            var permissions = new List<Permission>
            {
                new Permission { Name = "ManageClients", Description = "Create, edit, and delete clients" },
                new Permission { Name = "ManagePhotoShoots", Description = "Create, edit, and delete photo shoots" },
                new Permission { Name = "ManageInvoices", Description = "Create, edit, and delete invoices" },
                new Permission { Name = "ManageContracts", Description = "Create, edit, and delete contracts" },
                new Permission { Name = "ManageGalleries", Description = "Create, edit, and delete galleries" },
                new Permission { Name = "ManageUsers", Description = "Create, edit, and delete users" },
                new Permission { Name = "ViewReports", Description = "Access business reports and analytics" },
                new Permission { Name = "ManageSettings", Description = "Configure application settings" },
                new Permission { Name = "ManagePackages", Description = "Create, edit, and delete service packages" },
                new Permission { Name = "ProcessPayments", Description = "Process and refund payments" }
            };

            await context.Permissions.AddRangeAsync(permissions);
            await context.SaveChangesAsync();
            logger.LogInformation("Seeded Permissions.");
        }

        private static async Task<List<PhotoShoot>> SeedPhotoShootsAsync(
            ApplicationDbContext context, List<ClientProfile> clients, List<PhotographerProfile> photographers, ILogger logger)
        {
            var photoShoots = new List<PhotoShoot>();
            var shootTypes = new[] { ShootType.Wedding, ShootType.Portrait, ShootType.Family, ShootType.Event, ShootType.Newborn, ShootType.Corporate };
            var statuses = new[] { PhotoShootStatus.Scheduled, PhotoShootStatus.Completed, PhotoShootStatus.InProgress };
            var locations = new[] { "Studio A - Downtown", "Central Park", "Beach Location", "Client's Home", "Corporate Office", "Garden Venue" };

            for (int i = 0; i < Math.Min(clients.Count, 10); i++)
            {
                var scheduledDate = DateTime.UtcNow.AddDays(-30 + (i * 7));
                var shootType = shootTypes[i % shootTypes.Length];

                photoShoots.Add(new PhotoShoot
                {
                    Title = $"{shootType} Session - {clients[i % clients.Count].User?.FirstName ?? "Client"}",
                    Description = $"Professional {shootType.ToString().ToLower()} photography session",
                    ScheduledDate = scheduledDate,
                    EndTime = scheduledDate.AddHours(2 + (i % 4)),
                    DurationHours = 2 + (i % 4),
                    Location = locations[i % locations.Length],
                    Price = 300m + (i * 150),
                    ShootType = shootType,
                    Status = statuses[i % statuses.Length],
                    ClientProfileId = clients[i % clients.Count].Id,
                    PhotographerProfileId = photographers[i % photographers.Count].Id,
                    Notes = $"Session notes for {shootType} shoot"
                });
            }

            await context.PhotoShoots.AddRangeAsync(photoShoots);
            await context.SaveChangesAsync();
            logger.LogInformation("Seeded PhotoShoots.");
            return photoShoots;
        }

        private static async Task SeedAlbumsAndPhotosAsync(
            ApplicationDbContext context, List<PhotoShoot> photoShoots, List<ClientProfile> clients, ILogger logger)
        {
            foreach (var photoShoot in photoShoots.Take(5))
            {
                var clientProfile = clients.FirstOrDefault(c => c.Id == photoShoot.ClientProfileId) ?? clients.First();
                var albumNames = new[] { "Highlights", "Full Gallery", "Black & White" };

                foreach (var albumName in albumNames)
                {
                    var album = new Album
                    {
                        Name = albumName,
                        Description = $"{albumName} from {photoShoot.Title}",
                        PhotoShootId = photoShoot.Id,
                        ClientProfileId = clientProfile.Id,
                        IsPublic = albumName == "Highlights"
                    };

                    await context.Albums.AddAsync(album);
                    await context.SaveChangesAsync();

                    // Add photos to album
                    var photoCount = albumName == "Full Gallery" ? 25 : 10;
                    var photos = new List<Photo>();

                    for (int i = 1; i <= photoCount; i++)
                    {
                        photos.Add(new Photo
                        {
                            AlbumId = album.Id,
                            ClientProfileId = clientProfile.Id,
                            FileName = $"IMG_{photoShoot.Id}_{album.Id}_{i:D4}.jpg",
                            Title = $"Photo {i}",
                            FilePath = $"/uploads/photos/{photoShoot.Id}/{album.Id}/IMG_{i:D4}.jpg",
                            ThumbnailPath = $"/uploads/photos/{photoShoot.Id}/{album.Id}/thumbs/IMG_{i:D4}.jpg",
                            FullImagePath = $"/uploads/photos/{photoShoot.Id}/{album.Id}/full/IMG_{i:D4}.jpg",
                            DisplayOrder = i,
                            FileSize = 2500000 + (i * 100000),
                            UploadDate = DateTime.UtcNow.AddDays(-i),
                            UploadedDate = DateTime.UtcNow.AddDays(-i)
                        });
                    }

                    await context.Photos.AddRangeAsync(photos);
                }
            }

            await context.SaveChangesAsync();
            logger.LogInformation("Seeded Albums and Photos.");
        }

        private static async Task<List<Gallery>> SeedGalleriesAsync(ApplicationDbContext context, ILogger logger)
        {
            var galleries = new List<Gallery>
            {
                new Gallery
                {
                    Name = "Smith Wedding - June 2025",
                    Description = "Beautiful summer wedding at the botanical gardens",
                    IsActive = true,
                    ExpiryDate = DateTime.UtcNow.AddMonths(6),
                    BrandColor = "#2c3e50",
                    AllowPublicAccess = false,
                    WatermarkEnabled = true,
                    WatermarkText = "© MyPhotoBiz",
                    WatermarkOpacity = 0.3f
                },
                new Gallery
                {
                    Name = "Johnson Family Portraits",
                    Description = "Annual family portrait session",
                    IsActive = true,
                    ExpiryDate = DateTime.UtcNow.AddMonths(3),
                    BrandColor = "#27ae60",
                    AllowPublicAccess = true
                },
                new Gallery
                {
                    Name = "Corporate Event 2025",
                    Description = "Annual company conference photography",
                    IsActive = true,
                    ExpiryDate = DateTime.UtcNow.AddMonths(12),
                    BrandColor = "#3498db",
                    AllowPublicAccess = false
                },
                new Gallery
                {
                    Name = "Senior Portraits - Class of 2025",
                    Description = "High school senior portrait collection",
                    IsActive = true,
                    ExpiryDate = DateTime.UtcNow.AddMonths(6),
                    BrandColor = "#9b59b6"
                },
                new Gallery
                {
                    Name = "Newborn Session - Baby Emma",
                    Description = "Precious newborn photography session",
                    IsActive = true,
                    ExpiryDate = DateTime.UtcNow.AddMonths(12),
                    BrandColor = "#e91e63",
                    AllowPublicAccess = false
                }
            };

            // Generate public access tokens for galleries that allow public access
            foreach (var gallery in galleries.Where(g => g.AllowPublicAccess))
            {
                gallery.GeneratePublicAccessToken();
            }

            await context.Galleries.AddRangeAsync(galleries);
            await context.SaveChangesAsync();
            logger.LogInformation("Seeded Galleries.");
            return galleries;
        }

        private static async Task SeedGalleryAccessAsync(
            ApplicationDbContext context, List<Gallery> galleries, List<ClientProfile> clients, ILogger logger)
        {
            var accesses = new List<GalleryAccess>();

            for (int i = 0; i < galleries.Count; i++)
            {
                accesses.Add(new GalleryAccess
                {
                    GalleryId = galleries[i].Id,
                    ClientProfileId = clients[i % clients.Count].Id,
                    CanDownload = true,
                    CanProof = true,
                    CanOrder = true,
                    ExpiryDate = DateTime.UtcNow.AddMonths(3)
                });
            }

            await context.GalleryAccesses.AddRangeAsync(accesses);
            await context.SaveChangesAsync();
            logger.LogInformation("Seeded Gallery Access.");
        }

        private static async Task SeedInvoicesAsync(
            ApplicationDbContext context, List<ClientProfile> clients, List<PhotoShoot> photoShoots, ILogger logger)
        {
            var statuses = new[] { InvoiceStatus.Paid, InvoiceStatus.Pending, InvoiceStatus.Overdue, InvoiceStatus.Draft };
            var invoices = new List<Invoice>();

            for (int i = 0; i < Math.Min(clients.Count, 8); i++)
            {
                var status = statuses[i % statuses.Length];
                var invoice = new Invoice
                {
                    InvoiceNumber = $"INV-2025-{(1000 + i):D4}",
                    InvoiceDate = DateTime.UtcNow.AddDays(-30 + i),
                    DueDate = DateTime.UtcNow.AddDays(-15 + i),
                    Status = status,
                    Amount = 500m + (i * 250),
                    Tax = (500m + (i * 250)) * 0.08m,
                    ClientProfileId = clients[i].Id,
                    PhotoShootId = i < photoShoots.Count ? photoShoots[i].Id : null,
                    Notes = $"Invoice for photography services",
                    DepositAmount = (500m + (i * 250)) * 0.3m,
                    DepositPaid = status == InvoiceStatus.Paid || i % 2 == 0,
                    PaidDate = status == InvoiceStatus.Paid ? DateTime.UtcNow.AddDays(-5 + i) : null
                };
                invoices.Add(invoice);
            }

            await context.Invoices.AddRangeAsync(invoices);
            await context.SaveChangesAsync();

            // Add invoice items
            foreach (var invoice in invoices)
            {
                var items = new List<InvoiceItem>
                {
                    new InvoiceItem { InvoiceId = invoice.Id, Description = "Photography Session", Quantity = 1, UnitPrice = invoice.Amount * 0.7m },
                    new InvoiceItem { InvoiceId = invoice.Id, Description = "Photo Editing", Quantity = 1, UnitPrice = invoice.Amount * 0.2m },
                    new InvoiceItem { InvoiceId = invoice.Id, Description = "Online Gallery Access", Quantity = 1, UnitPrice = invoice.Amount * 0.1m }
                };
                await context.InvoiceItems.AddRangeAsync(items);
            }

            // Add payments for paid invoices
            foreach (var invoice in invoices.Where(i => i.Status == InvoiceStatus.Paid))
            {
                var payment = new Payment
                {
                    InvoiceId = invoice.Id,
                    Amount = invoice.TotalAmount,
                    PaymentDate = invoice.PaidDate ?? DateTime.UtcNow,
                    PaymentMethod = PaymentMethod.CreditCard,
                    TransactionId = $"TXN-{Guid.NewGuid().ToString()[..8].ToUpper()}",
                    Notes = "Payment received"
                };
                await context.Payments.AddAsync(payment);
            }

            await context.SaveChangesAsync();
            logger.LogInformation("Seeded Invoices, Items, and Payments.");
        }

        private static async Task SeedContractsAsync(
            ApplicationDbContext context, List<ClientProfile> clients, List<PhotoShoot> photoShoots,
            List<ContractTemplate> templates, ILogger logger)
        {
            var statuses = new[] { ContractStatus.Signed, ContractStatus.PendingSignature, ContractStatus.Draft, ContractStatus.Expired };
            var contracts = new List<Contract>();

            for (int i = 0; i < Math.Min(clients.Count, 6); i++)
            {
                var status = statuses[i % statuses.Length];
                var template = templates[i % templates.Count];

                contracts.Add(new Contract
                {
                    Title = $"{template.Name} - {clients[i].User?.FullName ?? "Client"}",
                    Content = template.ContentTemplate,
                    Status = status,
                    ClientProfileId = clients[i].Id,
                    PhotoShootId = i < photoShoots.Count ? photoShoots[i].Id : null,
                    SignedDate = status == ContractStatus.Signed ? DateTime.UtcNow.AddDays(-10 + i) : null,
                    SentDate = status != ContractStatus.Draft ? DateTime.UtcNow.AddDays(-15 + i) : null,
                    AwardBadgeOnSign = template.AwardBadgeOnSign,
                    BadgeToAwardId = template.BadgeToAwardId
                });
            }

            await context.Contracts.AddRangeAsync(contracts);
            await context.SaveChangesAsync();
            logger.LogInformation("Seeded Contracts.");
        }

        private static async Task SeedBookingRequestsAsync(
            ApplicationDbContext context, List<ClientProfile> clients, List<PhotographerProfile> photographers,
            List<ServicePackage> packages, ILogger logger)
        {
            var statuses = new[] { BookingStatus.Pending, BookingStatus.Confirmed, BookingStatus.Completed, BookingStatus.Cancelled };
            var eventTypes = new[] { "Wedding", "Portrait Session", "Family Photos", "Corporate Event", "Engagement" };
            var bookings = new List<BookingRequest>();

            for (int i = 0; i < Math.Min(clients.Count, 6); i++)
            {
                var status = statuses[i % statuses.Length];
                var package = packages[i % packages.Count];

                bookings.Add(new BookingRequest
                {
                    BookingReference = BookingRequest.GenerateBookingReference(),
                    ClientProfileId = clients[i].Id,
                    PhotographerProfileId = i % 2 == 0 ? photographers[i % photographers.Count].Id : null,
                    ServicePackageId = package.Id,
                    EventType = eventTypes[i % eventTypes.Length],
                    PreferredDate = DateTime.UtcNow.AddDays(14 + (i * 7)),
                    AlternativeDate = DateTime.UtcNow.AddDays(21 + (i * 7)),
                    PreferredStartTime = TimeSpan.FromHours(10 + i),
                    EstimatedDurationHours = package.DurationHours,
                    Location = $"Location {i + 1}, City",
                    SpecialRequirements = i % 2 == 0 ? "Please bring extra lighting" : null,
                    EstimatedPrice = package.EffectivePrice,
                    Status = status,
                    ConfirmedDate = status == BookingStatus.Confirmed || status == BookingStatus.Completed ? DateTime.UtcNow.AddDays(-5) : null
                });
            }

            await context.BookingRequests.AddRangeAsync(bookings);
            await context.SaveChangesAsync();
            logger.LogInformation("Seeded Booking Requests.");
        }

        private static async Task SeedClientBadgesAsync(
            ApplicationDbContext context, List<ClientProfile> clients, List<Badge> badges, ILogger logger)
        {
            var clientBadges = new List<ClientBadge>();

            // Assign badges to some clients
            for (int i = 0; i < Math.Min(clients.Count, 5); i++)
            {
                // Each client gets 1-3 random badges
                var badgesToAssign = badges.OrderBy(x => Guid.NewGuid()).Take(1 + (i % 3));
                foreach (var badge in badgesToAssign)
                {
                    clientBadges.Add(new ClientBadge
                    {
                        ClientProfileId = clients[i].Id,
                        BadgeId = badge.Id,
                        EarnedDate = DateTime.UtcNow.AddDays(-30 + i),
                        Notes = $"Automatically awarded"
                    });
                }
            }

            await context.ClientBadges.AddRangeAsync(clientBadges);
            await context.SaveChangesAsync();
            logger.LogInformation("Seeded Client Badges.");
        }

        private static async Task SeedNotificationsAsync(
            ApplicationDbContext context, List<ApplicationUser> clients, ILogger logger)
        {
            var notifications = new List<Notification>();
            var types = new[] { NotificationType.Info, NotificationType.Success, NotificationType.Invoice, NotificationType.PhotoShoot };
            var messages = new[]
            {
                "Your gallery is ready for viewing!",
                "Payment received - thank you!",
                "Your photo session is confirmed",
                "New photos have been uploaded to your gallery",
                "Contract is ready for your signature",
                "Reminder: Session scheduled for next week"
            };

            for (int i = 0; i < Math.Min(clients.Count, 6); i++)
            {
                notifications.Add(new Notification
                {
                    UserId = clients[i].Id,
                    Title = $"Notification {i + 1}",
                    Message = messages[i % messages.Length],
                    Type = types[i % types.Length],
                    IsRead = i % 2 == 0,
                    ReadDate = i % 2 == 0 ? DateTime.UtcNow.AddDays(-1) : null,
                    Link = i % 3 == 0 ? "/galleries" : null,
                    Icon = "ti-bell"
                });
            }

            await context.Notifications.AddRangeAsync(notifications);
            await context.SaveChangesAsync();
            logger.LogInformation("Seeded Notifications.");
        }

        private static async Task SeedActivitiesAsync(
            ApplicationDbContext context, ApplicationUser admin, List<ApplicationUser> clients, ILogger logger)
        {
            var activities = new List<Activity>
            {
                new Activity { ActionType = "Created", EntityType = "Client", EntityName = "Emily Davis", Description = "New client registered", UserId = admin.Id, CreatedAt = DateTime.UtcNow.AddDays(-7) },
                new Activity { ActionType = "Created", EntityType = "PhotoShoot", EntityName = "Wedding Session", Description = "New photo shoot scheduled", UserId = admin.Id, CreatedAt = DateTime.UtcNow.AddDays(-6) },
                new Activity { ActionType = "Sent", EntityType = "Contract", EntityName = "Wedding Contract", Description = "Contract sent to client", UserId = admin.Id, CreatedAt = DateTime.UtcNow.AddDays(-5) },
                new Activity { ActionType = "Signed", EntityType = "Contract", EntityName = "Wedding Contract", Description = "Contract signed by client", UserId = clients.FirstOrDefault()?.Id, CreatedAt = DateTime.UtcNow.AddDays(-4) },
                new Activity { ActionType = "Paid", EntityType = "Invoice", EntityName = "INV-2025-1001", Description = "Invoice paid in full", UserId = clients.FirstOrDefault()?.Id, CreatedAt = DateTime.UtcNow.AddDays(-3) },
                new Activity { ActionType = "Uploaded", EntityType = "Photo", EntityName = "Wedding Gallery", Description = "50 photos uploaded", UserId = admin.Id, CreatedAt = DateTime.UtcNow.AddDays(-2) },
                new Activity { ActionType = "Created", EntityType = "Gallery", EntityName = "Smith Wedding", Description = "Gallery created and shared", UserId = admin.Id, CreatedAt = DateTime.UtcNow.AddDays(-1) },
                new Activity { ActionType = "Viewed", EntityType = "Gallery", EntityName = "Smith Wedding", Description = "Gallery viewed by client", UserId = clients.FirstOrDefault()?.Id, CreatedAt = DateTime.UtcNow }
            };

            await context.Activities.AddRangeAsync(activities);
            await context.SaveChangesAsync();
            logger.LogInformation("Seeded Activities.");
        }

        private static async Task SeedTagsAsync(ApplicationDbContext context, ILogger logger)
        {
            var tags = new List<Tag>
            {
                new Tag { Name = "Wedding", FileItemTags = new List<FileItemTag>() },
                new Tag { Name = "Portrait", FileItemTags = new List<FileItemTag>() },
                new Tag { Name = "Family", FileItemTags = new List<FileItemTag>() },
                new Tag { Name = "Event", FileItemTags = new List<FileItemTag>() },
                new Tag { Name = "Corporate", FileItemTags = new List<FileItemTag>() },
                new Tag { Name = "Newborn", FileItemTags = new List<FileItemTag>() },
                new Tag { Name = "Engagement", FileItemTags = new List<FileItemTag>() },
                new Tag { Name = "Senior", FileItemTags = new List<FileItemTag>() },
                new Tag { Name = "Maternity", FileItemTags = new List<FileItemTag>() },
                new Tag { Name = "Headshot", FileItemTags = new List<FileItemTag>() },
                new Tag { Name = "Favorites", FileItemTags = new List<FileItemTag>() },
                new Tag { Name = "Edited", FileItemTags = new List<FileItemTag>() },
                new Tag { Name = "Raw", FileItemTags = new List<FileItemTag>() },
                new Tag { Name = "Print Ready", FileItemTags = new List<FileItemTag>() }
            };

            await context.Tags.AddRangeAsync(tags);
            await context.SaveChangesAsync();
            logger.LogInformation("Seeded Tags.");
        }
    }
}
