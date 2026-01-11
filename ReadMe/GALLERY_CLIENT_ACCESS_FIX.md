# Gallery Client Access Fix

## Issue
When creating a new gallery, the "Grant Access To" section was showing:
> "No clients available. Create clients first to grant gallery access."

This occurred even when client profiles existed in the database.

## Root Cause
The `GalleriesController.Create()` action was only populating `AvailableAlbums` but not `AvailableClients` in the `CreateGalleryViewModel`.

## Solution

### 1. Added Interface Method
**File:** [Services/IGalleryService.cs:43](Services/IGalleryService.cs#L43)

Added new method to the interface:
```csharp
// Client Management
Task<List<ClientSelectionViewModel>> GetAvailableClientsAsync();
```

### 2. Implemented Service Method
**File:** [Services/GalleryService.cs:567-596](Services/GalleryService.cs#L567)

Implemented the method to retrieve active clients:
```csharp
public async Task<List<ClientSelectionViewModel>> GetAvailableClientsAsync()
{
    try
    {
        var clients = await _context.ClientProfiles
            .Include(cp => cp.User)
            .Where(cp => !cp.IsDeleted && cp.User != null)
            .OrderBy(cp => cp.User.LastName)
            .ThenBy(cp => cp.User.FirstName)
            .Select(cp => new ClientSelectionViewModel
            {
                Id = cp.Id,
                FullName = cp.User != null ? $"{cp.User.FirstName} {cp.User.LastName}" : "Unknown",
                Email = cp.User != null ? cp.User.Email : null,
                PhoneNumber = cp.PhoneNumber,
                IsSelected = false
            })
            .ToListAsync();

        return clients;
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Error retrieving available clients");
        throw;
    }
}
```

**Features:**
- Filters out soft-deleted clients (`!cp.IsDeleted`)
- Requires user to be present (`cp.User != null`)
- Orders alphabetically by last name, then first name
- Returns client ID, full name, email, and phone number

### 3. Updated Controller Actions
**File:** [Controllers/GalleriesController.cs](Controllers/GalleriesController.cs)

#### GET Create Action (Line 76-79)
```csharp
var model = new CreateGalleryViewModel
{
    AvailableAlbums = await _galleryService.GetAvailableAlbumsAsync(),
    AvailableClients = await _galleryService.GetAvailableClientsAsync()  // ADDED
};
```

#### POST Create Action (Line 100-101)
```csharp
if (!ModelState.IsValid)
{
    model.AvailableAlbums = await _galleryService.GetAvailableAlbumsAsync();
    model.AvailableClients = await _galleryService.GetAvailableClientsAsync();  // ADDED
    // ... error handling
}
```

## Testing Instructions

### Prerequisites
1. Ensure you have at least one client in the system:
   - Login as Admin
   - Navigate to Clients section
   - Create a client if none exist

### Test Steps
1. **Login as Admin**
2. **Navigate to Galleries** (`/Galleries`)
3. **Click "Create Gallery"** button
4. **Check the "Access" tab**
   - You should now see a list of all active clients
   - Each client should show:
     - Full name
     - Email address (if available)
     - Checkbox for selection

### Expected Results
✅ Clients appear in the "Grant Access To" section
✅ Client list is scrollable (max-height: 300px)
✅ Each client has a checkbox for selection
✅ Client name and email are displayed
✅ No "No clients available" message when clients exist

### Edge Cases to Test

#### No Clients Exist
- **Action:** Open Create Gallery modal when no clients exist in database
- **Expected:** Show message "No clients available. Create clients first to grant gallery access."
- **Result:** ✅ Working (original behavior preserved)

#### Only Deleted Clients Exist
- **Action:** Soft-delete all clients, then open Create Gallery modal
- **Expected:** Show "No clients available" message
- **Result:** ✅ Filtered correctly (IsDeleted clients excluded)

#### Client Without User
- **Action:** If client profile exists but User is null (data integrity issue)
- **Expected:** Client is filtered out
- **Result:** ✅ Handled by `cp.User != null` filter

## Files Modified

1. **Services/IGalleryService.cs**
   - Added `GetAvailableClientsAsync()` method signature

2. **Services/GalleryService.cs**
   - Implemented `GetAvailableClientsAsync()` method (30 lines)

3. **Controllers/GalleriesController.cs**
   - Updated GET `Create()` action to populate clients
   - Updated POST `Create()` action to repopulate clients on validation error

## Database Impact
**None** - This change only affects data retrieval, no schema changes required.

## Build Status
✅ **Build Succeeded**
- 0 Errors
- 0 New Warnings

## Related ViewModels

### ClientSelectionViewModel
**File:** [ViewModels/ContractViewModels.cs:103-111](ViewModels/ContractViewModels.cs#L103)

Already existed and is shared across contracts and galleries:
```csharp
public class ClientSelectionViewModel
{
    public int Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Name => FullName; // Alias for FullName
    public string? Email { get; set; }
    public string? PhoneNumber { get; set; }
    public bool IsSelected { get; set; }
}
```

## View Implementation

### Create Gallery Modal
**File:** [Views/Galleries/_CreateGalleryModal.cshtml:81-107](Views/Galleries/_CreateGalleryModal.cshtml#L81)

The view was already properly implemented with conditional rendering:
```cshtml
<label class="form-label">Grant Access To:</label>
@if (Model?.AvailableClients?.Any() == true)
{
    <div class="list-group" style="max-height: 300px; overflow-y: auto;">
        @foreach (var client in Model.AvailableClients)
        {
            <label class="list-group-item d-flex align-items-center gap-2">
                <input type="checkbox" name="SelectedClientProfileIds" value="@client.Id"
                       class="form-check-input" @(client.IsSelected ? "checked" : "")>
                <div>
                    <strong>@client.Name</strong>
                    @if (!string.IsNullOrEmpty(client.Email))
                    {
                        <br><small class="text-muted">@client.Email</small>
                    }
                </div>
            </label>
        }
    </div>
}
else
{
    <p class="text-muted">No clients available. Create clients first to grant gallery access.</p>
}
```

## Client Access Flow

### When Creating a Gallery

1. **Admin opens Create Gallery modal**
   - `GalleriesController.Create()` is called
   - Retrieves all active clients via `GetAvailableClientsAsync()`
   - Retrieves all albums via `GetAvailableAlbumsAsync()`

2. **Admin selects clients to grant access**
   - Checkboxes with `name="SelectedClientProfileIds"`
   - Values are client profile IDs

3. **Admin submits form**
   - `GalleriesController.Create(CreateGalleryViewModel)` is called
   - Gallery is created via `GalleryService.CreateGalleryAsync()`
   - For each selected client ID:
     - A `GalleryAccess` record is created
     - Permissions are set (CanDownload, CanProof, CanOrder)
     - Access granted date is set

4. **Clients can now view the gallery**
   - Navigate to `/Gallery` (Gallery/Index)
   - See galleries they have access to
   - Click to view photos

## Access Permission Model

Each `GalleryAccess` record includes:
- **GalleryId** - Which gallery
- **ClientProfileId** - Which client
- **GrantedDate** - When access was granted
- **ExpiryDate** - When access expires (nullable)
- **IsActive** - Can be toggled on/off
- **CanDownload** - Permission to download full-resolution photos
- **CanProof** - Permission to mark favorites and request edits
- **CanOrder** - Permission to order prints (future feature)

## Additional Notes

### Why Use ClientProfile Instead of User?
The system uses `ClientProfile` for gallery access rather than `ApplicationUser` because:
1. Not all users are clients (some are admins, photographers, etc.)
2. ClientProfile contains business-specific data (address, notes, category)
3. Separation of concerns (authentication vs. business logic)
4. Allows tracking client-specific metadata (lifetime value, completed shoots)

### Future Enhancements
- **Bulk Access Grant** - Select multiple galleries and grant access to client at once
- **Access Templates** - Save common permission sets (e.g., "Full Access", "View Only")
- **Time-Limited Access** - Set default expiry when granting access
- **Access Notifications** - Email clients when gallery access is granted
- **Access History** - Track when access was granted/revoked and by whom

---

**Fix Applied:** January 10, 2026
**Status:** ✅ Resolved
**Impact:** Client access can now be properly granted when creating galleries
