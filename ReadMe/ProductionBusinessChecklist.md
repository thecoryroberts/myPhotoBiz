# Production Readiness Checklist (Per-View)

Use this checklist for **every view** (page, modal, partial, or route) in the app before release. The goal is to make each view consistently functional, secure, performant, accessible, and supportable in production.

---

## 1) Basic View Inventory (per view)
- TODO [ ] **View name & route documented** (URL, controller/action, area).
- TODO [ ] **Purpose statement**: what user job-to-be-done this view supports.
- TODO [ ] **Owner & dependencies** (API endpoints, DB tables, external services, feature flags).
- TODO [ ] **User roles/permissions required** and role matrix updated.
- TODO [ ] **Analytics event name** for page view and key actions.

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
- TODO [x] **Analytics event name**: `view_home_dashboard` (page view) + `action_dashboard_widget_toggle` (key action) — names confirmed for analytics wiring.

### Application-wide (All Views)
- TODO [ ] **View name & route documented**: map all views to controller actions (include attribute-routed endpoints and Areas).
- TODO [ ] **Purpose statement**: add a 1-line purpose per view (staff vs. client vs. public).
- TODO [ ] **Owner & dependencies**: confirm controller/service ownership and DB dependencies per view.
- TODO [ ] **User roles/permissions required**: build a role matrix per view/action.
- TODO [ ] **Analytics event name**: define consistent event naming for page view + key actions per view.

#### Static scan (view inventory)

#### View inventory (alphabetical)

### View: Account/DeleteAccount.cshtml
- TODO [x] **View name & route documented**: View file `Views/Account/DeleteAccount.cshtml`; no controller action exists, so currently **unused/unrouted**. To expose, add `GET /Account/DeleteAccount` action in an Account/Identity controller returning this view. Used by: none today.
- TODO [x] **Purpose statement**: Account deactivation notice with a “Reactivate Now” CTA; informs users their account is inactive and offers reactivation path.
- TODO [x] **Owner & dependencies**: Ownership: Identity/Auth flow owner (TBD). Dependencies: shared layout `_BaseLayout`, static assets `/images/logo.png`, `/images/logo-black.png`, `/images/delete.png`; no data/API bindings.
- TODO [x] **User roles/permissions required**: Intended for deactivated authenticated users; currently anonymous access because no route/auth is wired. When wired, protect with auth + deactivated-state check.
- TODO [x] **Analytics event name**: `view_account_deleteaccount` (page view). Key action candidate: `action_account_reactivate` on the Reactivate button once it is wired.

### View: Account/LockScreen.cshtml
- TODO [ ] **View name & route documented**: GET /Account/LockScreen (Identity/Razor Pages or custom route; verify). Used by: not found
- TODO [ ] **Purpose statement**: Lock screen / re-auth.
- TODO [ ] **Owner & dependencies**: Identity/auth flow (no AccountController in /Controllers)
- TODO [ ] **User roles/permissions required**: Public/auth flow (verify)
- TODO [ ] **Analytics event name**: `view_account_lockscreen`

### View: Account/Login.cshtml
- TODO [ ] **View name & route documented**: GET /Account/Login (Identity/Razor Pages or custom route; verify). Used by: not found
- TODO [ ] **Purpose statement**: User login.
- TODO [ ] **Owner & dependencies**: Identity/auth flow (no AccountController in /Controllers)
- TODO [ ] **User roles/permissions required**: Public/auth flow (verify)
- TODO [ ] **Analytics event name**: `view_account_login`

### View: Account/LoginPin.cshtml
- TODO [ ] **View name & route documented**: GET /Account/LoginPin (Identity/Razor Pages or custom route; verify). Used by: not found
- TODO [ ] **Purpose statement**: PIN login.
- TODO [ ] **Owner & dependencies**: Identity/auth flow (no AccountController in /Controllers)
- TODO [ ] **User roles/permissions required**: Public/auth flow (verify)
- TODO [ ] **Analytics event name**: `view_account_loginpin`

### View: Account/NewPass.cshtml
- TODO [ ] **View name & route documented**: GET /Account/NewPass (Identity/Razor Pages or custom route; verify). Used by: not found
- TODO [ ] **Purpose statement**: Set new password.
- TODO [ ] **Owner & dependencies**: Identity/auth flow (no AccountController in /Controllers)
- TODO [ ] **User roles/permissions required**: Public/auth flow (verify)
- TODO [ ] **Analytics event name**: `view_account_newpass`

### View: Account/ResetPass.cshtml
- TODO [ ] **View name & route documented**: GET /Account/ResetPass (Identity/Razor Pages or custom route; verify). Used by: not found
- TODO [ ] **Purpose statement**: Password reset request.
- TODO [ ] **Owner & dependencies**: Identity/auth flow (no AccountController in /Controllers)
- TODO [ ] **User roles/permissions required**: Public/auth flow (verify)
- TODO [ ] **Analytics event name**: `view_account_resetpass`

### View: Account/SignUp.cshtml
- TODO [ ] **View name & route documented**: GET /Account/SignUp (Identity/Razor Pages or custom route; verify). Used by: not found
- TODO [ ] **Purpose statement**: User registration.
- TODO [ ] **Owner & dependencies**: Identity/auth flow (no AccountController in /Controllers)
- TODO [ ] **User roles/permissions required**: Public/auth flow (verify)
- TODO [ ] **Analytics event name**: `view_account_signup`

### View: Account/SuccessMail.cshtml
- TODO [ ] **View name & route documented**: GET /Account/SuccessMail (Identity/Razor Pages or custom route; verify). Used by: not found
- TODO [ ] **Purpose statement**: Email confirmation notice.
- TODO [ ] **Owner & dependencies**: Identity/auth flow (no AccountController in /Controllers)
- TODO [ ] **User roles/permissions required**: Public/auth flow (verify)
- TODO [ ] **Analytics event name**: `view_account_successmail`

### View: Account/TwoFactor.cshtml
- TODO [ ] **View name & route documented**: GET /Account/TwoFactor (Identity/Razor Pages or custom route; verify). Used by: not found
- TODO [ ] **Purpose statement**: Account flow view.
- TODO [ ] **Owner & dependencies**: Identity/auth flow (no AccountController in /Controllers)
- TODO [ ] **User roles/permissions required**: Public/auth flow (verify)
- TODO [ ] **Analytics event name**: `view_account_twofactor`

### View: Albums/Create.cshtml
- TODO [ ] **View name & route documented**: GET /Albums/Create (AlbumsController.Create)
- TODO [ ] **Purpose statement**: Create album.
- TODO [ ] **Owner & dependencies**: AlbumsController
- TODO [ ] **User roles/permissions required**: Roles: Admin, Photographer (also uses default authorization; verify per action)
- TODO [ ] **Analytics event name**: `view_albums_create`

### View: Albums/Delete.cshtml
- TODO [ ] **View name & route documented**: GET /Albums/Delete (AlbumsController.Delete)
- TODO [ ] **Purpose statement**: Confirm delete album.
- TODO [ ] **Owner & dependencies**: AlbumsController
- TODO [ ] **User roles/permissions required**: Roles: Admin, Photographer (also uses default authorization; verify per action)
- TODO [ ] **Analytics event name**: `view_albums_delete`

### View: Albums/Details.cshtml
- TODO [ ] **View name & route documented**: GET /Albums/Details (AlbumsController.Details)
- TODO [ ] **Purpose statement**: View album details.
- TODO [ ] **Owner & dependencies**: AlbumsController
- TODO [ ] **User roles/permissions required**: Roles: Admin, Photographer (also uses default authorization; verify per action)
- TODO [ ] **Analytics event name**: `view_albums_details`

### View: Albums/Edit.cshtml
- TODO [ ] **View name & route documented**: GET /Albums/Edit (AlbumsController.Edit)
- TODO [ ] **Purpose statement**: Edit album.
- TODO [ ] **Owner & dependencies**: AlbumsController
- TODO [ ] **User roles/permissions required**: Roles: Admin, Photographer (also uses default authorization; verify per action)
- TODO [ ] **Analytics event name**: `view_albums_edit`

### View: Albums/Index.cshtml
- TODO [ ] **View name & route documented**: GET /Albums (AlbumsController.Index) | GET /Albums/Index (AlbumsController.Index)
- TODO [ ] **Purpose statement**: List and manage albums.
- TODO [ ] **Owner & dependencies**: AlbumsController
- TODO [ ] **User roles/permissions required**: Roles: Admin, Photographer (also uses default authorization; verify per action)
- TODO [ ] **Analytics event name**: `view_albums_index`

### View: Badges/Create.cshtml
- TODO [ ] **View name & route documented**: GET /Badges/Create (BadgesController.Create)
- TODO [ ] **Purpose statement**: Create badge.
- TODO [ ] **Owner & dependencies**: BadgesController
- TODO [ ] **User roles/permissions required**: Roles: Admin (verify per action)
- TODO [ ] **Analytics event name**: `view_badges_create`

### View: Badges/Edit.cshtml
- TODO [ ] **View name & route documented**: GET /Badges/Edit (BadgesController.Edit)
- TODO [ ] **Purpose statement**: Edit badge.
- TODO [ ] **Owner & dependencies**: BadgesController
- TODO [ ] **User roles/permissions required**: Roles: Admin (verify per action)
- TODO [ ] **Analytics event name**: `view_badges_edit`

### View: Badges/Index.cshtml
- TODO [ ] **View name & route documented**: GET /Badges (BadgesController.Index) | GET /Badges/Index (BadgesController.Index)
- TODO [ ] **Purpose statement**: List and manage badges.
- TODO [ ] **Owner & dependencies**: BadgesController
- TODO [ ] **User roles/permissions required**: Roles: Admin (verify per action)
- TODO [ ] **Analytics event name**: `view_badges_index`

### View: Bookings/Availability.cshtml
- TODO [ ] **View name & route documented**: GET /Bookings/Availability (BookingsController.Availability)
- TODO [ ] **Purpose statement**: View for booking (Availability).
- TODO [ ] **Owner & dependencies**: BookingsController
- TODO [ ] **User roles/permissions required**: Roles: Admin, Photographer, Client (some actions allow anonymous; verify per action)
- TODO [ ] **Analytics event name**: `view_bookings_availability`

### View: Bookings/Book.cshtml
- TODO [ ] **View name & route documented**: GET /Bookings/Book (BookingsController.Book)
- TODO [ ] **Purpose statement**: View for booking (Book).
- TODO [ ] **Owner & dependencies**: BookingsController
- TODO [ ] **User roles/permissions required**: Roles: Admin, Photographer, Client (some actions allow anonymous; verify per action)
- TODO [ ] **Analytics event name**: `view_bookings_book`

