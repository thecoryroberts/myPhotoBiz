using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyPhotoBiz.Data;
using MyPhotoBiz.Models;

namespace MyPhotoBiz.Controllers
{
    /// <summary>
    /// Controller for managing custom contract variables.
    /// Only accessible by Admins.
    /// </summary>
    [Authorize(Roles = "Admin")]
    public class ContractVariablesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<ContractVariablesController> _logger;

        // Built-in system variables that cannot be created as custom variables
        private static readonly HashSet<string> ReservedVariableNames = new(StringComparer.OrdinalIgnoreCase)
        {
            "ClientName", "ClientEmail", "ClientPhone",
            "ShootTitle", "ShootDate", "EventDate",
            "Location", "PhotographerName", "CurrentDate",
            "GuardianName", "PackageName", "Duration"
        };

        public ContractVariablesController(
            ApplicationDbContext context,
            ILogger<ContractVariablesController> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            var variables = await _context.ContractVariables
                .AsNoTracking()
                .OrderBy(v => v.Name)
                .ToListAsync();

            return View(variables);
        }

        public IActionResult Create()
        {
            return View(new ContractVariable());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ContractVariable model)
        {
            // Check for reserved names
            if (ReservedVariableNames.Contains(model.Name))
            {
                ModelState.AddModelError("Name", $"'{model.Name}' is a reserved system variable name and cannot be used.");
            }

            // Check for duplicates
            var exists = await _context.ContractVariables
                .AnyAsync(v => v.Name.ToLower() == model.Name.ToLower());
            if (exists)
            {
                ModelState.AddModelError("Name", $"A variable with the name '{model.Name}' already exists.");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    model.CreatedDate = DateTime.UtcNow;
                    model.UpdatedDate = DateTime.UtcNow;
                    model.IsSystemVariable = false;

                    _context.ContractVariables.Add(model);
                    await _context.SaveChangesAsync();

                    TempData["Success"] = "Variable '{{" + model.Name + "}}' created successfully!";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error creating contract variable");
                    ModelState.AddModelError("", "An error occurred while creating the variable.");
                }
            }

            return View(model);
        }

        public async Task<IActionResult> Edit(int id)
        {
            var variable = await _context.ContractVariables.FindAsync(id);
            if (variable == null)
                return NotFound();

            return View(variable);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, ContractVariable model)
        {
            if (id != model.Id)
                return NotFound();

            var existingVariable = await _context.ContractVariables.FindAsync(id);
            if (existingVariable == null)
                return NotFound();

            // Prevent editing system variables
            if (existingVariable.IsSystemVariable)
            {
                TempData["Error"] = "System variables cannot be edited.";
                return RedirectToAction(nameof(Index));
            }

            // Check if name changed and new name is reserved
            if (!existingVariable.Name.Equals(model.Name, StringComparison.OrdinalIgnoreCase))
            {
                if (ReservedVariableNames.Contains(model.Name))
                {
                    ModelState.AddModelError("Name", $"'{model.Name}' is a reserved system variable name and cannot be used.");
                }

                // Check for duplicates
                var nameExists = await _context.ContractVariables
                    .AnyAsync(v => v.Id != id && v.Name.ToLower() == model.Name.ToLower());
                if (nameExists)
                {
                    ModelState.AddModelError("Name", $"A variable with the name '{model.Name}' already exists.");
                }
            }

            if (ModelState.IsValid)
            {
                try
                {
                    existingVariable.Name = model.Name;
                    existingVariable.Description = model.Description;
                    existingVariable.DefaultValue = model.DefaultValue;
                    existingVariable.IsActive = model.IsActive;
                    existingVariable.UpdatedDate = DateTime.UtcNow;

                    await _context.SaveChangesAsync();

                    TempData["Success"] = "Variable '{{" + model.Name + "}}' updated successfully!";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error updating contract variable");
                    ModelState.AddModelError("", "An error occurred while updating the variable.");
                }
            }

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var variable = await _context.ContractVariables.FindAsync(id);
                if (variable == null)
                    return NotFound();

                // Prevent deleting system variables
                if (variable.IsSystemVariable)
                {
                    TempData["Error"] = "System variables cannot be deleted.";
                    return RedirectToAction(nameof(Index));
                }

                // Delete associated variable values
                var values = await _context.ContractVariableValues
                    .Where(cvv => cvv.ContractVariableId == id)
                    .ToListAsync();
                _context.ContractVariableValues.RemoveRange(values);

                _context.ContractVariables.Remove(variable);
                await _context.SaveChangesAsync();

                TempData["Success"] = "Variable '{{" + variable.Name + "}}' deleted successfully!";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting contract variable");
                TempData["Error"] = "An error occurred while deleting the variable.";
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleActive(int id)
        {
            try
            {
                var variable = await _context.ContractVariables.FindAsync(id);
                if (variable == null)
                    return NotFound();

                variable.IsActive = !variable.IsActive;
                variable.UpdatedDate = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                TempData["Success"] = variable.IsActive
                    ? "Variable '{{" + variable.Name + "}}' activated."
                    : "Variable '{{" + variable.Name + "}}' deactivated.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error toggling contract variable status");
                TempData["Error"] = "An error occurred while updating the variable.";
            }

            return RedirectToAction(nameof(Index));
        }

        /// <summary>
        /// Returns a list of all available variables (system + custom) for use in templates
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetAllVariables()
        {
            var systemVariables = ReservedVariableNames
                .Select(name => new
                {
                    Name = name,
                    Placeholder = $"{{{{{name}}}}}",
                    IsSystem = true,
                    Description = (string?)GetSystemVariableDescription(name)
                });

            var customVariables = await _context.ContractVariables
                .AsNoTracking()
                .Where(v => v.IsActive)
                .Select(v => new
                {
                    v.Name,
                    Placeholder = $"{{{{{v.Name}}}}}",
                    IsSystem = false,
                    v.Description
                })
                .ToListAsync();

            var allVariables = systemVariables
                .Concat(customVariables)
                .OrderBy(v => v.Name);

            return Json(allVariables);
        }

        private static string GetSystemVariableDescription(string name)
        {
            return name switch
            {
                "ClientName" => "Client's full name",
                "ClientEmail" => "Client's email address",
                "ClientPhone" => "Client's phone number",
                "ShootTitle" => "Photo shoot title",
                "ShootDate" => "Photo shoot scheduled date",
                "EventDate" => "Event/shoot date (alias for ShootDate)",
                "Location" => "Photo shoot location",
                "PhotographerName" => "Assigned photographer's name",
                "CurrentDate" => "Current date when viewing",
                "GuardianName" => "Guardian name for minor releases",
                "PackageName" => "Service package name",
                "Duration" => "Photo shoot duration",
                _ => ""
            };
        }
    }
}
