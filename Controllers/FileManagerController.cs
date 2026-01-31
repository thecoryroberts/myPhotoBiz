using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyPhotoBiz.Models;
using MyPhotoBiz.Services;
using MyPhotoBiz.ViewModels;

namespace MyPhotoBiz.Controllers;

/// <summary>
/// Handles HTTP requests for file manager.
/// </summary>
[Authorize]
public class FileManagerController : Controller
{
    private readonly IFileService _fileService;

    public FileManagerController(IFileService fileService)
    {
        _fileService = fileService;
    }

    public async Task<IActionResult> Index(int? folderId = null, string filterType = "", int page = 1, int pageSize = 50)
    {
        IEnumerable<FileItem> files;

        // Handle special filters
        var normalizedFilter = filterType?.ToLowerInvariant() ?? string.Empty;
        switch (normalizedFilter)
        {
            case "favorites":
                files = await _fileService.GetFavoritesAsync(User.Identity?.Name ?? "Unknown", page, pageSize);
                break;
            case "recent":
                files = await _fileService.GetRecentFilesAsync(User.Identity?.Name ?? "Unknown", page, pageSize);
                break;
            default:
                files = await _fileService.GetFilesInFolderAsync(folderId, normalizedFilter, page, pageSize);
                break;
        }

        // Get breadcrumbs for navigation
        var breadcrumbs = await _fileService.GetBreadcrumbsAsync(folderId);

        var vm = new FileManagerViewModel
        {
            Files = files,
            FilterType = normalizedFilter,
            PageSize = pageSize,
            CurrentPage = page,
            CurrentFolderId = folderId,
            Breadcrumbs = breadcrumbs
        };

        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Upload(IFormFile file)
    {
        if (file != null && file.Length > 0)
        {
            var owner = User.Identity?.Name ?? "Unknown";
            await _fileService.UploadFileAsync(file, owner);
            TempData["SuccessMessage"] = "File uploaded successfully!";
        }
        else
        {
            TempData["ErrorMessage"] = "Please select a file to upload.";
        }

        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Download(int id)
    {
        var fileItem = await _fileService.GetFileAsync(id);
        if (fileItem == null || fileItem.FilePath == null)
        {
            TempData["ErrorMessage"] = "File not found.";
            return RedirectToAction(nameof(Index));
        }

        if (!System.IO.File.Exists(fileItem.FilePath))
        {
            TempData["ErrorMessage"] = "File no longer exists on disk.";
            return RedirectToAction(nameof(Index));
        }

        // Increment download count and update last accessed
        await _fileService.IncrementDownloadCountAsync(id);

        var mimeType = fileItem.MimeType ?? "application/octet-stream";
        return PhysicalFile(fileItem.FilePath, mimeType, fileItem.Name);
    }

    [HttpDelete]
    [Route("api/files/{id}")]
    public async Task<IActionResult> DeleteApi(int id)
    {
        var fileItem = await _fileService.GetFileAsync(id);
        if (fileItem == null)
        {
            return NotFound(new { message = "File not found" });
        }

        await _fileService.DeleteFileAsync(id);
        return Ok(new { success = true, message = "File deleted successfully" });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        await _fileService.DeleteFileAsync(id);
        TempData["SuccessMessage"] = "File deleted successfully!";
        return RedirectToAction(nameof(Index));
    }

    #region Folder Management

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateFolder(string folderName, int? parentFolderId = null)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(folderName))
            {
                TempData["ErrorMessage"] = "Folder name cannot be empty.";
                return RedirectToAction(nameof(Index), new { folderId = parentFolderId });
            }

            var owner = User.Identity?.Name ?? "Unknown";
            await _fileService.CreateFolderAsync(folderName, owner, parentFolderId);
            TempData["SuccessMessage"] = $"Folder '{folderName}' created successfully!";
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = $"Error creating folder: {ex.Message}";
        }

        return RedirectToAction(nameof(Index), new { folderId = parentFolderId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UploadFiles(IFormFileCollection files, int? parentFolderId = null)
    {
        try
        {
            if (files == null || files.Count == 0)
            {
                TempData["ErrorMessage"] = "Please select files to upload.";
                return RedirectToAction(nameof(Index), new { folderId = parentFolderId });
            }

            var owner = User.Identity?.Name ?? "Unknown";
            await _fileService.UploadFilesAsync(files, owner, parentFolderId);
            TempData["SuccessMessage"] = $"{files.Count} file(s) uploaded successfully!";
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = $"Error uploading files: {ex.Message}";
        }

        return RedirectToAction(nameof(Index), new { folderId = parentFolderId });
    }

    #endregion

    #region Metadata Management

    [HttpPost]
    [Route("api/files/{id}/metadata")]
    public async Task<IActionResult> UpdateMetadata(int id, [FromBody] UpdateMetadataRequest request)
    {
        try
        {
            await _fileService.UpdateMetadataAsync(id, request.Description, request.Tags, request.IsFavorite);
            return Ok(new { success = true, message = "Metadata updated successfully" });
        }
        catch (Exception ex)
        {
            return ErrorResponse(ex);
        }
    }

    [HttpPost]
    [Route("api/files/{id}/favorite")]
    public async Task<IActionResult> ToggleFavorite(int id)
    {
        try
        {
            var (file, error) = await GetFileOrNotFoundAsync(id);
            if (error != null)
                return error;

            await _fileService.UpdateMetadataAsync(id, null, null, !file.IsFavorite);
            return Ok(new { success = true, isFavorite = !file.IsFavorite });
        }
        catch (Exception ex)
        {
            return ErrorResponse(ex);
        }
    }

    [HttpGet]
    [Route("api/files/{id}/metadata")]
    public async Task<IActionResult> GetMetadata(int id)
    {
        try
        {
            var (file, error) = await GetFileOrNotFoundAsync(id);
            if (error != null)
                return error;

            return Ok(new
            {
                success = true,
                data = new
                {
                    id = file.Id,
                    name = file.Name,
                    description = file.Description,
                    tags = file.Tags,
                    isFavorite = file.IsFavorite,
                    created = file.Created,
                    modified = file.Modified,
                    size = file.Size,
                    downloadCount = file.DownloadCount,
                    lastAccessed = file.LastAccessed
                }
            });
        }
        catch (Exception ex)
        {
            return ErrorResponse(ex);
        }
    }

    #endregion

    private IActionResult ErrorResponse(Exception ex)
    {
        return BadRequest(new { success = false, message = ex.Message });
    }

    private async Task<(FileItem File, IActionResult? Error)> GetFileOrNotFoundAsync(int id)
    {
        var file = await _fileService.GetFileAsync(id);
        if (file == null)
            return (null!, NotFound(new { success = false, message = "File not found" }));

        return (file, null);
    }
}

// Request models
public class UpdateMetadataRequest
{
    public string? Description { get; set; }
    public string? Tags { get; set; }
    public bool? IsFavorite { get; set; }
}
