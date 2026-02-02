using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using MyPhotoBiz.Models;
using System.Security.Claims;

namespace MyPhotoBiz.Services
{
    public class CustomUserClaimsPrincipalFactory : UserClaimsPrincipalFactory<ApplicationUser>
    {
        public CustomUserClaimsPrincipalFactory(
            UserManager<ApplicationUser> userManager,
            IOptions<IdentityOptions> optionsAccessor)
            : base(userManager, optionsAccessor)
        {
        }

        protected override async Task<ClaimsIdentity> GenerateClaimsAsync(ApplicationUser user)
        {
            var identity = await base.GenerateClaimsAsync(user);

            // Add FullName claim
            var fullName = $"{user.FirstName} {user.LastName}".Trim();
            if (!string.IsNullOrEmpty(fullName))
            {
                identity.AddClaim(new Claim("FullName", fullName));
            }

            return identity;
        }
    }
}