### View: Bookings/Confirmation.cshtml
- TODO [ ] **View name & route documented**: GET /Bookings/Confirmation (BookingsController.Confirmation)
- TODO [ ] **Purpose statement**: View for booking (Confirmation).
- TODO [ ] **Owner & dependencies**: BookingsController
- TODO [ ] **User roles/permissions required**: Roles: Admin, Photographer, Client (some actions allow anonymous; verify per action)
- TODO [ ] **Analytics event name**: `view_bookings_confirmation`

### View: Bookings/Details.cshtml
- TODO [ ] **View name & route documented**: GET /Bookings/Details (BookingsController.Details)
- TODO [ ] **Purpose statement**: View booking details.
- TODO [ ] **Owner & dependencies**: BookingsController
- TODO [ ] **User roles/permissions required**: Roles: Admin, Photographer, Client (some actions allow anonymous; verify per action)
- TODO [ ] **Analytics event name**: `view_bookings_details`

### View: Bookings/Index.cshtml
- TODO [ ] **View name & route documented**: GET /Bookings (BookingsController.Index) | GET /Bookings/Index (BookingsController.Index)
- TODO [ ] **Purpose statement**: List and manage bookings.
- TODO [ ] **Owner & dependencies**: BookingsController
- TODO [ ] **User roles/permissions required**: Roles: Admin, Photographer, Client (some actions allow anonymous; verify per action)
- TODO [ ] **Analytics event name**: `view_bookings_index`

### View: Bookings/MyBookings.cshtml
- TODO [ ] **View name & route documented**: GET /Bookings/MyBookings (BookingsController.MyBookings)
- TODO [ ] **Purpose statement**: View for booking (MyBookings).
- TODO [ ] **Owner & dependencies**: BookingsController
- TODO [ ] **User roles/permissions required**: Roles: Admin, Photographer, Client (some actions allow anonymous; verify per action)
- TODO [ ] **Analytics event name**: `view_bookings_mybookings`

### View: Clients/Create.cshtml
- TODO [ ] **View name & route documented**: GET /Clients/Create (ClientsController.Create)
- TODO [ ] **Purpose statement**: Create client.
- TODO [ ] **Owner & dependencies**: ClientsController
- TODO [ ] **User roles/permissions required**: Roles: Admin, Photographer, Client (verify per action)
- TODO [ ] **Analytics event name**: `view_clients_create`

### View: Clients/Details.cshtml
- TODO [ ] **View name & route documented**: GET /Clients/Details (ClientsController.Details)
- TODO [ ] **Purpose statement**: View client details.
- TODO [ ] **Owner & dependencies**: ClientsController
- TODO [ ] **User roles/permissions required**: Roles: Admin, Photographer, Client (verify per action)
- TODO [ ] **Analytics event name**: `view_clients_details`

### View: Clients/Edit.cshtml
- TODO [ ] **View name & route documented**: GET /Clients/Edit (ClientsController.Edit)
- TODO [ ] **Purpose statement**: Edit client.
- TODO [ ] **Owner & dependencies**: ClientsController
- TODO [ ] **User roles/permissions required**: Roles: Admin, Photographer, Client (verify per action)
- TODO [ ] **Analytics event name**: `view_clients_edit`

### View: Clients/Index.cshtml
- TODO [ ] **View name & route documented**: GET /Clients (ClientsController.Index) | GET /Clients/Index (ClientsController.Index)
- TODO [ ] **Purpose statement**: List and manage clients.
- TODO [ ] **Owner & dependencies**: ClientsController
- TODO [ ] **User roles/permissions required**: Roles: Admin, Photographer, Client (verify per action)
- TODO [ ] **Analytics event name**: `view_clients_index`

### View: Clients/MyProfile.cshtml
- TODO [ ] **View name & route documented**: GET /Clients/MyProfile (ClientsController.MyProfile)
- TODO [ ] **Purpose statement**: View for client (MyProfile).
- TODO [ ] **Owner & dependencies**: ClientsController
- TODO [ ] **User roles/permissions required**: Roles: Admin, Photographer, Client (verify per action)
- TODO [ ] **Analytics event name**: `view_clients_myprofile`

### View: Contracts/Create.cshtml
- TODO [ ] **View name & route documented**: GET /Contracts/Create (ContractsController.Create)
- TODO [ ] **Purpose statement**: Create contract.
- TODO [ ] **Owner & dependencies**: ContractsController
- TODO [ ] **User roles/permissions required**: Roles: Admin, Photographer (some actions allow anonymous; verify per action)
- TODO [ ] **Analytics event name**: `view_contracts_create`

### View: Contracts/Details.cshtml
- TODO [ ] **View name & route documented**: GET /Contracts/Details (ContractsController.Details)
- TODO [ ] **Purpose statement**: View contract details.
- TODO [ ] **Owner & dependencies**: ContractsController
- TODO [ ] **User roles/permissions required**: Roles: Admin, Photographer (some actions allow anonymous; verify per action)
- TODO [ ] **Analytics event name**: `view_contracts_details`

### View: Contracts/Edit.cshtml
- TODO [ ] **View name & route documented**: GET /Contracts/Edit (ContractsController.Edit)
- TODO [ ] **Purpose statement**: Edit contract.
- TODO [ ] **Owner & dependencies**: ContractsController
- TODO [ ] **User roles/permissions required**: Roles: Admin, Photographer (some actions allow anonymous; verify per action)
- TODO [ ] **Analytics event name**: `view_contracts_edit`

### View: Contracts/Index.cshtml
- TODO [ ] **View name & route documented**: GET /Contracts (ContractsController.Index) | GET /Contracts/Index (ContractsController.Index)
- TODO [ ] **Purpose statement**: List and manage contracts.
- TODO [ ] **Owner & dependencies**: ContractsController
- TODO [ ] **User roles/permissions required**: Roles: Admin, Photographer (some actions allow anonymous; verify per action)
- TODO [ ] **Analytics event name**: `view_contracts_index`

### View: Contracts/Sign.cshtml
- TODO [ ] **View name & route documented**: GET /Contracts/Sign (ContractsController.Sign)
- TODO [ ] **Purpose statement**: View for contract (Sign).
- TODO [ ] **Owner & dependencies**: ContractsController
- TODO [ ] **User roles/permissions required**: Roles: Admin, Photographer (some actions allow anonymous; verify per action)
- TODO [ ] **Analytics event name**: `view_contracts_sign`

### View: ContractTemplates/Create.cshtml
- TODO [ ] **View name & route documented**: GET /ContractTemplates/Create (ContractTemplatesController.Create)
- TODO [ ] **Purpose statement**: Create contract template.
- TODO [ ] **Owner & dependencies**: ContractTemplatesController
- TODO [ ] **User roles/permissions required**: Roles: Admin, Photographer (verify per action)
- TODO [ ] **Analytics event name**: `view_contracttemplates_create`

### View: ContractTemplates/Edit.cshtml
- TODO [ ] **View name & route documented**: GET /ContractTemplates/Edit (ContractTemplatesController.Edit)
- TODO [ ] **Purpose statement**: Edit contract template.
- TODO [ ] **Owner & dependencies**: ContractTemplatesController
- TODO [ ] **User roles/permissions required**: Roles: Admin, Photographer (verify per action)
- TODO [ ] **Analytics event name**: `view_contracttemplates_edit`

### View: ContractTemplates/Index.cshtml
- TODO [ ] **View name & route documented**: GET /ContractTemplates (ContractTemplatesController.Index) | GET /ContractTemplates/Index (ContractTemplatesController.Index)
- TODO [ ] **Purpose statement**: List and manage contract templates.
- TODO [ ] **Owner & dependencies**: ContractTemplatesController
- TODO [ ] **User roles/permissions required**: Roles: Admin, Photographer (verify per action)
- TODO [ ] **Analytics event name**: `view_contracttemplates_index`

### View: Error/Error400.cshtml
- TODO [ ] **View name & route documented**: Unused/indirect (no GET action found). Used by: not found
- TODO [ ] **Purpose statement**: Error page (Error400).
- TODO [ ] **Owner & dependencies**: ErrorController
- TODO [ ] **User roles/permissions required**: No [Authorize] on controller; verify global policy
- TODO [ ] **Analytics event name**: `view_error_error400`

### View: Error/Error401.cshtml
- TODO [ ] **View name & route documented**: Unused/indirect (no GET action found). Used by: not found
- TODO [ ] **Purpose statement**: Error page (Error401).
- TODO [ ] **Owner & dependencies**: ErrorController
- TODO [ ] **User roles/permissions required**: No [Authorize] on controller; verify global policy
- TODO [ ] **Analytics event name**: `view_error_error401`

### View: Error/Error403.cshtml
- TODO [ ] **View name & route documented**: Unused/indirect (no GET action found). Used by: not found
- TODO [ ] **Purpose statement**: Error page (Error403).
- TODO [ ] **Owner & dependencies**: ErrorController
- TODO [ ] **User roles/permissions required**: No [Authorize] on controller; verify global policy
- TODO [ ] **Analytics event name**: `view_error_error403`

### View: Error/Error404.cshtml
- TODO [ ] **View name & route documented**: Unused/indirect (no GET action found). Used by: not found
- TODO [ ] **Purpose statement**: Error page (Error404).
- TODO [ ] **Owner & dependencies**: ErrorController
- TODO [ ] **User roles/permissions required**: No [Authorize] on controller; verify global policy
- TODO [ ] **Analytics event name**: `view_error_error404`

### View: Error/Error408.cshtml
- TODO [ ] **View name & route documented**: Unused/indirect (no GET action found). Used by: not found
- TODO [ ] **Purpose statement**: Error page (Error408).
- TODO [ ] **Owner & dependencies**: ErrorController
- TODO [ ] **User roles/permissions required**: No [Authorize] on controller; verify global policy
- TODO [ ] **Analytics event name**: `view_error_error408`

### View: Error/Error500.cshtml
- TODO [ ] **View name & route documented**: Unused/indirect (no GET action found). Used by: not found
- TODO [ ] **Purpose statement**: Error page (Error500).
- TODO [ ] **Owner & dependencies**: ErrorController
- TODO [ ] **User roles/permissions required**: No [Authorize] on controller; verify global policy
- TODO [ ] **Analytics event name**: `view_error_error500`

### View: Error/Maintenance.cshtml
- TODO [ ] **View name & route documented**: Unused/indirect (no GET action found). Used by: not found
- TODO [ ] **Purpose statement**: Error page (Maintenance).
- TODO [ ] **Owner & dependencies**: ErrorController
- TODO [ ] **User roles/permissions required**: No [Authorize] on controller; verify global policy
- TODO [ ] **Analytics event name**: `view_error_maintenance`

