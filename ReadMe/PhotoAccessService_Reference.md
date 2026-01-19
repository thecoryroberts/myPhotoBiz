# PhotoAccessService Reference

## Overview

The `PhotoAccessService` centralizes photo access authorization logic, eliminating duplicate permission checks across controllers. It determines whether a user can access a specific photo based on their role and ownership.

---

## Interface: IPhotoAccessService

**Location:** `Services/IPhotoAccessService.cs`

```csharp
public interface IPhotoAccessService
{
    Task<PhotoAccessResult> CanAccessPhotoAsync(Photo photo, ClaimsPrincipal user);
    bool IsPhotoInPublicGallery(Photo photo);
}
```

---

## Methods

### CanAccessPhotoAsync

Checks if the current user can access a photo.

```csharp
Task<PhotoAccessResult> CanAccessPhotoAsync(Photo photo, ClaimsPrincipal user);
```

**Authorization Logic:**
1. If photo is in a public gallery (active, non-expired) -> **Allow**
2. If user is not authenticated and not in public gallery -> **Deny**
3. If user is Admin or Photographer -> **Allow**
4. If user is a Client and owns the photo -> **Allow**
5. Otherwise -> **Deny**

**Example:**
```csharp
var photo = await _photoService.GetPhotoByIdAsync(id);
var result = await _photoAccessService.CanAccessPhotoAsync(photo, User);

if (!result.IsAllowed)
{
    return Forbid();
}

// Proceed with photo access
```

### IsPhotoInPublicGallery

Checks if a photo is in a public (active, non-expired) gallery.

```csharp
bool IsPhotoInPublicGallery(Photo photo);
```

**Returns:** `true` if the photo belongs to at least one gallery that is:
- `IsActive == true`
- `ExpiryDate > DateTime.UtcNow`

---

## PhotoAccessResult

Result wrapper for access checks.

```csharp
public class PhotoAccessResult
{
    public bool IsAllowed { get; set; }
    public string? DenialReason { get; set; }

    public static PhotoAccessResult Allowed();
    public static PhotoAccessResult Denied(string reason);
}
```

### Denial Reasons

| Reason | Description |
|--------|-------------|
| "Authentication required" | User is not logged in and photo is not public |
| "User not found" | Could not find user ID in claims |
| "Client profile not found" | User doesn't have a client profile |
| "Access denied to this photo" | Client doesn't own the photo |

---

## Usage in Controllers

### Before (Duplicate Code)

```csharp
// This pattern was repeated in multiple actions
if (!User.IsInRole("Admin") && !User.IsInRole("Photographer"))
{
    var userId = _userManager.GetUserId(User);
    var client = await _clientService.GetClientByUserIdAsync(userId!);
    if (client == null || photo.Album?.PhotoShoot?.ClientProfileId != client.Id)
    {
        return Forbid();
    }
}
```

### After (Using Service)

```csharp
public async Task<IActionResult> View(int id)
{
    var photo = await _photoService.GetPhotoByIdAsync(id);
    if (photo == null)
        return NotFound();

    // Staff can bypass detailed check
    if (!this.IsStaffUser())
    {
        var accessResult = await _photoAccessService.CanAccessPhotoAsync(photo, User);
        if (!accessResult.IsAllowed)
            return Forbid();
    }

    return await ServePhotoFileAsync(photo.FilePath);
}
```

### For Anonymous Endpoints

```csharp
[AllowAnonymous]
public async Task<IActionResult> Thumbnail(int id)
{
    var photo = await _photoService.GetPhotoByIdAsync(id);
    if (photo == null)
        return NotFound();

    // Works for both authenticated and anonymous users
    var accessResult = await _photoAccessService.CanAccessPhotoAsync(photo, User);
    if (!accessResult.IsAllowed)
        return Forbid();

    return await ServePhotoFileAsync(photo.ThumbnailPath ?? photo.FilePath);
}
```

---

## Dependency Injection

### Registration (Program.cs)

```csharp
builder.Services.AddScoped<IPhotoAccessService, PhotoAccessService>();
```

### Injection

```csharp
public class PhotosController : Controller
{
    private readonly IPhotoAccessService _photoAccessService;

    public PhotosController(IPhotoAccessService photoAccessService)
    {
        _photoAccessService = photoAccessService;
    }
}
```

---

## Access Matrix

| User Type | Public Gallery | Own Photo | Other's Photo |
|-----------|----------------|-----------|---------------|
| Anonymous | Allow | Deny | Deny |
| Client | Allow | Allow | Deny |
| Photographer | Allow | Allow | Allow |
| Admin | Allow | Allow | Allow |

---

## Implementation Notes

The service uses `AppConstants.Roles` for role checks:

```csharp
private static bool IsStaffUser(ClaimsPrincipal user)
{
    return user.IsInRole(AppConstants.Roles.Admin) ||
           user.IsInRole(AppConstants.Roles.Photographer);
}
```

Photo ownership is determined by checking:
```csharp
photo.Album?.PhotoShoot?.ClientProfileId == client.Id
```

This requires the Photo entity to be loaded with:
- Album navigation property
- Album.PhotoShoot navigation property
- PhotoShoot.ClientProfileId
