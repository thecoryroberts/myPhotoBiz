# Production Readiness Checklist (Per-View)

Use this checklist for **every view** (page, modal, partial, or route) in the app before release. The goal is to make each view consistently functional, secure, performant, accessible, and supportable in production.

---

## 1) Basic View Inventory (per view)
- [ ] **View name & route documented** (URL, controller/action, area).
- [ ] **Purpose statement**: what user job-to-be-done this view supports.
- [ ] **Owner & dependencies** (API endpoints, DB tables, external services, feature flags).
- [ ] **User roles/permissions required** and role matrix updated.
- [ ] **Analytics event name** for page view and key actions.

### View: Home/Dashboard (Staff)
- [x] **View name & route documented**: `Views/Home/Dashboard.cshtml` served by `HomeController.Index()` (Admin/Photographer) and `HomeController.Dashboard()` (`/Home/Dashboard`, [Authorize]).
  - Primary routes: `GET /` → `Home/Index` (returns Dashboard for Admin/Photographer), `GET /Home/Dashboard`.
- [x] **Purpose statement**: Provide staff with high-level business KPIs and operational overview (clients, shoots, invoices, utilization, activity).
- [x] **Owner & dependencies**:
  - Owner: `HomeController`, `DashboardService`.
  - Dependencies: `IDashboardService`, `ApplicationDbContext` tables: `ClientProfiles`, `PhotoShoots`, `BookingRequests`, `Contracts`, `Invoices`, `Activities`.
  - Caching: `IMemoryCache` via `DashboardService`.
  - External services/feature flags: none identified.
- [x] **User roles/permissions required**: `Admin`, `Photographer` (Clients are redirected to `Clients/MyProfile`).
- [ ] **Analytics event name**: `view_home_dashboard` (page view) + `action_dashboard_widget_toggle` (key action).

### Application-wide (All Views)
- [ ] **View name & route documented**: map all views to controller actions (include attribute-routed endpoints and Areas).
- [ ] **Purpose statement**: add a 1-line purpose per view (staff vs. client vs. public).
- [ ] **Owner & dependencies**: confirm controller/service ownership and DB dependencies per view.
- [ ] **User roles/permissions required**: build a role matrix per view/action.
- [ ] **Analytics event name**: use `view_{area}_{view}` for page views and `action_{area}_{action}` for key actions (all lowercase, underscores).

#### Static scan (view inventory)

#### View inventory (alphabetical)

### View: Account/DeleteAccount.cshtml
- [ ] **View name & route documented**: GET /Account/DeleteAccount
- [ ] **Purpose statement**: Account deletion confirmation.
- [ ] **Owner & dependencies**: Identity/auth flow (no AccountController in /Controllers)
- [ ] **User roles/permissions required**: Public/auth flow (verify)
- [ ] **Analytics event name**: `view_account_deleteaccount`

### View: Account/LockScreen.cshtml
- [ ] **View name & route documented**: GET /Account/LockScreen
- [ ] **Purpose statement**: Lock screen / re-auth.
- [ ] **Owner & dependencies**: Identity/auth flow (no AccountController in /Controllers)
- [ ] **User roles/permissions required**: Public/auth flow (verify)
- [ ] **Analytics event name**: `view_account_lockscreen`

### View: Account/Login.cshtml
- [ ] **View name & route documented**: GET /Account/Login
- [ ] **Purpose statement**: User login.
- [ ] **Owner & dependencies**: Identity/auth flow (no AccountController in /Controllers)
- [ ] **User roles/permissions required**: Public/auth flow (verify)
- [ ] **Analytics event name**: `view_account_login`

### View: Account/LoginPin.cshtml
- [ ] **View name & route documented**: GET /Account/LoginPin
- [ ] **Purpose statement**: PIN login.
- [ ] **Owner & dependencies**: Identity/auth flow (no AccountController in /Controllers)
- [ ] **User roles/permissions required**: Public/auth flow (verify)
- [ ] **Analytics event name**: `view_account_loginpin`

### View: Account/NewPass.cshtml
- [ ] **View name & route documented**: GET /Account/NewPass
- [ ] **Purpose statement**: Set new password.
- [ ] **Owner & dependencies**: Identity/auth flow (no AccountController in /Controllers)
- [ ] **User roles/permissions required**: Public/auth flow (verify)
- [ ] **Analytics event name**: `view_account_newpass`

### View: Account/ResetPass.cshtml
- [ ] **View name & route documented**: GET /Account/ResetPass
- [ ] **Purpose statement**: Password reset request.
- [ ] **Owner & dependencies**: Identity/auth flow (no AccountController in /Controllers)
- [ ] **User roles/permissions required**: Public/auth flow (verify)
- [ ] **Analytics event name**: `view_account_resetpass`

### View: Account/SignUp.cshtml
- [ ] **View name & route documented**: GET /Account/SignUp
- [ ] **Purpose statement**: User registration.
- [ ] **Owner & dependencies**: Identity/auth flow (no AccountController in /Controllers)
- [ ] **User roles/permissions required**: Public/auth flow (verify)
- [ ] **Analytics event name**: `view_account_signup`

### View: Account/SuccessMail.cshtml
- [ ] **View name & route documented**: GET /Account/SuccessMail
- [ ] **Purpose statement**: Email confirmation notice.
- [ ] **Owner & dependencies**: Identity/auth flow (no AccountController in /Controllers)
- [ ] **User roles/permissions required**: Public/auth flow (verify)
- [ ] **Analytics event name**: `view_account_successmail`

### View: Account/TwoFactor.cshtml
- [ ] **View name & route documented**: GET /Account/TwoFactor
- [ ] **Purpose statement**: Account flow view.
- [ ] **Owner & dependencies**: Identity/auth flow (no AccountController in /Controllers)
- [ ] **User roles/permissions required**: Public/auth flow (verify)
- [ ] **Analytics event name**: `view_account_twofactor`

### View: Albums/Create.cshtml
- [ ] **View name & route documented**: GET /Albums/Create
- [ ] **Purpose statement**: Create album.
- [ ] **Owner & dependencies**: AlbumsController
- [ ] **User roles/permissions required**: Roles: Admin, Photographer (also uses default authorization; verify per action)
- [ ] **Analytics event name**: `view_albums_create`

### View: Albums/Delete.cshtml
- [ ] **View name & route documented**: GET /Albums/Delete
- [ ] **Purpose statement**: Confirm delete album.
- [ ] **Owner & dependencies**: AlbumsController
- [ ] **User roles/permissions required**: Roles: Admin, Photographer (also uses default authorization; verify per action)
- [ ] **Analytics event name**: `view_albums_delete`

### View: Albums/Details.cshtml
- [ ] **View name & route documented**: GET /Albums/Details
- [ ] **Purpose statement**: View album details.
- [ ] **Owner & dependencies**: AlbumsController
- [ ] **User roles/permissions required**: Roles: Admin, Photographer (also uses default authorization; verify per action)
- [ ] **Analytics event name**: `view_albums_details`

### View: Albums/Edit.cshtml
- [ ] **View name & route documented**: GET /Albums/Edit
- [ ] **Purpose statement**: Edit album.
- [ ] **Owner & dependencies**: AlbumsController
- [ ] **User roles/permissions required**: Roles: Admin, Photographer (also uses default authorization; verify per action)
- [ ] **Analytics event name**: `view_albums_edit`

### View: Albums/Index.cshtml
- [ ] **View name & route documented**: GET /Albums, GET /Albums/Index
- [ ] **Purpose statement**: List and manage albums.
- [ ] **Owner & dependencies**: AlbumsController
- [ ] **User roles/permissions required**: Roles: Admin, Photographer (also uses default authorization; verify per action)
- [ ] **Analytics event name**: `view_albums_index`

### View: Badges/Create.cshtml
- [ ] **View name & route documented**: GET /Badges/Create
- [ ] **Purpose statement**: Create badge.
- [ ] **Owner & dependencies**: BadgesController
- [ ] **User roles/permissions required**: Roles: Admin (verify per action)
- [ ] **Analytics event name**: `view_badges_create`

### View: Badges/Edit.cshtml
- [ ] **View name & route documented**: GET /Badges/Edit
- [ ] **Purpose statement**: Edit badge.
- [ ] **Owner & dependencies**: BadgesController
- [ ] **User roles/permissions required**: Roles: Admin (verify per action)
- [ ] **Analytics event name**: `view_badges_edit`

### View: Badges/Index.cshtml
- [ ] **View name & route documented**: GET /Badges, GET /Badges/Index
- [ ] **Purpose statement**: List and manage badges.
- [ ] **Owner & dependencies**: BadgesController
- [ ] **User roles/permissions required**: Roles: Admin (verify per action)
- [ ] **Analytics event name**: `view_badges_index`

### View: Bookings/Availability.cshtml
- [ ] **View name & route documented**: GET /Bookings/Availability
- [ ] **Purpose statement**: TODO
- [ ] **Owner & dependencies**: BookingsController
- [ ] **User roles/permissions required**: Roles: Admin, Photographer, Client (some actions allow anonymous; verify per action)
- [ ] **Analytics event name**: `view_bookings_availability`

### View: Bookings/Book.cshtml
- [ ] **View name & route documented**: GET /Bookings/Book
- [ ] **Purpose statement**: TODO
- [ ] **Owner & dependencies**: BookingsController
- [ ] **User roles/permissions required**: Roles: Admin, Photographer, Client (some actions allow anonymous; verify per action)
- [ ] **Analytics event name**: `view_bookings_book`

