
// Controllers/ClientsController.cs
using Microsoft.AspNetCore.Mvc;
using MyPhotoBiz.Models;
using MyPhotoBiz.Data;
using Microsoft.EntityFrameworkCore;
using MyPhotoBiz.ViewModels;

namespace MyPhotoBiz.Controllers;


public class FileManagerController : Controller
{
    private readonly IWebHostEnvironment _env;
    private readonly ApplicationDbContext _context;

    public FileManagerController(IWebHostEnvironment env, ApplicationDbContext context)
    {
        _env = env;
        _context = context;
    }

    public async Task<IActionResult> Index(string filterType, int page = 1, int pageSize = 10)
    {
        var query = _context.Files.AsQueryable();

        if (!string.IsNullOrEmpty(filterType))
            query = query.Where(f => f.Type.Equals(filterType, StringComparison.OrdinalIgnoreCase));

        var files = await query
            .OrderByDescending(f => f.Modified)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var vm = new FileManagerViewModel
        {
            Files = files,
            FilterType = filterType,
            PageSize = pageSize,
            CurrentPage = page
        };

        return View(vm);
    }

    [HttpPost]
    public async Task<IActionResult> Upload(IFormFile file)
    {
        if (file != null && file.Length > 0)
        {
            var uploadPath = Path.Combine(_env.WebRootPath, "uploads");
            if (!Directory.Exists(uploadPath))
                Directory.CreateDirectory(uploadPath);

            var filePath = Path.Combine(uploadPath, file.FileName);
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            var fileItem = new FileItem
            {
                Name = file.FileName,
                Type = Path.GetExtension(file.FileName).Trim('.').ToUpper(),
                Size = file.Length,
                Modified = DateTime.Now,
                Owner = User.Identity?.Name ?? "Unknown",
                FilePath = filePath
            };

            _context.Files.Add(fileItem);
            await _context.SaveChangesAsync();
        }
        return RedirectToAction("Index");
    }

    public async Task<IActionResult> Download(int id)
    {
        var fileItem = await _context.Files.FindAsync(id);
        if (fileItem == null) return NotFound();

        var mimeType = "application/octet-stream";
        return PhysicalFile(fileItem.FilePath, mimeType, fileItem.Name);
    }

    [HttpPost]
    public async Task<IActionResult> Delete(int id)
    {
        var fileItem = await _context.Files.FindAsync(id);
        if (fileItem != null)
        {
            if (System.IO.File.Exists(fileItem.FilePath))
                System.IO.File.Delete(fileItem.FilePath);

            _context.Files.Remove(fileItem);
            await _context.SaveChangesAsync();
        }
        return RedirectToAction("Index");
    }
}
