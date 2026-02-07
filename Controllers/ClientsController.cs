
// Controllers/ClientsController.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyPhotoBiz.Data;
using MyPhotoBiz.Models;
using MyPhotoBiz.Services;
using MyPhotoBiz.Helpers;
using MyPhotoBiz.ViewModels;

namespace MyPhotoBiz.Controllers
{
    /// <summary>
    /// Handles HTTP requests for clients.
    /// </summary>
    public class ClientsController : Controller
    {
        private readonly IClientService _clientService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _context;
        private readonly IActivityService _activityService;

        public ClientsController(IClientService clientService, UserManager<ApplicationUser> userManager,
            ApplicationDbContext context, IActivityService activityService)
        {
            _clientService = clientService;
            _userManager = userManager;
            _context = context;
            _activityService = activityService;
        }

        private ClientDetailsViewModel MapToClientDetailsViewModel(ClientProfile clientProfile)
        {
            return new ClientDetailsViewModel
            {
                Id = clientProfile.Id,
                FirstName = clientProfile.User?.FirstName ?? "",
                LastName = clientProfile.User?.LastName ?? "",
                Email = clientProfile.User?.Email ?? "",
                PhoneNumber = clientProfile.PhoneNumber,
                Address = clientProfile.Address,
                Notes = clientProfile.Notes,
                UpdatedDate = clientProfile.UpdatedDate,
                CreatedDate = clientProfile.CreatedDate,
                User = clientProfile.User,
                PhotoShootCount = clientProfile.PhotoShoots?.Count ?? 0,
                InvoiceCount = clientProfile.Invoices?.Count ?? 0,
                TotalRevenue = clientProfile.Invoices?.Sum(i => i.Amount + i.Tax) ?? 0m,
                PhotoShoots = clientProfile.PhotoShoots?.Select(ps => new PhotoShootViewModel
                {
                    Id = ps.Id,
                    Title = ps.Title,
                    ClientId = ps.ClientProfileId,
                    ScheduledDate = ps.ScheduledDate,
                    UpdatedDate = ps.UpdatedDate,
                    Location = ps.Location,
                    Status = ps.Status,
                    Price = ps.Price,
                    Notes = ps.Notes,
                    DurationHours = ps.DurationHours,
                    DurationMinutes = ps.DurationMinutes
                }).ToList() ?? new List<PhotoShootViewModel>(),
                Invoices = clientProfile.Invoices?.ToList() ?? new List<Invoice>(),
                ClientBadges = clientProfile.ClientBadges?.ToList() ?? new List<ClientBadge>(),
                Contracts = clientProfile.Contracts?.ToList() ?? new List<Contract>()
            };
        }

        [Authorize(Roles = "Admin,Photographer")]
        public async Task<IActionResult> Index()
        {
            var clients = await _clientService.GetAllClientsAsync();
            return View(clients);
        }

        [Authorize(Roles = "Admin,Photographer")]
        public async Task<IActionResult> Details(int id)
        {
            var clientProfile = await _clientService.GetClientByIdAsync(id);
            if (clientProfile == null)
            {
                return NotFound();
            }

            var model = MapToClientDetailsViewModel(clientProfile);
            return View("Details", model);
        }