### View: Bookings/Confirmation.cshtml
- [ ] **View name & route documented**: GET /Bookings/Confirmation
- [ ] **Purpose statement**: TODO
- [ ] **Owner & dependencies**: BookingsController
- [ ] **User roles/permissions required**: Roles: Admin, Photographer, Client (some actions allow anonymous; verify per action)
- [ ] **Analytics event name**: `view_bookings_confirmation`

### View: Bookings/Details.cshtml
- [ ] **View name & route documented**: GET /Bookings/Details
- [ ] **Purpose statement**: View booking details.
- [ ] **Owner & dependencies**: BookingsController
- [ ] **User roles/permissions required**: Roles: Admin, Photographer, Client (some actions allow anonymous; verify per action)
- [ ] **Analytics event name**: `view_bookings_details`

### View: Bookings/Index.cshtml
- [ ] **View name & route documented**: GET /Bookings, GET /Bookings/Index
- [ ] **Purpose statement**: List and manage bookings.
- [ ] **Owner & dependencies**: BookingsController
- [ ] **User roles/permissions required**: Roles: Admin, Photographer, Client (some actions allow anonymous; verify per action)
- [ ] **Analytics event name**: `view_bookings_index`

### View: Bookings/MyBookings.cshtml
- [ ] **View name & route documented**: GET /Bookings/MyBookings
- [ ] **Purpose statement**: TODO
- [ ] **Owner & dependencies**: BookingsController
- [ ] **User roles/permissions required**: Roles: Admin, Photographer, Client (some actions allow anonymous; verify per action)
- [ ] **Analytics event name**: `view_bookings_mybookings`

### View: Clients/Create.cshtml
- [ ] **View name & route documented**: GET /Clients/Create
- [ ] **Purpose statement**: Create client.
- [ ] **Owner & dependencies**: ClientsController
- [ ] **User roles/permissions required**: Roles: Admin, Photographer, Client (verify per action)
- [ ] **Analytics event name**: `view_clients_create`

### View: Clients/Details.cshtml
- [ ] **View name & route documented**: GET /Clients/Details
- [ ] **Purpose statement**: View client details.
- [ ] **Owner & dependencies**: ClientsController
- [ ] **User roles/permissions required**: Roles: Admin, Photographer, Client (verify per action)
- [ ] **Analytics event name**: `view_clients_details`

### View: Clients/Edit.cshtml
- [ ] **View name & route documented**: GET /Clients/Edit
- [ ] **Purpose statement**: Edit client.
- [ ] **Owner & dependencies**: ClientsController
- [ ] **User roles/permissions required**: Roles: Admin, Photographer, Client (verify per action)
- [ ] **Analytics event name**: `view_clients_edit`

### View: Clients/Index.cshtml
- [ ] **View name & route documented**: GET /Clients, GET /Clients/Index
- [ ] **Purpose statement**: List and manage clients.
- [ ] **Owner & dependencies**: ClientsController
- [ ] **User roles/permissions required**: Roles: Admin, Photographer, Client (verify per action)
- [ ] **Analytics event name**: `view_clients_index`

### View: Clients/MyProfile.cshtml
- [ ] **View name & route documented**: GET /Clients/MyProfile
- [ ] **Purpose statement**: TODO
- [ ] **Owner & dependencies**: ClientsController
- [ ] **User roles/permissions required**: Roles: Admin, Photographer, Client (verify per action)
- [ ] **Analytics event name**: `view_clients_myprofile`

### View: Contracts/Create.cshtml
- [ ] **View name & route documented**: GET /Contracts/Create
- [ ] **Purpose statement**: Create contract.
- [ ] **Owner & dependencies**: ContractsController
- [ ] **User roles/permissions required**: Roles: Admin, Photographer (some actions allow anonymous; verify per action)
- [ ] **Analytics event name**: `view_contracts_create`

### View: Contracts/Details.cshtml
- [ ] **View name & route documented**: GET /Contracts/Details
- [ ] **Purpose statement**: View contract details.
- [ ] **Owner & dependencies**: ContractsController
- [ ] **User roles/permissions required**: Roles: Admin, Photographer (some actions allow anonymous; verify per action)
- [ ] **Analytics event name**: `view_contracts_details`

### View: Contracts/Edit.cshtml
- [ ] **View name & route documented**: GET /Contracts/Edit
- [ ] **Purpose statement**: Edit contract.
- [ ] **Owner & dependencies**: ContractsController
- [ ] **User roles/permissions required**: Roles: Admin, Photographer (some actions allow anonymous; verify per action)
- [ ] **Analytics event name**: `view_contracts_edit`

### View: Contracts/Index.cshtml
- [ ] **View name & route documented**: GET /Contracts, GET /Contracts/Index
- [ ] **Purpose statement**: List and manage contracts.
- [ ] **Owner & dependencies**: ContractsController
- [ ] **User roles/permissions required**: Roles: Admin, Photographer (some actions allow anonymous; verify per action)
- [ ] **Analytics event name**: `view_contracts_index`

### View: Contracts/Sign.cshtml
- [ ] **View name & route documented**: GET /Contracts/Sign
- [ ] **Purpose statement**: TODO
- [ ] **Owner & dependencies**: ContractsController
- [ ] **User roles/permissions required**: Roles: Admin, Photographer (some actions allow anonymous; verify per action)
- [ ] **Analytics event name**: `view_contracts_sign`

### View: ContractTemplates/Create.cshtml
- [ ] **View name & route documented**: GET /ContractTemplates/Create
- [ ] **Purpose statement**: Create contract template.
- [ ] **Owner & dependencies**: ContractTemplatesController
- [ ] **User roles/permissions required**: Roles: Admin, Photographer (verify per action)
- [ ] **Analytics event name**: `view_contracttemplates_create`

### View: ContractTemplates/Edit.cshtml
- [ ] **View name & route documented**: GET /ContractTemplates/Edit
- [ ] **Purpose statement**: Edit contract template.
- [ ] **Owner & dependencies**: ContractTemplatesController
- [ ] **User roles/permissions required**: Roles: Admin, Photographer (verify per action)
- [ ] **Analytics event name**: `view_contracttemplates_edit`

### View: ContractTemplates/Index.cshtml
- [ ] **View name & route documented**: GET /ContractTemplates, GET /ContractTemplates/Index
- [ ] **Purpose statement**: List and manage contract templates.
- [ ] **Owner & dependencies**: ContractTemplatesController
- [ ] **User roles/permissions required**: Roles: Admin, Photographer (verify per action)
- [ ] **Analytics event name**: `view_contracttemplates_index`

### View: Error/Error400.cshtml
- [ ] **View name & route documented**: GET /Error/Error400
- [ ] **Purpose statement**: Error page (Error400).
- [ ] **Owner & dependencies**: ErrorController
- [ ] **User roles/permissions required**: No [Authorize] on controller; verify global policy
- [ ] **Analytics event name**: `view_error_error400`

### View: Error/Error401.cshtml
- [ ] **View name & route documented**: GET /Error/Error401
- [ ] **Purpose statement**: Error page (Error401).
- [ ] **Owner & dependencies**: ErrorController
- [ ] **User roles/permissions required**: No [Authorize] on controller; verify global policy
- [ ] **Analytics event name**: `view_error_error401`

### View: Error/Error403.cshtml
- [ ] **View name & route documented**: GET /Error/Error403
- [ ] **Purpose statement**: Error page (Error403).
- [ ] **Owner & dependencies**: ErrorController
- [ ] **User roles/permissions required**: No [Authorize] on controller; verify global policy
- [ ] **Analytics event name**: `view_error_error403`

### View: Error/Error404.cshtml
- [ ] **View name & route documented**: GET /Error/Error404
- [ ] **Purpose statement**: Error page (Error404).
- [ ] **Owner & dependencies**: ErrorController
- [ ] **User roles/permissions required**: No [Authorize] on controller; verify global policy
- [ ] **Analytics event name**: `view_error_error404`

### View: Error/Error408.cshtml
- [ ] **View name & route documented**: GET /Error/Error408
- [ ] **Purpose statement**: Error page (Error408).
- [ ] **Owner & dependencies**: ErrorController
- [ ] **User roles/permissions required**: No [Authorize] on controller; verify global policy
- [ ] **Analytics event name**: `view_error_error408`

### View: Error/Error500.cshtml
- [ ] **View name & route documented**: GET /Error/Error500
- [ ] **Purpose statement**: Error page (Error500).
- [ ] **Owner & dependencies**: ErrorController
- [ ] **User roles/permissions required**: No [Authorize] on controller; verify global policy
- [ ] **Analytics event name**: `view_error_error500`

### View: Error/Maintenance.cshtml
- [ ] **View name & route documented**: GET /Error/Maintenance
- [ ] **Purpose statement**: Error page (Maintenance).
- [ ] **Owner & dependencies**: ErrorController
- [ ] **User roles/permissions required**: No [Authorize] on controller; verify global policy
- [ ] **Analytics event name**: `view_error_maintenance`

### View: FileManager/FileManager.cshtml
- [ ] **View name & route documented**: GET /FileManager/FileManager
- [ ] **Purpose statement**: TODO
- [ ] **Owner & dependencies**: FileManagerController
- [ ] **User roles/permissions required**: Authorized (default policy); verify per action
- [ ] **Analytics event name**: `view_filemanager_filemanager`