### View: FileManager/FileManager.cshtml
- TODO [ ] **View name & route documented**: Unused/indirect (no GET action found). Used by: not found
- TODO [ ] **Purpose statement**: View for file (FileManager).
- TODO [ ] **Owner & dependencies**: FileManagerController
- TODO [ ] **User roles/permissions required**: Authorized (default policy); verify per action
- TODO [ ] **Analytics event name**: `view_filemanager_filemanager`

### View: FileManager/Index.cshtml
- TODO [ ] **View name & route documented**: GET /FileManager (FileManagerController.Index) | GET /FileManager/Index (FileManagerController.Index)
- TODO [ ] **Purpose statement**: List and manage files.
- TODO [ ] **Owner & dependencies**: FileManagerController
- TODO [ ] **User roles/permissions required**: Authorized (default policy); verify per action
- TODO [ ] **Analytics event name**: `view_filemanager_index`

### View: Galleries/_CreateGalleryModal.cshtml
- TODO [ ] **View name & route documented**: Partial or shared view (not directly routed). Used by: not found
- TODO [ ] **Purpose statement**: Partial for galleries module.
- TODO [ ] **Owner & dependencies**: Partial view in Galleries module
- TODO [ ] **User roles/permissions required**: Inherits from parent view
- TODO [ ] **Analytics event name**: `partial_galleries_creategallerymodal`

### View: Galleries/_EditGalleryModal.cshtml
- TODO [ ] **View name & route documented**: Partial or shared view (not directly routed). Used by: not found
- TODO [ ] **Purpose statement**: Partial for galleries module.
- TODO [ ] **Owner & dependencies**: Partial view in Galleries module
- TODO [ ] **User roles/permissions required**: Inherits from parent view
- TODO [ ] **Analytics event name**: `partial_galleries_editgallerymodal`

### View: Galleries/_GalleryDetailsModal.cshtml
- TODO [ ] **View name & route documented**: Partial or shared view (not directly routed). Used by: not found
- TODO [ ] **Purpose statement**: Partial for galleries module.
- TODO [ ] **Owner & dependencies**: Partial view in Galleries module
- TODO [ ] **User roles/permissions required**: Inherits from parent view
- TODO [ ] **Analytics event name**: `partial_galleries_gallerydetailsmodal`

### View: Galleries/_GallerySessionsModal.cshtml
- TODO [ ] **View name & route documented**: Partial or shared view (not directly routed). Used by: not found
- TODO [ ] **Purpose statement**: Partial for galleries module.
- TODO [ ] **Owner & dependencies**: Partial view in Galleries module
- TODO [ ] **User roles/permissions required**: Inherits from parent view
- TODO [ ] **Analytics event name**: `partial_galleries_gallerysessionsmodal`

### View: Galleries/Index.cshtml
- TODO [ ] **View name & route documented**: GET /Galleries (GalleriesController.Index) | GET /Galleries/Index (GalleriesController.Index)
- TODO [ ] **Purpose statement**: List and manage galleries.
- TODO [ ] **Owner & dependencies**: GalleriesController
- TODO [ ] **User roles/permissions required**: Roles: Admin, Photographer (verify per action)
- TODO [ ] **Analytics event name**: `view_galleries_index`

### View: Galleries/_ManageAccessModal.cshtml
- TODO [ ] **View name & route documented**: Partial or shared view (not directly routed). Used by: not found
- TODO [ ] **Purpose statement**: Partial for galleries module.
- TODO [ ] **Owner & dependencies**: Partial view in Galleries module
- TODO [ ] **User roles/permissions required**: Inherits from parent view
- TODO [ ] **Analytics event name**: `partial_galleries_manageaccessmodal`

### View: Galleries/_RegenerateCodeModal.cshtml
- TODO [ ] **View name & route documented**: Partial or shared view (not directly routed). Used by: not found
- TODO [ ] **Purpose statement**: Partial for galleries module.
- TODO [ ] **Owner & dependencies**: Partial view in Galleries module
- TODO [ ] **User roles/permissions required**: Inherits from parent view
- TODO [ ] **Analytics event name**: `partial_galleries_regeneratecodemodal`

### View: Gallery/AccessGallery.cshtml
- TODO [x] **View name & route documented**: GET `/gallery/access` (GalleryController.AccessGallery, [AllowAnonymous]); redirects to `ViewPublicGallery` when token valid, otherwise shows form. Used by: public access entry for shared galleries.
- TODO [x] **Purpose statement**: Collect public access token from clients to open shared galleries; surface errors for invalid/expired codes.
- TODO [x] **Owner & dependencies**: GalleryController; depends on `IGalleryService.GetGalleryIdByTokenAsync`, shared layout `_BaseLayout`.
- TODO [x] **User roles/permissions required**: Anonymous (token-gated). When authenticated staff/client, same form works and redirects if token valid.
- TODO [x] **Analytics event name**: `view_gallery_accessgallery`; key action candidate: `action_gallery_access_submit` on form submission.

### View: Gallery/Index.cshtml
- TODO [ ] **View name & route documented**: Unused/indirect (no GET action found). Used by: not found
- TODO [ ] **Purpose statement**: List and manage galleries.
- TODO [ ] **Owner & dependencies**: GalleryController
- TODO [ ] **User roles/permissions required**: Roles: Admin, Photographer, Client (some actions allow anonymous; verify per action)
- TODO [ ] **Analytics event name**: `view_gallery_index`

### View: Gallery/NoAccess.cshtml
- TODO [ ] **View name & route documented**: GET /Gallery (GalleryController.Index) | GET /Gallery/Index (GalleryController.Index) | GET /gallery/view/{token} (GalleryController.ViewPublicGallery) | GET /gallery/{slug} (GalleryController.ViewPublicGalleryBySlug)
- TODO [ ] **Purpose statement**: View for gallery (NoAccess).
- TODO [ ] **Owner & dependencies**: GalleryController
- TODO [ ] **User roles/permissions required**: Roles: Admin, Photographer, Client (some actions allow anonymous; verify per action)
- TODO [ ] **Analytics event name**: `view_gallery_noaccess`

### View: Gallery/ViewGallery.cshtml
- TODO [ ] **View name & route documented**: GET /Gallery/ViewGallery (GalleryController.ViewGallery) | GET /gallery/view/{token} (GalleryController.ViewPublicGallery) | GET /gallery/{slug} (GalleryController.ViewPublicGalleryBySlug)
- TODO [ ] **Purpose statement**: Client gallery view and proofing.
- TODO [ ] **Owner & dependencies**: GalleryController
- TODO [ ] **User roles/permissions required**: Roles: Admin, Photographer, Client (some actions allow anonymous; verify per action)
- TODO [ ] **Analytics event name**: `view_gallery_viewgallery`

### View: Home/Dashboard.cshtml
- TODO [ ] **View name & route documented**: GET / (HomeController.Index) | GET /Home/Index (HomeController.Index) | GET /Home/Dashboard (HomeController.Dashboard)
- TODO [ ] **Purpose statement**: Staff KPI dashboard.
- TODO [ ] **Owner & dependencies**: HomeController
- TODO [ ] **User roles/permissions required**: Roles: Admin, Photographer (verify per action)
- TODO [ ] **Analytics event name**: `view_home_dashboard`

### View: Home/Default.cshtml
- TODO [ ] **View name & route documented**: Unused/indirect (no GET action found). Used by: not found
- TODO [ ] **Purpose statement**: View for dashboard (Default).
- TODO [ ] **Owner & dependencies**: HomeController
- TODO [ ] **User roles/permissions required**: Roles: Admin, Photographer (verify per action)
- TODO [ ] **Analytics event name**: `view_home_default`

### View: Home/Index.cshtml
- TODO [ ] **View name & route documented**: Unused/indirect (no GET action found). Used by: not found
- TODO [ ] **Purpose statement**: List and manage dashboard.
- TODO [ ] **Owner & dependencies**: HomeController
- TODO [ ] **User roles/permissions required**: Roles: Admin, Photographer (verify per action)
- TODO [ ] **Analytics event name**: `view_home_index`

### View: Home/Landing.cshtml
- TODO [ ] **View name & route documented**: GET /Home/Landing (HomeController.Landing)
- TODO [ ] **Purpose statement**: View for dashboard (Landing).
- TODO [ ] **Owner & dependencies**: HomeController
- TODO [ ] **User roles/permissions required**: Roles: Admin, Photographer (verify per action)
- TODO [ ] **Analytics event name**: `view_home_landing`

### View: Invoices/Create.cshtml
- TODO [ ] **View name & route documented**: GET /Invoices/Create (InvoicesController.Create) | GET /Invoices/Edit (InvoicesController.Edit)
- TODO [ ] **Purpose statement**: Create invoice.
- TODO [ ] **Owner & dependencies**: InvoicesController
- TODO [ ] **User roles/permissions required**: Roles: Admin, Photographer (verify per action)
- TODO [ ] **Analytics event name**: `view_invoices_create`

### View: Invoices/Details.cshtml
- TODO [ ] **View name & route documented**: GET /Invoices/Details (InvoicesController.Details)
- TODO [ ] **Purpose statement**: View invoice details.
- TODO [ ] **Owner & dependencies**: InvoicesController
- TODO [ ] **User roles/permissions required**: Roles: Admin, Photographer (verify per action)
- TODO [ ] **Analytics event name**: `view_invoices_details`

### View: Invoices/Index.cshtml
- TODO [ ] **View name & route documented**: GET /Invoices (InvoicesController.Index) | GET /Invoices/Index (InvoicesController.Index)
- TODO [ ] **Purpose statement**: List and manage invoices.
- TODO [ ] **Owner & dependencies**: InvoicesController
- TODO [ ] **User roles/permissions required**: Roles: Admin, Photographer (verify per action)
- TODO [ ] **Analytics event name**: `view_invoices_index`

### View: Invoices/IndexNew.cshtml
- TODO [ ] **View name & route documented**: Unused/indirect (no GET action found). Used by: not found
- TODO [ ] **Purpose statement**: View for invoice (IndexNew).
- TODO [ ] **Owner & dependencies**: InvoicesController
- TODO [ ] **User roles/permissions required**: Roles: Admin, Photographer (verify per action)
- TODO [ ] **Analytics event name**: `view_invoices_indexnew`

