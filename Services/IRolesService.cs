using Microsoft.AspNetCore.Identity;
using MyPhotoBiz.Models;
using MyPhotoBiz.ViewModels;

namespace MyPhotoBiz.Services
{
    public interface IRolesService
    {
        // Role Management
        Task<IEnumerable<IdentityRole>> GetAllRolesAsync();
        Task<IdentityRole?> GetRoleByIdAsync(string roleId);
        Task<IdentityRole?> GetRoleByNameAsync(string roleName);
        Task<IdentityResult> CreateRoleAsync(string roleName);
        Task<IdentityResult> UpdateRoleAsync(string roleId, string newRoleName);
        Task<IdentityResult> DeleteRoleAsync(string roleId);
        Task<bool> RoleExistsAsync(string roleName);

        // User-Role Management
        Task<IEnumerable<ApplicationUser>> GetUsersInRoleAsync(string roleName);
        Task<IEnumerable<string>> GetUserRolesAsync(string userId);
        Task<IdentityResult> AssignUserToRoleAsync(string userId, string roleName);
        Task<IdentityResult> RemoveUserFromRoleAsync(string userId, string roleName);
        Task<bool> IsUserInRoleAsync(string userId, string roleName);
        Task<int> GetRoleUserCountAsync(string roleName);

        // ViewModel Methods
        Task<RolesIndexViewModel> GetRolesIndexViewModelAsync();
        Task<List<RoleViewModel>> GetAllRoleViewModelsAsync();
        Task<RoleViewModel?> GetRoleViewModelAsync(string roleId);
        Task<RoleDetailsViewModel?> GetRoleDetailsViewModelAsync(string roleId);
        Task<List<UserRoleViewModel>> GetUserRolesViewModelAsync();

        // Permissions persistence
        Task SetRolePermissionsAsync(string roleId, List<string> permissions);
        Task<List<string>> GetPersistedRolePermissionsAsync(string roleId);
        Task<List<string>> GetAllAvailablePermissionsAsync();

        // Helper Methods
        string GetRoleDescription(string roleName);
        string GetRoleIcon(string roleName);
        string GetRoleColorClass(string roleName);
        List<string> GetRolePermissions(string roleName);
    }
}