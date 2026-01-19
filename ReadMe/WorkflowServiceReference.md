# WorkflowService Reference

## Overview

The `WorkflowService` orchestrates multi-step business workflows, simplifying complex operations into single method calls. It handles client creation, booking management, photoshoot lifecycle, invoicing, and gallery creation.

---

## Interface: IWorkflowService

Location: `Services/IWorkflowService.cs`

---

## Client Workflow

### CreateClientWithResourcesAsync

Creates a client with all associated resources (user account, folder, profile).

```csharp
Task<WorkflowResult<ClientProfile>> CreateClientWithResourcesAsync(
    string firstName,
    string lastName,
    string email,
    string? phone = null,
    string? address = null);
```

**What it does:**
1. Checks if user already exists
2. Generates secure temporary password
3. Creates user account with Identity
4. Adds user to Client role
5. Creates ClientProfile
6. Creates client folder in File Manager
7. Sends welcome notification

**Example:**
```csharp
var result = await _workflowService.CreateClientWithResourcesAsync(
    "John",
    "Doe",
    "john.doe@email.com",
    "+1234567890",
    "123 Main St");

if (result.Success)
{
    var client = result.Data;
    // Client created successfully
}
else
{
    // Handle error: result.ErrorMessage
}
```

---

## Booking Workflow

### ApproveBookingAsync

Approves a booking request and creates associated photoshoot.

```csharp
Task<WorkflowResult<PhotoShoot>> ApproveBookingAsync(int bookingId, string approvedBy);
```

**What it does:**
1. Validates booking exists and is pending
2. Updates booking status to Confirmed
3. Creates PhotoShoot from booking details
4. Links booking to photoshoot
5. Sends approval notification to client

**Example:**
```csharp
var result = await _workflowService.ApproveBookingAsync(bookingId, "admin@company.com");

if (result.Success)
{
    var photoshoot = result.Data;
    // Photoshoot created and linked to booking
}
```

### RejectBookingAsync

Rejects a booking request with a reason.

```csharp
Task<WorkflowResult<BookingRequest>> RejectBookingAsync(int bookingId, string reason, string rejectedBy);
```

**What it does:**
1. Updates booking status to Cancelled
2. Records rejection reason in AdminNotes
3. Sends notification to client

---

## Photoshoot Workflow

### CompletePhotoshootAsync

Marks a photoshoot as complete and optionally creates album/gallery.

```csharp
Task<WorkflowResult<Album>> CompletePhotoshootAsync(int photoshootId, bool createGallery = true);
```

**What it does:**
1. Updates photoshoot status to Completed
2. Creates Album if none exists
3. Optionally creates Gallery from album
4. Sends completion notification to client

**Example:**
```csharp
// Complete photoshoot and auto-create gallery
var result = await _workflowService.CompletePhotoshootAsync(photoshootId, createGallery: true);

// Complete photoshoot without gallery
var result = await _workflowService.CompletePhotoshootAsync(photoshootId, createGallery: false);
```

### DeliverPhotoshootAsync

Marks a photoshoot as delivered after final payment.

```csharp
Task<WorkflowResult<PhotoShoot>> DeliverPhotoshootAsync(int photoshootId);
```

**What it does:**
1. Updates photoshoot status to Completed (delivered state)
2. Sends delivery notification to client

---

## Invoice Workflow

### GenerateInvoiceFromPhotoshootAsync

Generates an invoice from a completed photoshoot.

```csharp
Task<WorkflowResult<Invoice>> GenerateInvoiceFromPhotoshootAsync(int photoshootId);
```

**What it does:**
1. Validates photoshoot exists
2. Checks if invoice already exists (prevents duplicates)
3. Creates invoice with photoshoot price
4. Generates unique invoice number
5. Sends notification to client

**Example:**
```csharp
var result = await _workflowService.GenerateInvoiceFromPhotoshootAsync(photoshootId);

if (result.Success)
{
    var invoice = result.Data;
    // Invoice number: invoice.InvoiceNumber
}
```

### MarkInvoicePaidAsync

Marks an invoice as paid and triggers delivery workflow if fully paid.

```csharp
Task<WorkflowResult<Invoice>> MarkInvoicePaidAsync(int invoiceId, string paymentMethod, string? transactionId = null);
```

**What it does:**
1. Updates invoice status to Paid
2. Records payment date
3. Checks if all invoices for photoshoot are paid
4. Auto-triggers delivery workflow if all paid
5. Sends payment confirmation notification

---

## Gallery Workflow

### CreateGalleryFromAlbumAsync

Creates a gallery from an album with sharing settings.

```csharp
Task<WorkflowResult<Gallery>> CreateGalleryFromAlbumAsync(
    int albumId,
    string name,
    DateTime expiryDate,
    bool notifyClient = true);
```

**What it does:**
1. Creates Gallery with specified name and expiry
2. Links album to gallery
3. Optionally notifies client

**Example:**
```csharp
var result = await _workflowService.CreateGalleryFromAlbumAsync(
    albumId,
    "Wedding Gallery",
    DateTime.Now.AddDays(30),
    notifyClient: true);
```

---

## WorkflowResult<T>

All workflow methods return a `WorkflowResult<T>` wrapper.

### Properties

```csharp
public class WorkflowResult<T>
{
    public bool Success { get; set; }
    public T? Data { get; set; }
    public string? ErrorMessage { get; set; }
    public List<string> Warnings { get; set; }
}
```

### Static Factory Methods

```csharp
// Create success result
WorkflowResult<T>.Succeeded(data, warnings);

// Create failure result
WorkflowResult<T>.Failed("Error message");
```

### Usage Pattern

```csharp
var result = await _workflowService.SomeMethodAsync();

if (result.Success)
{
    var data = result.Data;

    // Check for warnings
    if (result.Warnings.Any())
    {
        foreach (var warning in result.Warnings)
        {
            _logger.LogWarning(warning);
        }
    }
}
else
{
    // Handle error
    ModelState.AddModelError("", result.ErrorMessage);
}
```

---

## Dependency Injection

Register in `Program.cs`:

```csharp
builder.Services.AddScoped<IWorkflowService, WorkflowService>();
```

Inject in controllers:

```csharp
public class BookingsController : Controller
{
    private readonly IWorkflowService _workflowService;

    public BookingsController(IWorkflowService workflowService)
    {
        _workflowService = workflowService;
    }
}
```