### View: Invoices/MyInvoices.cshtml
- TODO [x] **View name & route documented**: GET /Invoices/MyInvoices (InvoicesController.MyInvoices) [Authorize(Client)].
- TODO [x] **Purpose statement**: Client self-service list of their invoices with status, totals, and pay/view actions.
- TODO [x] **Owner & dependencies**: InvoicesController; depends on `IInvoiceService.GetClientInvoicesAsync`, `IClientService.GetClientByUserIdAsync`, `UserManager`.
- TODO [x] **User roles/permissions required**: Client.
- TODO [x] **Analytics event name**: `view_invoices_myinvoices`; key action candidate `action_invoice_pay_click`.

### View: Notifications/Index.cshtml
- TODO [ ] **View name & route documented**: GET /Notifications (NotificationsController.Index) | GET /Notifications/Index (NotificationsController.Index)
- TODO [ ] **Purpose statement**: List and manage notifications.
- TODO [ ] **Owner & dependencies**: NotificationsController
- TODO [ ] **User roles/permissions required**: Authorized (default policy); verify per action
- TODO [ ] **Analytics event name**: `view_notifications_index`

### View: Packages/AddOns.cshtml
- TODO [ ] **View name & route documented**: GET /Packages/AddOns (PackagesController.AddOns)
- TODO [ ] **Purpose statement**: View for package (AddOns).
- TODO [ ] **Owner & dependencies**: PackagesController
- TODO [ ] **User roles/permissions required**: Roles: Admin (some actions allow anonymous; verify per action)
- TODO [ ] **Analytics event name**: `view_packages_addons`

### View: Packages/CreateAddOn.cshtml
- TODO [ ] **View name & route documented**: GET /Packages/CreateAddOn (PackagesController.CreateAddOn)
- TODO [ ] **Purpose statement**: View for package (CreateAddOn).
- TODO [ ] **Owner & dependencies**: PackagesController
- TODO [ ] **User roles/permissions required**: Roles: Admin (some actions allow anonymous; verify per action)
- TODO [ ] **Analytics event name**: `view_packages_createaddon`

### View: Packages/Create.cshtml
- TODO [ ] **View name & route documented**: GET /Packages/Create (PackagesController.Create)
- TODO [ ] **Purpose statement**: Create package.
- TODO [ ] **Owner & dependencies**: PackagesController
- TODO [ ] **User roles/permissions required**: Roles: Admin (some actions allow anonymous; verify per action)
- TODO [ ] **Analytics event name**: `view_packages_create`

### View: Packages/Details.cshtml
- TODO [ ] **View name & route documented**: GET /Packages/Details (PackagesController.Details)
- TODO [ ] **Purpose statement**: View package details.
- TODO [ ] **Owner & dependencies**: PackagesController
- TODO [ ] **User roles/permissions required**: Roles: Admin (some actions allow anonymous; verify per action)
- TODO [ ] **Analytics event name**: `view_packages_details`

### View: Packages/EditAddOn.cshtml
- TODO [ ] **View name & route documented**: GET /Packages/EditAddOn (PackagesController.EditAddOn)
- TODO [ ] **Purpose statement**: View for package (EditAddOn).
- TODO [ ] **Owner & dependencies**: PackagesController
- TODO [ ] **User roles/permissions required**: Roles: Admin (some actions allow anonymous; verify per action)
- TODO [ ] **Analytics event name**: `view_packages_editaddon`

### View: Packages/Edit.cshtml
- TODO [ ] **View name & route documented**: GET /Packages/Edit (PackagesController.Edit)
- TODO [ ] **Purpose statement**: Edit package.
- TODO [ ] **Owner & dependencies**: PackagesController
- TODO [ ] **User roles/permissions required**: Roles: Admin (some actions allow anonymous; verify per action)
- TODO [ ] **Analytics event name**: `view_packages_edit`

### View: Packages/Index.cshtml
- TODO [ ] **View name & route documented**: GET /Packages (PackagesController.Index) | GET /Packages/Index (PackagesController.Index)
- TODO [ ] **Purpose statement**: List and manage packages.
- TODO [ ] **Owner & dependencies**: PackagesController
- TODO [ ] **User roles/permissions required**: Roles: Admin (some actions allow anonymous; verify per action)
- TODO [ ] **Analytics event name**: `view_packages_index`

### View: Packages/Manage.cshtml
- TODO [ ] **View name & route documented**: GET /Packages/Manage (PackagesController.Manage)
- TODO [ ] **Purpose statement**: View for package (Manage).
- TODO [ ] **Owner & dependencies**: PackagesController
- TODO [ ] **User roles/permissions required**: Roles: Admin (some actions allow anonymous; verify per action)
- TODO [ ] **Analytics event name**: `view_packages_manage`

### View: Permissions/_CreatePermissionModal.cshtml
- TODO [ ] **View name & route documented**: Partial or shared view (not directly routed). Used by: not found
- TODO [ ] **Purpose statement**: Partial for permissions module.
- TODO [ ] **Owner & dependencies**: Partial view in Permissions module
- TODO [ ] **User roles/permissions required**: Inherits from parent view
- TODO [ ] **Analytics event name**: `partial_permissions_createpermissionmodal`

### View: Permissions/_EditPermissionModal.cshtml
- TODO [ ] **View name & route documented**: Partial or shared view (not directly routed). Used by: not found
- TODO [ ] **Purpose statement**: Partial for permissions module.
- TODO [ ] **Owner & dependencies**: Partial view in Permissions module
- TODO [ ] **User roles/permissions required**: Inherits from parent view
- TODO [ ] **Analytics event name**: `partial_permissions_editpermissionmodal`

### View: Permissions/Index.cshtml
- TODO [ ] **View name & route documented**: GET /Permissions (PermissionsController.Index) | GET /Permissions/Index (PermissionsController.Index)
- TODO [ ] **Purpose statement**: List and manage permissions.
- TODO [ ] **Owner & dependencies**: PermissionsController
- TODO [ ] **User roles/permissions required**: Roles: Admin (verify per action)
- TODO [ ] **Analytics event name**: `view_permissions_index`

### View: Permissions/_PermissionDetailsModal.cshtml
- TODO [ ] **View name & route documented**: Partial or shared view (not directly routed). Used by: not found
- TODO [ ] **Purpose statement**: Partial for permissions module.
- TODO [ ] **Owner & dependencies**: Partial view in Permissions module
- TODO [ ] **User roles/permissions required**: Inherits from parent view
- TODO [ ] **Analytics event name**: `partial_permissions_permissiondetailsmodal`

### View: PhotoShoots/Calendar.cshtml
- TODO [ ] **View name & route documented**: Unused/indirect (no GET action found). Used by: not found
- TODO [ ] **Purpose statement**: Calendar and scheduling for photo shoots.
- TODO [ ] **Owner & dependencies**: PhotoShootsController
- TODO [ ] **User roles/permissions required**: Roles: Admin, Photographer, Client (verify per action)
- TODO [ ] **Analytics event name**: `view_photoshoots_calendar`

### View: PhotoShoots/Create.cshtml
- TODO [ ] **View name & route documented**: GET /PhotoShoots/Create (PhotoShootsController.Create)
- TODO [ ] **Purpose statement**: Create photo shoot.
- TODO [ ] **Owner & dependencies**: PhotoShootsController
- TODO [ ] **User roles/permissions required**: Roles: Admin, Photographer, Client (verify per action)
- TODO [ ] **Analytics event name**: `view_photoshoots_create`

### View: PhotoShoots/Details.cshtml
- TODO [ ] **View name & route documented**: GET /PhotoShoots/Details (PhotoShootsController.Details)
- TODO [ ] **Purpose statement**: View photo shoot details.
- TODO [ ] **Owner & dependencies**: PhotoShootsController
- TODO [ ] **User roles/permissions required**: Roles: Admin, Photographer, Client (verify per action)
- TODO [ ] **Analytics event name**: `view_photoshoots_details`

### View: PhotoShoots/Edit.cshtml
- TODO [ ] **View name & route documented**: GET /PhotoShoots/Edit (PhotoShootsController.Edit)
- TODO [ ] **Purpose statement**: Edit photo shoot.
- TODO [ ] **Owner & dependencies**: PhotoShootsController
- TODO [ ] **User roles/permissions required**: Roles: Admin, Photographer, Client (verify per action)
- TODO [ ] **Analytics event name**: `view_photoshoots_edit`

### View: PhotoShoots/Index.cshtml
- TODO [ ] **View name & route documented**: GET /PhotoShoots (PhotoShootsController.Index) | GET /PhotoShoots/Index (PhotoShootsController.Index)
- TODO [ ] **Purpose statement**: List and manage photo shoots.
- TODO [ ] **Owner & dependencies**: PhotoShootsController
- TODO [ ] **User roles/permissions required**: Roles: Admin, Photographer, Client (verify per action)
- TODO [ ] **Analytics event name**: `view_photoshoots_index`

### View: Photos/Index.cshtml
- TODO [ ] **View name & route documented**: GET /Photos (PhotosController.Index) | GET /Photos/Index (PhotosController.Index)
- TODO [ ] **Purpose statement**: List and manage photos.
- TODO [ ] **Owner & dependencies**: PhotosController
- TODO [ ] **User roles/permissions required**: Roles: Admin, Photographer (some actions allow anonymous; verify per action)
- TODO [ ] **Analytics event name**: `view_photos_index`

### View: Photos/Photo.cshtml
- TODO [ ] **View name & route documented**: Unused/indirect (no GET action found). Used by: not found
- TODO [ ] **Purpose statement**: View for photo (Photo).
- TODO [ ] **Owner & dependencies**: PhotosController
- TODO [ ] **User roles/permissions required**: Roles: Admin, Photographer (some actions allow anonymous; verify per action)
- TODO [ ] **Analytics event name**: `view_photos_photo`

### View: Photos/_PhotoGallery.cshtml
- TODO [ ] **View name & route documented**: Partial or shared view (not directly routed). Used by: Views/Albums/Details.cshtml:138
- TODO [ ] **Purpose statement**: Partial for photos module.
- TODO [ ] **Owner & dependencies**: Partial view in Photos module
- TODO [ ] **User roles/permissions required**: Inherits from parent view
- TODO [ ] **Analytics event name**: `partial_photos_photogallery`

### View: Photos/Upload.cshtml
- TODO [ ] **View name & route documented**: GET /Photos/Upload (PhotosController.Upload)
- TODO [ ] **Purpose statement**: View for photo (Upload).
- TODO [ ] **Owner & dependencies**: PhotosController
- TODO [ ] **User roles/permissions required**: Roles: Admin, Photographer (some actions allow anonymous; verify per action)
- TODO [ ] **Analytics event name**: `view_photos_upload`

