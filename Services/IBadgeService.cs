using MyPhotoBiz.ViewModels;

namespace MyPhotoBiz.Services
{
    public interface IBadgeService
    {
        // Selection ViewModels (for dropdowns, forms, etc.)
        Task<List<BadgeSelectionViewModel>> GetBadgeSelectionsAsync(CancellationToken cancellationToken = default);
    }
}
