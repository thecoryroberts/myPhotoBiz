# myPhotoBiz - Photography Business Management Platform

A comprehensive ASP.NET Core 8.0 MVC application designed for professional photographers to manage their entire business workflow, from client bookings to gallery delivery and invoicing.

## Overview

myPhotoBiz is a full-featured photography business management system that streamlines the workflow for professional photographers. Built with ASP.NET Core 8.0 MVC, Entity Framework Core, and Bootstrap 5, it provides an all-in-one solution for managing clients, photo shoots, galleries, proofing sessions, contracts, and invoicing.

### Key Features

- **Client Management** - Complete client profiles with contact info, photo shoots, and relationship tracking
- **Photo Shoot Scheduling** - Calendar integration with shoot planning, timing, and location management
- **Gallery System** - Secure client galleries with access controls, expiry dates, and download permissions
- **Photo Proofing** - Client proofing sessions with favorites, edit requests, and print ordering
- **Contract Management** - Digital contract creation, signing, and tracking with badge rewards
- **Invoicing** - Professional invoice generation with payment tracking and aging reports
- **Booking System** - Client booking requests with approval workflow
- **Badge System** - Gamification for client engagement (First Shoot, Contract Signed, etc.)
- **Global Search** - Fast search across clients, shoots, invoices, and galleries (Ctrl+K)
- **Role-based Access** - Admin, Photographer, and Client roles with appropriate permissions

## Technology Stack

### Backend
- **Framework**: ASP.NET Core 8.0 MVC
- **Database**: SQLite (development), Entity Framework Core
- **Authentication**: ASP.NET Core Identity
- **Language**: C# 12

### Frontend
- **UI Framework**: Bootstrap 5.3
- **Admin Template**: Custom theme based on modern admin patterns
- **JavaScript**: Vanilla JavaScript with modern ES6+ features
- **Icons**: Tabler Icons
- **Components**:
  - SweetAlert2 for alerts and confirmations
  - Flatpickr for date/time pickers
  - Custom lightbox for photo viewing

### Features & Utilities
- **File Upload**: Image processing with thumbnail generation
- **PDF Generation**: For contracts and invoices
- **Email Integration**: SMTP email sending
- **Session Management**: Gallery session tracking
- **Bulk Operations**: ZIP download for multiple photos
- **Real-time Notifications**: Toast notifications system

## 🚀 Getting Started

### Prerequisites
- .NET 8.0 SDK or later
- SQLite (included)
- A code editor (Visual Studio, VS Code, or Rider)

### Installation

1. **Clone the repository**
   ```bash
   cd /path/to/myPhotoBiz/myPhotoBiz
   ```

2. **Restore dependencies**
   ```bash
   dotnet restore
   ```

3. **Update database**
   ```bash
   dotnet ef database update
   ```

4. **Run the application**
   ```bash
   dotnet run
   ```

5. **Access the application**
   - Navigate to `https://localhost:5001` or `http://localhost:5000`
   - Default admin credentials are seeded in `SeedData.cs`

### Default Accounts

The application seeds default accounts for testing:
- **Admin**: admin@myphotobiz.com / Admin@123
- **Photographer**: photographer@myphotobiz.com / Photo@123
- **Client**: client@myphotobiz.com / Client@123

## 📁 Project Structure

```
myPhotoBiz/
├── Controllers/         # MVC Controllers
├── Models/              # Data models and entities
├── ViewModels/          # View-specific data transfer objects
├── Views/               # Razor views
├── Services/            # Business logic services
├── Data/                # DbContext and migrations
├── Helpers/             # Utility classes
├── Enums/               # Enumeration types
├── wwwroot/             # Static files (CSS, JS, images)
│   ├── css/
│   ├── js/
│   ├── images/
│   └── uploads/         # User-uploaded files
└── Areas/Identity/      # Identity pages and customization
```

## Core Workflows

### For Photographers/Admins

1. **Client Onboarding**
   - Create client profile
   - Send booking request
   - Create and send contract
   - Schedule photo shoot

2. **Photo Shoot Management**
   - Schedule shoots on calendar
   - Track shoot status (Scheduled → In Progress → Completed)
   - Upload photos to albums
   - Organize into galleries

