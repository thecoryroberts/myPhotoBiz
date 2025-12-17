# Notification System Documentation

## Overview
The notification system provides a complete solution for creating, managing, and displaying user notifications throughout the application. It includes a dropdown in the topbar, a dedicated notifications page, and helper methods for easy integration.

## Features
- **Real-time notification dropdown** in the topbar
- **Unread count badge** with animation
- **Type-based styling** (Info, Success, Warning, Error, Invoice, PhotoShoot, Client, Album)
- **Mark as read** functionality (individual or bulk)
- **Auto-refresh** every 2 minutes
- **Full notifications page** at `/Notifications`
- **Easy integration** with helper extension methods

## Usage in Controllers

### 1. Basic Usage

First, inject the `INotificationService` and `UserManager` into your controller:

```csharp
private readonly INotificationService _notificationService;
private readonly UserManager<ApplicationUser> _userManager;

public YourController(
    INotificationService notificationService,
    UserManager<ApplicationUser> userManager)
{
    _notificationService = notificationService;
    _userManager = userManager;
}
```

### 2. Using Helper Extension Methods

Add the using statement:
```csharp
using MyPhotoBiz.Extensions;
```

Then use the helper methods:

#### Invoice Notifications
```csharp
// When creating an invoice
var userId = _userManager.GetUserId(User);
await _notificationService.NotifyInvoiceCreated(userId, invoice.InvoiceNumber, invoice.Id);

// When invoice is paid
await _notificationService.NotifyInvoicePaid(userId, invoice.InvoiceNumber, invoice.Id);
```

#### Photo Shoot Notifications
```csharp
// When creating a photo shoot
await _notificationService.NotifyPhotoShootCreated(userId, photoShoot.Title, photoShoot.Id);

// When completed
await _notificationService.NotifyPhotoShootCompleted(userId, photoShoot.Title, photoShoot.Id);
```

#### Client Notifications
```csharp
// When adding a new client
var clientName = $"{client.FirstName} {client.LastName}";
await _notificationService.NotifyNewClient(userId, clientName, client.Id);
```

#### Album Notifications
```csharp
// When creating an album
await _notificationService.NotifyAlbumCreated(userId, album.Name, album.Id);
```

#### General Notifications
```csharp
// Success notification
await _notificationService.NotifySuccess(
    userId,
    "Operation Successful",
    "Your data has been saved successfully.",
    "/link/to/resource" // Optional link
);

// Warning notification
await _notificationService.NotifyWarning(
    userId,
    "Warning",
    "This action may have side effects.",
    null // No link
);

// Error notification
await _notificationService.NotifyError(
    userId,
    "Error Occurred",
    "An error occurred while processing your request."
);

// Info notification
await _notificationService.NotifyInfo(
    userId,
    "Information",
    "Please review the updated terms and conditions."
);
```

### 3. Manual Notification Creation

For custom notifications:

```csharp
await _notificationService.CreateNotificationAsync(new Notification
{
    UserId = userId,
    Title = "Custom Title",
    Message = "Custom message here",
    Type = NotificationType.Info,
    Link = "/custom/link", // Optional
    Icon = "ti-custom-icon" // Optional Tabler icon class
});
```

## Notification Types

| Type | Color | Default Icon | Use Case |
|------|-------|--------------|----------|
| `Info` | Blue | ti-info-circle | General information |
| `Success` | Green | ti-circle-check | Success messages |
| `Warning` | Orange | ti-alert-triangle | Warnings |
| `Error` | Red | ti-alert-circle | Errors |
| `Invoice` | Primary | ti-file-invoice | Invoice-related |
| `PhotoShoot` | Purple | ti-camera | Photo shoot events |
| `Client` | Teal | ti-users | Client-related |
| `Album` | Indigo | ti-photo | Album-related |

## Integration Examples

### Example 1: Invoice Creation
```csharp
[HttpPost]
public async Task<IActionResult> Create(InvoiceViewModel vm)
{
    // ... create invoice logic ...

    var userId = _userManager.GetUserId(User);
    await _notificationService.NotifyInvoiceCreated(
        userId,
        invoice.InvoiceNumber,
        invoice.Id
    );

    return RedirectToAction(nameof(Index));
}
```

### Example 2: Photo Shoot Completion
```csharp
[HttpPost]
public async Task<IActionResult> MarkAsCompleted(int id)
{
    var photoShoot = await _photoShootService.GetByIdAsync(id);
    photoShoot.Status = PhotoShootStatus.Completed;
    await _photoShootService.UpdateAsync(photoShoot);

    var userId = _userManager.GetUserId(User);
    await _notificationService.NotifyPhotoShootCompleted(
        userId,
        photoShoot.Title,
        photoShoot.Id
    );

    return RedirectToAction(nameof(Details), new { id });
}
```

### Example 3: Multiple Users
```csharp
// Notify all admins
var adminUsers = await _userManager.GetUsersInRoleAsync("Admin");
foreach (var admin in adminUsers)
{
    await _notificationService.NotifyWarning(
        admin.Id,
        "System Alert",
        "Low disk space detected on the server."
    );
}
```

## Frontend Integration

The notification system automatically works in the topbar. No additional JavaScript is needed.

### Accessing Notifications via JavaScript
```javascript
// Load notifications
await NotificationSystem.loadNotifications();

// Update unread count
await NotificationSystem.updateUnreadCount();

// Mark notification as read
await NotificationSystem.markAsRead(notificationId, linkUrl);

// Mark all as read
await NotificationSystem.markAllAsRead();
```

## API Endpoints

All endpoints require authentication:

- `GET /api/notifications` - Get user notifications (limit parameter optional)
- `GET /api/notifications/unread-count` - Get unread count
- `POST /api/notifications/{id}/read` - Mark notification as read
- `POST /api/notifications/mark-all-read` - Mark all as read
- `DELETE /api/notifications/{id}` - Delete notification
- `POST /api/notifications/create-test` - Create test notifications (development)

## Testing

Visit `/Notifications` and click "Create Test Notifications" to generate sample notifications and see the system in action.

## Cleanup

Old read notifications are automatically cleaned up using:
```csharp
await _notificationService.DeleteOldNotificationsAsync(30); // Delete notifications older than 30 days
```

Add this to a scheduled background job for automatic cleanup.

## Customization

### Adding New Notification Types

1. Update the `NotificationType` enum in `Models/Notification.cs`
2. Add color mapping in `wwwroot/css/notifications.css`
3. Add icon mapping in `wwwroot/js/notifications.js` (getNotificationStyle function)
4. Create helper extension method in `Extensions/NotificationExtensions.cs`

## Troubleshooting

**Notifications not showing up:**
- Check that the user is logged in
- Verify the notification was created in the database
- Check browser console for JavaScript errors
- Ensure `notifications.js` and `notifications.css` are loaded

**Badge not updating:**
- Clear browser cache
- Check network tab for API calls to `/api/notifications/unread-count`

**Styling issues:**
- Verify `notifications.css` is included in `_HeadCSS.cshtml`
- Check for CSS conflicts with existing styles