### View: FileManager/Index.cshtml
- [ ] **View name & route documented**: GET /FileManager, GET /FileManager/Index
- [ ] **Purpose statement**: List and manage files.
- [ ] **Owner & dependencies**: FileManagerController
- [ ] **User roles/permissions required**: Authorized (default policy); verify per action
- [ ] **Analytics event name**: `view_filemanager_index`

### View: Galleries/_CreateGalleryModal.cshtml
- [ ] **View name & route documented**: Partial or shared view (not directly routed)
- [ ] **Purpose statement**: Partial for galleries module.
- [ ] **Owner & dependencies**: Partial view in Galleries module
- [ ] **User roles/permissions required**: Inherits from parent view
- [ ] **Analytics event name**: `partial_galleries_creategallerymodal`

### View: Galleries/_EditGalleryModal.cshtml
- [ ] **View name & route documented**: Partial or shared view (not directly routed)
- [ ] **Purpose statement**: Partial for galleries module.
- [ ] **Owner & dependencies**: Partial view in Galleries module
- [ ] **User roles/permissions required**: Inherits from parent view
- [ ] **Analytics event name**: `partial_galleries_editgallerymodal`

### View: Galleries/_GalleryDetailsModal.cshtml
- [ ] **View name & route documented**: Partial or shared view (not directly routed)
- [ ] **Purpose statement**: Partial for galleries module.
- [ ] **Owner & dependencies**: Partial view in Galleries module
- [ ] **User roles/permissions required**: Inherits from parent view
- [ ] **Analytics event name**: `partial_galleries_gallerydetailsmodal`

### View: Galleries/_GallerySessionsModal.cshtml
- [ ] **View name & route documented**: Partial or shared view (not directly routed)
- [ ] **Purpose statement**: Partial for galleries module.
- [ ] **Owner & dependencies**: Partial view in Galleries module
- [ ] **User roles/permissions required**: Inherits from parent view
- [ ] **Analytics event name**: `partial_galleries_gallerysessionsmodal`

### View: Galleries/Index.cshtml
- [ ] **View name & route documented**: GET /Galleries, GET /Galleries/Index
- [ ] **Purpose statement**: List and manage galleries.
- [ ] **Owner & dependencies**: GalleriesController
- [ ] **User roles/permissions required**: Roles: Admin, Photographer (verify per action)
- [ ] **Analytics event name**: `view_galleries_index`

### View: Galleries/_ManageAccessModal.cshtml
- [ ] **View name & route documented**: Partial or shared view (not directly routed)
- [ ] **Purpose statement**: Partial for galleries module.
- [ ] **Owner & dependencies**: Partial view in Galleries module
- [ ] **User roles/permissions required**: Inherits from parent view
- [ ] **Analytics event name**: `partial_galleries_manageaccessmodal`

### View: Galleries/_RegenerateCodeModal.cshtml
- [ ] **View name & route documented**: Partial or shared view (not directly routed)
- [ ] **Purpose statement**: Partial for galleries module.
- [ ] **Owner & dependencies**: Partial view in Galleries module
- [ ] **User roles/permissions required**: Inherits from parent view
- [ ] **Analytics event name**: `partial_galleries_regeneratecodemodal`

### View: Gallery/AccessGallery.cshtml
- [ ] **View name & route documented**: GET /Gallery/AccessGallery
- [ ] **Purpose statement**: TODO
- [ ] **Owner & dependencies**: GalleryController
- [ ] **User roles/permissions required**: Roles: Admin, Photographer, Client (some actions allow anonymous; verify per action)
- [ ] **Analytics event name**: `view_gallery_accessgallery`

### View: Gallery/Index.cshtml
- [ ] **View name & route documented**: GET /Gallery, GET /Gallery/Index
- [ ] **Purpose statement**: List and manage galleries.
- [ ] **Owner & dependencies**: GalleryController
- [ ] **User roles/permissions required**: Roles: Admin, Photographer, Client (some actions allow anonymous; verify per action)
- [ ] **Analytics event name**: `view_gallery_index`

### View: Gallery/NoAccess.cshtml
- [ ] **View name & route documented**: GET /Gallery/NoAccess
- [ ] **Purpose statement**: TODO
- [ ] **Owner & dependencies**: GalleryController
- [ ] **User roles/permissions required**: Roles: Admin, Photographer, Client (some actions allow anonymous; verify per action)
- [ ] **Analytics event name**: `view_gallery_noaccess`

### View: Gallery/ViewGallery.cshtml
- [ ] **View name & route documented**: GET /Gallery/ViewGallery
- [ ] **Purpose statement**: Client gallery view and proofing.
- [ ] **Owner & dependencies**: GalleryController
- [ ] **User roles/permissions required**: Roles: Admin, Photographer, Client (some actions allow anonymous; verify per action)
- [ ] **Analytics event name**: `view_gallery_viewgallery`

### View: Home/Dashboard.cshtml
- [ ] **View name & route documented**: GET /Home/Dashboard
- [ ] **Purpose statement**: Staff KPI dashboard.
- [ ] **Owner & dependencies**: HomeController
- [ ] **User roles/permissions required**: Roles: Admin, Photographer (verify per action)
- [ ] **Analytics event name**: `view_home_dashboard`

### View: Home/Default.cshtml
- [ ] **View name & route documented**: GET /Home/Default
- [ ] **Purpose statement**: TODO
- [ ] **Owner & dependencies**: HomeController
- [ ] **User roles/permissions required**: Roles: Admin, Photographer (verify per action)
- [ ] **Analytics event name**: `view_home_default`

### View: Home/Index.cshtml
- [ ] **View name & route documented**: GET /, GET /Home/Index
- [ ] **Purpose statement**: List and manage dashboard.
- [ ] **Owner & dependencies**: HomeController
- [ ] **User roles/permissions required**: Roles: Admin, Photographer (verify per action)
- [ ] **Analytics event name**: `view_home_index`

### View: Home/Landing.cshtml
- [ ] **View name & route documented**: GET /Home/Landing
- [ ] **Purpose statement**: TODO
- [ ] **Owner & dependencies**: HomeController
- [ ] **User roles/permissions required**: Roles: Admin, Photographer (verify per action)
- [ ] **Analytics event name**: `view_home_landing`

### View: Invoices/Create.cshtml
- [ ] **View name & route documented**: GET /Invoices/Create
- [ ] **Purpose statement**: Create invoice.
- [ ] **Owner & dependencies**: InvoicesController
- [ ] **User roles/permissions required**: Roles: Admin, Photographer (verify per action)
- [ ] **Analytics event name**: `view_invoices_create`

### View: Invoices/Details.cshtml
- [ ] **View name & route documented**: GET /Invoices/Details
- [ ] **Purpose statement**: View invoice details.
- [ ] **Owner & dependencies**: InvoicesController
- [ ] **User roles/permissions required**: Roles: Admin, Photographer (verify per action)
- [ ] **Analytics event name**: `view_invoices_details`

### View: Invoices/Index.cshtml
- [ ] **View name & route documented**: GET /Invoices, GET /Invoices/Index
- [ ] **Purpose statement**: List and manage invoices.
- [ ] **Owner & dependencies**: InvoicesController
- [ ] **User roles/permissions required**: Roles: Admin, Photographer (verify per action)
- [ ] **Analytics event name**: `view_invoices_index`

### View: Invoices/IndexNew.cshtml
- [ ] **View name & route documented**: GET /Invoices/IndexNew
- [ ] **Purpose statement**: TODO
- [ ] **Owner & dependencies**: InvoicesController
- [ ] **User roles/permissions required**: Roles: Admin, Photographer (verify per action)
- [ ] **Analytics event name**: `view_invoices_indexnew`

### View: Notifications/Index.cshtml
- [ ] **View name & route documented**: GET /Notifications, GET /Notifications/Index
- [ ] **Purpose statement**: List and manage notifications.
- [ ] **Owner & dependencies**: NotificationsController
- [ ] **User roles/permissions required**: Authorized (default policy); verify per action
- [ ] **Analytics event name**: `view_notifications_index`

### View: Packages/AddOns.cshtml
- [ ] **View name & route documented**: GET /Packages/AddOns
- [ ] **Purpose statement**: TODO
- [ ] **Owner & dependencies**: PackagesController
- [ ] **User roles/permissions required**: Roles: Admin (some actions allow anonymous; verify per action)
- [ ] **Analytics event name**: `view_packages_addons`

### View: Packages/CreateAddOn.cshtml
- [ ] **View name & route documented**: GET /Packages/CreateAddOn
- [ ] **Purpose statement**: TODO
- [ ] **Owner & dependencies**: PackagesController
- [ ] **User roles/permissions required**: Roles: Admin (some actions allow anonymous; verify per action)
- [ ] **Analytics event name**: `view_packages_createaddon`

### View: Packages/Create.cshtml
- [ ] **View name & route documented**: GET /Packages/Create
- [ ] **Purpose statement**: Create package.
- [ ] **Owner & dependencies**: PackagesController
- [ ] **User roles/permissions required**: Roles: Admin (some actions allow anonymous; verify per action)
- [ ] **Analytics event name**: `view_packages_create`

### View: Packages/Details.cshtml
- [ ] **View name & route documented**: GET /Packages/Details
- [ ] **Purpose statement**: View package details.
- [ ] **Owner & dependencies**: PackagesController
- [ ] **User roles/permissions required**: Roles: Admin (some actions allow anonymous; verify per action)
- [ ] **Analytics event name**: `view_packages_details`