        [Authorize(Roles = "Admin,Photographer")]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Photographer")]
        public async Task<IActionResult> Create(CreateClientViewModel model)
        {
            if (ModelState.IsValid)
            {
                var existingUser = await _userManager.FindByEmailAsync(model.Email);
                if (existingUser != null)
                {
                    ModelState.AddModelError("Email", "A user with this email already exists.");
                    return View(model);
                }

                var temporaryPassword = PasswordGenerator.GenerateSecurePassword();

                var user = new ApplicationUser
                {
                    UserName = model.Email,
                    Email = model.Email,
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    EmailConfirmed = false
                };

                var result = await _userManager.CreateAsync(user, temporaryPassword);
                if (result.Succeeded)
                {
                    await _userManager.AddToRoleAsync(user, "Client");

                    // Create ClientProfile
                    var clientProfile = new ClientProfile
                    {
                        UserId = user.Id,
                        PhoneNumber = model.PhoneNumber,
                        Address = model.Address,
                        Notes = model.Notes ?? string.Empty,
                        CreatedDate = DateTime.UtcNow,
                        UpdatedDate = DateTime.UtcNow
                    };

                    await _clientService.CreateClientAsync(clientProfile);

                    // Auto-award "New User" badge
                    await AwardNewUserBadgeAsync(clientProfile.Id);

                    // Log activity
                    await _activityService.LogActivityAsync("Created", "Client", clientProfile.Id,
                        $"{model.FirstName} {model.LastName}", null, _userManager.GetUserId(User));

                    TempData["SuccessMessage"] = $"Client created successfully. Temporary password: {temporaryPassword}";
                    TempData["PasswordWarning"] = "Please share this password securely with the client. It will not be shown again.";

                    return RedirectToAction(nameof(Index));
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }
            return View(model);
        }

        [Authorize(Roles = "Admin,Photographer")]
        public async Task<IActionResult> Edit(int id)
        {
            var clientProfile = await _clientService.GetClientByIdAsync(id);
            if (clientProfile == null)
            {
                return NotFound();
            }

            var model = new EditClientViewModel
            {
                Id = clientProfile.Id,
                FirstName = clientProfile.User?.FirstName ?? "",
                LastName = clientProfile.User?.LastName ?? "",
                Email = clientProfile.User?.Email ?? "",
                PhoneNumber = clientProfile.PhoneNumber,
                Address = clientProfile.Address,
                Notes = clientProfile.Notes
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Photographer")]
        public async Task<IActionResult> Edit(int id, EditClientViewModel model)
        {
            if (id != model.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                var clientProfile = await _clientService.GetClientByIdAsync(id);
                if (clientProfile == null)
                {
                    return NotFound();
                }

                // Update ApplicationUser fields
                if (clientProfile.User != null)
                {
                    clientProfile.User.FirstName = model.FirstName;
                    clientProfile.User.LastName = model.LastName;
                    await _userManager.UpdateAsync(clientProfile.User);
                }

                // Update ClientProfile fields
                clientProfile.PhoneNumber = model.PhoneNumber;
                clientProfile.Address = model.Address;
                clientProfile.Notes = model.Notes ?? string.Empty;

                await _clientService.UpdateClientAsync(clientProfile);

                // Log activity
                await _activityService.LogActivityAsync("Updated", "Client", clientProfile.Id,
                    $"{model.FirstName} {model.LastName}", null, _userManager.GetUserId(User));

                return RedirectToAction(nameof(Index));
            }
            return View(model);
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var clientProfile = await _clientService.GetClientByIdAsync(id);
            if (clientProfile == null)
            {
                return NotFound();
            }
            return View(clientProfile);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var clientProfile = await _clientService.GetClientByIdAsync(id);
            var clientName = clientProfile != null
                ? $"{clientProfile.User?.FirstName} {clientProfile.User?.LastName}"
                : $"ID: {id}";
            await _clientService.SoftDeleteClientAsync(id);
            // Log activity
            await _activityService.LogActivityAsync("Deleted", "Client", id,
                clientName, null, _userManager.GetUserId(User));

            return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles = "Client")]
        public IActionResult MyProfile()
        {
            // Redirect to the Identity area Razor Page that manages user details.
            // The Identity `UserDetails` page already handles displaying and editing the current user's profile.
            return RedirectToPage("/Account/Manage/UserDetails", new { area = "Identity" });
        }

        /// <summary>
        /// Client portal: View assigned questionnaires and download documents.
        /// </summary>
        [Authorize(Roles = "Client")]
        public async Task<IActionResult> MyQuestionnaires()
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var assignments = await _context.QuestionnaireAssignments
                .Include(a => a.QuestionnaireTemplate)
                .Include(a => a.AssignedByUser)
                .Where(a => a.AssignedToUserId == userId)
                .OrderByDescending(a => a.AssignedDate)
                .ToListAsync();

            return View(assignments);
        }

        /// <summary>
        /// Download questionnaire document for an assignment.
        /// </summary>
        [Authorize(Roles = "Client")]
        public async Task<IActionResult> DownloadQuestionnaire(int id)
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var assignment = await _context.QuestionnaireAssignments
                .Include(a => a.QuestionnaireTemplate)
                .FirstOrDefaultAsync(a => a.Id == id && a.AssignedToUserId == userId);

            if (assignment == null)
            {
                return NotFound();
            }

            var template = assignment.QuestionnaireTemplate;
            if (template == null || string.IsNullOrEmpty(template.DocumentPath))
            {
                TempData["Error"] = "No document available for this questionnaire.";
                return RedirectToAction(nameof(MyQuestionnaires));
            }

            var webRootPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
            var fullPath = Path.Combine(webRootPath, template.DocumentPath.TrimStart('/'));
            if (!System.IO.File.Exists(fullPath))
            {
                TempData["Error"] = "Document file not found.";
                return RedirectToAction(nameof(MyQuestionnaires));
            }

            var contentType = template.DocumentType switch
            {
                "pdf" => "application/pdf",
                "docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
                "doc" => "application/msword",
                _ => "application/octet-stream"
            };

            var fileBytes = await System.IO.File.ReadAllBytesAsync(fullPath);
            return File(fileBytes, contentType, template.OriginalFileName);
        }

