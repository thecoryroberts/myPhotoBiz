using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyPhotoBiz.Data;
using MyPhotoBiz.Models;
using MyPhotoBiz.ViewModels;

namespace MyPhotoBiz.Controllers
{
    /// <summary>
    /// Handles HTTP requests for questionnaire templates.
    /// Supports uploading PDF/Word documents as questionnaire content.
    /// </summary>
    [Authorize(Roles = "Admin,Photographer")]
    public class QuestionnaireTemplatesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<QuestionnaireTemplatesController> _logger;
        private readonly IWebHostEnvironment _environment;

        private static readonly string[] AllowedExtensions = { ".pdf", ".doc", ".docx" };
        private const long MaxFileSize = 10 * 1024 * 1024; // 10MB

        public QuestionnaireTemplatesController(
            ApplicationDbContext context,
            ILogger<QuestionnaireTemplatesController> logger,
            IWebHostEnvironment environment)
        {
            _context = context;
            _logger = logger;
            _environment = environment;
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
            ViewBag.Title = "Upload Questionnaire";
            ViewBag.PageTitle = "Upload Questionnaire";
            ViewBag.SubTitle = "Questionnaire Library";
            ViewBag.SubTitleUrl = Url.Action("Index");
            ViewBag.Icon = "ti-file-upload";
            ViewBag.Color = "info";
            ViewBag.BackUrl = Url.Action("Index");

            return View(new QuestionnaireTemplateViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(QuestionnaireTemplateViewModel model)
        {
            // Validate that either a document or question text is provided
            if (model.DocumentFile == null && string.IsNullOrWhiteSpace(model.QuestionText))
            {
                ModelState.AddModelError("", "Please upload a document (PDF/Word) or enter question text.");
            }

            // Validate file if provided
            if (model.DocumentFile != null)
            {
                var validationError = ValidateDocument(model.DocumentFile);
                if (!string.IsNullOrEmpty(validationError))
                {
                    ModelState.AddModelError("DocumentFile", validationError);
                }
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var template = new QuestionnaireTemplate
                    {
                        Name = model.Name,
                        Category = model.Category,
                        Description = model.Description,
                        QuestionText = model.QuestionText,
                        IsActive = model.IsActive,
                        CreatedDate = DateTime.UtcNow
                    };

                    // Handle file upload
                    if (model.DocumentFile != null)
                    {
                        var (path, originalName, docType, size) = await SaveDocumentAsync(model.DocumentFile);
                        template.DocumentPath = path;
                        template.OriginalFileName = originalName;
                        template.DocumentType = docType;
                        template.FileSize = size;
                    }

                    _context.QuestionnaireTemplates.Add(template);
                    await _context.SaveChangesAsync();

                    TempData["Success"] = "Questionnaire uploaded to library successfully!";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error creating questionnaire template");
                    ModelState.AddModelError("", "An error occurred while uploading the questionnaire.");
                }
            }

            SetViewBagForCreate();
            return View(model);
        }

        private void SetViewBagForCreate()
        {
            ViewBag.Title = "Upload Questionnaire";
            ViewBag.PageTitle = "Upload Questionnaire";
            ViewBag.SubTitle = "Questionnaire Library";
            ViewBag.SubTitleUrl = Url.Action("Index");
            ViewBag.Icon = "ti-file-upload";
            ViewBag.Color = "info";
            ViewBag.BackUrl = Url.Action("Index");
        }

        public async Task<IActionResult> Edit(int id)
        {
            var template = await _context.QuestionnaireTemplates.FindAsync(id);
            if (template == null)
            {
                return NotFound();
            }

            var model = new QuestionnaireTemplateViewModel
            {
                Id = template.Id,
                Name = template.Name,
                Category = template.Category,
                Description = template.Description,
                QuestionText = template.QuestionText,
                DocumentPath = template.DocumentPath,
                OriginalFileName = template.OriginalFileName,
                DocumentType = template.DocumentType,
                FileSize = template.FileSize,
                IsActive = template.IsActive,
                CreatedDate = template.CreatedDate
            };

            SetViewBagForEdit();
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, QuestionnaireTemplateViewModel model)
        {
            if (id != model.Id)
            {
                return NotFound();
            }

            var template = await _context.QuestionnaireTemplates.FindAsync(id);
            if (template == null)
            {
                return NotFound();
            }

            // Validate that either a document exists/is uploaded or question text is provided
            if (model.DocumentFile == null && string.IsNullOrEmpty(template.DocumentPath) && string.IsNullOrWhiteSpace(model.QuestionText))
            {
                ModelState.AddModelError("", "Please upload a document (PDF/Word) or enter question text.");
            }

            // Validate new file if provided
            if (model.DocumentFile != null)
            {
                var validationError = ValidateDocument(model.DocumentFile);
                if (!string.IsNullOrEmpty(validationError))
                {
                    ModelState.AddModelError("DocumentFile", validationError);
                }
            }

            if (ModelState.IsValid)
            {
                try
                {
                    template.Name = model.Name;
                    template.Category = model.Category;
                    template.Description = model.Description;
                    template.QuestionText = model.QuestionText;
                    template.IsActive = model.IsActive;

                    // Handle new file upload (replace existing)
                    if (model.DocumentFile != null)
                    {
                        // Delete old file if exists
                        if (!string.IsNullOrEmpty(template.DocumentPath))
                        {
                            DeleteDocument(template.DocumentPath);
                        }

                        var (path, originalName, docType, size) = await SaveDocumentAsync(model.DocumentFile);
                        template.DocumentPath = path;
                        template.OriginalFileName = originalName;
                        template.DocumentType = docType;
                        template.FileSize = size;
                    }

                    await _context.SaveChangesAsync();

                    TempData["Success"] = "Questionnaire updated successfully!";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error updating questionnaire template");
                    ModelState.AddModelError("", "An error occurred while updating the questionnaire.");
                }
            }

            // Repopulate document info for view
            model.DocumentPath = template.DocumentPath;
            model.OriginalFileName = template.OriginalFileName;
            model.DocumentType = template.DocumentType;
            model.FileSize = template.FileSize;

            SetViewBagForEdit();
            return View(model);
        }

        private void SetViewBagForEdit()
        {
            ViewBag.Title = "Edit Questionnaire";
            ViewBag.PageTitle = "Edit Questionnaire";
            ViewBag.SubTitle = "Questionnaire Library";
            ViewBag.SubTitleUrl = Url.Action("Index");
            ViewBag.Icon = "ti-edit";
            ViewBag.Color = "info";
            ViewBag.BackUrl = Url.Action("Index");
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

                var documentPathToDelete = template.DocumentPath;

                _context.QuestionnaireTemplates.Remove(template);
                await _context.SaveChangesAsync();
                
                // Delete the document file after DB commit succeeds
                if (!string.IsNullOrEmpty(documentPathToDelete))
                {
                    DeleteDocument(documentPathToDelete);
                }

                TempData["Success"] = "Questionnaire deleted successfully!";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting questionnaire template");
                TempData["Error"] = "An error occurred while deleting the questionnaire.";
                return RedirectToAction(nameof(Index));
            }
        }

