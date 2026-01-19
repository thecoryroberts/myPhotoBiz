using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MyPhotoBiz.Data;
using MyPhotoBiz.Enums;
using MyPhotoBiz.Helpers;
using MyPhotoBiz.Models;

namespace MyPhotoBiz.Services
{
    /// <summary>
    /// Service for orchestrating multi-step business workflows
    /// Simplifies complex operations into single method calls
    /// </summary>
    public class WorkflowService : IWorkflowService
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IFileService _fileService;
        private readonly IGalleryService _galleryService;
        private readonly INotificationService _notificationService;
        private readonly ILogger<WorkflowService> _logger;

        public WorkflowService(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            IFileService fileService,
            IGalleryService galleryService,
            INotificationService notificationService,
            ILogger<WorkflowService> logger)
        {
            _context = context;
            _userManager = userManager;
            _fileService = fileService;
            _galleryService = galleryService;
            _notificationService = notificationService;
            _logger = logger;
        }

        #region Client Workflow

        public async Task<WorkflowResult<ClientProfile>> CreateClientWithResourcesAsync(
            string firstName,
            string lastName,
            string email,
            string? phone = null,
            string? address = null)
        {
            var warnings = new List<string>();

            try
            {
                // Check if user already exists
                var existingUser = await _userManager.FindByEmailAsync(email);
                if (existingUser != null)
                {
                    return WorkflowResult<ClientProfile>.Failed($"A user with email {email} already exists");
                }

                // Generate temporary password
                var tempPassword = PasswordGenerator.GenerateSecurePassword(16);

                // Create user account
                var user = new ApplicationUser
                {
                    UserName = email,
                    Email = email,
                    FirstName = firstName,
                    LastName = lastName,
                    EmailConfirmed = false
                };

                var createResult = await _userManager.CreateAsync(user, tempPassword);
                if (!createResult.Succeeded)
                {
                    var errors = string.Join(", ", createResult.Errors.Select(e => e.Description));
                    return WorkflowResult<ClientProfile>.Failed($"Failed to create user: {errors}");
                }

                // Add to Client role
                await _userManager.AddToRoleAsync(user, AppConstants.Roles.Client);

                // Create client profile
                var clientProfile = new ClientProfile
                {
                    UserId = user.Id,
                    PhoneNumber = phone,
                    Address = address,
                    CreatedDate = DateTime.Now,
                    User = user
                };

                _context.ClientProfiles.Add(clientProfile);
                await _context.SaveChangesAsync();

                // Create client folder in file manager
                try
                {
                    var folder = await _fileService.CreateClientFolderAsync($"{firstName} {lastName}", "System");
                    clientProfile.FolderId = folder.Id;
                    await _context.SaveChangesAsync();
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to create client folder for {Email}", email);
                    warnings.Add("Client folder could not be created");
                }

                // Send welcome notification
                try
                {
                    await CreateNotificationAsync(
                        user.Id,
                        "Welcome to MyPhotoBiz!",
                        "Your account has been created. Please check your email to set your password.",
                        NotificationType.Info);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to send welcome notification to {Email}", email);
                    warnings.Add("Welcome notification could not be sent");
                }

                _logger.LogInformation("Created client {Email} with profile ID {ProfileId}", email, clientProfile.Id);

                return WorkflowResult<ClientProfile>.Succeeded(clientProfile, warnings.ToArray());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating client {Email}", email);
                return WorkflowResult<ClientProfile>.Failed($"An error occurred: {ex.Message}");
            }
        }

        #endregion

        #region Booking Workflow