        /// <summary>
        /// Mark a questionnaire assignment as completed.
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Client")]
        public async Task<IActionResult> MarkQuestionnaireComplete(int id)
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var assignment = await _context.QuestionnaireAssignments
                .FirstOrDefaultAsync(a => a.Id == id && a.AssignedToUserId == userId);

            if (assignment == null)
            {
                return NotFound();
            }

            assignment.Status = QuestionnaireAssignmentStatus.Completed;
            await _context.SaveChangesAsync();

            TempData["Success"] = "Questionnaire marked as complete!";
            return RedirectToAction(nameof(MyQuestionnaires));
        }
        // API endpoint for getting clients list (used by manage access modal)
        [HttpGet]
        [Route("api/clients")]
        [Authorize(Roles = "Admin,Photographer")]
        public async Task<IActionResult> GetClientsApi()
        {
            var clients = await _clientService.GetAllClientsAsync();
            var result = clients.Select(c => new
            {
                id = c.Id,
                firstName = c.User?.FirstName ?? "",
                lastName = c.User?.LastName ?? "",
                email = c.User?.Email ?? ""
            });
            return Json(result);
        }

        private async Task AwardNewUserBadgeAsync(int clientProfileId)
        {
            try
            {
                // Find the "New User" badge
                var newUserBadge = await _context.Badges
                    .FirstOrDefaultAsync(b => b.Name == "New User" && b.IsActive);

                if (newUserBadge == null)
                    return; // Badge doesn't exist or isn't active, skip silently

                // Check if client already has this badge
                var hasBadge = await _context.ClientBadges
                    .AnyAsync(cb => cb.ClientProfileId == clientProfileId && cb.BadgeId == newUserBadge.Id);

                if (hasBadge)
                    return; // Already has the badge

                // Award the badge
                var clientBadge = new ClientBadge
                {
                    ClientProfileId = clientProfileId,
                    BadgeId = newUserBadge.Id,
                    EarnedDate = DateTime.UtcNow,
                    Notes = "Auto-awarded on account creation"
                };

                _context.ClientBadges.Add(clientBadge);
                await _context.SaveChangesAsync();
            }
            catch (Exception)
            {
                // Log the error but don't fail the client creation
                // Badge awarding is a non-critical feature
            }
        }
    }
}
