using Microsoft.AspNetCore.Mvc;

namespace myPhotoBiz.Controllers
{
    /// <summary>
    /// Handles HTTP requests for landing.
    /// </summary>
    public class LandingController : Controller
    {
        public IActionResult Index() => View();
    }
}
