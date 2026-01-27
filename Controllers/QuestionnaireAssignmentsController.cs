using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MyPhotoBiz.Data;
using MyPhotoBiz.Models;
using MyPhotoBiz.ViewModels;

namespace MyPhotoBiz.Controllers
{
    /// <summary>
    /// Handles HTTP requests for questionnaire assignments.
    /// </summary>
    [Authorize(Roles = "Admin,Photographer")]
    public class QuestionnaireAssignmentsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<QuestionnaireAssignmentsController> _logger;

        public QuestionnaireAssignmentsController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            ILogger<QuestionnaireAssignmentsController> logger)
        {
            _context = context;
            _userManager = userManager;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            var assignments = await _context.QuestionnaireAssignments
                .Include(a => a.QuestionnaireTemplate)
                .Include(a => a.AssignedToUser)
                .Include(a => a.AssignedByUser)
                .OrderByDescending(a => a.AssignedDate)
                .ToListAsync();

            return View(assignments);
        }

        public async Task<IActionResult> Create()
        {
            var model = await BuildCreateViewModelAsync();
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(QuestionnaireAssignmentCreateViewModel model)
        {
            if (!ModelState.IsValid)
            {
                var invalidModel = await BuildCreateViewModelAsync(model);
                return View(invalidModel);
            }

            try
            {
                var userId = _userManager.GetUserId(User);
                if (string.IsNullOrWhiteSpace(userId))
                {
                    return Unauthorized();
                }

                var assignment = new QuestionnaireAssignment
                {
                    QuestionnaireTemplateId = model.QuestionnaireTemplateId,
                    AssignedToUserId = model.AssignedToUserId,
                    AssignedByUserId = userId,
                    DueDate = model.DueDate,
                    AssignedDate = DateTime.UtcNow,
                    Status = QuestionnaireAssignmentStatus.Assigned
                };

                _context.QuestionnaireAssignments.Add(assignment);
                await _context.SaveChangesAsync();

                TempData["Success"] = "Questionnaire assigned successfully!";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating questionnaire assignment");
                ModelState.AddModelError("", "An error occurred while assigning the questionnaire.");
            }

            var fallbackModel = await BuildCreateViewModelAsync(model);
            return View(fallbackModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var assignment = await _context.QuestionnaireAssignments.FindAsync(id);
                if (assignment == null)
                {
                    return NotFound();
                }

                _context.QuestionnaireAssignments.Remove(assignment);
                await _context.SaveChangesAsync();

                TempData["Success"] = "Questionnaire assignment removed.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting questionnaire assignment");
                TempData["Error"] = "An error occurred while removing the assignment.";
            }

            return RedirectToAction(nameof(Index));
        }

        private async Task<QuestionnaireAssignmentCreateViewModel> BuildCreateViewModelAsync(
            QuestionnaireAssignmentCreateViewModel? model = null)
        {
            var templates = await _context.QuestionnaireTemplates
                .Where(t => t.IsActive)
                .OrderBy(t => t.Category)
                .ThenBy(t => t.Name)
                .ToListAsync();

            var users = await _context.Users
                .Where(u => u.IsActive)
                .OrderBy(u => u.LastName)
                .ThenBy(u => u.FirstName)
                .ToListAsync();

            var viewModel = model ?? new QuestionnaireAssignmentCreateViewModel();
            viewModel.TemplateOptions = templates
                .Select(t => new SelectListItem
                {
                    Value = t.Id.ToString(),
                    Text = string.IsNullOrWhiteSpace(t.Category) ? t.Name : $"{t.Category} - {t.Name}"
                })
                .ToList();

            viewModel.UserOptions = users
                .Select(u => new SelectListItem
                {
                    Value = u.Id,
                    Text = $"{u.FullName} ({u.Email})"
                })
                .ToList();

            return viewModel;
        }
    }
}