### View: PrintOrder/CartPreview.cshtml
- TODO [ ] **View name & route documented**: GET /PrintOrder/CartPreview (PrintOrderController.CartPreview)
- TODO [ ] **Purpose statement**: View for print order (CartPreview).
- TODO [ ] **Owner & dependencies**: PrintOrderController
- TODO [ ] **User roles/permissions required**: Roles: Admin, Photographer, Client (some actions allow anonymous; verify per action)
- TODO [ ] **Analytics event name**: `view_printorder_cartpreview`

### View: PrintOrder/Details.cshtml
- TODO [ ] **View name & route documented**: GET /PrintOrder/Details (PrintOrderController.Details)
- TODO [ ] **Purpose statement**: View print order details.
- TODO [ ] **Owner & dependencies**: PrintOrderController
- TODO [ ] **User roles/permissions required**: Roles: Admin, Photographer, Client (some actions allow anonymous; verify per action)
- TODO [ ] **Analytics event name**: `view_printorder_details`

### View: PrintOrder/Index.cshtml
- TODO [ ] **View name & route documented**: GET /PrintOrder (PrintOrderController.Index) | GET /PrintOrder/Index (PrintOrderController.Index)
- TODO [ ] **Purpose statement**: List and manage print orders.
- TODO [ ] **Owner & dependencies**: PrintOrderController
- TODO [ ] **User roles/permissions required**: Roles: Admin, Photographer, Client (some actions allow anonymous; verify per action)
- TODO [ ] **Analytics event name**: `view_printorder_index`

### View: PrintOrder/MyOrders.cshtml
- TODO [ ] **View name & route documented**: GET /PrintOrder/MyOrders (PrintOrderController.MyOrders)
- TODO [ ] **Purpose statement**: View for print order (MyOrders).
- TODO [ ] **Owner & dependencies**: PrintOrderController
- TODO [ ] **User roles/permissions required**: Roles: Admin, Photographer, Client (some actions allow anonymous; verify per action)
- TODO [ ] **Analytics event name**: `view_printorder_myorders`

### View: PrintOrder/OrderConfirmation.cshtml
- TODO [ ] **View name & route documented**: GET /PrintOrder/OrderConfirmation (PrintOrderController.OrderConfirmation)
- TODO [ ] **Purpose statement**: View for print order (OrderConfirmation).
- TODO [ ] **Owner & dependencies**: PrintOrderController
- TODO [ ] **User roles/permissions required**: Roles: Admin, Photographer, Client (some actions allow anonymous; verify per action)
- TODO [ ] **Analytics event name**: `view_printorder_orderconfirmation`

### View: PrintOrder/Pricing.cshtml
- TODO [ ] **View name & route documented**: GET /PrintOrder/Pricing (PrintOrderController.Pricing)
- TODO [ ] **Purpose statement**: View for print order (Pricing).
- TODO [ ] **Owner & dependencies**: PrintOrderController
- TODO [ ] **User roles/permissions required**: Roles: Admin, Photographer, Client (some actions allow anonymous; verify per action)
- TODO [ ] **Analytics event name**: `view_printorder_pricing`

### View: Proofs/Analytics.cshtml
- TODO [ ] **View name & route documented**: GET /Proofs/Analytics (ProofsController.Analytics)
- TODO [ ] **Purpose statement**: View for proof (Analytics).
- TODO [ ] **Owner & dependencies**: ProofsController
- TODO [ ] **User roles/permissions required**: Roles: Admin, Photographer (verify per action)
- TODO [ ] **Analytics event name**: `view_proofs_analytics`

### View: Proofs/Index.cshtml
- TODO [ ] **View name & route documented**: GET /Proofs (ProofsController.Index) | GET /Proofs/Index (ProofsController.Index)
- TODO [ ] **Purpose statement**: List and manage proofs.
- TODO [ ] **Owner & dependencies**: ProofsController
- TODO [ ] **User roles/permissions required**: Roles: Admin, Photographer (verify per action)
- TODO [ ] **Analytics event name**: `view_proofs_index`

### View: Proofs/_ProofDetailsModal.cshtml
- TODO [ ] **View name & route documented**: Partial or shared view (not directly routed). Used by: not found
- TODO [ ] **Purpose statement**: Partial for proofs module.
- TODO [ ] **Owner & dependencies**: Partial view in Proofs module
- TODO [ ] **User roles/permissions required**: Inherits from parent view
- TODO [ ] **Analytics event name**: `partial_proofs_proofdetailsmodal`

### View: QuestionnaireAssignments/Create.cshtml
- TODO [ ] **View name & route documented**: GET /QuestionnaireAssignments/Create (QuestionnaireAssignmentsController.Create)
- TODO [ ] **Purpose statement**: Create questionnaire assignment.
- TODO [ ] **Owner & dependencies**: QuestionnaireAssignmentsController
- TODO [ ] **User roles/permissions required**: Roles: Admin, Photographer (verify per action)
- TODO [ ] **Analytics event name**: `view_questionnaireassignments_create`

### View: QuestionnaireAssignments/Index.cshtml
- TODO [ ] **View name & route documented**: GET /QuestionnaireAssignments (QuestionnaireAssignmentsController.Index) | GET /QuestionnaireAssignments/Index (QuestionnaireAssignmentsController.Index)
- TODO [ ] **Purpose statement**: List and manage questionnaire assignments.
- TODO [ ] **Owner & dependencies**: QuestionnaireAssignmentsController
- TODO [ ] **User roles/permissions required**: Roles: Admin, Photographer (verify per action)
- TODO [ ] **Analytics event name**: `view_questionnaireassignments_index`

### View: QuestionnaireTemplates/Create.cshtml
- TODO [ ] **View name & route documented**: GET /QuestionnaireTemplates/Create (QuestionnaireTemplatesController.Create)
- TODO [ ] **Purpose statement**: Create questionnaire template.
- TODO [ ] **Owner & dependencies**: QuestionnaireTemplatesController
- TODO [ ] **User roles/permissions required**: Roles: Admin, Photographer (verify per action)
- TODO [ ] **Analytics event name**: `view_questionnairetemplates_create`

### View: QuestionnaireTemplates/Edit.cshtml
- TODO [ ] **View name & route documented**: GET /QuestionnaireTemplates/Edit (QuestionnaireTemplatesController.Edit)
- TODO [ ] **Purpose statement**: Edit questionnaire template.
- TODO [ ] **Owner & dependencies**: QuestionnaireTemplatesController
- TODO [ ] **User roles/permissions required**: Roles: Admin, Photographer (verify per action)
- TODO [ ] **Analytics event name**: `view_questionnairetemplates_edit`

### View: QuestionnaireTemplates/Index.cshtml
- TODO [ ] **View name & route documented**: GET /QuestionnaireTemplates (QuestionnaireTemplatesController.Index) | GET /QuestionnaireTemplates/Index (QuestionnaireTemplatesController.Index)
- TODO [ ] **Purpose statement**: List and manage questionnaire templates.
- TODO [ ] **Owner & dependencies**: QuestionnaireTemplatesController
- TODO [ ] **User roles/permissions required**: Roles: Admin, Photographer (verify per action)
- TODO [ ] **Analytics event name**: `view_questionnairetemplates_index`

### View: Roles/_CreateRoleModal.cshtml
- TODO [ ] **View name & route documented**: Partial or shared view (not directly routed). Used by: not found
- TODO [ ] **Purpose statement**: Partial for roles module.
- TODO [ ] **Owner & dependencies**: Partial view in Roles module
- TODO [ ] **User roles/permissions required**: Inherits from parent view
- TODO [ ] **Analytics event name**: `partial_roles_createrolemodal`

### View: Roles/_DeleteModal.cshtml
- TODO [ ] **View name & route documented**: Partial or shared view (not directly routed). Used by: Views/Roles/_DeleteModal.cshtml:6
- TODO [ ] **Purpose statement**: Partial for roles module.
- TODO [ ] **Owner & dependencies**: Partial view in Roles module
- TODO [ ] **User roles/permissions required**: Inherits from parent view
- TODO [ ] **Analytics event name**: `partial_roles_deletemodal`

### View: Roles/_EditRoleModal.cshtml
- TODO [ ] **View name & route documented**: Partial or shared view (not directly routed). Used by: not found
- TODO [ ] **Purpose statement**: Partial for roles module.
- TODO [ ] **Owner & dependencies**: Partial view in Roles module
- TODO [ ] **User roles/permissions required**: Inherits from parent view
- TODO [ ] **Analytics event name**: `partial_roles_editrolemodal`

### View: Roles/Index.cshtml
- TODO [ ] **View name & route documented**: GET /Roles (RolesController.Index) | GET /Roles/Index (RolesController.Index)
- TODO [ ] **Purpose statement**: List and manage roles.
- TODO [ ] **Owner & dependencies**: RolesController
- TODO [ ] **User roles/permissions required**: Roles: Admin (verify per action)
- TODO [ ] **Analytics event name**: `view_roles_index`

### View: Roles/_RoleCard.cshtml
- TODO [ ] **View name & route documented**: Partial or shared view (not directly routed). Used by: not found
- TODO [ ] **Purpose statement**: Partial for roles module.
- TODO [ ] **Owner & dependencies**: Partial view in Roles module
- TODO [ ] **User roles/permissions required**: Inherits from parent view
- TODO [ ] **Analytics event name**: `partial_roles_rolecard`

### View: Roles/_RoleDetailsModal.cshtml
- TODO [ ] **View name & route documented**: Partial or shared view (not directly routed). Used by: not found
- TODO [ ] **Purpose statement**: Partial for roles module.
- TODO [ ] **Owner & dependencies**: Partial view in Roles module
- TODO [ ] **User roles/permissions required**: Inherits from parent view
- TODO [ ] **Analytics event name**: `partial_roles_roledetailsmodal`

### View: Shared/_BaseLayout.cshtml
- TODO [ ] **View name & route documented**: Partial or shared view (not directly routed). Used by: not found
- TODO [ ] **Purpose statement**: Shared layout/partial (navigation, scripts, common UI).
- TODO [ ] **Owner & dependencies**: Shared layout/partial
- TODO [ ] **User roles/permissions required**: Inherits from parent view
- TODO [ ] **Analytics event name**: `partial_shared_baselayout`

### View: Shared/_ClientBadges.cshtml
- TODO [ ] **View name & route documented**: Partial or shared view (not directly routed). Used by: Views/Clients/Details.cshtml:620,Views/Users/Details.cshtml:394
- TODO [ ] **Purpose statement**: Shared layout/partial (navigation, scripts, common UI).
- TODO [ ] **Owner & dependencies**: Shared layout/partial
- TODO [ ] **User roles/permissions required**: Inherits from parent view
- TODO [ ] **Analytics event name**: `partial_shared_clientbadges`

