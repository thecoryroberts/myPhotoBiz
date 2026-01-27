using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyPhotoBiz.Data;
using MyPhotoBiz.Models;

namespace MyPhotoBiz.Controllers
{
    /// <summary>
    /// Handles HTTP requests for questionnaire templates.
    /// </summary>
    [Authorize(Roles = "Admin,Photographer")]
    public class QuestionnaireTemplatesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<QuestionnaireTemplatesController> _logger;

        public QuestionnaireTemplatesController(ApplicationDbContext context, ILogger<QuestionnaireTemplatesController> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                var templates = await _context.QuestionnaireTemplates
                    .OrderBy(t => t.Category)
                    .ThenBy(t => t.Name)
                    .ToListAsync();

                return View(templates);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving questionnaire templates");
                TempData["Error"] = "An error occurred while loading templates.";
                return View(new List<QuestionnaireTemplate>());
            }
        }

        public IActionResult Create()
        {
            return View(new QuestionnaireTemplate());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(QuestionnaireTemplate template)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    template.CreatedDate = DateTime.UtcNow;
                    _context.QuestionnaireTemplates.Add(template);
                    await _context.SaveChangesAsync();

                    TempData["Success"] = "Questionnaire template created successfully!";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error creating questionnaire template");
                    ModelState.AddModelError("", "An error occurred while creating the template.");
                }
            }

            return View(template);
        }

        public async Task<IActionResult> Edit(int id)
        {
            var template = await _context.QuestionnaireTemplates.FindAsync(id);
            if (template == null)
            {
                return NotFound();
            }

            return View(template);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, QuestionnaireTemplate template)
        {
            if (id != template.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(template);
                    await _context.SaveChangesAsync();

                    TempData["Success"] = "Questionnaire template updated successfully!";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error updating questionnaire template");
                    ModelState.AddModelError("", "An error occurred while updating the template.");
                }
            }

            return View(template);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var template = await _context.QuestionnaireTemplates.FindAsync(id);
                if (template == null)
                {
                    return NotFound();
                }

                _context.QuestionnaireTemplates.Remove(template);
                await _context.SaveChangesAsync();

                TempData["Success"] = "Questionnaire template deleted successfully!";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting questionnaire template");
                TempData["Error"] = "An error occurred while deleting the template.";
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleActive(int id)
        {
            try
            {
                var template = await _context.QuestionnaireTemplates.FindAsync(id);
                if (template == null)
                {
                    return NotFound();
                }

                template.IsActive = !template.IsActive;
                await _context.SaveChangesAsync();

                TempData["Success"] = $"Template {(template.IsActive ? "activated" : "deactivated")} successfully!";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error toggling questionnaire template status");
                TempData["Error"] = "An error occurred while updating the template.";
                return RedirectToAction(nameof(Index));
            }
        }
    }
}
