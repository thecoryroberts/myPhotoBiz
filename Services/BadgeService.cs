using Microsoft.EntityFrameworkCore;
using MyPhotoBiz.Data;
using MyPhotoBiz.ViewModels;

namespace MyPhotoBiz.Services
{
    /// <summary>
    /// Service for managing badges and achievements
    /// </summary>
    public class BadgeService : IBadgeService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<BadgeService> _logger;

        public BadgeService(ApplicationDbContext context, ILogger<BadgeService> logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Get lightweight badge selection view models for dropdowns and forms
        /// </summary>
        public async Task<List<BadgeSelectionViewModel>> GetBadgeSelectionsAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Retrieving badge selections for dropdown");

                return await _context.Badges
                    .AsNoTracking()
                    .Where(b => b.IsActive)
                    .OrderBy(b => b.Name)
                    .Select(b => new BadgeSelectionViewModel
                    {
                        Id = b.Id,
                        Name = b.Name,
                        Description = b.Description,
                        Icon = b.Icon,
                        Color = b.Color
                    })
                    .ToListAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving badge selections");
                throw;
            }
        }
    }
}
