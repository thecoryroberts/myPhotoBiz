# Shared Utilities Reference

## Overview

This document provides a reference for all shared utility classes and helpers available in the myPhotoBiz solution.

---

## AppConstants (Helpers/AppConstants.cs)

Centralized constants to avoid magic numbers and strings throughout the codebase.

### File Sizes
```csharp
AppConstants.FileSizes.MaxPhotoUploadBytes  // 20 MB
AppConstants.FileSizes.MaxDocumentUploadBytes  // 10 MB
```

### Pagination
```csharp
AppConstants.Pagination.DefaultPageSize  // 48
AppConstants.Pagination.SmallPageSize    // 10
AppConstants.Pagination.LargePageSize    // 100
```

### Cache Durations
```csharp
AppConstants.Cache.DashboardCacheMinutes  // 5
AppConstants.Cache.GalleryCacheMinutes    // 10
```

### File Types
```csharp
AppConstants.FileTypes.ImageExtensions     // JPG, JPEG, PNG, GIF, BMP, WEBP
AppConstants.FileTypes.DocumentExtensions  // PDF, DOC, DOCX, ODT, TXT, RTF
AppConstants.FileTypes.VideoExtensions     // MP4, AVI, MOV, WMV, MKV
AppConstants.FileTypes.ArchiveExtensions   // ZIP, RAR, 7Z, TAR, GZ
AppConstants.FileTypes.ImageMimeTypes      // image/jpeg, image/png, etc.
```

### User Roles
```csharp
AppConstants.Roles.Admin         // "Admin"
AppConstants.Roles.Photographer  // "Photographer"
AppConstants.Roles.Client        // "Client"
AppConstants.Roles.StaffRoles    // ["Admin", "Photographer"]
```

---

## FileHelper (Helpers/FileHelper.cs)

Static helper methods for file operations.

### Methods

#### IsImageFile
Checks if a file is an allowed image type based on MIME type.
```csharp
bool isImage = FileHelper.IsImageFile(file);
```

#### IsImageExtension
Checks if a file extension is an image type.
```csharp
bool isImage = FileHelper.IsImageExtension(".jpg");
```

#### GetMimeType
Gets the MIME type for a file based on its extension.
```csharp
string mimeType = FileHelper.GetMimeType("/path/to/file.jpg");
// Returns: "image/jpeg"
```

#### GetAbsolutePath
Converts relative web path to absolute server path.
```csharp
string absolutePath = FileHelper.GetAbsolutePath("/uploads/photo.jpg", webRootPath);
```

#### SanitizeFileName
Sanitizes a string for use as a filename.
```csharp
string safe = FileHelper.SanitizeFileName("My Photo (1).jpg");
// Returns: "My_Photo_1.jpg"
```

#### GetFileCategory
Gets file category based on extension.
```csharp
string category = FileHelper.GetFileCategory(".jpg");
// Returns: "images"
```

#### GetExtensionsForCategory
Gets extensions for a given category.
```csharp
string[] extensions = FileHelper.GetExtensionsForCategory("images");
// Returns: ["JPG", "JPEG", "PNG", "GIF", "BMP", "WEBP"]
```

#### ReadFileToMemoryAsync
Reads file into memory stream for serving.
```csharp
MemoryStream? stream = await FileHelper.ReadFileToMemoryAsync(filePath);
```

---

## ControllerExtensions (Extensions/ControllerExtensions.cs)

Extension methods for ASP.NET Core controllers.

### User Identity Methods

#### GetCurrentUserId
Gets the current user's ID from claims.
```csharp
string? userId = this.GetCurrentUserId();
```

#### IsStaffUser
Checks if user is in any staff role (Admin or Photographer).
```csharp
if (this.IsStaffUser())
{
    // Allow admin/photographer access
}
```

#### IsAdmin
Checks if user is an admin.
```csharp
if (this.IsAdmin())
{
    // Allow admin-only access
}
```

### API Response Methods

#### ApiSuccess
Creates a successful JSON response.
```csharp
return this.ApiSuccess(data, "Operation completed");
```

#### ApiError
Creates an error JSON response.
```csharp
return this.ApiError("Something went wrong", 400);
```

---

## Usage Examples

### In Controllers

```csharp
// Using AppConstants
if (file.Length > AppConstants.FileSizes.MaxPhotoUploadBytes)
{
    return BadRequest("File too large");
}

// Using FileHelper
if (!FileHelper.IsImageFile(file))
{
    return BadRequest("Only images are allowed");
}

var mimeType = FileHelper.GetMimeType(photo.FilePath);
return File(stream, mimeType);

// Using ControllerExtensions
if (!this.IsStaffUser())
{
    return Forbid();
}

var userId = this.GetCurrentUserId();
```

### In Services

```csharp
// Using AppConstants for cache
var cacheOptions = new MemoryCacheEntryOptions()
    .SetSlidingExpiration(TimeSpan.FromMinutes(AppConstants.Cache.DashboardCacheMinutes));

// Using FileHelper for filtering
var extensions = FileHelper.GetExtensionsForCategory("images");
query = query.Where(f => extensions.Contains(f.Type));
```
