# Gallery Functionality - Complete Integration

## Overview
The gallery system is fully integrated and operational. It enables photographers to create galleries, organize photos from albums, manage client access, and share galleries securely.

## Components

### Models
- **Gallery** (`Models/Gallery.cs`)
  - Represents a collection of albums with shared access/permissions
  - Many-to-many relationship with Albums
  - Supports public access via token-based authentication
  - Properties: Name, Description, ExpiryDate, IsActive, BrandColor, PublicAccessToken, Slug

- **GallerySession** (`Models/GallerySession.cs`)
  - Tracks client viewing sessions
  - Links to Proofs (selections) and PrintOrders
  - Properties: ClientProfileId, GalleryId, CreatedDate, LastAccessDate, SessionStatus

- **GalleryAccess** (`Models/GalleryAccess.cs`)
  - Controls client-level permissions on galleries
  - Permissions: CanDownload, CanProof, CanOrder
  - Optional expiry dates for time-limited access

- **Album** (`Models/Album.cs`)
  - Contains photos from a specific photo shoot
  - Many-to-many with Gallery
  - Related to PhotoShoot and ClientProfile

- **Photo** (`Models/Photo.cs`)
  - Individual image files with thumbnails
  - Has DisplayOrder for custom ordering
  - Tracks uploads and linked to Album and ClientProfile

### Controllers

#### GalleriesController (Admin Only)
- **Index** - List all galleries with stats (total, active, expired, sessions)
- **Create** - Create new gallery with album selection
- **Edit** - Update gallery details
- **Delete** - Remove gallery
- **Details** - View gallery photos with pagination
- **GrantAccess** - Assign gallery to clients
- **RevokeAccess** - Remove client access
- **EnablePublicAccess** - Generate public token for sharing
- **DisablePublicAccess** - Revoke public sharing

#### GalleryController (Client)
- **Index** - List accessible galleries (identity-based + public token)
- **ViewGallery** - Display gallery photos (paginated)
- **Download** - Get photo with audit logging
- **RequestProof** - Submit proof selections

#### PhotosController (Admin)
- **Upload** - Upload photos to album (multi-file dropzone)
- **Delete** - Remove photo from album
- **UpdateOrder** - Reorder photos

### Services

#### IGalleryService / GalleryService
Core functionality:
- `GetAllGalleriesAsync()` - SQL-optimized retrieval
- `GetGalleryDetailsAsync(id, page, pageSize)` - Paginated photos
- `CreateGalleryAsync()` - Create with albums + client access
- `UpdateGalleryAsync()` - Update details and relationships
- `DeleteGalleryAsync()` - Cascade delete sessions
- `AddAlbumsToGalleryAsync()` - M:N link
- `GrantAccessAsync()` - Configurable client permissions
- `RevokeAccessAsync()` - Remove access
- `EnablePublicAccessAsync()` - Token generation
- `ValidatePublicAccessAsync()` - Token validation

#### IImageService / ImageService
- `ProcessAndSaveAlbumImageAsync()` - Save full image + thumbnail (enqueued for async thumbnail generation via background task queue)

#### IAlbumService / AlbumService
- Album CRUD operations
- Photo management within albums

### Views

**Admin Views** (`Views/Galleries/`)
- **Index.cshtml** - Gallery list with stats cards and filtering
- **_CreateGalleryModal.cshtml** - Create form (album selection, expiry, clients)
- **_EditGalleryModal.cshtml** - Edit form
- **_GalleryDetailsModal.cshtml** - View photos, sessions, proofs, print orders
- **_ManageAccessModal.cshtml** - Client permission management
- **_GallerySessionsModal.cshtml** - Active sessions
- **_RegenerateCodeModal.cshtml** - Public token refresh

**Client Views** (`Views/Gallery/`)
- **Index.cshtml** - Accessible galleries list
- **ViewGallery.cshtml** - Photo gallery viewer (paginated)
- **NoAccess.cshtml** - Fallback for unauthorized clients

### Database Configuration

**DbContext** (`Data/ApplicationDbContext.cs`)
- DbSet<Gallery>, DbSet<GallerySession>, DbSet<GalleryAccess>
- Relationships configured in `ConfigureGalleryRelationships()`
- Cascade delete on GallerySession
- M:N with Album using join table