        public async Task<WorkflowResult<PhotoShoot>> ApproveBookingAsync(int bookingId, string approvedBy)
        {
            var warnings = new List<string>();

            try
            {
                var booking = await _context.BookingRequests
                    .Include(b => b.ClientProfile)
                        .ThenInclude(c => c!.User)
                    .FirstOrDefaultAsync(b => b.Id == bookingId);

                if (booking == null)
                {
                    return WorkflowResult<PhotoShoot>.Failed("Booking not found");
                }

                if (booking.Status != BookingStatus.Pending)
                {
                    return WorkflowResult<PhotoShoot>.Failed($"Booking is already {booking.Status}");
                }

                // Update booking status
                booking.Status = BookingStatus.Confirmed;

                // Create photoshoot from booking
                var photoShoot = new PhotoShoot
                {
                    Title = booking.EventType ?? $"Photoshoot for {booking.ClientProfile?.User?.FirstName}",
                    ClientProfileId = booking.ClientProfileId,
                    ScheduledDate = booking.PreferredDate,
                    Location = booking.Location,
                    Status = PhotoShootStatus.Scheduled,
                    Price = booking.EstimatedPrice ?? 0,
                    Notes = booking.SpecialRequirements,
                    DurationHours = (int)booking.EstimatedDurationHours,
                    CreatedDate = DateTime.Now
                };

                _context.PhotoShoots.Add(photoShoot);
                await _context.SaveChangesAsync();

                // Link booking to photoshoot
                booking.PhotoShootId = photoShoot.Id;

                // Send approval notification
                if (booking.ClientProfile?.UserId != null)
                {
                    try
                    {
                        await CreateNotificationAsync(
                            booking.ClientProfile.UserId,
                            "Booking Approved!",
                            $"Your booking for {photoShoot.Title} has been approved for {photoShoot.ScheduledDate:MMMM dd, yyyy}.",
                            NotificationType.Success);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Failed to send approval notification for booking {BookingId}", bookingId);
                        warnings.Add("Notification could not be sent");
                    }
                }

                await _context.SaveChangesAsync();

                _logger.LogInformation("Approved booking {BookingId}, created photoshoot {PhotoShootId}", bookingId, photoShoot.Id);

                return WorkflowResult<PhotoShoot>.Succeeded(photoShoot, warnings.ToArray());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error approving booking {BookingId}", bookingId);
                return WorkflowResult<PhotoShoot>.Failed($"An error occurred: {ex.Message}");
            }
        }

        public async Task<WorkflowResult<BookingRequest>> RejectBookingAsync(int bookingId, string reason, string rejectedBy)
        {
            try
            {
                var booking = await _context.BookingRequests
                    .Include(b => b.ClientProfile)
                    .FirstOrDefaultAsync(b => b.Id == bookingId);

                if (booking == null)
                {
                    return WorkflowResult<BookingRequest>.Failed("Booking not found");
                }

                booking.Status = BookingStatus.Cancelled;
                booking.AdminNotes = reason;

                // Send rejection notification
                if (booking.ClientProfile?.UserId != null)
                {
                    await CreateNotificationAsync(
                        booking.ClientProfile.UserId,
                        "Booking Update",
                        $"Your booking request could not be approved. Reason: {reason}",
                        NotificationType.Warning);
                }

                await _context.SaveChangesAsync();

                return WorkflowResult<BookingRequest>.Succeeded(booking);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error rejecting booking {BookingId}", bookingId);
                return WorkflowResult<BookingRequest>.Failed($"An error occurred: {ex.Message}");
            }
        }

        #endregion

        #region Photoshoot Workflow

