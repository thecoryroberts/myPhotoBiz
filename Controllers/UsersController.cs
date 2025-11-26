using Microsoft.AspNetCore.Mvc;

namespace myPhotoBiz.Controllers
{
    public class UsersController : Controller
    {
        public IActionResult Permissions() => View();
        public IActionResult Roles() => View();
        
    }
}