**Migrations**
- Tables created and indexed for performance
- Indexes on: IsActive, CreatedDate, ExpiryDate, ClientProfileId

**Constraints**
- Gallery.ExpiryDate must be > DateTime.UtcNow for active galleries
- GalleryAccess optional expiry dates for time-limited access
- Public token is Base64-URL-safe (no padding)

## Workflow Examples

### Create and Share a Gallery

```
1. Admin creates gallery
   - Set name, description, expiry date
   - Select albums with photos
   - Choose default brand color

2. Admin grants access to clients
   - Select client(s)
   - Set permissions: Download, Proof, Order
   - Optional expiry date

3. Client views gallery
   - Logs in → Dashboard → Galleries
   - Views photos (paginated, 48 per page)
   - Can download (if permission granted)
   - Can request proofs (selections)

4. Optional: Share via public link
   - Admin enables public access → token generated
   - Share URL with clients (no login needed)
   - Client views gallery with limited permissions
```

### Upload Photos to Album

```
1. Admin navigates to album
2. Clicks "Upload Photos"
3. Selects/drags files (multi-file)
4. ImageService processes:
   - Full-size image (max 1920px width, 90% quality)
   - Thumbnail (400x300px, 80% quality)
5. Enqueue thumbnail generation to async background service
6. Photos appear in galleries containing this album
```

## Performance Optimizations

- **SQL-level aggregation** in GetAllGalleriesAsync (counts at DB, not in-memory)
- **Pagination** in GetGalleryDetailsAsync (48 photos/page)
- **Async image processing** (thumbnails generated in background)
- **Indexes** on frequently queried columns (IsActive, ExpiryDate, ClientProfileId)
- **AsNoTracking()** for read-only queries

## Security

- **Role-based access**: GalleriesController [Authorize(Roles = "Admin")]
- **Identity-based**: GalleryAccess tied to ClientProfile
- **Token-based public**: PublicAccessToken for sharing without login
- **Expiry enforcement**: GalleryAccess.ExpiryDate checked at query time
- **Audit logging**: Downloads tracked with userId, ipAddress, timestamp

## Known TODOs / Future Enhancements

- [ ] Gallery search/filtering functionality
- [ ] Gallery expiry notification emails
- [ ] Rate limiting on photo enumeration
- [ ] Watermarking support for client-facing photos
- [ ] Batch download (ZIP) functionality
- [ ] Photo reordering UI (backend exists, frontend needed)
- [ ] Email invitations for gallery access
- [ ] Advanced analytics (view duration, download counts)

## Testing Checklist

- [x] Gallery CRUD operations
- [x] Album M:N linking
- [x] Client permission grants
- [x] Public token generation and validation
- [x] Photo upload with image processing
- [x] Async thumbnail generation
- [x] Pagination (photos)
- [x] ExpiredDate filtering
- [x] Database relationships and cascades
- [x] Controllers and service integration
- [x] Views rendering (modals, tables, forms)
- [x] Build compilation successful

## File Locations

**Code**
- Models: `Models/Gallery.cs`, `Models/GallerySession.cs`, `Models/GalleryAccess.cs`, `Models/Album.cs`, `Models/Photo.cs`
- Controllers: `Controllers/GalleriesController.cs`, `Controllers/GalleryController.cs`, `Controllers/PhotosController.cs`
- Services: `Services/GalleryService.cs`, `Services/IGalleryService.cs`, `Services/IImageService.cs`
- Views: `Views/Galleries/*.cshtml`, `Views/Gallery/*.cshtml`
- Database: `Data/ApplicationDbContext.cs` (ConfigureGalleryRelationships method)

**Documentation**
- This file: `GALLERY_FUNCTIONALITY.md`

## Quick Start

1. **Create a PhotoShoot** (if not already done)
2. **Upload photos** to the album via Admin → Album → Upload
3. **Create Gallery** via Admin → Galleries → Create
   - Select album(s) with photos
   - Set expiry date (e.g., 30 days from today)
4. **Grant access** to clients via Galleries → Details → Manage Access
5. **Share** with client or generate public link

That's it! Gallery is operational and ready for use.