### View: Packages/EditAddOn.cshtml
- [ ] **View name & route documented**: GET /Packages/EditAddOn
- [ ] **Purpose statement**: TODO
- [ ] **Owner & dependencies**: PackagesController
- [ ] **User roles/permissions required**: Roles: Admin (some actions allow anonymous; verify per action)
- [ ] **Analytics event name**: `view_packages_editaddon`

### View: Packages/Edit.cshtml
- [ ] **View name & route documented**: GET /Packages/Edit
- [ ] **Purpose statement**: Edit package.
- [ ] **Owner & dependencies**: PackagesController
- [ ] **User roles/permissions required**: Roles: Admin (some actions allow anonymous; verify per action)
- [ ] **Analytics event name**: `view_packages_edit`

### View: Packages/Index.cshtml
- [ ] **View name & route documented**: GET /Packages, GET /Packages/Index
- [ ] **Purpose statement**: List and manage packages.
- [ ] **Owner & dependencies**: PackagesController
- [ ] **User roles/permissions required**: Roles: Admin (some actions allow anonymous; verify per action)
- [ ] **Analytics event name**: `view_packages_index`

### View: Packages/Manage.cshtml
- [ ] **View name & route documented**: GET /Packages/Manage
- [ ] **Purpose statement**: TODO
- [ ] **Owner & dependencies**: PackagesController
- [ ] **User roles/permissions required**: Roles: Admin (some actions allow anonymous; verify per action)
- [ ] **Analytics event name**: `view_packages_manage`

### View: Permissions/_CreatePermissionModal.cshtml
- [ ] **View name & route documented**: Partial or shared view (not directly routed)
- [ ] **Purpose statement**: Partial for permissions module.
- [ ] **Owner & dependencies**: Partial view in Permissions module
- [ ] **User roles/permissions required**: Inherits from parent view
- [ ] **Analytics event name**: `partial_permissions_createpermissionmodal`

### View: Permissions/_EditPermissionModal.cshtml
- [ ] **View name & route documented**: Partial or shared view (not directly routed)
- [ ] **Purpose statement**: Partial for permissions module.
- [ ] **Owner & dependencies**: Partial view in Permissions module
- [ ] **User roles/permissions required**: Inherits from parent view
- [ ] **Analytics event name**: `partial_permissions_editpermissionmodal`

### View: Permissions/Index.cshtml
- [ ] **View name & route documented**: GET /Permissions, GET /Permissions/Index
- [ ] **Purpose statement**: List and manage permissions.
- [ ] **Owner & dependencies**: PermissionsController
- [ ] **User roles/permissions required**: Roles: Admin (verify per action)
- [ ] **Analytics event name**: `view_permissions_index`

### View: Permissions/_PermissionDetailsModal.cshtml
- [ ] **View name & route documented**: Partial or shared view (not directly routed)
- [ ] **Purpose statement**: Partial for permissions module.
- [ ] **Owner & dependencies**: Partial view in Permissions module
- [ ] **User roles/permissions required**: Inherits from parent view
- [ ] **Analytics event name**: `partial_permissions_permissiondetailsmodal`

### View: PhotoShoots/Calendar.cshtml
- [ ] **View name & route documented**: GET /PhotoShoots/Calendar
- [ ] **Purpose statement**: Calendar and scheduling for photo shoots.
- [ ] **Owner & dependencies**: PhotoShootsController
- [ ] **User roles/permissions required**: Roles: Admin, Photographer, Client (verify per action)
- [ ] **Analytics event name**: `view_photoshoots_calendar`

### View: PhotoShoots/Create.cshtml
- [ ] **View name & route documented**: GET /PhotoShoots/Create
- [ ] **Purpose statement**: Create photo shoot.
- [ ] **Owner & dependencies**: PhotoShootsController
- [ ] **User roles/permissions required**: Roles: Admin, Photographer, Client (verify per action)
- [ ] **Analytics event name**: `view_photoshoots_create`

### View: PhotoShoots/Details.cshtml
- [ ] **View name & route documented**: GET /PhotoShoots/Details
- [ ] **Purpose statement**: View photo shoot details.
- [ ] **Owner & dependencies**: PhotoShootsController
- [ ] **User roles/permissions required**: Roles: Admin, Photographer, Client (verify per action)
- [ ] **Analytics event name**: `view_photoshoots_details`

### View: PhotoShoots/Edit.cshtml
- [ ] **View name & route documented**: GET /PhotoShoots/Edit
- [ ] **Purpose statement**: Edit photo shoot.
- [ ] **Owner & dependencies**: PhotoShootsController
- [ ] **User roles/permissions required**: Roles: Admin, Photographer, Client (verify per action)
- [ ] **Analytics event name**: `view_photoshoots_edit`

### View: PhotoShoots/Index.cshtml
- [ ] **View name & route documented**: GET /PhotoShoots, GET /PhotoShoots/Index
- [ ] **Purpose statement**: List and manage photo shoots.
- [ ] **Owner & dependencies**: PhotoShootsController
- [ ] **User roles/permissions required**: Roles: Admin, Photographer, Client (verify per action)
- [ ] **Analytics event name**: `view_photoshoots_index`

### View: Photos/Index.cshtml
- [ ] **View name & route documented**: GET /Photos, GET /Photos/Index
- [ ] **Purpose statement**: List and manage photos.
- [ ] **Owner & dependencies**: PhotosController
- [ ] **User roles/permissions required**: Roles: Admin, Photographer (some actions allow anonymous; verify per action)
- [ ] **Analytics event name**: `view_photos_index`

### View: Photos/Photo.cshtml
- [ ] **View name & route documented**: GET /Photos/Photo
- [ ] **Purpose statement**: TODO
- [ ] **Owner & dependencies**: PhotosController
- [ ] **User roles/permissions required**: Roles: Admin, Photographer (some actions allow anonymous; verify per action)
- [ ] **Analytics event name**: `view_photos_photo`

### View: Photos/_PhotoGallery.cshtml
- [ ] **View name & route documented**: Partial or shared view (not directly routed)
- [ ] **Purpose statement**: Partial for photos module.
- [ ] **Owner & dependencies**: Partial view in Photos module
- [ ] **User roles/permissions required**: Inherits from parent view
- [ ] **Analytics event name**: `partial_photos_photogallery`

### View: Photos/Upload.cshtml
- [ ] **View name & route documented**: GET /Photos/Upload
- [ ] **Purpose statement**: TODO
- [ ] **Owner & dependencies**: PhotosController
- [ ] **User roles/permissions required**: Roles: Admin, Photographer (some actions allow anonymous; verify per action)
- [ ] **Analytics event name**: `view_photos_upload`

### View: PrintOrder/CartPreview.cshtml
- [ ] **View name & route documented**: GET /PrintOrder/CartPreview
- [ ] **Purpose statement**: TODO
- [ ] **Owner & dependencies**: PrintOrderController
- [ ] **User roles/permissions required**: Roles: Admin, Photographer, Client (some actions allow anonymous; verify per action)
- [ ] **Analytics event name**: `view_printorder_cartpreview`

### View: PrintOrder/Details.cshtml
- [ ] **View name & route documented**: GET /PrintOrder/Details
- [ ] **Purpose statement**: View print order details.
- [ ] **Owner & dependencies**: PrintOrderController
- [ ] **User roles/permissions required**: Roles: Admin, Photographer, Client (some actions allow anonymous; verify per action)
- [ ] **Analytics event name**: `view_printorder_details`

### View: PrintOrder/Index.cshtml
- [ ] **View name & route documented**: GET /PrintOrder, GET /PrintOrder/Index
- [ ] **Purpose statement**: List and manage print orders.
- [ ] **Owner & dependencies**: PrintOrderController
- [ ] **User roles/permissions required**: Roles: Admin, Photographer, Client (some actions allow anonymous; verify per action)
- [ ] **Analytics event name**: `view_printorder_index`

### View: PrintOrder/MyOrders.cshtml
- [ ] **View name & route documented**: GET /PrintOrder/MyOrders
- [ ] **Purpose statement**: TODO
- [ ] **Owner & dependencies**: PrintOrderController
- [ ] **User roles/permissions required**: Roles: Admin, Photographer, Client (some actions allow anonymous; verify per action)
- [ ] **Analytics event name**: `view_printorder_myorders`

### View: PrintOrder/OrderConfirmation.cshtml
- [ ] **View name & route documented**: GET /PrintOrder/OrderConfirmation
- [ ] **Purpose statement**: TODO
- [ ] **Owner & dependencies**: PrintOrderController
- [ ] **User roles/permissions required**: Roles: Admin, Photographer, Client (some actions allow anonymous; verify per action)
- [ ] **Analytics event name**: `view_printorder_orderconfirmation`

### View: PrintOrder/Pricing.cshtml
- [ ] **View name & route documented**: GET /PrintOrder/Pricing
- [ ] **Purpose statement**: TODO
- [ ] **Owner & dependencies**: PrintOrderController
- [ ] **User roles/permissions required**: Roles: Admin, Photographer, Client (some actions allow anonymous; verify per action)
- [ ] **Analytics event name**: `view_printorder_pricing`

### View: Proofs/Analytics.cshtml
- [ ] **View name & route documented**: GET /Proofs/Analytics
- [ ] **Purpose statement**: TODO
- [ ] **Owner & dependencies**: ProofsController
- [ ] **User roles/permissions required**: Roles: Admin, Photographer (verify per action)
- [ ] **Analytics event name**: `view_proofs_analytics`

