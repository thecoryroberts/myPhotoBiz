using MyPhotoBiz.ViewModels;

namespace MyPhotoBiz.Services
{
    /// <summary>
    /// Defines the badge service contract.
    /// </summary>
    public interface IBadgeService
    {
        // Selection ViewModels (for dropdowns, forms, etc.)
        Task<List<BadgeSelectionViewModel>> GetBadgeSelectionsAsync(CancellationToken cancellationToken = default);
    }
}