3. **Gallery & Proofing**
   - Create client gallery with expiry date
   - Set access permissions (view, download, proof)
   - Review client selections and edit requests
   - Deliver final photos via ZIP download

4. **Billing & Invoicing**
   - Create invoices with line items
   - Track payment status
   - Monitor overdue invoices with aging reports
   - Send payment reminders

### For Clients

1. **Gallery Access**
   - Receive gallery link from photographer
   - Browse photos with lightbox navigation
   - Mark favorites with heart icon
   - Request edits on specific photos
   - Download selected photos (if permitted)

2. **Proofing Session**
   - Review photos from shoot
   - Select favorites for final delivery
   - Add notes or edit requests
   - Submit proof selections

## ✅ Recently Completed Improvements

### Critical (Production Blockers) - All Completed ✅
- ✅ **Form labels** - All PhotoShoots Create/Edit views have proper labels
- ✅ **Image error fallbacks** - Gallery thumbnails fallback to full image or placeholder
- ✅ **Form loading states** - All CRUD operations have loading indicators (form-loading.js)
- ✅ **Lightbox navigation** - Full keyboard support (arrows, space, E, D, escape)
- ✅ **Global search** - Ctrl+K command palette searches across all entities
- ✅ **Keyboard shortcuts** - Complete gallery proofing workflow shortcuts

### High Priority - All Completed ✅
- ✅ **Gallery pagination** - Load more button with infinite scroll for 100+ photos
- ✅ **Validation feedback** - Standardized SweetAlert2 toasts and confirmations
- ✅ **Skeleton loading** - Image placeholders while photos load
- ✅ **Dashboard alerts** - Actionable inline buttons for overdue invoices, pending contracts, bookings
- ✅ **Sidebar persistence** - localStorage remembers expanded sections (sidebar-state.js)
- ✅ **Invoice aging** - Dashboard breakdown showing 1-30, 31-60, 61-90, 90+ day aging
- ✅ **Flatpickr everywhere** - Consistent date/time pickers across all forms
- ✅ **ZIP bulk download** - Server-side ZIP generation for selected photos

## TODO List

### Medium Priority - Usability Improvements

#### Navigation & Information
- [ ] **Empty states with CTAs** - Add friendly empty states to all listing pages
  - Impact: Empty tables look broken, confuse new users
  - Solution: Illustration + "Create your first {entity}" button

- [ ] **Breadcrumb navigation** - Replace `javascript:void(0)` with actual links
  - Impact: Users cannot navigate via breadcrumbs
  - Solution: Proper asp-action routes in breadcrumb partial

- [ ] **Gallery expiry warning** - Client-facing banner
  - Impact: Clients surprised when access expires
  - Solution: Prominent "Access expires in X days" banner

#### Form & Field Enhancements
- [ ] **Real-time field validation** - Blur-event validation for email, dates
  - Impact: Errors only shown after submit wastes user time
  - Solution: Client-side validation with immediate feedback

- [ ] **Mobile-responsive gallery actions** - Stack buttons vertically on mobile
  - Impact: Action buttons overflow on small screens
  - Solution: CSS media queries for vertical stacking

#### Photography Features
- [ ] **Comparison mode for proofing** - Side-by-side photo view
  - Impact: Clients cannot compare similar shots
  - Solution: Two-up lightbox mode with synchronized zoom

- [ ] **Photo metadata display** - Show EXIF, dimensions, file size
  - Impact: Photographers lack context about images
  - Solution: Info panel in lightbox with metadata

#### Dashboard & Analytics
- [ ] **Photographer utilization metrics** - Booking capacity widget
  - Impact: Capacity planning invisible, overbooking risk
  - Solution: Calendar heatmap showing booked vs. available days

#### Accessibility
- [ ] **ARIA landmarks and roles** - Semantic HTML improvements
  - Impact: Screen reader users cannot navigate efficiently
  - Solution: Add `<nav>`, `<main>`, proper ARIA attributes

- [ ] **Print styling for invoices** - @media print CSS
  - Impact: Browser print looks broken
  - Solution: Print-optimized CSS or rely on PDF generation

### Nice-to-Have - Polish & Enhancement

#### Photo Management
- [ ] **Star ratings (1-5)** - More granular selection than favorites
  - Impact: Favorites alone insufficient for detailed culling
  - Solution: 5-star rating UI with filtering