        /// <summary>
        /// Download the questionnaire document.
        /// </summary>
        public async Task<IActionResult> Download(int id)
        {
            var template = await _context.QuestionnaireTemplates.FindAsync(id);
            if (template == null || string.IsNullOrEmpty(template.DocumentPath))
            {
                return NotFound();
            }

            var fullPath = Path.Combine(_environment.WebRootPath, template.DocumentPath.TrimStart('/'));
            if (!System.IO.File.Exists(fullPath))
            {
                TempData["Error"] = "Document file not found.";
                return RedirectToAction(nameof(Index));
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
        /// Preview the questionnaire document (inline display for PDFs).
        /// </summary>
        public async Task<IActionResult> Preview(int id)
        {
            var template = await _context.QuestionnaireTemplates.FindAsync(id);
            if (template == null || string.IsNullOrEmpty(template.DocumentPath))
            {
                return NotFound();
            }

            var fullPath = Path.Combine(_environment.WebRootPath, template.DocumentPath.TrimStart('/'));
            if (!System.IO.File.Exists(fullPath))
            {
                TempData["Error"] = "Document file not found.";
                return RedirectToAction(nameof(Index));
            }

            // Only PDFs can be previewed inline
            if (template.DocumentType != "pdf")
            {
                return RedirectToAction(nameof(Download), new { id });
            }

            var fileBytes = await System.IO.File.ReadAllBytesAsync(fullPath);
            return File(fileBytes, "application/pdf");
        }

        /// <summary>
        /// Remove the document from a template (keep the template with just text).
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveDocument(int id)
        {
            try
            {
                var template = await _context.QuestionnaireTemplates.FindAsync(id);
                if (template == null)
                {
                    return NotFound();
                }

                if (!string.IsNullOrEmpty(template.DocumentPath))
                {
                    var pathToDelete = template.DocumentPath;
                    template.DocumentPath = null;
                    template.OriginalFileName = null;
                    template.DocumentType = null;
                    template.FileSize = null;

                    await _context.SaveChangesAsync();
                    
                    // Delete file only after DB commit succeeds
                    DeleteDocument(pathToDelete);
                    
                    TempData["Success"] = "Document removed successfully.";
                }

                return RedirectToAction(nameof(Edit), new { id });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing document from template");
                TempData["Error"] = "An error occurred while removing the document.";
                return RedirectToAction(nameof(Edit), new { id });
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

                TempData["Success"] = $"Questionnaire {(template.IsActive ? "activated" : "deactivated")} successfully!";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error toggling questionnaire template status");
                TempData["Error"] = "An error occurred while updating the questionnaire.";
                return RedirectToAction(nameof(Index));
            }
        }

        #region File Handling Helpers

        private string? ValidateDocument(IFormFile file)
        {
            if (file.Length == 0)
                return "File is empty.";

            if (file.Length > MaxFileSize)
                return $"File size must be less than {MaxFileSize / (1024 * 1024)}MB.";

            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!AllowedExtensions.Contains(extension))
                return $"Only PDF and Word documents are allowed ({string.Join(", ", AllowedExtensions)}).";

            return null;
        }

        private async Task<(string path, string originalName, string docType, long size)> SaveDocumentAsync(IFormFile file)
        {
            var uploadsDir = Path.Combine(_environment.WebRootPath, "uploads", "questionnaires");
            if (!Directory.Exists(uploadsDir))
                Directory.CreateDirectory(uploadsDir);

            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            var fileName = $"questionnaire_{DateTime.UtcNow:yyyyMMddHHmmss}_{Guid.NewGuid():N}{extension}";
            var filePath = Path.Combine(uploadsDir, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            var docType = extension.TrimStart('.');
            var relativePath = $"/uploads/questionnaires/{fileName}";

            _logger.LogInformation("Saved questionnaire document: {Path}", relativePath);

            return (relativePath, file.FileName, docType, file.Length);
        }

        private void DeleteDocument(string documentPath)
        {
            try
            {
                var fullPath = Path.Combine(_environment.WebRootPath, documentPath.TrimStart('/'));
                if (System.IO.File.Exists(fullPath))
                {
                    System.IO.File.Delete(fullPath);
                    _logger.LogInformation("Deleted questionnaire document: {Path}", documentPath);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to delete questionnaire document: {Path}", documentPath);
            }
        }

        #endregion
    }
}