### View: Proofs/Index.cshtml
- [ ] **View name & route documented**: GET /Proofs, GET /Proofs/Index
- [ ] **Purpose statement**: List and manage proofs.
- [ ] **Owner & dependencies**: ProofsController
- [ ] **User roles/permissions required**: Roles: Admin, Photographer (verify per action)
- [ ] **Analytics event name**: `view_proofs_index`

### View: Proofs/_ProofDetailsModal.cshtml
- [ ] **View name & route documented**: Partial or shared view (not directly routed)
- [ ] **Purpose statement**: Partial for proofs module.
- [ ] **Owner & dependencies**: Partial view in Proofs module
- [ ] **User roles/permissions required**: Inherits from parent view
- [ ] **Analytics event name**: `partial_proofs_proofdetailsmodal`

### View: QuestionnaireAssignments/Create.cshtml
- [ ] **View name & route documented**: GET /QuestionnaireAssignments/Create
- [ ] **Purpose statement**: Create questionnaire assignment.
- [ ] **Owner & dependencies**: QuestionnaireAssignmentsController
- [ ] **User roles/permissions required**: Roles: Admin, Photographer (verify per action)
- [ ] **Analytics event name**: `view_questionnaireassignments_create`

### View: QuestionnaireAssignments/Index.cshtml
- [ ] **View name & route documented**: GET /QuestionnaireAssignments, GET /QuestionnaireAssignments/Index
- [ ] **Purpose statement**: List and manage questionnaire assignments.
- [ ] **Owner & dependencies**: QuestionnaireAssignmentsController
- [ ] **User roles/permissions required**: Roles: Admin, Photographer (verify per action)
- [ ] **Analytics event name**: `view_questionnaireassignments_index`

### View: QuestionnaireTemplates/Create.cshtml
- [ ] **View name & route documented**: GET /QuestionnaireTemplates/Create
- [ ] **Purpose statement**: Create questionnaire template.
- [ ] **Owner & dependencies**: QuestionnaireTemplatesController
- [ ] **User roles/permissions required**: Roles: Admin, Photographer (verify per action)
- [ ] **Analytics event name**: `view_questionnairetemplates_create`

### View: QuestionnaireTemplates/Edit.cshtml
- [ ] **View name & route documented**: GET /QuestionnaireTemplates/Edit
- [ ] **Purpose statement**: Edit questionnaire template.
- [ ] **Owner & dependencies**: QuestionnaireTemplatesController
- [ ] **User roles/permissions required**: Roles: Admin, Photographer (verify per action)
- [ ] **Analytics event name**: `view_questionnairetemplates_edit`

### View: QuestionnaireTemplates/Index.cshtml
- [ ] **View name & route documented**: GET /QuestionnaireTemplates, GET /QuestionnaireTemplates/Index
- [ ] **Purpose statement**: List and manage questionnaire templates.
- [ ] **Owner & dependencies**: QuestionnaireTemplatesController
- [ ] **User roles/permissions required**: Roles: Admin, Photographer (verify per action)
- [ ] **Analytics event name**: `view_questionnairetemplates_index`

### View: Roles/_CreateRoleModal.cshtml
- [ ] **View name & route documented**: Partial or shared view (not directly routed)
- [ ] **Purpose statement**: Partial for roles module.
- [ ] **Owner & dependencies**: Partial view in Roles module
- [ ] **User roles/permissions required**: Inherits from parent view
- [ ] **Analytics event name**: `partial_roles_createrolemodal`

### View: Roles/_DeleteModal.cshtml
- [ ] **View name & route documented**: Partial or shared view (not directly routed)
- [ ] **Purpose statement**: Partial for roles module.
- [ ] **Owner & dependencies**: Partial view in Roles module
- [ ] **User roles/permissions required**: Inherits from parent view
- [ ] **Analytics event name**: `partial_roles_deletemodal`

### View: Roles/_EditRoleModal.cshtml
- [ ] **View name & route documented**: Partial or shared view (not directly routed)
- [ ] **Purpose statement**: Partial for roles module.
- [ ] **Owner & dependencies**: Partial view in Roles module
- [ ] **User roles/permissions required**: Inherits from parent view
- [ ] **Analytics event name**: `partial_roles_editrolemodal`

### View: Roles/Index.cshtml
- [ ] **View name & route documented**: GET /Roles, GET /Roles/Index
- [ ] **Purpose statement**: List and manage roles.
- [ ] **Owner & dependencies**: RolesController
- [ ] **User roles/permissions required**: Roles: Admin (verify per action)
- [ ] **Analytics event name**: `view_roles_index`

### View: Roles/_RoleCard.cshtml
- [ ] **View name & route documented**: Partial or shared view (not directly routed)
- [ ] **Purpose statement**: Partial for roles module.
- [ ] **Owner & dependencies**: Partial view in Roles module
- [ ] **User roles/permissions required**: Inherits from parent view
- [ ] **Analytics event name**: `partial_roles_rolecard`

### View: Roles/_RoleDetailsModal.cshtml
- [ ] **View name & route documented**: Partial or shared view (not directly routed)
- [ ] **Purpose statement**: Partial for roles module.
- [ ] **Owner & dependencies**: Partial view in Roles module
- [ ] **User roles/permissions required**: Inherits from parent view
- [ ] **Analytics event name**: `partial_roles_roledetailsmodal`

### View: Shared/_BaseLayout.cshtml
- [ ] **View name & route documented**: Partial or shared view (not directly routed)
- [ ] **Purpose statement**: Shared layout/partial (navigation, scripts, common UI).
- [ ] **Owner & dependencies**: Shared layout/partial
- [ ] **User roles/permissions required**: Inherits from parent view
- [ ] **Analytics event name**: `partial_shared_baselayout`

### View: Shared/_ClientBadges.cshtml
- [ ] **View name & route documented**: Partial or shared view (not directly routed)
- [ ] **Purpose statement**: Shared layout/partial (navigation, scripts, common UI).
- [ ] **Owner & dependencies**: Shared layout/partial
- [ ] **User roles/permissions required**: Inherits from parent view
- [ ] **Analytics event name**: `partial_shared_clientbadges`

### View: Shared/_Layout.cshtml
- [ ] **View name & route documented**: Partial or shared view (not directly routed)
- [ ] **Purpose statement**: Shared layout/partial (navigation, scripts, common UI).
- [ ] **Owner & dependencies**: Shared layout/partial
- [ ] **User roles/permissions required**: Inherits from parent view
- [ ] **Analytics event name**: `partial_shared_layout`

### View: Shared/_LoginPartial.cshtml
- [ ] **View name & route documented**: Partial or shared view (not directly routed)
- [ ] **Purpose statement**: Shared layout/partial (navigation, scripts, common UI).
- [ ] **Owner & dependencies**: Shared layout/partial
- [ ] **User roles/permissions required**: Inherits from parent view
- [ ] **Analytics event name**: `partial_shared_loginpartial`

### View: Shared/Partials/_ActionButtons.cshtml
- [ ] **View name & route documented**: Partial or shared view (not directly routed)
- [ ] **Purpose statement**: Shared layout/partial (navigation, scripts, common UI).
- [ ] **Owner & dependencies**: Shared layout/partial
- [ ] **User roles/permissions required**: Inherits from parent view
- [ ] **Analytics event name**: `partial_shared_actionbuttons`

### View: Shared/Partials/_Button.cshtml
- [ ] **View name & route documented**: Partial or shared view (not directly routed)
- [ ] **Purpose statement**: Shared layout/partial (navigation, scripts, common UI).
- [ ] **Owner & dependencies**: Shared layout/partial
- [ ] **User roles/permissions required**: Inherits from parent view
- [ ] **Analytics event name**: `partial_shared_button`

### View: Shared/Partials/_ConfirmModal.cshtml
- [ ] **View name & route documented**: Partial or shared view (not directly routed)
- [ ] **Purpose statement**: Shared layout/partial (navigation, scripts, common UI).
- [ ] **Owner & dependencies**: Shared layout/partial
- [ ] **User roles/permissions required**: Inherits from parent view
- [ ] **Analytics event name**: `partial_shared_confirmmodal`

### View: Shared/Partials/_CreateRoleModal.cshtml
- [ ] **View name & route documented**: Partial or shared view (not directly routed)
- [ ] **Purpose statement**: Shared layout/partial (navigation, scripts, common UI).
- [ ] **Owner & dependencies**: Shared layout/partial
- [ ] **User roles/permissions required**: Inherits from parent view
- [ ] **Analytics event name**: `partial_shared_createrolemodal`

### View: Shared/Partials/_EditRoleModal.cshtml
- [ ] **View name & route documented**: Partial or shared view (not directly routed)
- [ ] **Purpose statement**: Shared layout/partial (navigation, scripts, common UI).
- [ ] **Owner & dependencies**: Shared layout/partial
- [ ] **User roles/permissions required**: Inherits from parent view
- [ ] **Analytics event name**: `partial_shared_editrolemodal`

### View: Shared/Partials/_Flash.cshtml
- [ ] **View name & route documented**: Partial or shared view (not directly routed)
- [ ] **Purpose statement**: Shared layout/partial (navigation, scripts, common UI).
- [ ] **Owner & dependencies**: Shared layout/partial
- [ ] **User roles/permissions required**: Inherits from parent view
- [ ] **Analytics event name**: `partial_shared_flash`

### View: Shared/Partials/_Footer.cshtml
- [ ] **View name & route documented**: Partial or shared view (not directly routed)
- [ ] **Purpose statement**: Shared layout/partial (navigation, scripts, common UI).
- [ ] **Owner & dependencies**: Shared layout/partial
- [ ] **User roles/permissions required**: Inherits from parent view
- [ ] **Analytics event name**: `partial_shared_footer`

