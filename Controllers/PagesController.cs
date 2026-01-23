using Microsoft.AspNetCore.Mvc;

namespace myPhotoBiz.Controllers
{
    /// <summary>
    /// Handles HTTP requests for pages.
    /// </summary>
    public class PagesController : Controller
    {
        public IActionResult Index() => View();
    }
}