### View: Shared/_Layout.cshtml
- TODO [ ] **View name & route documented**: Partial or shared view (not directly routed). Used by: not found
- TODO [ ] **Purpose statement**: Shared layout/partial (navigation, scripts, common UI).
- TODO [ ] **Owner & dependencies**: Shared layout/partial
- TODO [ ] **User roles/permissions required**: Inherits from parent view
- TODO [ ] **Analytics event name**: `partial_shared_layout`

### View: Shared/_LoginPartial.cshtml
- TODO [ ] **View name & route documented**: Partial or shared view (not directly routed). Used by: Views/Shared/_Layout.cshtml:47
- TODO [ ] **Purpose statement**: Shared layout/partial (navigation, scripts, common UI).
- TODO [ ] **Owner & dependencies**: Shared layout/partial
- TODO [ ] **User roles/permissions required**: Inherits from parent view
- TODO [ ] **Analytics event name**: `partial_shared_loginpartial`

### View: Shared/Partials/_ActionButtons.cshtml
- TODO [ ] **View name & route documented**: Partial or shared view (not directly routed). Used by: Views/PhotoShoots/Index.cshtml:125,Views/Clients/Index.cshtml:104 Views/Shared/Partials/_ActionButtons.cshtml:3
- TODO [ ] **Purpose statement**: Shared layout/partial (navigation, scripts, common UI).
- TODO [ ] **Owner & dependencies**: Shared layout/partial
- TODO [ ] **User roles/permissions required**: Inherits from parent view
- TODO [ ] **Analytics event name**: `partial_shared_actionbuttons`

### View: Shared/Partials/_Button.cshtml
- TODO [ ] **View name & route documented**: Partial or shared view (not directly routed). Used by: not found
- TODO [ ] **Purpose statement**: Shared layout/partial (navigation, scripts, common UI).
- TODO [ ] **Owner & dependencies**: Shared layout/partial
- TODO [ ] **User roles/permissions required**: Inherits from parent view
- TODO [ ] **Analytics event name**: `partial_shared_button`

### View: Shared/Partials/_ConfirmModal.cshtml
- TODO [ ] **View name & route documented**: Partial or shared view (not directly routed). Used by: Views/Shared/_Layout.cshtml:87
- TODO [ ] **Purpose statement**: Shared layout/partial (navigation, scripts, common UI).
- TODO [ ] **Owner & dependencies**: Shared layout/partial
- TODO [ ] **User roles/permissions required**: Inherits from parent view
- TODO [ ] **Analytics event name**: `partial_shared_confirmmodal`

### View: Shared/Partials/_CreateRoleModal.cshtml
- TODO [ ] **View name & route documented**: Partial or shared view (not directly routed). Used by: Controllers/RolesController.cs:51
- TODO [ ] **Purpose statement**: Shared layout/partial (navigation, scripts, common UI).
- TODO [ ] **Owner & dependencies**: Shared layout/partial
- TODO [ ] **User roles/permissions required**: Inherits from parent view
- TODO [ ] **Analytics event name**: `partial_shared_createrolemodal`

### View: Shared/Partials/_EditRoleModal.cshtml
- TODO [ ] **View name & route documented**: Partial or shared view (not directly routed). Used by: Controllers/RolesController.cs:112
- TODO [ ] **Purpose statement**: Shared layout/partial (navigation, scripts, common UI).
- TODO [ ] **Owner & dependencies**: Shared layout/partial
- TODO [ ] **User roles/permissions required**: Inherits from parent view
- TODO [ ] **Analytics event name**: `partial_shared_editrolemodal`

### View: Shared/Partials/_Flash.cshtml
- TODO [ ] **View name & route documented**: Partial or shared view (not directly routed). Used by: not found
- TODO [ ] **Purpose statement**: Shared layout/partial (navigation, scripts, common UI).
- TODO [ ] **Owner & dependencies**: Shared layout/partial
- TODO [ ] **User roles/permissions required**: Inherits from parent view
- TODO [ ] **Analytics event name**: `partial_shared_flash`

### View: Shared/Partials/_Footer.cshtml
- TODO [ ] **View name & route documented**: Partial or shared view (not directly routed). Used by: Views/Shared/_VerticalLayout.cshtml:32
- TODO [ ] **Purpose statement**: Shared layout/partial (navigation, scripts, common UI).
- TODO [ ] **Owner & dependencies**: Shared layout/partial
- TODO [ ] **User roles/permissions required**: Inherits from parent view
- TODO [ ] **Analytics event name**: `partial_shared_footer`

### View: Shared/Partials/_FooterScripts.cshtml
- TODO [ ] **View name & route documented**: Partial or shared view (not directly routed). Used by: Views/Shared/_BaseLayout.cshtml:20,Views/Shared/_VerticalLayout.cshtml:36
- TODO [ ] **Purpose statement**: Shared layout/partial (navigation, scripts, common UI).
- TODO [ ] **Owner & dependencies**: Shared layout/partial
- TODO [ ] **User roles/permissions required**: Inherits from parent view
- TODO [ ] **Analytics event name**: `partial_shared_footerscripts`

### View: Shared/Partials/_GlobalSearch.cshtml
- TODO [ ] **View name & route documented**: Partial or shared view (not directly routed). Used by: Views/Shared/_VerticalLayout.cshtml:42
- TODO [ ] **Purpose statement**: Shared layout/partial (navigation, scripts, common UI).
- TODO [ ] **Owner & dependencies**: Shared layout/partial
- TODO [ ] **User roles/permissions required**: Inherits from parent view
- TODO [ ] **Analytics event name**: `partial_shared_globalsearch`

### View: Shared/Partials/_HeadCSS.cshtml
- TODO [ ] **View name & route documented**: Partial or shared view (not directly routed). Used by: Views/Shared/_VerticalLayout.cshtml:13,Views/Shared/_BaseLayout.cshtml:11
- TODO [ ] **Purpose statement**: Shared layout/partial (navigation, scripts, common UI).
- TODO [ ] **Owner & dependencies**: Shared layout/partial
- TODO [ ] **User roles/permissions required**: Inherits from parent view
- TODO [ ] **Analytics event name**: `partial_shared_headcss`

### View: Shared/Partials/_HorizontalNav.cshtml
- TODO [ ] **View name & route documented**: Partial or shared view (not directly routed). Used by: not found
- TODO [ ] **Purpose statement**: Shared layout/partial (navigation, scripts, common UI).
- TODO [ ] **Owner & dependencies**: Shared layout/partial
- TODO [ ] **User roles/permissions required**: Inherits from parent view
- TODO [ ] **Analytics event name**: `partial_shared_horizontalnav`

### View: Shared/Partials/_IndexTableShell.cshtml
- TODO [ ] **View name & route documented**: Partial or shared view (not directly routed). Used by: Views/Clients/Index.cshtml:48,Views/Albums/Index.cshtml:40 Views/QuestionnaireAssignments/Index.cshtml:38,Views/QuestionnaireTemplates/Index.cshtml:26 Views/Users/Index.cshtml:64
- TODO [ ] **Purpose statement**: Shared layout/partial (navigation, scripts, common UI).
- TODO [ ] **Owner & dependencies**: Shared layout/partial
- TODO [ ] **User roles/permissions required**: Inherits from parent view
- TODO [ ] **Analytics event name**: `partial_shared_indextableshell`

### View: Shared/Partials/_PageTitle.cshtml
- TODO [ ] **View name & route documented**: Partial or shared view (not directly routed). Used by: Views/Invoices/Details.cshtml:13,Views/PhotoShoots/Details.cshtml:17 Views/Invoices/Index.cshtml:14,Views/PhotoShoots/Calendar.cshtml:20 Views/PhotoShoots/Index.cshtml:18
- TODO [ ] **Purpose statement**: Shared layout/partial (navigation, scripts, common UI).
- TODO [ ] **Owner & dependencies**: Shared layout/partial
- TODO [ ] **User roles/permissions required**: Inherits from parent view
- TODO [ ] **Analytics event name**: `partial_shared_pagetitle`

### View: Shared/Partials/_SideNav.cshtml
- TODO [ ] **View name & route documented**: Partial or shared view (not directly routed). Used by: Views/Shared/_VerticalLayout.cshtml:25
- TODO [ ] **Purpose statement**: Shared layout/partial (navigation, scripts, common UI).
- TODO [ ] **Owner & dependencies**: Shared layout/partial
- TODO [ ] **User roles/permissions required**: Inherits from parent view
- TODO [ ] **Analytics event name**: `partial_shared_sidenav`

### View: Shared/Partials/_StatusBadge.cshtml
- TODO [ ] **View name & route documented**: Partial or shared view (not directly routed). Used by: Views/Shared/Partials/_StatusBadge.cshtml:3
- TODO [ ] **Purpose statement**: Shared layout/partial (navigation, scripts, common UI).
- TODO [ ] **Owner & dependencies**: Shared layout/partial
- TODO [ ] **User roles/permissions required**: Inherits from parent view
- TODO [ ] **Analytics event name**: `partial_shared_statusbadge`

### View: Shared/Partials/_TitleMeta.cshtml
- TODO [ ] **View name & route documented**: Partial or shared view (not directly routed). Used by: Views/Shared/_VerticalLayout.cshtml:12,Views/Shared/_BaseLayout.cshtml:9
- TODO [ ] **Purpose statement**: Shared layout/partial (navigation, scripts, common UI).
- TODO [ ] **Owner & dependencies**: Shared layout/partial
- TODO [ ] **User roles/permissions required**: Inherits from parent view
- TODO [ ] **Analytics event name**: `partial_shared_titlemeta`

### View: Shared/Partials/_Toasts.cshtml
- TODO [ ] **View name & route documented**: Partial or shared view (not directly routed). Used by: Views/Shared/_Layout.cshtml:86,Views/Shared/_VerticalLayout.cshtml:39
- TODO [ ] **Purpose statement**: Shared layout/partial (navigation, scripts, common UI).
- TODO [ ] **Owner & dependencies**: Shared layout/partial
- TODO [ ] **User roles/permissions required**: Inherits from parent view
- TODO [ ] **Analytics event name**: `partial_shared_toasts`