- [ ] **Color labels** - Industry-standard Red/Yellow/Green/Blue/Purple
  - Impact: Professional workflows expect color coding
  - Solution: Color picker with keyboard shortcuts (1-5)

- [ ] **Photo metadata display** - EXIF data, dimensions, file size
  - Impact: Photographers need technical details
  - Solution: Expandable metadata panel in lightbox

#### UI Improvements
- [ ] **"Today's Shoots" sidebar** - Quick access with badge count
  - Impact: Common task requires dashboard navigation
  - Solution: Pinned sidebar item with notification badge

- [ ] **Search highlighting** - Highlight matching terms in results
  - Impact: Users don't see why items matched
  - Solution: Bold/highlight matched query terms

- [ ] **Relative date display** - "2 days ago" option
  - Impact: Full dates harder to scan quickly
  - Solution: Toggle between relative and absolute dates

#### Architecture & Performance
- [x] **Consolidate duplicate menus** - Clarify Galleries/Albums/Photos hierarchy
  - Impact: Conceptual confusion about photo organization
  - Status: ✅ Completed - Unified into single "Media Library" menu with clear workflow hierarchy

- [ ] **Dashboard caching** - Redis or in-memory caching
  - Impact: 14+ queries per dashboard load is slow
  - Solution: Cache dashboard metrics for 5-10 minutes

- [ ] **Soft delete with recovery** - 30-day undo for deletions
  - Impact: Accidental deletes are unrecoverable
  - Solution: IsDeleted flag with cleanup job

- [ ] **Gallery theme customization** - Brand colors and logos
  - Impact: Generic white-label appearance
  - Solution: Gallery-level theme settings

- [ ] **Batch operations UI** - Multi-select with bulk actions
  - Impact: One-by-one changes are tedious
  - Solution: Checkbox select with bulk status/delete

## Security Features

- **Authentication** - ASP.NET Core Identity with role-based authorization
- **Password Policy** - Enforced requirements (uppercase, lowercase, digit, special character)
- **CSRF Protection** - Anti-forgery tokens on all forms
- **Input Validation** - Server-side validation with ModelState
- **Path Traversal Protection** - Validated file paths in download operations
- **Access Control** - Gallery-level permissions (CanView, CanDownload, CanProof)
- **Session Tracking** - Gallery access sessions with expiry dates
- **SQL Injection Protection** - Entity Framework parameterized queries

## Keyboard Shortcuts

### Global
- `Ctrl+K` / `Cmd+K` - Open global search command palette

### Gallery Proofing (in lightbox)
- `←` / `→` - Navigate previous/next photo
- `Space` / `F` - Toggle favorite
- `E` - Request edit
- `D` - Download current photo
- `Esc` - Close lightbox

## Database Schema

### Core Entities
- **ApplicationUser** - Extended Identity user with photographer/client flags
- **ClientProfile** - Client information and relationships
- **PhotoShoot** - Scheduled photo sessions
- **Album** - Photo collections within shoots
- **Photo** - Individual photo files with metadata
- **Gallery** - Client-accessible photo collections
- **GalleryAccess** - Permission mapping for client gallery access
- **GallerySession** - Tracking gallery views and interactions
- **Contract** - Digital contracts with signatures
- **Invoice** - Billing and payment tracking
- **BookingRequest** - Client booking inquiries
- **Badge** - Achievement system for client engagement
- **Proof** - Client photo selections and edit requests

## Contributing

This is a private photography business application. For feature requests or bug reports, please contact the development team.

## License

Proprietary - All rights reserved

## Support

For technical support or questions:
- Check the detailed production readiness review in `/ReadMe/README.md`
- Review inline code comments and XML documentation
- Contact the development team

## 🔄 Recent Updates

### January 2026
- ✅ Fixed gallery thumbnails to fallback properly when ThumbnailPath is null
- ✅ Completed all 6 critical priority items (production blockers)
- ✅ Completed all 8 high priority items (UX improvements)
- ✅ Added ZIP bulk download for gallery photos
- ✅ Implemented sidebar state persistence
- ✅ Added global search with Ctrl+K shortcut
- ✅ Standardized date pickers with Flatpickr
- ✅ Enhanced dashboard alerts with actionable buttons

---

**Built with ❤️ for professional photographers**
