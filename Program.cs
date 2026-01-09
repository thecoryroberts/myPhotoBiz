
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;
using MyPhotoBiz.Data;
using MyPhotoBiz.Models;
using MyPhotoBiz.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        b => b.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery)
    )
);

builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequiredLength = 12;
    options.Password.RequireNonAlphanumeric = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireLowercase = true;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

// Add memory cache for dashboard stats
builder.Services.AddMemoryCache();

// Register all services
builder.Services.AddScoped<IClientService, ClientService>();
builder.Services.AddScoped<IPhotoShootService, PhotoShootService>();
builder.Services.AddScoped<IAlbumService, AlbumService>();
builder.Services.AddScoped<IPhotoService, PhotoService>();
builder.Services.AddScoped<IInvoiceService, InvoiceService>();
builder.Services.AddScoped<IDashboardService, DashboardService>();
builder.Services.AddScoped<IPdfService, PdfService>();
builder.Services.AddScoped<IImageService, ImageService>();
builder.Services.AddScoped<IRolesService, RolesService>();
builder.Services.AddScoped<IPermissionService, PermissionService>();
builder.Services.AddScoped<IUserManagementService, UserManagementService>();
builder.Services.AddScoped<IFileService, FileService>();
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<IGalleryService, GalleryService>();
builder.Services.AddScoped<IProofService, ProofService>();
builder.Services.AddScoped<IActivityService, ActivityService>();
builder.Services.AddScoped<IBookingService, BookingService>();
builder.Services.AddScoped<IPackageService, PackageService>();

// Register Email Sender
builder.Services.AddTransient<IEmailSender, EmailSender>();

// Background task queue and hosted service for async image processing
builder.Services.AddSingleton<IBackgroundTaskQueue, BackgroundTaskQueue>();
builder.Services.AddHostedService<ImageProcessingHostedService>();

var app = builder.Build();

// Configure the HTTP request pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapRazorPages();

// Seed data
using (var scope = app.Services.CreateScope())
{
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

    // Seed roles using SeedData class
    try
    {
        await SeedData.SeedRolesAsync(userManager, roleManager);
    }
    catch (Exception)
    {
        // Roles might already exist, continue
    }

    // Seed SuperAdmin users
    try
    {
        await SeedData.SeedSuperAdminAsync(userManager, roleManager);
    }
    catch (Exception)
    {
        // SuperAdmin might already exist, continue
    }
}

app.Run();
