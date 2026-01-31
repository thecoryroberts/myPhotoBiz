using Microsoft.EntityFrameworkCore;
using MyPhotoBiz.Data;
using MyPhotoBiz.Helpers;
using MyPhotoBiz.Models;

namespace MyPhotoBiz.Services
{
    /// <summary>
    /// Provides album operations.
    /// </summary>
    public class AlbumService : IAlbumService
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _environment;

        public AlbumService(ApplicationDbContext context, IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
        }

        // Get all albums with related data
        public async Task<IEnumerable<Album>> GetAllAlbumsAsync()
        {
            return await BaseAlbumQuery()
                .OrderByDescending(a => a.CreatedDate)
                .ToListAsync();
        }

        // Get albums by client ID
        public async Task<IEnumerable<Album>> GetAlbumsByClientIdAsync(int clientId)
        {
            return await BaseAlbumQuery()
                .Where(a => a.ClientProfileId == clientId)
                .OrderByDescending(a => a.CreatedDate)
                .ToListAsync();
        }

        // Get albums by photo shoot ID
        public async Task<IEnumerable<Album>> GetAlbumsByPhotoShootIdAsync(int photoShootId)
        {
            return await BaseAlbumQuery()
                .Where(a => a.PhotoShootId == photoShootId)
                .OrderByDescending(a => a.CreatedDate)
                .ToListAsync();
        }

        // Get single album by ID with all related data
        public async Task<Album?> GetAlbumByIdAsync(int id)
        {
            return await _context.Albums
                .Include(a => a.PhotoShoot)
                    .ThenInclude(ps => ps.ClientProfile)
                        .ThenInclude(cp => cp!.User)
                .Include(a => a.ClientProfile)
                    .ThenInclude(cp => cp!.User)
                .Include(a => a.Photos)
                .FirstOrDefaultAsync(a => a.Id == id);
        }

        // Create new album
        public async Task<Album> CreateAlbumAsync(Album album)
        {
            _context.Albums.Add(album);
            await _context.SaveChangesAsync();
            return album;
        }

        // Update existing album
        public async Task<Album> UpdateAlbumAsync(Album album)
        {
            _context.Albums.Update(album);
            await _context.SaveChangesAsync();
            return album;
        }

        // Delete album and all its photos
        public async Task<bool> DeleteAlbumAsync(int id)
        {
            var album = await _context.Albums
                .Include(a => a.Photos)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (album == null)
                return false;

            // Delete all physical photo files
            foreach (var photo in album.Photos)
            {
                FileHelper.DeleteFileIfExists(photo.FilePath);
                FileHelper.DeleteFileIfExists(photo.ThumbnailPath);
            }


            // Remove album (cascade delete will handle photos in DB)
            _context.Albums.Remove(album);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<Photo> UpdatePhotoAsync(Photo photo)
        {
            _context.Photos.Update(photo);
            await _context.SaveChangesAsync();
            return photo;
        }

        private IQueryable<Album> BaseAlbumQuery()
        {
            return _context.Albums
                .Include(a => a.PhotoShoot)
                    .ThenInclude(ps => ps.ClientProfile)
                .Include(a => a.ClientProfile)
                .Include(a => a.Photos);
        }
    }
}