### View: Shared/Partials/_FooterScripts.cshtml
- [ ] **View name & route documented**: Partial or shared view (not directly routed)
- [ ] **Purpose statement**: Shared layout/partial (navigation, scripts, common UI).
- [ ] **Owner & dependencies**: Shared layout/partial
- [ ] **User roles/permissions required**: Inherits from parent view
- [ ] **Analytics event name**: `partial_shared_footerscripts`

### View: Shared/Partials/_GlobalSearch.cshtml
- [ ] **View name & route documented**: Partial or shared view (not directly routed)
- [ ] **Purpose statement**: Shared layout/partial (navigation, scripts, common UI).
- [ ] **Owner & dependencies**: Shared layout/partial
- [ ] **User roles/permissions required**: Inherits from parent view
- [ ] **Analytics event name**: `partial_shared_globalsearch`

### View: Shared/Partials/_HeadCSS.cshtml
- [ ] **View name & route documented**: Partial or shared view (not directly routed)
- [ ] **Purpose statement**: Shared layout/partial (navigation, scripts, common UI).
- [ ] **Owner & dependencies**: Shared layout/partial
- [ ] **User roles/permissions required**: Inherits from parent view
- [ ] **Analytics event name**: `partial_shared_headcss`

### View: Shared/Partials/_HorizontalNav.cshtml
- [ ] **View name & route documented**: Partial or shared view (not directly routed)
- [ ] **Purpose statement**: Shared layout/partial (navigation, scripts, common UI).
- [ ] **Owner & dependencies**: Shared layout/partial
- [ ] **User roles/permissions required**: Inherits from parent view
- [ ] **Analytics event name**: `partial_shared_horizontalnav`

### View: Shared/Partials/_IndexTableShell.cshtml
- [ ] **View name & route documented**: Partial or shared view (not directly routed)
- [ ] **Purpose statement**: Shared layout/partial (navigation, scripts, common UI).
- [ ] **Owner & dependencies**: Shared layout/partial
- [ ] **User roles/permissions required**: Inherits from parent view
- [ ] **Analytics event name**: `partial_shared_indextableshell`

### View: Shared/Partials/_PageTitle.cshtml
- [ ] **View name & route documented**: Partial or shared view (not directly routed)
- [ ] **Purpose statement**: Shared layout/partial (navigation, scripts, common UI).
- [ ] **Owner & dependencies**: Shared layout/partial
- [ ] **User roles/permissions required**: Inherits from parent view
- [ ] **Analytics event name**: `partial_shared_pagetitle`

### View: Shared/Partials/_SideNav.cshtml
- [ ] **View name & route documented**: Partial or shared view (not directly routed)
- [ ] **Purpose statement**: Shared layout/partial (navigation, scripts, common UI).
- [ ] **Owner & dependencies**: Shared layout/partial
- [ ] **User roles/permissions required**: Inherits from parent view
- [ ] **Analytics event name**: `partial_shared_sidenav`

### View: Shared/Partials/_StatusBadge.cshtml
- [ ] **View name & route documented**: Partial or shared view (not directly routed)
- [ ] **Purpose statement**: Shared layout/partial (navigation, scripts, common UI).
- [ ] **Owner & dependencies**: Shared layout/partial
- [ ] **User roles/permissions required**: Inherits from parent view
- [ ] **Analytics event name**: `partial_shared_statusbadge`

### View: Shared/Partials/_TitleMeta.cshtml
- [ ] **View name & route documented**: Partial or shared view (not directly routed)
- [ ] **Purpose statement**: Shared layout/partial (navigation, scripts, common UI).
- [ ] **Owner & dependencies**: Shared layout/partial
- [ ] **User roles/permissions required**: Inherits from parent view
- [ ] **Analytics event name**: `partial_shared_titlemeta`

### View: Shared/Partials/_Toasts.cshtml
- [ ] **View name & route documented**: Partial or shared view (not directly routed)
- [ ] **Purpose statement**: Shared layout/partial (navigation, scripts, common UI).
- [ ] **Owner & dependencies**: Shared layout/partial
- [ ] **User roles/permissions required**: Inherits from parent view
- [ ] **Analytics event name**: `partial_shared_toasts`

### View: Shared/Partials/_TopBar.cshtml
- [ ] **View name & route documented**: Partial or shared view (not directly routed)
- [ ] **Purpose statement**: Shared layout/partial (navigation, scripts, common UI).
- [ ] **Owner & dependencies**: Shared layout/partial
- [ ] **User roles/permissions required**: Inherits from parent view
- [ ] **Analytics event name**: `partial_shared_topbar`

### View: Shared/_ValidationScriptsPartial.cshtml
- [ ] **View name & route documented**: Partial or shared view (not directly routed)
- [ ] **Purpose statement**: Shared layout/partial (navigation, scripts, common UI).
- [ ] **Owner & dependencies**: Shared layout/partial
- [ ] **User roles/permissions required**: Inherits from parent view
- [ ] **Analytics event name**: `partial_shared_validationscriptspartial`

### View: Shared/_VerticalLayout.cshtml
- [ ] **View name & route documented**: Partial or shared view (not directly routed)
- [ ] **Purpose statement**: Shared layout/partial (navigation, scripts, common UI).
- [ ] **Owner & dependencies**: Shared layout/partial
- [ ] **User roles/permissions required**: Inherits from parent view
- [ ] **Analytics event name**: `partial_shared_verticallayout`

### View: Users/_ChangePasswordModal.cshtml
- [ ] **View name & route documented**: Partial or shared view (not directly routed)
- [ ] **Purpose statement**: Partial for users module.
- [ ] **Owner & dependencies**: Partial view in Users module
- [ ] **User roles/permissions required**: Inherits from parent view
- [ ] **Analytics event name**: `partial_users_changepasswordmodal`

### View: Users/_CreateUserModal.cshtml
- [ ] **View name & route documented**: Partial or shared view (not directly routed)
- [ ] **Purpose statement**: Partial for users module.
- [ ] **Owner & dependencies**: Partial view in Users module
- [ ] **User roles/permissions required**: Inherits from parent view
- [ ] **Analytics event name**: `partial_users_createusermodal`

### View: Users/Details.cshtml
- [ ] **View name & route documented**: GET /Users/Details
- [ ] **Purpose statement**: View user details.
- [ ] **Owner & dependencies**: UsersController
- [ ] **User roles/permissions required**: Roles: Admin (verify per action)
- [ ] **Analytics event name**: `view_users_details`

### View: Users/_EditUserModal.cshtml
- [ ] **View name & route documented**: Partial or shared view (not directly routed)
- [ ] **Purpose statement**: Partial for users module.
- [ ] **Owner & dependencies**: Partial view in Users module
- [ ] **User roles/permissions required**: Inherits from parent view
- [ ] **Analytics event name**: `partial_users_editusermodal`

### View: Users/Index.cshtml
- [ ] **View name & route documented**: GET /Users, GET /Users/Index
- [ ] **Purpose statement**: List and manage users.
- [ ] **Owner & dependencies**: UsersController
- [ ] **User roles/permissions required**: Roles: Admin (verify per action)
- [ ] **Analytics event name**: `view_users_index`

### View: Users/Roles.cshtml
- [ ] **View name & route documented**: GET /Users/Roles
- [ ] **Purpose statement**: TODO
- [ ] **Owner & dependencies**: UsersController
- [ ] **User roles/permissions required**: Roles: Admin (verify per action)
- [ ] **Analytics event name**: `view_users_roles`
- **Total view files**: 138 `.cshtml` under `/Views`.
- **Controllers**: 26 under `/Controllers` (Albums, Badges, Bookings, Clients, Contracts, ContractTemplates, Debug, Error, FileManager, Galleries, Gallery, Home, Invoices, Notifications, Packages, Permissions, Photos, PhotoShoots, PrintOrder, Proofing, Proofs, QuestionnaireAssignments, QuestionnaireTemplates, Roles, Search, Users).
- **Views by folder**:
  - `Account`: 9
  - `Albums`: 5
  - `Badges`: 3
  - `Bookings`: 6
  - `Clients`: 5
  - `Contracts`: 5
  - `ContractTemplates`: 3
  - `Error`: 7
  - `FileManager`: 2
  - `Galleries`: 7
  - `Gallery`: 4
  - `Home`: 4
  - `Invoices`: 4
  - `Notifications`: 1
  - `Packages`: 8
  - `Permissions`: 4
  - `Photos`: 4
  - `PhotoShoots`: 5
  - `PrintOrder`: 6
  - `Proofs`: 3
  - `QuestionnaireAssignments`: 2
  - `QuestionnaireTemplates`: 3
  - `Roles`: 6
  - `Shared`: 24
  - `Users`: 6
  - `_ViewImports.cshtml`: 1
  - `_ViewStart.cshtml`: 1

## 2) Data & API Integration
- [ ] **Data contract validated** (DTOs/Models align with API response).
- [ ] **Loading states** (skeleton/spinner) displayed for initial fetch.
- [ ] **Empty states** for zero results (clear CTAs to resolve).
- [ ] **Error states** for 4xx/5xx with recovery guidance.
- [ ] **Retries/backoff** for transient failures where appropriate.
- [ ] **Timeout handling** (user message and safe fallback).
- [ ] **Caching strategy** (in-memory/HTTP caching) confirmed, if applicable.
- [ ] **Pagination/infinite scroll** handles edge cases (last page, duplicates).
- [ ] **Sorting/filtering** consistent with backend logic.
- [ ] **Data formatting** (dates, currency, time zones) aligns with locale.
- [ ] **Sensitive data masking** (PII, financial info) obeys policy.

