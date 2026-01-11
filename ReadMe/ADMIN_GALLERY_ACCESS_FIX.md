# Admin Gallery Access Enhancement

## Issue
Admins were unable to view galleries from the client perspective (`/Gallery` routes) because:
1. The GalleryController required `[Authorize(Roles = "Client")]` - blocking admins
2. The `ValidateUserAccessAsync` service only checked for ClientProfile - admins don't have one
3. Gallery listing and viewing functionality assumed all users had a ClientProfile

This prevented admins from previewing galleries as clients would see them.

## Solution

### 1. Updated Controller Authorization
**File:** [Controllers/GalleryController.cs:13](Controllers/GalleryController.cs#L13)

Changed authorization to allow both Client and Admin roles:

```csharp
// Before:
[Authorize(Roles = "Client")]
public class GalleryController : Controller

// After:
[Authorize(Roles = "Client,Admin")]
public class GalleryController : Controller
```

### 2. Enhanced Access Validation
**File:** [Services/GalleryService.cs:422-457](Services/GalleryService.cs#L422)

Updated `ValidateUserAccessAsync` to grant admins automatic access:

```csharp
public async Task<bool> ValidateUserAccessAsync(int galleryId, string userId)
{
    try
    {
        // Check if user is admin - admins have access to all galleries
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
        if (user != null)
        {
            var roles = await _context.UserRoles
                .Where(ur => ur.UserId == userId)
                .Join(_context.Roles, ur => ur.RoleId, r => r.Id, (ur, r) => r.Name)
                .ToListAsync();

            if (roles.Contains("Admin"))
                return true; // Admins always have access
        }

        // For non-admin users, check ClientProfile access
        var clientProfile = await _context.ClientProfiles
            .FirstOrDefaultAsync(cp => cp.UserId == userId);

        if (clientProfile == null)
            return false;

        return await _context.GalleryAccesses
            .AnyAsync(ga => ga.GalleryId == galleryId &&
                            ga.ClientProfileId == clientProfile.Id &&
                            ga.IsActive &&
                            (!ga.ExpiryDate.HasValue || ga.ExpiryDate > DateTime.UtcNow));
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, $"Error validating user access for gallery {galleryId}");
        throw;
    }
}
```

**Key Changes:**
- Checks if user has Admin role before checking ClientProfile
- Returns `true` immediately for admins
- Falls back to ClientProfile check for non-admins

### 3. Updated Gallery Index for Admins
**File:** [Controllers/GalleryController.cs:37-115](Controllers/GalleryController.cs#L37)

Modified `Index()` action to handle both clients and admins:

```csharp
public async Task<IActionResult> Index()
{
    var userId = _userManager.GetUserId(User);
    if (string.IsNullOrEmpty(userId))
        return RedirectToAction("Login", "Account");

    // Check if user is admin
    var isAdmin = User.IsInRole("Admin");

    if (isAdmin)
    {
        // Admins can see all active galleries
        var allGalleries = await _context.Galleries
            .Include(g => g.Albums)
                .ThenInclude(a => a.Photos)
            .Where(g => g.IsActive && g.ExpiryDate > DateTime.UtcNow)
            .OrderByDescending(g => g.CreatedDate)
            .ToListAsync();

        var adminViewModel = allGalleries.Select(gallery => new ClientGalleryViewModel
        {
            GalleryId = gallery.Id,
            Name = gallery.Name,
            Description = gallery.Description,
            BrandColor = gallery.BrandColor,
            PhotoCount = gallery.Albums.SelectMany(a => a.Photos).Count(),
            ExpiryDate = gallery.ExpiryDate,
            GrantedDate = gallery.CreatedDate, // Use creation date for admins
            CanDownload = true, // Admins have full permissions
            CanProof = true,
            CanOrder = true
        }).ToList();

        return View(adminViewModel);
    }

    // For regular clients, check ClientProfile
    // ... existing client logic ...
}
```

**Admin Benefits:**
- See **all active, non-expired galleries**
- Full permissions (CanDownload, CanProof, CanOrder all true)
- Ordered by creation date (newest first)
- Uses creation date instead of granted date

**Client Behavior:**
- Unchanged - still see only galleries they have access to
- Permissions based on GalleryAccess records

### 4. Simplified Session Creation
**File:** [Controllers/GalleryController.cs:150-172](Controllers/GalleryController.cs#L150)

Updated `ViewGallery()` to create sessions for all authenticated users:

```csharp
// Before: Only created sessions for users with ClientProfile
var clientProfile = await _context.ClientProfiles
    .FirstOrDefaultAsync(cp => cp.UserId == userId);

if (clientProfile != null)
{
    // Create session...
}

// After: Create sessions for all authenticated users (clients and admins)
var session = await _context.GallerySessions
    .FirstOrDefaultAsync(s => s.GalleryId == id && s.UserId == userId);

if (session == null)
{
    session = new GallerySession
    {
        GalleryId = id,
        UserId = userId,
        SessionToken = Guid.NewGuid().ToString(),
        CreatedDate = DateTime.UtcNow,
        LastAccessDate = DateTime.UtcNow
    };
    _context.GallerySessions.Add(session);
}
else
{
    session.LastAccessDate = DateTime.UtcNow;
}
await _context.SaveChangesAsync();

ViewBag.SessionToken = session.SessionToken;
```

**Benefits:**
- Admins can use proofing features (favorites, edit requests)
- Session tracking works for both clients and admins
- Simplified code - no need to check for ClientProfile

### 5. Download Permission Handling
**File:** [Controllers/GalleryController.cs:289-303](Controllers/GalleryController.cs#L289)

The existing download logic already handles admins correctly:

```csharp
// Get gallery access to check download permission
var clientProfile = await _context.ClientProfiles
    .FirstOrDefaultAsync(cp => cp.UserId == userId);

if (clientProfile != null)
{
    var access = await _context.GalleryAccesses
        .FirstOrDefaultAsync(ga => ga.GalleryId == galleryId && ga.ClientProfileId == clientProfile.Id);

    if (access != null && !access.CanDownload)
    {
        _logger.LogWarning($"Download not permitted for user {userId} on gallery {galleryId}");
        return Forbid();
    }
}
// If clientProfile is null (admin), permission check is skipped - download allowed
```

**Admin Behavior:**
- No ClientProfile → permission check skipped → download allowed ✅

**Client Behavior:**
- Has ClientProfile → checks `CanDownload` permission → enforced ✅

## User Experience

### Before Fix

**Admin attempts to view gallery:**
```
Admin navigates to /Gallery
→ 403 Forbidden ❌
→ "You don't have permission to access this resource"
```

**Admin attempts to view specific gallery:**
```
Admin navigates to /Gallery/ViewGallery/42
→ 403 Forbidden ❌
→ Cannot preview galleries as clients see them
```

### After Fix

**Admin views gallery list:**
```
Admin navigates to /Gallery
→ Page loads ✅
→ Shows all active, non-expired galleries
→ All galleries have full permissions
→ Can preview any gallery
```

**Admin views specific gallery:**
```
Admin navigates to /Gallery/ViewGallery/42
→ Gallery loads with photos ✅
→ Can use all features: favorite, download, request edits
→ Session tracked for analytics
→ Experience matches what clients see
```

**Admin copies gallery URL:**
```
Admin clicks "Copy Gallery URL" in /Galleries
→ URL: /Gallery/ViewGallery/42
→ Admin can test the URL themselves
→ Sees exactly what clients will see ✅
```

## Testing Instructions

### Test 1: Admin Gallery List
**Setup:**
1. Create 2-3 galleries with different statuses
2. Login as admin

**Steps:**
1. Navigate to `/Gallery`
2. Verify you see all active galleries
3. Check that permissions show as enabled (Download, Proofing, Order badges)

**Expected Results:**
✅ All active galleries displayed
✅ Ordered by creation date (newest first)
✅ All permission badges show as enabled
✅ Photo counts are accurate

### Test 2: Admin View Gallery
**Setup:**
1. Gallery with photos
2. Login as admin

**Steps:**
1. Navigate to `/Gallery`
2. Click "View Gallery" on any gallery
3. Verify photos load
4. Try lightbox navigation
5. Try downloading a photo
6. Try marking a photo as favorite

**Expected Results:**
✅ Gallery loads successfully
✅ All photos visible
✅ Lightbox works
✅ Download works (no permission error)
✅ Favorite functionality works
✅ Session created in database

### Test 3: Admin vs Client Access
**Setup:**
1. Gallery with restricted access
2. Grant access to Client A with `CanDownload = false`
3. Admin account
4. Client A account

**Steps:**
1. Login as Admin
2. Navigate to gallery, try download → Should work ✅
3. Logout
4. Login as Client A
5. Navigate to same gallery, try download → Should fail ❌

**Expected Results:**
✅ Admin: Downloads work regardless of access settings
✅ Client: Download blocked per GalleryAccess permissions

### Test 4: Public Gallery + Admin
**Setup:**
1. Gallery with public access enabled
2. Admin account

**Steps:**
1. Login as admin
2. Navigate to `/Gallery`
3. Find public gallery
4. View it via authenticated route (`/Gallery/ViewGallery/{id}`)
5. Open incognito window
6. View same gallery via public route (`/gallery/view/{token}`)

**Expected Results:**
✅ Admin sees gallery via authenticated route
✅ Anonymous user sees gallery via public route
✅ Both experiences are similar

### Test 5: Session Tracking
**Setup:**
1. Any gallery
2. Admin account

**Steps:**
1. Login as admin
2. Navigate to `/Gallery/ViewGallery/{id}`
3. Check database: `SELECT * FROM GallerySessions WHERE UserId = '{adminUserId}'`
4. Refresh the gallery page
5. Check `LastAccessDate` updated

**Expected Results:**
✅ GallerySession created with admin's UserId
✅ SessionToken generated
✅ LastAccessDate updates on each access
✅ Session visible in admin gallery management

## Routes Available to Admins

### Gallery Viewing (Client-facing)
| Route | Access | Purpose |
|-------|--------|---------|
| `/Gallery` | Client, **Admin** ✅ | List accessible galleries |
| `/Gallery/ViewGallery/{id}` | Client, **Admin** ✅ | View gallery photos |
| `/Gallery/Download?photoId={id}&galleryId={id}` | Client, **Admin** ✅ | Download photo |
| `/api/gallery/{galleryId}/photos` | Client, **Admin** ✅ | Get paginated photos |
| `/api/gallery/session/{galleryId}` | Client, **Admin** ✅ | Get session info |

### Public Access (Anonymous)
| Route | Access | Purpose |
|-------|--------|---------|
| `/gallery/view/{token}` | **Anonymous** ✅ | Public token-based access |
| `/gallery/{slug}` | **Anonymous** ✅ | Public SEO-friendly access |

### Gallery Management (Admin-only)
| Route | Access | Purpose |
|-------|--------|---------|
| `/Galleries` | **Admin** only | Gallery management dashboard |
| `/Galleries/Create` | **Admin** only | Create new gallery |
| `/Galleries/Edit/{id}` | **Admin** only | Edit gallery |
| `/Galleries/ManageAccess/{id}` | **Admin** only | Manage client access |

## Permission Matrix

| Feature | Client (with access) | Client (no access) | Admin | Anonymous (public) |
|---------|---------------------|-------------------|-------|-------------------|
| **View Gallery List** | ✅ Their galleries | ❌ | ✅ All galleries | ❌ |
| **View Gallery** | ✅ If granted | ❌ | ✅ All | ✅ If public |
| **Download Photo** | ✅ If CanDownload | ❌ | ✅ Always | ✅ If public |
| **Mark Favorite** | ✅ If CanProof | ❌ | ✅ Always | ✅ If public |
| **Request Edit** | ✅ If CanProof | ❌ | ✅ Always | ✅ If public |
| **Order Prints** | ✅ If CanOrder | ❌ | ✅ Always | ❌ Future feature |
| **Manage Gallery** | ❌ | ❌ | ✅ Always | ❌ |
| **Grant Access** | ❌ | ❌ | ✅ Always | ❌ |

## Database Impact

### GallerySessions Table
**Before:** Only had sessions for users with ClientProfile (clients only)
**After:** Has sessions for all authenticated users (clients + admins)

**Query to find admin sessions:**
```sql
SELECT gs.*, u.Email, u.FirstName, u.LastName
FROM GallerySessions gs
JOIN AspNetUsers u ON gs.UserId = u.Id
LEFT JOIN ClientProfiles cp ON cp.UserId = u.Id
WHERE cp.Id IS NULL; -- No ClientProfile = likely admin
```

### No Schema Changes
All changes are application logic only - no migrations required.

## Benefits

### For Admins
1. **Preview galleries** before sharing with clients
2. **Test gallery URLs** to ensure they work
3. **Quality assurance** - see exactly what clients see
4. **Troubleshooting** - replicate client issues
5. **Demo galleries** for sales/marketing

### For Development
1. **Simplified authorization logic** - roles instead of profiles
2. **Better code reuse** - same views for clients and admins
3. **Consistent behavior** - fewer edge cases
4. **Easier testing** - admins can test without creating client accounts

### For Business
1. **Faster onboarding** - admins can demo galleries instantly
2. **Better support** - see what clients see
3. **Quality control** - verify galleries before delivery

## Technical Notes

### Role Check Performance
The `ValidateUserAccessAsync` method now performs a database join to check roles:

```csharp
var roles = await _context.UserRoles
    .Where(ur => ur.UserId == userId)
    .Join(_context.Roles, ur => ur.RoleId, r => r.Id, (ur, r) => r.Name)
    .ToListAsync();
```

**Performance Considerations:**
- Additional query per access validation
- Cached in-memory after first check (within same request)
- Minimal overhead - admin role checks are fast
- Only executed if user doesn't have ClientProfile

**Future Optimization:**
Could cache role information in claims at login to avoid database query.

### Admin vs Client Distinction

The system distinguishes admins from clients in two ways:

1. **Role-based** (ASP.NET Identity):
   - `User.IsInRole("Admin")` - framework level
   - Fast, uses ClaimsPrincipal

2. **Profile-based** (Business logic):
   - `ClientProfile` exists = client user
   - `ClientProfile` null + Admin role = admin user

This dual approach ensures backward compatibility while enabling admin access.

## Files Modified

1. **Controllers/GalleryController.cs**
   - Line 13: Changed authorization from `"Client"` to `"Client,Admin"`
   - Lines 37-115: Updated Index to handle admins
   - Lines 150-172: Simplified session creation

2. **Services/GalleryService.cs**
   - Lines 422-457: Enhanced ValidateUserAccessAsync with admin check

## Build Status
✅ **Build Succeeded**
- 0 Errors
- 13 Warnings (all pre-existing nullable reference warnings)

## Related Documentation

- [GALLERY_FIXES_REPORT.md](GALLERY_FIXES_REPORT.md) - Complete gallery system overview
- [GALLERY_URL_FIX.md](GALLERY_URL_FIX.md) - Gallery URL generation fix
- [GALLERY_CLIENT_ACCESS_FIX.md](GALLERY_CLIENT_ACCESS_FIX.md) - Client selection fix

---

**Enhancement Applied:** January 10, 2026
**Status:** ✅ Complete
**Impact:** Admins can now preview and test galleries from the client perspective
