using Microsoft.EntityFrameworkCore;
using MyPhotoBiz.Data;
using MyPhotoBiz.Enums;
using MyPhotoBiz.Models;
using MyPhotoBiz.ViewModels;

namespace MyPhotoBiz.Services
{
    /// <summary>
    /// Provides photo shoot operations.
    /// </summary>
    public class PhotoShootService : IPhotoShootService
    {
        private readonly ApplicationDbContext _context;

        public PhotoShootService(ApplicationDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<IEnumerable<PhotoShoot>> GetAllPhotoShootsAsync() =>
            await _context.PhotoShoots
                .Include(p => p.ClientProfile)
                    .ThenInclude(cp => cp.User)
                .OrderByDescending(p => p.ScheduledDate)
                .ToListAsync();

        public async Task<PhotoShoot?> GetPhotoShootByIdAsync(int id) =>
            await _context.PhotoShoots
                .Include(p => p.ClientProfile)
                    .ThenInclude(cp => cp.User)
                .Include(p => p.Albums)
                    .ThenInclude(a => a.Photos)
                .FirstOrDefaultAsync(p => p.Id == id);

        public async Task<IEnumerable<PhotoShoot>> GetUpcomingPhotoShootsAsync(int daysAhead = 7) =>
            await _context.PhotoShoots
                .Include(p => p.ClientProfile)
                    .ThenInclude(cp => cp.User)
                .Where(p => p.ScheduledDate >= DateTime.Today && p.ScheduledDate <= DateTime.Today.AddDays(daysAhead))
                .OrderBy(p => p.ScheduledDate)
                .ToListAsync();

        public async Task<IEnumerable<PhotoShoot>> GetPhotoShootsByClientIdAsync(int clientId) =>
            await _context.PhotoShoots
                .Include(p => p.ClientProfile)
                    .ThenInclude(cp => cp.User)
                .Include(p => p.Albums)
                .Where(p => p.ClientProfileId == clientId)
                .OrderByDescending(p => p.ScheduledDate)
                .ToListAsync();

        public async Task<PhotoShoot> CreatePhotoShootAsync(PhotoShoot shoot)
        {
            if (shoot == null) throw new ArgumentNullException(nameof(shoot));

            // ✅ Ensure parent Client exists
            var clientExists = await _context.ClientProfiles.AnyAsync(c => c.Id == shoot.ClientProfileId);
            if (!clientExists)
                throw new InvalidOperationException($"Client with Id {shoot.ClientProfileId} does not exist.");

            // ✅ Ensure PhotographerProfile exists (if provided)
            if (shoot.PhotographerProfileId.HasValue)
            {
                var photographerExists = await _context.PhotographerProfiles.AnyAsync(pp => pp.Id == shoot.PhotographerProfileId);
                if (!photographerExists)
                    throw new InvalidOperationException($"PhotographerProfile with Id {shoot.PhotographerProfileId} does not exist.");
            }

            _context.PhotoShoots.Add(shoot);
            await _context.SaveChangesAsync();
            return shoot;
        }

        public async Task<PhotoShoot> UpdatePhotoShootAsync(PhotoShoot shoot)
        {
            if (shoot == null) throw new ArgumentNullException(nameof(shoot));
            var existing = await _context.PhotoShoots.FindAsync(shoot.Id);
            if (existing == null) throw new InvalidOperationException("PhotoShoot not found");

            // ✅ Validate parent records again
            var clientExists = await _context.ClientProfiles.AnyAsync(c => c.Id == shoot.ClientProfileId);
            if (!clientExists)
                throw new InvalidOperationException($"Client with Id {shoot.ClientProfileId} does not exist.");

            if (shoot.PhotographerProfileId.HasValue)
            {
                var photographerExists = await _context.PhotographerProfiles.AnyAsync(pp => pp.Id == shoot.PhotographerProfileId);
                if (!photographerExists)
                    throw new InvalidOperationException($"PhotographerProfile with Id {shoot.PhotographerProfileId} does not exist.");
            }

            _context.Entry(existing).CurrentValues.SetValues(shoot);
            await _context.SaveChangesAsync();
            return existing;
        }

        public async Task<bool> DeletePhotoShootAsync(int id)
        {
            var shoot = await _context.PhotoShoots.FindAsync(id);
            if (shoot == null) return false;

            _context.PhotoShoots.Remove(shoot);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<int> GetPhotoShootsCountAsync() =>
            await _context.PhotoShoots.CountAsync();

        public async Task<int> GetPendingPhotoShootsCountAsync() =>
            await _context.PhotoShoots.CountAsync(p => p.Status == PhotoShootStatus.Scheduled);

        /// <summary>
        /// Get lightweight photo shoot selection view models for dropdowns and forms
        /// </summary>
        public async Task<List<PhotoShootSelectionViewModel>> GetPhotoShootSelectionsAsync(CancellationToken cancellationToken = default)
        {
            return await _context.PhotoShoots
                .AsNoTracking()
                .Include(ps => ps.ClientProfile)
                    .ThenInclude(cp => cp.User)
                .OrderByDescending(ps => ps.ScheduledDate)
                .Select(ps => new PhotoShootSelectionViewModel
                {
                    Id = ps.Id,
                    Title = ps.Title,
                    ShootDate = ps.ScheduledDate,
                    ClientName = ps.ClientProfile != null && ps.ClientProfile.User != null
                        ? $"{ps.ClientProfile.User.FirstName} {ps.ClientProfile.User.LastName}"
                        : "Unknown"
                })
                .ToListAsync(cancellationToken);
        }
    }
}
