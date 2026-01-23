
// Controllers/HomeController.cs

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyPhotoBiz.Services;
using System.Diagnostics;

namespace MyPhotoBiz.Controllers
{
    /// <summary>
    /// Handles HTTP requests for home.
    /// </summary>
    public class HomeController : Controller
    {
        private readonly IDashboardService _dashboardService;

        public HomeController(IDashboardService dashboardService)
        {
            _dashboardService = dashboardService;
        }
        public IActionResult Landing()
        {
            return View();
        }

        public async Task<IActionResult> Index()
        {
            if (User.IsInRole("Admin") || User.IsInRole("Photographer"))
            {
                var dashboardData = await _dashboardService.GetDashboardDataAsync();
                return View("Dashboard", dashboardData);
            }

            // Client home page
            if (User.IsInRole("Client"))
            {
                return RedirectToAction("MyProfile", "Clients");
            }

            return View();
        }

        [Authorize(Roles = "Admin,Photographer")]
        public async Task<IActionResult> Dashboard()
        {
            var dashboardData = await _dashboardService.GetDashboardDataAsync();
            return View(dashboardData);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }

    /// <summary>
    /// Represents view model data for error.
    /// </summary>
    public class ErrorViewModel
    {
        public string? RequestId { get; set; }
        public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
    }
}