### View: Shared/Partials/_TopBar.cshtml
- TODO [ ] **View name & route documented**: Partial or shared view (not directly routed). Used by: Views/Shared/_VerticalLayout.cshtml:24
- TODO [ ] **Purpose statement**: Shared layout/partial (navigation, scripts, common UI).
- TODO [ ] **Owner & dependencies**: Shared layout/partial
- TODO [ ] **User roles/permissions required**: Inherits from parent view
- TODO [ ] **Analytics event name**: `partial_shared_topbar`

### View: Shared/_ValidationScriptsPartial.cshtml
- TODO [ ] **View name & route documented**: Partial or shared view (not directly routed). Used by: Views/ContractTemplates/Edit.cshtml:170,Views/PhotoShoots/Edit.cshtml:142 Views/ContractTemplates/Create.cshtml:165,Views/PhotoShoots/Create.cshtml:141 Views/Clients/Index.cshtml:196
- TODO [ ] **Purpose statement**: Shared layout/partial (navigation, scripts, common UI).
- TODO [ ] **Owner & dependencies**: Shared layout/partial
- TODO [ ] **User roles/permissions required**: Inherits from parent view
- TODO [ ] **Analytics event name**: `partial_shared_validationscriptspartial`

### View: Shared/_VerticalLayout.cshtml
- TODO [ ] **View name & route documented**: Partial or shared view (not directly routed). Used by: not found
- TODO [ ] **Purpose statement**: Shared layout/partial (navigation, scripts, common UI).
- TODO [ ] **Owner & dependencies**: Shared layout/partial
- TODO [ ] **User roles/permissions required**: Inherits from parent view
- TODO [ ] **Analytics event name**: `partial_shared_verticallayout`

### View: Users/_ChangePasswordModal.cshtml
- TODO [ ] **View name & route documented**: Partial or shared view (not directly routed). Used by: not found
- TODO [ ] **Purpose statement**: Partial for users module.
- TODO [ ] **Owner & dependencies**: Partial view in Users module
- TODO [ ] **User roles/permissions required**: Inherits from parent view
- TODO [ ] **Analytics event name**: `partial_users_changepasswordmodal`

### View: Users/_CreateUserModal.cshtml
- TODO [ ] **View name & route documented**: Partial or shared view (not directly routed). Used by: not found
- TODO [ ] **Purpose statement**: Partial for users module.
- TODO [ ] **Owner & dependencies**: Partial view in Users module
- TODO [ ] **User roles/permissions required**: Inherits from parent view
- TODO [ ] **Analytics event name**: `partial_users_createusermodal`

### View: Users/Details.cshtml
- TODO [ ] **View name & route documented**: GET /Users/Details (UsersController.Details)
- TODO [ ] **Purpose statement**: View user details.
- TODO [ ] **Owner & dependencies**: UsersController
- TODO [ ] **User roles/permissions required**: Roles: Admin (verify per action)
- TODO [ ] **Analytics event name**: `view_users_details`

### View: Users/_EditUserModal.cshtml
- TODO [ ] **View name & route documented**: Partial or shared view (not directly routed). Used by: not found
- TODO [ ] **Purpose statement**: Partial for users module.
- TODO [ ] **Owner & dependencies**: Partial view in Users module
- TODO [ ] **User roles/permissions required**: Inherits from parent view
- TODO [ ] **Analytics event name**: `partial_users_editusermodal`

### View: Users/Index.cshtml
- TODO [ ] **View name & route documented**: GET /Users (UsersController.Index) | GET /Users/Index (UsersController.Index)
- TODO [ ] **Purpose statement**: List and manage users.
- TODO [ ] **Owner & dependencies**: UsersController
- TODO [ ] **User roles/permissions required**: Roles: Admin (verify per action)
- TODO [ ] **Analytics event name**: `view_users_index`

### View: Users/Roles.cshtml
- TODO [ ] **View name & route documented**: Unused/indirect (no GET action found). Used by: not found
- TODO [ ] **Purpose statement**: View for user (Roles).
- TODO [ ] **Owner & dependencies**: UsersController
- TODO [ ] **User roles/permissions required**: Roles: Admin (verify per action)
- TODO [ ] **Analytics event name**: `view_users_roles`

## 2) Data & API Integration
- TODO [ ] **Data contract validated** (DTOs/Models align with API response).
- TODO [ ] **Loading states** (skeleton/spinner) displayed for initial fetch.
- TODO [ ] **Empty states** for zero results (clear CTAs to resolve).
- TODO [ ] **Error states** for 4xx/5xx with recovery guidance.
- TODO [ ] **Retries/backoff** for transient failures where appropriate.
- TODO [ ] **Timeout handling** (user message and safe fallback).
- TODO [ ] **Caching strategy** (in-memory/HTTP caching) confirmed, if applicable.
- TODO [ ] **Pagination/infinite scroll** handles edge cases (last page, duplicates).
- TODO [ ] **Sorting/filtering** consistent with backend logic.
- TODO [ ] **Data formatting** (dates, currency, time zones) aligns with locale.
- TODO [ ] **Sensitive data masking** (PII, financial info) obeys policy.

### View: Home/Dashboard (Staff)
- [x] **Data contract validated**: server-rendered `DashboardViewModel` from `IDashboardService`.
- TODO [ ] **Loading states**: none visible (server render only); add skeleton for slow responses if needed.
- TODO [ ] **Empty states**: some sections handle empty (e.g., today’s schedule); verify all widgets.
- TODO [ ] **Error states**: no explicit error UI in view; relies on server render.
- TODO [ ] **Retries/backoff**: N/A (server-side).
- TODO [ ] **Timeout handling**: not implemented for dashboard load.
- [x] **Caching strategy**: `IMemoryCache` in `DashboardService` (`DashboardCacheKey`).
- TODO [ ] **Pagination/infinite scroll**: N/A (fixed-size summary widgets).
- [x] **Sorting/filtering**: handled in service queries for recent lists.
- [x] **Data formatting**: dates and currency formatted in view (`ToString("C0")`, `MMM dd, yyyy`, etc.).
- TODO [ ] **Sensitive data masking**: verify KPI exposure aligns with policy (revenue/invoice totals).

### Application-wide (All Views)
- TODO [ ] **Data contract validated**: audit each view/action that binds DTOs to JS or view models to ensure field names and types match API responses.
- TODO [ ] **Loading states**: standardize spinners/skeletons for all async fetches (modals, tables, galleries, file manager).
- TODO [ ] **Empty states**: ensure clear CTAs on lists with zero items (galleries, contracts, roles, permissions, files).
- TODO [ ] **Error states**: add consistent 4xx/5xx handling in fetch flows (toast + inline guidance).
- TODO [ ] **Retries/backoff**: identify transient endpoints (notifications, file metadata) and add retry policy where safe.
- TODO [ ] **Timeout handling**: add fetch timeouts with user messaging for long-running requests.
- TODO [ ] **Caching strategy**: confirm cache headers for APIs that can be cached (search, notifications, galleries).
- TODO [ ] **Pagination/infinite scroll**: verify edge cases for gallery paging, file manager lists, search results.
- TODO [ ] **Sorting/filtering**: keep client-side filters in sync with server-side sort order.
- TODO [ ] **Data formatting**: confirm locale/timezone correctness for photo shoots, invoices, contract dates.
- TODO [ ] **Sensitive data masking**: verify PII exposure in client lists, search, and roles/permissions views.

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
- TODO [ ] **All required fields enforced** client-side + server-side.
- TODO [ ] **Field-level validation messages** are clear and actionable.
- TODO [ ] **Inline help text** for complex fields.
- TODO [ ] **Input constraints** (length, file types, ranges) validated.
- TODO [ ] **Autosave/confirm dialogs** for destructive actions.
- TODO [ ] **Keyboard navigation** complete and logical.
- TODO [ ] **Focus management** after submit/error.
- TODO [ ] **Double-submit prevention** (disabled buttons, idempotency).
- TODO [ ] **File uploads** show progress and handle max size limits.

### View: Home/Dashboard (Staff)
- TODO [ ] **All required fields enforced**: N/A (no forms on dashboard view).
- TODO [ ] **Field-level validation messages**: N/A.
- TODO [ ] **Inline help text**: N/A.
- TODO [ ] **Input constraints**: N/A.
- TODO [ ] **Autosave/confirm dialogs**: N/A.
- TODO [ ] **Keyboard navigation**: verify for widget toggles and dropdown.
- TODO [ ] **Focus management**: verify customize dropdown and collapsible widget buttons.
- TODO [ ] **Double-submit prevention**: N/A.
- TODO [ ] **File uploads**: N/A.

### Application-wide (All Views)
- TODO [ ] **All required fields enforced**: Validate each form; many ViewModels use `[Required]` and controllers check `ModelState.IsValid`, but not confirmed across all views.
- TODO [ ] **Field-level validation messages**: Ensure every form includes `asp-validation-for` or summary and `_ValidationScriptsPartial`.
- TODO [ ] **Inline help text**: Verify complex fields across modules (packages, pricing, uploads).
- TODO [ ] **Input constraints**: Confirm length, range, and file constraints are enforced server-side everywhere (not only UI).
- TODO [ ] **Autosave/confirm dialogs**: Audit destructive actions (delete, revoke, regenerate) and unsaved changes prompts.
- TODO [ ] **Keyboard navigation**: Review modal forms and custom widgets for tab order.
- TODO [ ] **Focus management**: Confirm focus moves to errors and returns appropriately on modal close.
- TODO [ ] **Double-submit prevention**: Ensure submit buttons disable or idempotency is handled across all forms.
- TODO [ ] **File uploads**: Verify progress, size limits, and error handling for profile/album/file uploads.

#### Static scan (forms/validation)
- **Forms found**: 69 `.cshtml` files contain `<form>`.
- **Missing `_ValidationScriptsPartial` in the same file**: 47 (many are filters/search or simple postbacks; verify per view).
- **Missing any `asp-validation-*` tags**: 38 (audit to confirm if validation is required).
- **Next action**: review each form to classify as data entry vs. simple action/filter and ensure client+server validation where required.

---

## 4) UX & UI Consistency
- TODO [ ] **Layout consistent** with design system (spacing, typography, colors).
- TODO [ ] **Breadcrumbs/headers** communicate location in app.
- TODO [ ] **CTAs** are clearly labeled with action verbs.
- TODO [ ] **Confirmation UX** for destructive actions.
- TODO [ ] **Feedback** on success (toast, banner, inline message).
- TODO [ ] **Modals** trap focus and are dismissible.
- TODO [ ] **Images/icons** have alt text and appropriate sizes.
- TODO [ ] **Copy review** (grammar, clarity, tone).