        public async Task<WorkflowResult<Album>> CompletePhotoshootAsync(int photoshootId, bool createGallery = true)
        {
            var warnings = new List<string>();

            try
            {
                var photoshoot = await _context.PhotoShoots
                    .Include(p => p.ClientProfile)
                        .ThenInclude(c => c!.User)
                    .Include(p => p.Albums)
                    .FirstOrDefaultAsync(p => p.Id == photoshootId);

                if (photoshoot == null)
                {
                    return WorkflowResult<Album>.Failed("Photoshoot not found");
                }

                // Update status
                photoshoot.Status = PhotoShootStatus.Completed;

                // Create album if none exists
                var album = photoshoot.Albums.FirstOrDefault();
                if (album == null)
                {
                    album = new Album
                    {
                        Name = $"{photoshoot.Title} - Photos",
                        PhotoShootId = photoshootId,
                        CreatedDate = DateTime.Now
                    };
                    _context.Albums.Add(album);
                    await _context.SaveChangesAsync();
                }

                // Create gallery if requested
                if (createGallery)
                {
                    try
                    {
                        var galleryResult = await CreateGalleryFromAlbumAsync(
                            album.Id,
                            $"{photoshoot.Title} Gallery",
                            DateTime.Now.AddDays(30),
                            notifyClient: true);

                        if (!galleryResult.Success)
                        {
                            warnings.Add($"Gallery creation failed: {galleryResult.ErrorMessage}");
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Failed to create gallery for photoshoot {PhotoshootId}", photoshootId);
                        warnings.Add("Gallery could not be created automatically");
                    }
                }

                // Send completion notification
                if (photoshoot.ClientProfile?.UserId != null)
                {
                    await CreateNotificationAsync(
                        photoshoot.ClientProfile.UserId,
                        "Photos Ready!",
                        $"Your photos from {photoshoot.Title} are ready for viewing.",
                        NotificationType.Success);
                }

                await _context.SaveChangesAsync();

                return WorkflowResult<Album>.Succeeded(album, warnings.ToArray());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error completing photoshoot {PhotoshootId}", photoshootId);
                return WorkflowResult<Album>.Failed($"An error occurred: {ex.Message}");
            }
        }

        public async Task<WorkflowResult<PhotoShoot>> DeliverPhotoshootAsync(int photoshootId)
        {
            try
            {
                var photoshoot = await _context.PhotoShoots
                    .Include(p => p.ClientProfile)
                    .FirstOrDefaultAsync(p => p.Id == photoshootId);

                if (photoshoot == null)
                {
                    return WorkflowResult<PhotoShoot>.Failed("Photoshoot not found");
                }

                // Mark as completed (delivered state)
                photoshoot.Status = PhotoShootStatus.Completed;

                if (photoshoot.ClientProfile?.UserId != null)
                {
                    await CreateNotificationAsync(
                        photoshoot.ClientProfile.UserId,
                        "Photos Delivered!",
                        $"Your final photos from {photoshoot.Title} are now available for download.",
                        NotificationType.Success);
                }

                await _context.SaveChangesAsync();

                return WorkflowResult<PhotoShoot>.Succeeded(photoshoot);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error delivering photoshoot {PhotoshootId}", photoshootId);
                return WorkflowResult<PhotoShoot>.Failed($"An error occurred: {ex.Message}");
            }
        }

        #endregion

        #region Invoice Workflow

        public async Task<WorkflowResult<Invoice>> GenerateInvoiceFromPhotoshootAsync(int photoshootId)
        {
            try
            {
                var photoshoot = await _context.PhotoShoots
                    .Include(p => p.ClientProfile)
                    .Include(p => p.Invoices)
                    .FirstOrDefaultAsync(p => p.Id == photoshootId);

                if (photoshoot == null)
                {
                    return WorkflowResult<Invoice>.Failed("Photoshoot not found");
                }

                // Check if invoice already exists
                if (photoshoot.Invoices.Any(i => !i.IsDeleted))
                {
                    return WorkflowResult<Invoice>.Failed("An invoice already exists for this photoshoot");
                }

                var invoice = new Invoice
                {
                    PhotoShootId = photoshootId,
                    ClientProfileId = photoshoot.ClientProfileId,
                    InvoiceNumber = $"INV-{DateTime.Now:yyyyMMdd}-{photoshootId}",
                    InvoiceDate = DateTime.Now,
                    DueDate = DateTime.Now.AddDays(30),
                    Amount = photoshoot.Price,
                    Tax = 0,
                    Status = InvoiceStatus.Pending,
                    Notes = $"Invoice for {photoshoot.Title}"
                };

                _context.Invoices.Add(invoice);
                await _context.SaveChangesAsync();

                // Send notification
                if (photoshoot.ClientProfile?.UserId != null)
                {
                    await CreateNotificationAsync(
                        photoshoot.ClientProfile.UserId,
                        "New Invoice",
                        $"Invoice {invoice.InvoiceNumber} for {invoice.Amount:C} is ready.",
                        NotificationType.Info);
                }

                return WorkflowResult<Invoice>.Succeeded(invoice);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating invoice for photoshoot {PhotoshootId}", photoshootId);
                return WorkflowResult<Invoice>.Failed($"An error occurred: {ex.Message}");
            }
        }

        public async Task<WorkflowResult<Invoice>> MarkInvoicePaidAsync(int invoiceId, string paymentMethod, string? transactionId = null)
        {
            try
            {
                var invoice = await _context.Invoices
                    .Include(i => i.PhotoShoot)
                    .Include(i => i.ClientProfile)
                    .FirstOrDefaultAsync(i => i.Id == invoiceId);

                if (invoice == null)
                {
                    return WorkflowResult<Invoice>.Failed("Invoice not found");
                }

                invoice.Status = InvoiceStatus.Paid;
                invoice.PaidDate = DateTime.Now;

                // Check if all invoices for the photoshoot are paid and trigger delivery
                if (invoice.PhotoShootId.HasValue)
                {
                    var allPaid = !await _context.Invoices
                        .Where(i => i.PhotoShootId == invoice.PhotoShootId && i.Id != invoiceId && !i.IsDeleted)
                        .AnyAsync(i => i.Status != InvoiceStatus.Paid);

                    if (allPaid && invoice.PhotoShoot?.Status == PhotoShootStatus.Completed)
                    {
                        await DeliverPhotoshootAsync(invoice.PhotoShootId.Value);
                    }
                }

                // Send payment confirmation
                if (invoice.ClientProfile?.UserId != null)
                {
                    await CreateNotificationAsync(
                        invoice.ClientProfile.UserId,
                        "Payment Received",
                        $"We received your payment of {(invoice.Amount + invoice.Tax):C}. Thank you!",
                        NotificationType.Success);
                }

                await _context.SaveChangesAsync();

                return WorkflowResult<Invoice>.Succeeded(invoice);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error marking invoice {InvoiceId} as paid", invoiceId);
                return WorkflowResult<Invoice>.Failed($"An error occurred: {ex.Message}");
            }
        }

        #endregion

        #region Gallery Workflow

        public async Task<WorkflowResult<Gallery>> CreateGalleryFromAlbumAsync(
            int albumId,
            string name,
            DateTime expiryDate,
            bool notifyClient = true)
        {
            try
            {
                var album = await _context.Albums
                    .Include(a => a.PhotoShoot)
                        .ThenInclude(p => p!.ClientProfile)
                            .ThenInclude(c => c!.User)
                    .FirstOrDefaultAsync(a => a.Id == albumId);

                if (album == null)
                {
                    return WorkflowResult<Gallery>.Failed("Album not found");
                }

                var gallery = new Gallery
                {
                    Name = name,
                    Description = $"Gallery for {album.Name}",
                    ExpiryDate = expiryDate,
                    IsActive = true,
                    CreatedDate = DateTime.Now
                };

                _context.Galleries.Add(gallery);
                await _context.SaveChangesAsync();

                // Link album to gallery
                gallery.Albums.Add(album);
                await _context.SaveChangesAsync();

                // Send notification with gallery link
                if (notifyClient && album.PhotoShoot?.ClientProfile?.UserId != null)
                {
                    await CreateNotificationAsync(
                        album.PhotoShoot.ClientProfile.UserId,
                        "Your Gallery is Ready!",
                        $"View your photos at the gallery: {gallery.Name}",
                        NotificationType.Success);
                }

                return WorkflowResult<Gallery>.Succeeded(gallery);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating gallery from album {AlbumId}", albumId);
                return WorkflowResult<Gallery>.Failed($"An error occurred: {ex.Message}");
            }
        }

        #endregion

        #region Private Helpers

        /// <summary>
        /// Helper method to create notifications
        /// </summary>
        private async Task CreateNotificationAsync(string userId, string title, string message, NotificationType type)
        {
            var notification = new Notification
            {
                UserId = userId,
                Title = title,
                Message = message,
                Type = type,
                CreatedDate = DateTime.Now,
                IsRead = false
            };

            await _notificationService.CreateNotificationAsync(notification);
        }

        #endregion
    }
}
