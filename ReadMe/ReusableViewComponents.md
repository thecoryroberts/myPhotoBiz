# Reusable View Components

## Overview

This document describes the reusable Razor partial views available for building consistent UI across the application.

---

## _StatusBadge

**Location:** `Views/Shared/Partials/_StatusBadge.cshtml`

A universal status badge component that automatically applies appropriate colors and icons based on the status value.

### Usage

```html
<partial name="Partials/_StatusBadge" model="@(("Completed", "photoshoot"))" />
```

### Parameters

The model is a tuple of `(string Status, string Type)`:

- **Status**: The status text to display (e.g., "Completed", "Pending", "Cancelled")
- **Type**: The entity type for context (e.g., "photoshoot", "invoice", "booking", "contract", "gallery")

### Automatic Color Mapping

| Status | Color Class |
|--------|-------------|
| Completed, Paid, Signed, Approved, Active, Delivered | `text-bg-success` (green) |
| Scheduled, Pending, Sent, Pending Signature | `text-bg-primary` (blue) |
| InProgress, In Progress, Processing, Partial | `text-bg-warning` (yellow) |
| Cancelled, Overdue, Rejected, Failed, Expired | `text-bg-danger` (red) |
| Draft, New | `text-bg-info` (cyan) |
| Other | `text-bg-secondary` (gray) |

### Automatic Icon Mapping

| Status | Icon |
|--------|------|
| Completed, Paid, Signed, Approved | `ti ti-check` |
| Scheduled | `ti ti-calendar` |
| Pending, Pending Signature | `ti ti-clock` |
| InProgress, In Progress | `ti ti-loader` |
| Cancelled | `ti ti-x` |
| Overdue | `ti ti-alert-triangle` |
| Active | `ti ti-circle-check` |
| Expired | `ti ti-clock-off` |

### Examples

```html
@* In a table row *@
<td>
    <partial name="Partials/_StatusBadge" model="@((shoot.Status.ToString(), "photoshoot"))" />
</td>

@* In a card *@
<div class="card-header">
    <partial name="Partials/_StatusBadge" model="@((invoice.Status.ToString(), "invoice"))" />
</div>
```

---

## _ActionButtons

**Location:** `Views/Shared/Partials/_ActionButtons.cshtml`

Reusable CRUD action buttons (View, Edit, Delete) with support for custom additional buttons.

### Usage

```html
@{
    var actionConfig = ActionButtonsModel.ForEntity(item.Id, "Clients", item.Name);
}
<partial name="Partials/_ActionButtons" model="@actionConfig" />
```

### ActionButtonsModel Properties

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| Id | int | required | Entity ID for route |
| Controller | string | required | Controller name |
| ShowView | bool | true | Show view/details button |
| ShowEdit | bool | true | Show edit button |
| ShowDelete | bool | true | Show delete button |
| DeleteDataAttribute | string? | null | Data attribute name for delete |
| DeleteDataValue | string? | null | Value for delete data attribute |
| ExtraButtons | List<ExtraButton>? | null | Additional custom buttons |

### Factory Methods

```csharp
// Standard CRUD buttons
var config = ActionButtonsModel.ForEntity(id, "Clients", "John Doe");

// View only (no edit/delete)
var config = ActionButtonsModel.ViewOnly(id, "Clients");
```

### Custom Configuration

```csharp
var config = new ActionButtonsModel
{
    Id = item.Id,
    Controller = "PhotoShoots",
    ShowView = true,
    ShowEdit = true,
    ShowDelete = false,  // Hide delete
    ExtraButtons = new List<ExtraButton>
    {
        new ExtraButton
        {
            Action = "Calendar",
            Icon = "ti ti-calendar",
            Title = "View Calendar",
            Color = "info"
        }
    }
};
```

### ExtraButton Properties

| Property | Type | Description |
|----------|------|-------------|
| Action | string | Action name |
| Controller | string? | Controller name (defaults to parent) |
| Icon | string | Icon class (e.g., "ti ti-calendar") |
| Title | string | Tooltip text |
| Color | string? | Button color (defaults to "light") |

---

## _DeleteModal

**Location:** `Views/Shared/Partials/_DeleteModal.cshtml`

A reusable delete confirmation modal with AJAX support and SweetAlert2 integration.

### Usage

1. Include the partial once per page:

```html
<partial name="Partials/_DeleteModal" model="@("Photo Shoot")" />
```

2. Add onclick handler to delete buttons:

```html
<button onclick="showDeleteModal('/api/photoshoots/@item.Id', '@item.Title')">
    <i class="ti ti-trash"></i>
</button>
```

### Parameters

The model is a string representing the entity type name (e.g., "Photo Shoot", "Client", "Invoice").

### JavaScript API

#### showDeleteModal(url, itemName, callback)

Shows the delete confirmation modal.

| Parameter | Type | Description |
|-----------|------|-------------|
| url | string | DELETE endpoint URL |
| itemName | string | Name of item being deleted |
| callback | function? | Optional callback after deletion |

```javascript
// Basic usage
showDeleteModal('/api/clients/123', 'John Doe');

// With callback
showDeleteModal('/api/clients/123', 'John Doe', function(success, name) {
    if (success) {
        // Remove row from table
        document.querySelector(`tr[data-id="123"]`).remove();
    }
});
```

### Features

- Automatic loading state on confirm button
- Integrates with SweetAlert2 for toast notifications
- Falls back to browser alert if SweetAlert2 not available
- Default behavior: reloads page after successful delete

---

## Complete Example

Here's a complete example using all components in a table:

```html
@model IEnumerable<PhotoShoot>

<table class="table">
    <thead>
        <tr>
            <th>Title</th>
            <th>Status</th>
            <th>Actions</th>
        </tr>
    </thead>
    <tbody>
        @foreach (var shoot in Model)
        {
            <tr data-id="@shoot.Id">
                <td>@shoot.Title</td>
                <td>
                    <partial name="Partials/_StatusBadge"
                             model="@((shoot.Status.ToString(), "photoshoot"))" />
                </td>
                <td>
                    @{
                        var actions = new ActionButtonsModel
                        {
                            Id = shoot.Id,
                            Controller = "PhotoShoots",
                            DeleteDataValue = shoot.Title
                        };
                    }
                    <partial name="Partials/_ActionButtons" model="@actions" />
                </td>
            </tr>
        }
    </tbody>
</table>

@* Include delete modal once *@
<partial name="Partials/_DeleteModal" model="@("Photo Shoot")" />

@section scripts {
    <script>
        // Override delete button clicks to use modal
        document.querySelectorAll('.delete-btn').forEach(btn => {
            btn.addEventListener('click', function(e) {
                e.preventDefault();
                var id = this.closest('tr').dataset.id;
                var name = this.dataset.itemName;
                showDeleteModal('/api/photoshoots/' + id, name);
            });
        });
    </script>
}
```
