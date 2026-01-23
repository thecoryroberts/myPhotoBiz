using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyPhotoBiz.Data;
using MyPhotoBiz.Models;

namespace MyPhotoBiz.Controllers
{
    /// <summary>
    /// Handles HTTP requests for contract templates.
    /// </summary>
    [Authorize(Roles = "Admin,Photographer")]
    public class ContractTemplatesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<ContractTemplatesController> _logger;

        public ContractTemplatesController(ApplicationDbContext context, ILogger<ContractTemplatesController> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                var templates = await _context.ContractTemplates
                    .Include(t => t.BadgeToAward)
                    .OrderBy(t => t.Category)
                    .ThenBy(t => t.Name)
                    .ToListAsync();

                return View(templates);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving contract templates");
                TempData["Error"] = "An error occurred while loading templates.";
                return View(new List<ContractTemplate>());
            }
        }

        public async Task<IActionResult> Create()
        {
            var badges = await _context.Badges
                .Where(b => b.IsActive)
                .OrderBy(b => b.Name)
                .ToListAsync();

            ViewBag.Badges = badges;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ContractTemplate template)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    template.CreatedDate = DateTime.UtcNow;
                    _context.ContractTemplates.Add(template);
                    await _context.SaveChangesAsync();

                    TempData["Success"] = "Contract template created successfully!";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error creating contract template");
                    ModelState.AddModelError("", "An error occurred while creating the template.");
                }
            }

            var badges = await _context.Badges
                .Where(b => b.IsActive)
                .OrderBy(b => b.Name)
                .ToListAsync();

            ViewBag.Badges = badges;
            return View(template);
        }

        public async Task<IActionResult> Edit(int id)
        {
            var template = await _context.ContractTemplates.FindAsync(id);
            if (template == null)
                return NotFound();

            var badges = await _context.Badges
                .Where(b => b.IsActive)
                .OrderBy(b => b.Name)
                .ToListAsync();

            ViewBag.Badges = badges;
            return View(template);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, ContractTemplate template)
        {
            if (id != template.Id)
                return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(template);
                    await _context.SaveChangesAsync();

                    TempData["Success"] = "Contract template updated successfully!";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error updating contract template");
                    ModelState.AddModelError("", "An error occurred while updating the template.");
                }
            }

            var badges = await _context.Badges
                .Where(b => b.IsActive)
                .OrderBy(b => b.Name)
                .ToListAsync();

            ViewBag.Badges = badges;
            return View(template);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var template = await _context.ContractTemplates.FindAsync(id);
                if (template == null)
                    return NotFound();

                _context.ContractTemplates.Remove(template);
                await _context.SaveChangesAsync();

                TempData["Success"] = "Contract template deleted successfully!";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting contract template");
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
                var template = await _context.ContractTemplates.FindAsync(id);
                if (template == null)
                    return NotFound();

                template.IsActive = !template.IsActive;
                await _context.SaveChangesAsync();

                TempData["Success"] = $"Template {(template.IsActive ? "activated" : "deactivated")} successfully!";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error toggling template active status");
                TempData["Error"] = "An error occurred while updating the template.";
                return RedirectToAction(nameof(Index));
            }
        }
    }
}
