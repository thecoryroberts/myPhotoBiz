# DashboardService Optimizations

## Overview

The `DashboardService` was refactored to improve code reusability and performance. This document describes the optimizations made.

---

## Changes Made

### 1. Extracted Revenue Calculation Helpers

Revenue calculations that were duplicated are now centralized in reusable private methods.

#### GetRevenueForMonthAsync

Calculates revenue for a specific month/year.

```csharp
private async Task<decimal> GetRevenueForMonthAsync(int month, int year)
{
    return await _context.Invoices
        .Where(i => i.InvoiceDate.Month == month &&
                    i.InvoiceDate.Year == year &&
                    i.Status == InvoiceStatus.Paid)
        .SumAsync(i => i.Amount + i.Tax);
}
```

**Before:** This logic was repeated 3 times in `GetDashboardDataAsync()` for current month, last month, etc.

#### GetRevenueSinceAsync

Calculates revenue from a start date.

```csharp
private async Task<decimal> GetRevenueSinceAsync(DateTime startDate)
{
    return await _context.Invoices
        .Where(i => i.InvoiceDate >= startDate && i.Status == InvoiceStatus.Paid)
        .SumAsync(i => i.Amount + i.Tax);
}
```

#### GetMonthlyRevenueDataAsync

Gets monthly revenue data for the last N months (for charts).

```csharp
private async Task<Dictionary<string, decimal>> GetMonthlyRevenueDataAsync(int months = 12)
```

**Optimization:** Uses a single database query with `GroupBy` instead of 12 separate queries.

---

### 2. Extracted Status Counts Helper

#### GetPhotoshootStatusCountsAsync

Gets photoshoot status counts in a single query.

```csharp
private async Task<Dictionary<string, int>> GetPhotoshootStatusCountsAsync()
{
    var statusCounts = await _context.PhotoShoots
        .Where(p => !p.IsDeleted)
        .GroupBy(p => p.Status)
        .Select(g => new { Status = g.Key, Count = g.Count() })
        .ToListAsync();

    return new Dictionary<string, int>
    {
        ["Scheduled"] = statusCounts.FirstOrDefault(s => s.Status == PhotoShootStatus.Scheduled)?.Count ?? 0,
        ["Completed"] = statusCounts.FirstOrDefault(s => s.Status == PhotoShootStatus.Completed)?.Count ?? 0,
        // ...
    };
}
```

**Before:** 4 separate `CountAsync` queries for each status.

---

### 3. Extracted ViewModel Mapping

#### MapToViewModel

Converts PhotoShoot to PhotoShootViewModel.

```csharp
private static PhotoShootViewModel MapToViewModel(PhotoShoot ps)
{
    return new PhotoShootViewModel
    {
        Id = ps.Id,
        Title = ps.Title,
        // ...
    };
}
```

**Before:** This mapping was written inline twice in `GetDashboardDataAsync()`.

---

### 4. Parallel Query Execution

The main `GetDashboardDataAsync()` method now uses `Task.WhenAll()` to run independent queries in parallel.

```csharp
// Run independent queries in parallel
var totalClientsTask = GetClientsCountAsync();
var pendingPhotoShootsTask = GetPendingPhotoShootsCountAsync();
var completedPhotoShootsTask = _context.PhotoShoots.CountAsync(p => p.Status == PhotoShootStatus.Completed);
// ... more tasks

// Await all tasks
await Task.WhenAll(
    totalClientsTask, pendingPhotoShootsTask, completedPhotoShootsTask,
    // ... all tasks
);

// Get results
var currentMonthRevenue = await currentMonthRevenueTask;
```

**Performance Impact:** Queries that don't depend on each other now run concurrently, reducing total execution time.

---

### 5. Centralized Cache Configuration

Uses `AppConstants` for cache duration instead of magic numbers.

```csharp
// Before
private const int CacheDurationMinutes = 5;
_cache.Set(cacheKey, dashboardData, TimeSpan.FromMinutes(CacheDurationMinutes));

// After
_cache.Set(DashboardCacheKey, dashboardData,
    TimeSpan.FromMinutes(AppConstants.Cache.DashboardCacheMinutes));
```

---

### 6. Optimized LINQ Queries

Changed from `.Select().ToList().Sum()` pattern to direct `.SumAsync()`.

```csharp
// Before (inefficient - fetches all values to memory)
var values = await _context.Invoices
    .Where(...)
    .Select(i => i.Amount + i.Tax)
    .ToListAsync();
var total = values.Sum();

// After (efficient - computed in database)
var total = await _context.Invoices
    .Where(...)
    .SumAsync(i => i.Amount + i.Tax);
```

---

## Performance Comparison

| Metric | Before | After |
|--------|--------|-------|
| Database queries | ~20 sequential | ~20 parallel |
| Revenue queries | 4 separate | 3 (reused) |
| Status queries | 4 separate | 1 grouped |
| Lines of code | ~195 | ~100 |
| Reusable methods | 0 | 4 |

---

## Usage

The service interface remains unchanged:

```csharp
public interface IDashboardService
{
    Task<DashboardViewModel> GetDashboardDataAsync();
    void ClearDashboardCache();
    // ... other methods
}
```

Controllers continue to use it the same way:

```csharp
var dashboardData = await _dashboardService.GetDashboardDataAsync();
```

---

## Cache Behavior

- **Sliding expiration:** 5 minutes
- **Absolute expiration:** 10 minutes
- **Clear cache:** Call `ClearDashboardCache()` after data changes

```csharp
// After creating/updating records that affect dashboard
_dashboardService.ClearDashboardCache();
```