### View: Home/Dashboard (Staff)
- [x] **Data contract validated**: server-rendered `DashboardViewModel` from `IDashboardService`.
- [ ] **Loading states**: none visible (server render only); add skeleton for slow responses if needed.
- [ ] **Empty states**: some sections handle empty (e.g., today’s schedule); verify all widgets.
- [ ] **Error states**: no explicit error UI in view; relies on server render.
- [ ] **Retries/backoff**: N/A (server-side).
- [ ] **Timeout handling**: not implemented for dashboard load.
- [x] **Caching strategy**: `IMemoryCache` in `DashboardService` (`DashboardCacheKey`).
- [ ] **Pagination/infinite scroll**: N/A (fixed-size summary widgets).
- [x] **Sorting/filtering**: handled in service queries for recent lists.
- [x] **Data formatting**: dates and currency formatted in view (`ToString("C0")`, `MMM dd, yyyy`, etc.).
- [ ] **Sensitive data masking**: verify KPI exposure aligns with policy (revenue/invoice totals).

### Application-wide (All Views)
- [ ] **Data contract validated**: audit each view/action that binds DTOs to JS or view models to ensure field names and types match API responses.
- [ ] **Loading states**: standardize spinners/skeletons for all async fetches (modals, tables, galleries, file manager).
- [ ] **Empty states**: ensure clear CTAs on lists with zero items (galleries, contracts, roles, permissions, files).
- [ ] **Error states**: add consistent 4xx/5xx handling in fetch flows (toast + inline guidance).
- [ ] **Retries/backoff**: identify transient endpoints (notifications, file metadata) and add retry policy where safe.
- [ ] **Timeout handling**: add fetch timeouts with user messaging for long-running requests.
- [ ] **Caching strategy**: confirm cache headers for APIs that can be cached (search, notifications, galleries).
- [ ] **Pagination/infinite scroll**: verify edge cases for gallery paging, file manager lists, search results.
- [ ] **Sorting/filtering**: keep client-side filters in sync with server-side sort order.
- [ ] **Data formatting**: confirm locale/timezone correctness for photo shoots, invoices, contract dates.
- [ ] **Sensitive data masking**: verify PII exposure in client lists, search, and roles/permissions views.

#### Static scan (AJAX/fetch)
- **Views with fetch/ajax**: 18
- **Files**:
  - `Views/Badges/Index.cshtml`
  - `Views/Contracts/Create.cshtml`
  - `Views/Contracts/Index.cshtml`
  - `Views/ContractTemplates/Index.cshtml`
  - `Views/FileManager/Index.cshtml`
  - `Views/Galleries/_ManageAccessModal.cshtml`
  - `Views/Galleries/_RegenerateCodeModal.cshtml`
  - `Views/Gallery/ViewGallery.cshtml`
  - `Views/Notifications/Index.cshtml`
  - `Views/Permissions/_CreatePermissionModal.cshtml`
  - `Views/Permissions/_EditPermissionModal.cshtml`
  - `Views/Permissions/Index.cshtml`
  - `Views/PhotoShoots/Calendar.cshtml`
  - `Views/PhotoShoots/Index.cshtml`
  - `Views/Roles/_DeleteModal.cshtml`
  - `Views/Roles/Index.cshtml`
  - `Views/Shared/Partials/_GlobalSearch.cshtml`
  - `Views/Users/Roles.cshtml`
- **JS files with fetch/ajax**: `wwwroot/js/notifications.js`
- **Endpoints referenced in views** (some URLs are built server-side at runtime):
  - `/api/clients`
  - `/api/files/{id}`
  - `/api/files/{fileId}/favorite`
  - `/api/files/{fileId}/metadata`
  - `/api/gallery/{galleryId}/photos?page={n}&pageSize={n}`
  - `/api/notifications/{id}/read`
  - `/api/notifications/create-test`
  - `/api/proofing/favorites/{sessionToken}`
  - `/api/proofing/mark-favorite?photoId={id}&sessionToken={token}&isFavorite={bool}`
  - `/api/proofing/mark-for-editing?photoId={id}&sessionToken={token}`
  - `/api/proofing/summary/{sessionToken}`
  - `/api/search?q={query}&limit=15`
  - `/api/photoshoots/{id}`
  - `/Contracts/GetTemplate/{templateId}`
  - `/ContractTemplates/Delete/{id}`
  - `/Galleries/GrantAccess`
  - `/Galleries/RevokeAccess`
  - `/Galleries/RegenerateCode/{galleryId}`
  - `/Gallery/DownloadBulk?galleryId={id}`
  - `/PhotoShoots/GetClients`
  - `/PhotoShoots/GetPhotographers`
  - `/PhotoShoots/CreateAjax`
  - `/PhotoShoots/UpdateAjax`
  - `/PhotoShoots/DeleteAjax?id={id}`
  - `/PhotoShoots/Move`
  - `/Roles/Create`, `/Roles/Edit`, `/Roles/Details`, `/Roles/Delete`
  - `/Permissions/Create`, `/Permissions/Edit`, `/Permissions/Details`, `/Permissions/Delete`

---

## 3) Forms & Validation
- [ ] **All required fields enforced** client-side + server-side.
- [ ] **Field-level validation messages** are clear and actionable.
- [ ] **Inline help text** for complex fields.
- [ ] **Input constraints** (length, file types, ranges) validated.
- [ ] **Autosave/confirm dialogs** for destructive actions.
- [ ] **Keyboard navigation** complete and logical.
- [ ] **Focus management** after submit/error.
- [ ] **Double-submit prevention** (disabled buttons, idempotency).
- [ ] **File uploads** show progress and handle max size limits.

### View: Home/Dashboard (Staff)
- [ ] **All required fields enforced**: N/A (no forms on dashboard view).
- [ ] **Field-level validation messages**: N/A.
- [ ] **Inline help text**: N/A.
- [ ] **Input constraints**: N/A.
- [ ] **Autosave/confirm dialogs**: N/A.
- [ ] **Keyboard navigation**: verify for widget toggles and dropdown.
- [ ] **Focus management**: verify customize dropdown and collapsible widget buttons.
- [ ] **Double-submit prevention**: N/A.
- [ ] **File uploads**: N/A.

### Application-wide (All Views)
- [ ] **All required fields enforced**: Validate each form; many ViewModels use `[Required]` and controllers check `ModelState.IsValid`, but not confirmed across all views.
- [ ] **Field-level validation messages**: Ensure every form includes `asp-validation-for` or summary and `_ValidationScriptsPartial`.
- [ ] **Inline help text**: Verify complex fields across modules (packages, pricing, uploads).
- [ ] **Input constraints**: Confirm length, range, and file constraints are enforced server-side everywhere (not only UI).
- [ ] **Autosave/confirm dialogs**: Audit destructive actions (delete, revoke, regenerate) and unsaved changes prompts.
- [ ] **Keyboard navigation**: Review modal forms and custom widgets for tab order.
- [ ] **Focus management**: Confirm focus moves to errors and returns appropriately on modal close.
- [ ] **Double-submit prevention**: Ensure submit buttons disable or idempotency is handled across all forms.
- [ ] **File uploads**: Verify progress, size limits, and error handling for profile/album/file uploads.

#### Static scan (forms/validation)
- **Forms found**: 69 `.cshtml` files contain `<form>`.
- **Missing `_ValidationScriptsPartial` in the same file**: 47 (many are filters/search or simple postbacks; verify per view).
- **Missing any `asp-validation-*` tags**: 38 (audit to confirm if validation is required).
- **Next action**: review each form to classify as data entry vs. simple action/filter and ensure client+server validation where required.

---

## 4) UX & UI Consistency
- [ ] **Layout consistent** with design system (spacing, typography, colors).
- [ ] **Breadcrumbs/headers** communicate location in app.
- [ ] **CTAs** are clearly labeled with action verbs.
- [ ] **Confirmation UX** for destructive actions.
- [ ] **Feedback** on success (toast, banner, inline message).
- [ ] **Modals** trap focus and are dismissible.
- [ ] **Images/icons** have alt text and appropriate sizes.
- [ ] **Copy review** (grammar, clarity, tone).

### Application-wide (All Views)
- [ ] **Layout consistency**: align cards, headers, and tables to one system (spacing scale, typography, color tokens).
- [ ] **CTA clarity**: normalize action verbs (Create, Save, Delete, Regenerate) and avoid duplicates.
- [ ] **Success/error feedback**: ensure every create/edit/delete action shows a toast or inline status.
- [ ] **Modal UX**: confirm focus trap, escape to close, and aria labels on all modals.
- [ ] **Copy review**: run a pass for typos, capitalization, and button label consistency.

---

## 5) Accessibility (WCAG basics)
- [ ] **Semantic HTML** and correct heading hierarchy.
- [ ] **Visible focus states** for all interactive elements.
- [ ] **Color contrast** meets WCAG AA.
- [ ] **Screen reader labels** (aria-labels, sr-only text).
- [ ] **Form inputs** properly associated with labels.
- [ ] **No keyboard traps**; tab order is logical.
- [ ] **Error summaries** for forms with multiple issues.
- [ ] **Dynamic content announcements** (aria-live for toasts).

