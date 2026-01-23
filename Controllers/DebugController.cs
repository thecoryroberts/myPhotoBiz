using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using MyPhotoBiz.Models;

namespace MyPhotoBiz.Controllers
{
    /// <summary>
    /// Handles HTTP requests for debug.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class DebugController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IConfiguration _configuration;

        public DebugController(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, IConfiguration configuration)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _configuration = configuration;
        }

        [HttpGet("config")]
        public IActionResult GetConfig()
        {
            var email = _configuration["Seed:PrimaryAdmin:Email"];
            var password = _configuration["Seed:PrimaryAdmin:Password"];
            var userName = _configuration["Seed:PrimaryAdmin:UserName"];

            // Log individual character codes
            var chars = password?.Select(static (c, i) => (index: i, @char: c, code: (int)c)).ToArray() ?? Array.Empty<(int index, char @char, int code)>();

            return Ok(new
            {
                email,
                userName,
                password,
                passwordLength = password?.Length ?? 0,
                passwordChars = chars.Take(30).Select(static x => $"{x.@char}({x.code})")
            });
        }

        [HttpPost("test-login")]
        public async Task<IActionResult> TestLogin(string email, string password)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                return Ok(new { status = "user_not_found", email });
            }

            if (user.UserName == null)
            {
                return Ok(new { status = "user_name_null", email });
            }

            var result = await _signInManager.PasswordSignInAsync(user.UserName, password, false, false);

            // Also test direct password verification
            var hasher = new PasswordHasher<ApplicationUser>();
            var hashResult = user.PasswordHash != null ? hasher.VerifyHashedPassword(user, user.PasswordHash, password) : PasswordVerificationResult.Failed;

            // Get configured password
            var configPassword = _configuration["Seed:PrimaryAdmin:Password"];

            return Ok(new
            {
                status = "login_attempted",
                email,
                userName = user.UserName,
                succeeded = result.Succeeded,
                requiresTwoFactor = result.RequiresTwoFactor,
                isLockedOut = result.IsLockedOut,
                isNotAllowed = result.IsNotAllowed,
                passwordHashVerification = hashResult.ToString(),
                configPassword = configPassword,
                testPasswordLength = password.Length,
                configPasswordLength = configPassword?.Length ?? 0,
                storedHashLength = user.PasswordHash?.Length ?? 0
            });
        }
    }
}
