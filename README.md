# myPhotoBiz

myPhotoBiz is an ASP.NET Core MVC application for running a photography business. It centralizes client intake, bookings, photo shoots, galleries, proofing, print orders, invoicing, contracts, and file delivery so photographers can manage a studio from one dashboard.

## What the app does

- **Client management**: client profiles, badges, permissions, and activity history.
- **Bookings & photo shoots**: accept booking requests, schedule shoots, and track status.
- **Albums & photos**: upload images, generate thumbnails, and organize albums by shoot.
- **Galleries & proofing**: build galleries from albums, grant access, enable public links, collect proof selections, and track sessions.
- **Print orders & invoices**: manage pricing, orders, invoice line items, and payment status.
- **Contracts & releases**: template contracts and store model releases.
- **Notifications & workflows**: send in-app notifications and orchestrate multi-step workflows.
- **File manager**: create client folders, validate paths, and secure downloads.

## Architecture overview

```
Controllers  ->  Services  ->  Data (EF Core)
Views/Razor  ->  ViewModels ->  Models
Background tasks (image processing, queue)
```

- **Controllers** handle HTTP requests and delegate business logic.
- **Services** contain the domain workflows (clients, shoots, galleries, invoices, packages, badges, permissions, and notifications).
- **Data** uses Entity Framework Core with SQLite by default.
- **Background tasks** run image processing (thumbnail generation, watermarking) outside the request pipeline.

## Tech stack

- **ASP.NET Core MVC + Razor Pages**
- **Entity Framework Core (SQLite)**
- **ASP.NET Core Identity** for authentication/authorization
- **Gulp + Sass** for front-end assets
- **Bootstrap 5** and supporting JS libraries

## Getting started

### Prerequisites

- .NET SDK **8.0.122** (see `global.json`)
- Node.js + npm (for front-end assets)

### Run locally

```bash
# Restore .NET dependencies

dotnet restore

# Run database migrations + seed default roles/users on startup

dotnet run
```

The application uses SQLite (`app.db`) by default. On first run it will:

- Apply EF Core migrations automatically.
- Create default roles (Admin, Photographer, Client).
- Create a default admin user (see the `Program.cs` seed values).

If you want to override seed credentials, update `appsettings.json` under `Seed:PrimaryAdmin` (or use environment variables with the same keys).

### Front-end assets

```bash
npm install
npm run dev
```

## Key areas in the codebase

| Area | Location | Purpose |
| --- | --- | --- |
| Controllers | `Controllers/` | HTTP endpoints for admin + client flows |
| Services | `Services/` | Business logic + orchestration |
| Models | `Models/` | EF Core entities |
| ViewModels | `ViewModels/` | UI-focused shapes for views |
| DTOs | `DTOs/` | Request/response payloads |
| Data | `Data/` | DbContext + seeding helpers |
| Helpers/Extensions | `Helpers/`, `Extensions/` | Shared utilities |
| Views | `Views/` | Razor views & partials |
| Assets | `wwwroot/` | Built static files |

## Common workflows

### Create a client and schedule a shoot

1. Create a client profile (client folder + notifications are handled automatically).
2. Accept or create a booking request.
3. Convert the booking into a scheduled photo shoot.
4. Upload photos to an album and link the album to galleries.

### Deliver a gallery

1. Create a gallery and attach albums.
2. Grant access to specific clients or enable a public token link.
3. Collect proof selections and optionally allow downloads/print orders.

## Testing & quality checks

```bash
# Run the .NET build

dotnet build
```

## Related documentation

The `ReadMe/` directory includes deeper reports and implementation notes (gallery workflows, seeding guidance, optimization reports, etc.).

---

If you’re new to the project, start with the dashboard and gallery flows—they touch most of the core services.
