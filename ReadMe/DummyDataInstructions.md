# Dummy Data Seeding Instructions

A new seeder class `DummyDataSeeder` has been created in `Data/DummyDataSeeder.cs` to populate the application with sample data (Galleries, Albums, Photos, PhotoShoots).

## How to Enable

To run this seeder on application startup, update your `Program.cs` file to call `DummyDataSeeder.SeedAsync`.

Locate the seeding section in `Program.cs` (usually near `SeedData.SeedRolesAsync`) and add the following line:

```csharp
// Existing seeding
await SeedData.SeedRolesAsync(services, configuration);

// Add this line:
await DummyDataSeeder.SeedAsync(services);
```

## What gets seeded?
- **PhotoShoots**: 3 standard packages (Wedding, Portrait, Event)
- **Galleries**: 3 sample galleries (Wedding, Corporate, Family)
- **Albums**: 3 albums per gallery
- **Photos**: 12 placeholder photos per album