### Application-wide (All Views)
- [ ] **Alt text**: ensure all `<img>` have meaningful `alt` (or `alt=""` for decorative).
- [ ] **Focus visibility**: verify focus styles for buttons, links, selects, and custom controls.
- [ ] **Contrast**: re-check badge colors, charts, and muted text on cards.
- [ ] **Form labels**: ensure every input/select/textarea has a label or `aria-label`.
- [ ] **Keyboard only**: validate modal flows and dropdowns without mouse.
- [ ] **ARIA live regions**: add for toast notifications and async updates.

#### Static scan (accessibility)
- **`<img>` tags**: 149 lines.
- **`<img>` without `alt=` on same line**: 15 (line-based scan; verify multi-line tags).
  - `Views/Proofs/Analytics.cshtml`
  - `Views/Proofs/Index.cshtml`
  - `Views/Proofs/_ProofDetailsModal.cshtml`
  - `Views/Gallery/ViewGallery.cshtml`
  - `Views/Users/Index.cshtml`
  - `Views/Photos/_PhotoGallery.cshtml`
  - `Views/Photos/Photo.cshtml`
  - `Views/Photos/Index.cshtml`
  - `Views/PrintOrder/CartPreview.cshtml`
  - `Views/PrintOrder/Details.cshtml`
  - `Views/PrintOrder/OrderConfirmation.cshtml`
  - `Views/Galleries/_GalleryDetailsModal.cshtml`
  - `Views/Users/_EditUserModal.cshtml`
- **ARIA attributes**: 260 lines contain `aria-`.
- **`tabindex` usage**: 42 lines.

---

## 6) Security & Privacy
- [ ] **Authorization checks** verified on server for all actions.
- [ ] **CSRF protections** for write actions.
- [ ] **XSS protections** (encoding, safe rendering).
- [ ] **No sensitive data in query params** or client logs.
- [ ] **Download links** time-limited if needed.
- [ ] **Role/permission mismatch** handled with friendly error.
- [ ] **Audit logging** for critical actions.

### Application-wide (All Views)
- [ ] **Authorization**: verify every controller action has the correct role policy (especially Gallery/Proofing/Clients routes).
- [ ] **CSRF**: ensure all POST/PUT/DELETE endpoints have antiforgery protection (including AJAX endpoints).
- [ ] **XSS**: confirm HTML content is encoded and user-provided content is sanitized.
- [ ] **PII exposure**: audit client search, global search, and public gallery endpoints.
- [ ] **Downloads**: confirm gallery downloads/exports are time-limited and logged.

#### Static scan (security)
- **`[ValidateAntiForgeryToken]` occurrences**: 88 in `/Controllers`.
- **No `AutoValidateAntiforgeryToken` found** in `Program.cs` or controllers (verify global antiforgery policy).

---

## 7) Performance & Reliability
- [ ] **First load performance** acceptable (<2s on typical network).
- [ ] **Bundle size** within target; no unused assets.
- [ ] **Image optimization** (compressed, responsive sizes).
- [ ] **Lazy-loading** for below-the-fold content.
- [ ] **Avoid unnecessary reflows** (virtualize big lists).
- [ ] **Server-side response time** under SLA.
- [ ] **Graceful degradation** for partial outages.

### Application-wide (All Views)
- [ ] **Script loading**: defer non-critical scripts and reduce duplicate includes.
- [ ] **Image optimization**: use thumbnails in lists, lazy-load galleries, and provide modern formats where possible.
- [ ] **Large lists**: virtualize or paginate big tables (files, galleries, notifications).
- [ ] **API timeouts**: add consistent timeout handling for long-running fetches.

#### Static scan (performance)
- **`<script>` tags in views**: 126 lines.
- **`<script>` without `defer` or `async` on same line**: 119 (line-based scan; verify bundled scripts).
- **`loading="lazy"` usage**: 7 lines.

---

## 8) Observability & Monitoring
- [ ] **Client errors captured** (JS errors, failed requests).
- [ ] **Server errors logged** with correlation ID.
- [ ] **Performance metrics** (FCP/LCP/CLS) tracked.
- [ ] **Audit events** on critical actions.
- [ ] **Alert thresholds** configured for failure spikes.

### Application-wide (All Views)
- [ ] **Client error logging**: add Sentry/AppInsights (or equivalent) for JS errors and failed fetches.
- [ ] **Correlation IDs**: ensure server logs include request IDs and return them in error responses.
- [ ] **Audit logs**: log create/edit/delete actions for galleries, users, contracts, and payments.
- [ ] **Alerts**: define thresholds for 4xx/5xx spikes and job failures.

#### Static scan (observability)
- **No client-side error tracking library found** via string scan (Sentry/AppInsights/Datadog).

---

## 9) SEO & Sharing (public-facing views)
- [ ] **Title & meta description** unique and meaningful.
- [ ] **OpenGraph/Twitter tags** for share previews.
- [ ] **Canonical URLs** where applicable.
- [ ] **Robots directives** set properly.

### Application-wide (Public Views)
- [ ] Identify all public pages (landing, packages, gallery share) and add proper meta/OG tags.
- [ ] Ensure noindex on private/authenticated views.

---

## 10) QA & Test Coverage
- [ ] **Unit tests** for key logic.
- [ ] **Integration tests** for data flow and API handling.
- [ ] **E2E tests** for critical user paths.
- [ ] **Cross-browser testing** (Chrome/Firefox/Safari/Edge).
- [ ] **Responsive testing** (mobile/tablet/desktop).
- [ ] **Localization checks** if app supports multiple locales.

### Application-wide (All Views)
- [ ] Define a smoke test plan for staff + client journeys (login, gallery view, booking, invoice).
- [ ] Add E2E coverage for gallery proofing and checkout flows.

---

# View-Specific Checklists

Use the baseline checklist above for every view, then add the relevant items below depending on the view type. This section includes **all current view areas** in `/Views` to keep coverage complete.

## Account
- [ ] Login: rate limiting, lockout messaging, remember-me cookie behavior.
- [ ] Registration: email verification flow, duplicate email checks.
- [ ] Password reset: token expiry handling, success messaging.
- [ ] MFA/2FA: backup codes, device remember list.

## Albums
- [ ] Album creation: cover selection, privacy defaults, empty album state.
- [ ] Sorting: drag/drop persists order, manual overrides stored.
- [ ] Sharing: permissions, expiration, copy link behavior.

## Badges
- [ ] Badge issuance flow tested end-to-end.
- [ ] Badge visibility respects permissions and filters.

## Bookings
- [ ] Booking creation: conflict checks, timezone handling.
- [ ] Calendar view: drag-drop updates and persistence.
- [ ] Status transitions: pending -> confirmed -> completed.

## Clients
- [ ] Client creation: duplicate detection, contact validation.
- [ ] Client detail: all related entities visible (bookings, invoices, galleries).

## ContractTemplates
- [ ] Template editor: placeholders preview, versioning.
- [ ] Template availability: role-based access to create/edit.

## Contracts
- [ ] Contract creation: template selection, auto-fill variables.
- [ ] Signature flow: signer authentication, audit trail.

## Error
- [ ] 404 page: user escape routes, link to home.
- [ ] 500 page: support contact and error ID.

## FileManager
- [ ] Upload: size/type validation, progress, cancel.
- [ ] Folders: rename/delete with confirmation.
- [ ] Bulk actions: select all, undo, throttling.

## Galleries
- [ ] Gallery list: filters, pagination, search.
- [ ] Shared galleries: access keys and expiration.

## Gallery
- [ ] Photo grid: lazy-load images, placeholders.

## Invoices
- [ ] Invoice creation: line items validate, tax/discount calculation.
- [ ] Status flow: draft -> sent -> paid -> void.
- [ ] PDF/export: consistent formatting and permissions.

## Notifications
- [ ] Read/unread state consistent across list and badge.
- [ ] Real-time or polling updates for new notifications.

## Packages
- [ ] Package visibility: public vs. private pricing rules.
- [ ] Package assignment: only eligible clients can see selected packages.

## Permissions
- [ ] Permission changes propagate to roles/users correctly.
- [ ] Permission audit trail tracked.

## Photos
- [ ] Photo upload: size/type validation and preview.
- [ ] Photo metadata: edit/save and visibility.

## PhotoShoots
- [ ] Calendar: timezone handling and overlap checks.
- [ ] Status transitions: scheduled -> completed.

## PrintOrder
- [ ] Checkout flow: totals, taxes, and shipping verified.
- [ ] Order fulfillment: status updates and notifications.

## Proofs
- [ ] Proof selection: favorite/edit flow works with session token.
- [ ] Approval: admin notification and state updates.

## QuestionnaireAssignments
- [ ] Assignment delivery: email/link access works.
- [ ] Responses: saved and versioned.

## QuestionnaireTemplates
- [ ] Template editor: validation and preview.
- [ ] Versioning: changes do not break assigned questionnaires.

## Roles
- [ ] Role creation: name uniqueness.
- [ ] Permission inheritance tested.

## Shared
- [ ] Shared partials: no dependency on view-specific data.
- [ ] Shared layout: top-nav/side-nav consistent.

## Users
- [ ] User management: deactivate/reactivate logic.
- [ ] User profile: data updates validated.

---

# Release Sign-Off (per view)
- [ ] **Product** approves view behavior & copy.
- [ ] **Design** signs off on layout & UX polish.
- [ ] **QA** completes test pass with no blocker defects.
- [ ] **Security** checks completed if view handles PII/payment.
- [ ] **Ops** has monitoring/alerts in place.
