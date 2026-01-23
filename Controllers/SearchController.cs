using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyPhotoBiz.Data;
using MyPhotoBiz.Enums;

namespace MyPhotoBiz.Controllers
{
    /// <summary>
    /// Handles HTTP requests for search.
    /// </summary>
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class SearchController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public SearchController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Search([FromQuery] string q, [FromQuery] int limit = 10)
        {
            if (string.IsNullOrWhiteSpace(q) || q.Length < 2)
            {
                return Ok(new { results = Array.Empty<object>() });
            }

            var query = q.ToLower().Trim();
            var results = new List<SearchResult>();

            // Search Clients
            var clients = await _context.ClientProfiles
                .Include(c => c.User)
                .Where(c => !c.IsDeleted && (
                    (c.User != null && (c.User.FirstName + " " + c.User.LastName).ToLower().Contains(query)) ||
                    (c.User != null && c.User.Email != null && c.User.Email.ToLower().Contains(query)) ||
                    (c.PhoneNumber != null && c.PhoneNumber.Contains(query)) ||
                    (c.Address != null && c.Address.ToLower().Contains(query))
                ))
                .Take(limit)
                .Select(c => new SearchResult
                {
                    Id = c.Id,
                    Type = "client",
                    Title = c.User != null ? $"{c.User.FirstName} {c.User.LastName}" : "Unknown Client",
                    Subtitle = c.User != null ? c.User.Email ?? "" : "",
                    Icon = "ti-user",
                    Url = $"/Clients/Details/{c.Id}",
                    Color = "primary"
                })
                .ToListAsync();
            results.AddRange(clients);

            // Search Photo Shoots
            var shoots = await _context.PhotoShoots
                .Include(s => s.ClientProfile)
                    .ThenInclude(cp => cp!.User)
                .Where(s => !s.IsDeleted && (
                    s.Title.ToLower().Contains(query) ||
                    (s.Location != null && s.Location.ToLower().Contains(query)) ||
                    (s.ClientProfile != null && s.ClientProfile.User != null &&
                     (s.ClientProfile.User.FirstName + " " + s.ClientProfile.User.LastName).ToLower().Contains(query))
                ))
                .Take(limit)
                .Select(s => new SearchResult
                {
                    Id = s.Id,
                    Type = "shoot",
                    Title = s.Title,
                    Subtitle = s.ScheduledDate.ToString("MMM dd, yyyy") + (s.Location != null ? $" - {s.Location}" : ""),
                    Icon = "ti-camera",
                    Url = $"/PhotoShoots/Details/{s.Id}",
                    Color = "warning"
                })
                .ToListAsync();
            results.AddRange(shoots);

            // Search Invoices
            var invoices = await _context.Invoices
                .Include(i => i.ClientProfile)
                    .ThenInclude(cp => cp!.User)
                .Where(i => !i.IsDeleted && (
                    i.InvoiceNumber.ToLower().Contains(query) ||
                    (i.ClientProfile != null && i.ClientProfile.User != null &&
                     (i.ClientProfile.User.FirstName + " " + i.ClientProfile.User.LastName).ToLower().Contains(query))
                ))
                .Take(limit)
                .ToListAsync();

            results.AddRange(invoices.Select(i => new SearchResult
            {
                Id = i.Id,
                Type = "invoice",
                Title = $"Invoice #{i.InvoiceNumber}",
                Subtitle = $"{i.TotalAmount:C} - {i.Status}",
                Icon = "ti-file-invoice",
                Url = $"/Invoices/Details/{i.Id}",
                Color = i.Status == InvoiceStatus.Paid ? "success" : (i.Status == InvoiceStatus.Overdue ? "danger" : "info")
            }));

            // Search Galleries
            var galleries = await _context.Galleries
                .Where(g => g.IsActive && (
                    g.Name.ToLower().Contains(query) ||
                    (g.Description != null && g.Description.ToLower().Contains(query))
                ))
                .Take(limit)
                .Select(g => new SearchResult
                {
                    Id = g.Id,
                    Type = "gallery",
                    Title = g.Name,
                    Subtitle = g.Description ?? "",
                    Icon = "ti-photo-album",
                    Url = $"/Galleries?details={g.Id}",
                    Color = "purple"
                })
                .ToListAsync();
            results.AddRange(galleries);

            // Search Albums
            var albums = await _context.Albums
                .Include(a => a.ClientProfile)
                    .ThenInclude(cp => cp.User)
                .Where(a =>
                    a.Name.ToLower().Contains(query) ||
                    (a.Description != null && a.Description.ToLower().Contains(query))
                )
                .Take(limit)
                .Select(a => new SearchResult
                {
                    Id = a.Id,
                    Type = "album",
                    Title = a.Name,
                    Subtitle = a.ClientProfile != null && a.ClientProfile.User != null
                        ? $"{a.ClientProfile.User.FirstName} {a.ClientProfile.User.LastName}"
                        : (a.Description ?? "No description"),
                    Icon = "ti-folders",
                    Url = $"/Albums/Details/{a.Id}",
                    Color = "teal"
                })
                .ToListAsync();
            results.AddRange(albums);

            // Limit total results and sort by relevance (exact matches first)
            var sortedResults = results
                .OrderByDescending(r => r.Title.ToLower().StartsWith(query))
                .ThenByDescending(r => r.Title.ToLower().Contains(query))
                .Take(limit * 2)
                .ToList();

            return Ok(new { results = sortedResults, query = q });
        }

        /// <summary>
        /// Represents the search result.
        /// </summary>
        private class SearchResult
        {
            public int Id { get; set; }
            public string Type { get; set; } = "";
            public string Title { get; set; } = "";
            public string Subtitle { get; set; } = "";
            public string Icon { get; set; } = "";
            public string Url { get; set; } = "";
            public string Color { get; set; } = "primary";
        }
    }
}