### Application-wide (All Views)
- TODO [ ] **Layout consistency**: align cards, headers, and tables to one system (spacing scale, typography, color tokens).
- TODO [ ] **CTA clarity**: normalize action verbs (Create, Save, Delete, Regenerate) and avoid duplicates.
- TODO [ ] **Success/error feedback**: ensure every create/edit/delete action shows a toast or inline status.
- TODO [ ] **Modal UX**: confirm focus trap, escape to close, and aria labels on all modals.
- TODO [ ] **Copy review**: run a pass for typos, capitalization, and button label consistency.

---

## 5) Accessibility (WCAG basics)
- TODO [ ] **Semantic HTML** and correct heading hierarchy.
- TODO [ ] **Visible focus states** for all interactive elements.
- TODO [ ] **Color contrast** meets WCAG AA.
- TODO [ ] **Screen reader labels** (aria-labels, sr-only text).
- TODO [ ] **Form inputs** properly associated with labels.
- TODO [ ] **No keyboard traps**; tab order is logical.
- TODO [ ] **Error summaries** for forms with multiple issues.
- TODO [ ] **Dynamic content announcements** (aria-live for toasts).

### Application-wide (All Views)
- TODO [x] **Alt text**: ensure all `<img>` have meaningful `alt` (or `alt=""` for decorative). Flagged files audited on 2026-02-02; all contain explicit `alt` attributes (multi-line tags caused prior false positives).
- TODO [ ] **Focus visibility**: verify focus styles for buttons, links, selects, and custom controls.
- TODO [ ] **Contrast**: re-check badge colors, charts, and muted text on cards.
- TODO [ ] **Form labels**: ensure every input/select/textarea has a label or `aria-label`.
- TODO [ ] **Keyboard only**: validate modal flows and dropdowns without mouse.
- TODO [ ] **ARIA live regions**: add for toast notifications and async updates.

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
  - Audit note: all above images include `alt`; scanner flagged because attributes were on separate lines.
- **ARIA attributes**: 260 lines contain `aria-`.
- **`tabindex` usage**: 42 lines.

---

## 6) Security & Privacy
- TODO [ ] **Authorization checks** verified on server for all actions.
- TODO [ ] **CSRF protections** for write actions.
- TODO [ ] **XSS protections** (encoding, safe rendering).
- TODO [ ] **No sensitive data in query params** or client logs.
- TODO [ ] **Download links** time-limited if needed.
- TODO [ ] **Role/permission mismatch** handled with friendly error.
- TODO [ ] **Audit logging** for critical actions.

### Application-wide (All Views)
- TODO [ ] **Authorization**: verify every controller action has the correct role policy (especially Gallery/Proofing/Clients routes).
- TODO [ ] **CSRF**: ensure all POST/PUT/DELETE endpoints have antiforgery protection (including AJAX endpoints).
- TODO [ ] **XSS**: confirm HTML content is encoded and user-provided content is sanitized.
- TODO [ ] **PII exposure**: audit client search, global search, and public gallery endpoints.
- TODO [ ] **Downloads**: confirm gallery downloads/exports are time-limited and logged.

#### Static scan (security)
- **`[ValidateAntiForgeryToken]` occurrences**: 88 in `/Controllers`.
- **No `AutoValidateAntiforgeryToken` found** in `Program.cs` or controllers (verify global antiforgery policy).

---

## 7) Performance & Reliability
- TODO [ ] **First load performance** acceptable (<2s on typical network).
- TODO [ ] **Bundle size** within target; no unused assets.
- TODO [ ] **Image optimization** (compressed, responsive sizes).
- TODO [ ] **Lazy-loading** for below-the-fold content.
- TODO [ ] **Avoid unnecessary reflows** (virtualize big lists).
- TODO [ ] **Server-side response time** under SLA.
- TODO [ ] **Graceful degradation** for partial outages.

### Application-wide (All Views)
- TODO [ ] **Script loading**: defer non-critical scripts and reduce duplicate includes.
- TODO [ ] **Image optimization**: use thumbnails in lists, lazy-load galleries, and provide modern formats where possible.
- TODO [ ] **Large lists**: virtualize or paginate big tables (files, galleries, notifications).
- TODO [ ] **API timeouts**: add consistent timeout handling for long-running fetches.

#### Static scan (performance)
- **`<script>` tags in views**: 126 lines.
- **`<script>` without `defer` or `async` on same line**: 119 (line-based scan; verify bundled scripts).
- **`loading="lazy"` usage**: 7 lines.

---

## 8) Observability & Monitoring
- TODO [ ] **Client errors captured** (JS errors, failed requests).
- TODO [ ] **Server errors logged** with correlation ID.
- TODO [ ] **Performance metrics** (FCP/LCP/CLS) tracked.
- TODO [ ] **Audit events** on critical actions.
- TODO [ ] **Alert thresholds** configured for failure spikes.

### Application-wide (All Views)
- TODO [ ] **Client error logging**: add Sentry/AppInsights (or equivalent) for JS errors and failed fetches.
- TODO [ ] **Correlation IDs**: ensure server logs include request IDs and return them in error responses.
- TODO [ ] **Audit logs**: log create/edit/delete actions for galleries, users, contracts, and payments.
- TODO [ ] **Alerts**: define thresholds for 4xx/5xx spikes and job failures.

#### Static scan (observability)
- **No client-side error tracking library found** via string scan (Sentry/AppInsights/Datadog).

---

## 9) SEO & Sharing (public-facing views)
- TODO [ ] **Title & meta description** unique and meaningful.
- TODO [ ] **OpenGraph/Twitter tags** for share previews.
- TODO [ ] **Canonical URLs** where applicable.
- TODO [ ] **Robots directives** set properly.

### Application-wide (Public Views)
- TODO [ ] Identify all public pages (landing, packages, gallery share) and add proper meta/OG tags.
- TODO [ ] Ensure noindex on private/authenticated views.

---

## 10) QA & Test Coverage
- TODO [ ] **Unit tests** for key logic.
- TODO [ ] **Integration tests** for data flow and API handling.
- TODO [ ] **E2E tests** for critical user paths.
- TODO [ ] **Cross-browser testing** (Chrome/Firefox/Safari/Edge).
- TODO [ ] **Responsive testing** (mobile/tablet/desktop).
- TODO [ ] **Localization checks** if app supports multiple locales.

### Application-wide (All Views)
- TODO [ ] Define a smoke test plan for staff + client journeys (login, gallery view, booking, invoice).
- TODO [ ] Add E2E coverage for gallery proofing and checkout flows.

---

# View-Specific Checklists

Use the baseline checklist above for every view, then add the relevant items below depending on the view type. This section includes **all current view areas** in `/Views` to keep coverage complete.

## Account
- TODO [ ] Login: rate limiting, lockout messaging, remember-me cookie behavior.
- TODO [ ] Registration: email verification flow, duplicate email checks.
- TODO [ ] Password reset: token expiry handling, success messaging.
- TODO [ ] MFA/2FA: backup codes, device remember list.

## Albums
- TODO [ ] Album creation: cover selection, privacy defaults, empty album state.
- TODO [ ] Sorting: drag/drop persists order, manual overrides stored.
- TODO [ ] Sharing: permissions, expiration, copy link behavior.

## Badges
- TODO [ ] Badge issuance flow tested end-to-end.
- TODO [ ] Badge visibility respects permissions and filters.

## Bookings
- TODO [ ] Booking creation: conflict checks, timezone handling.
- TODO [ ] Calendar view: drag-drop updates and persistence.
- TODO [ ] Status transitions: pending -> confirmed -> completed.

## Clients
- TODO [ ] Client creation: duplicate detection, contact validation.
- TODO [ ] Client detail: all related entities visible (bookings, invoices, galleries).

## ContractTemplates
- TODO [ ] Template editor: placeholders preview, versioning.
- TODO [ ] Template availability: role-based access to create/edit.

## Contracts
- TODO [ ] Contract creation: template selection, auto-fill variables.
- TODO [ ] Signature flow: signer authentication, audit trail.

## Error
- TODO [ ] 404 page: user escape routes, link to home.
- TODO [ ] 500 page: support contact and error ID.

## FileManager
- TODO [ ] Upload: size/type validation, progress, cancel.
- TODO [ ] Folders: rename/delete with confirmation.
- TODO [ ] Bulk actions: select all, undo, throttling.

## Galleries
- TODO [ ] Gallery list: filters, pagination, search.
- TODO [ ] Shared galleries: access keys and expiration.

## Gallery
- TODO [ ] Photo grid: lazy-load images, placeholders.

## Invoices
- TODO [ ] Invoice creation: line items validate, tax/discount calculation.
- TODO [ ] Status flow: draft -> sent -> paid -> void.
- TODO [ ] PDF/export: consistent formatting and permissions.

## Notifications
- TODO [ ] Read/unread state consistent across list and badge.
- TODO [ ] Real-time or polling updates for new notifications.

## Packages
- TODO [ ] Package visibility: public vs. private pricing rules.
- TODO [ ] Package assignment: only eligible clients can see selected packages.

## Permissions
- TODO [ ] Permission changes propagate to roles/users correctly.
- TODO [ ] Permission audit trail tracked.

## Photos
- TODO [ ] Photo upload: size/type validation and preview.
- TODO [ ] Photo metadata: edit/save and visibility.

## PhotoShoots
- TODO [ ] Calendar: timezone handling and overlap checks.
- TODO [ ] Status transitions: scheduled -> completed.

## PrintOrder
- TODO [ ] Checkout flow: totals, taxes, and shipping verified.
- TODO [ ] Order fulfillment: status updates and notifications.

## Proofs
- TODO [ ] Proof selection: favorite/edit flow works with session token.
- TODO [ ] Approval: admin notification and state updates.

## QuestionnaireAssignments
- TODO [ ] Assignment delivery: email/link access works.
- TODO [ ] Responses: saved and versioned.

## QuestionnaireTemplates
- TODO [ ] Template editor: validation and preview.
- TODO [ ] Versioning: changes do not break assigned questionnaires.

## Roles
- TODO [ ] Role creation: name uniqueness.
- TODO [ ] Permission inheritance tested.

## Shared
- TODO [ ] Shared partials: no dependency on view-specific data.
- TODO [ ] Shared layout: top-nav/side-nav consistent.

## Users
- TODO [ ] User management: deactivate/reactivate logic.
- TODO [ ] User profile: data updates validated.

---

# Release Sign-Off (per view)
- TODO [ ] **Product** approves view behavior & copy.
- TODO [ ] **Design** signs off on layout & UX polish.
- TODO [ ] **QA** completes test pass with no blocker defects.
- TODO [ ] **Security** checks completed if view handles PII/payment.
- TODO [ ] **Ops** has monitoring/alerts in place.
