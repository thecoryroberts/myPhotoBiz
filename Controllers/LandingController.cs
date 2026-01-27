using Microsoft.AspNetCore.Mvc;

namespace MyPhotoBiz.Controllers
{
    /// <summary>
    /// Handles HTTP requests for landing.
    /// </summary>
    public class LandingController : Controller
    {
        public IActionResult Index() => View();
    }
}
