using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MyPhotoBiz.Data;
using MyPhotoBiz.Enums;
using MyPhotoBiz.Models;

namespace MyPhotoBiz.Services
{
    /// <summary>
    /// Service for managing client profiles and related operations.
    /// Features: Soft delete with validation, client categorization (VIP/Regular/Prospect/Corporate),
    /// advanced search filters, duplicate detection, lifetime value calculation, referral tracking,
    /// and client preferences management.
    /// </summary>
    public class ClientService : IClientService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<ClientService> _logger;

        public ClientService(ApplicationDbContext context, ILogger<ClientService> logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        #region Basic CRUD Operations

        public async Task<IEnumerable<ClientProfile>> GetAllClientsAsync(bool includeDeleted = false)
        {
            try
            {
                _logger.LogInformation("Retrieving all clients (includeDeleted: {IncludeDeleted})", includeDeleted);

                var query = _context.ClientProfiles
                    .AsNoTracking()
                    .Include(c => c.Invoices)
                    .Include(c => c.User)
                    .Include(c => c.ClientBadges)
                        .ThenInclude(cb => cb.Badge)
                    .AsQueryable();

                if (!includeDeleted)
                {
                    query = query.Where(c => !c.IsDeleted);
                }

                return await query
                    .OrderBy(c => c.User.LastName)
                    .ThenBy(c => c.User.FirstName)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving all clients");
                throw;
            }
        }

        public async Task<ClientProfile?> GetClientByIdAsync(int id)
        {
            try
            {
                _logger.LogInformation("Retrieving client with ID: {ClientId}", id);
                return await _context.ClientProfiles
                    .Include(c => c.Invoices)
                    .Include(c => c.PhotoShoots)
                    .Include(c => c.User)
                    .Include(c => c.ClientBadges)
                        .ThenInclude(cb => cb.Badge)
                    .Include(c => c.GalleryAccesses)
                        .ThenInclude(ga => ga.Gallery)
                    .Include(c => c.BookingRequests)
                    .FirstOrDefaultAsync(c => c.Id == id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving client with ID: {ClientId}", id);
                throw;
            }
        }

        public async Task<ClientProfile?> GetClientByUserIdAsync(string userId)
        {
            try
            {
                return await _context.ClientProfiles
                    .Include(c => c.Invoices)
                    .Include(c => c.PhotoShoots)
                    .Include(c => c.User)
                    .Include(c => c.ClientBadges)
                        .ThenInclude(cb => cb.Badge)
                    .Include(c => c.GalleryAccesses)
                        .ThenInclude(ga => ga.Gallery)
                    .Include(c => c.BookingRequests)
                    .FirstOrDefaultAsync(c => c.UserId == userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving client by user ID: {UserId}", userId);
                throw;
            }
        }

        public async Task<ClientProfile> CreateClientAsync(ClientProfile clientProfile)
        {
            if (clientProfile == null) throw new ArgumentNullException(nameof(clientProfile));

            try
            {
                _logger.LogInformation("Creating new client profile for user: {UserId}", clientProfile.UserId);

                // Check for potential duplicates
                var duplicates = await FindDuplicatesByEmailAsync(clientProfile.User?.Email ?? "");
                if (duplicates.Any())
                {
                    _logger.LogWarning("Potential duplicate client detected for email: {Email}", clientProfile.User?.Email);
                }

                _context.ClientProfiles.Add(clientProfile);
                await _context.SaveChangesAsync();
                _logger.LogInformation("Successfully created client profile with ID: {ClientId}", clientProfile.Id);
                return clientProfile;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating client profile for user: {UserId}", clientProfile.UserId);
                throw;
            }
        }

        public async Task<ClientProfile> UpdateClientAsync(ClientProfile clientProfile)
        {
            if (clientProfile == null) throw new ArgumentNullException(nameof(clientProfile));

            try
            {
                _logger.LogInformation("Updating client profile with ID: {ClientId}", clientProfile.Id);
                var existing = await _context.ClientProfiles.FindAsync(clientProfile.Id);
                if (existing == null)
                {
                    _logger.LogWarning("Attempted to update non-existent client profile with ID: {ClientId}", clientProfile.Id);
                    throw new InvalidOperationException("Client profile not found");
                }

                existing.PhoneNumber = clientProfile.PhoneNumber;
                existing.Address = clientProfile.Address;
                existing.Notes = clientProfile.Notes;
                existing.Status = clientProfile.Status;
                existing.Category = clientProfile.Category;
                existing.ReferralSource = clientProfile.ReferralSource;
                existing.ReferralDetails = clientProfile.ReferralDetails;
                existing.ContactPreference = clientProfile.ContactPreference;
                existing.UpdatedDate = DateTime.UtcNow;

                await _context.SaveChangesAsync();
                _logger.LogInformation("Successfully updated client profile with ID: {ClientId}", clientProfile.Id);
                return existing;
            }
            catch (Exception ex) when (ex is not InvalidOperationException)
            {
                _logger.LogError(ex, "Error updating client profile with ID: {ClientId}", clientProfile.Id);
                throw;
            }
        }

        #endregion

        #region Soft Delete Operations

        public async Task<(bool Success, string? ErrorMessage)> SoftDeleteClientAsync(int id, bool force = false)
        {
            try
            {
                _logger.LogInformation("Soft deleting client profile with ID: {ClientId} (force: {Force})", id, force);

                var client = await _context.ClientProfiles
                    .Include(c => c.Invoices)
                    .Include(c => c.PhotoShoots)
                    .Include(c => c.BookingRequests)
                    .FirstOrDefaultAsync(c => c.Id == id);

                if (client == null)
                {
                    return (false, "Client not found");
                }

                if (client.IsDeleted)
                {
                    return (false, "Client is already deleted");
                }

                // Validate if not forcing
                if (!force)
                {
                    var validation = await ValidateDeleteAsync(id);
                    if (!validation.CanDelete)
                    {
                        var reasons = string.Join("; ", validation.BlockingReasons);
                        return (false, $"Cannot delete client: {reasons}");
                    }
                }

                client.IsDeleted = true;
                client.DeletedDate = DateTime.UtcNow;
                client.Status = ClientStatus.Archived;
                client.UpdatedDate = DateTime.UtcNow;

                await _context.SaveChangesAsync();
                _logger.LogInformation("Successfully soft deleted client profile with ID: {ClientId}", id);

                return (true, null);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error soft deleting client profile with ID: {ClientId}", id);
                throw;
            }
        }

        public async Task<bool> RestoreClientAsync(int id)
        {
            try
            {
                _logger.LogInformation("Restoring client profile with ID: {ClientId}", id);

                var client = await _context.ClientProfiles.FindAsync(id);
                if (client == null)
                {
                    _logger.LogWarning("Attempted to restore non-existent client profile with ID: {ClientId}", id);
                    return false;
                }

                if (!client.IsDeleted)
                {
                    _logger.LogWarning("Attempted to restore non-deleted client profile with ID: {ClientId}", id);
                    return false;
                }

                client.IsDeleted = false;
                client.DeletedDate = null;
                client.Status = ClientStatus.Inactive; // Restored as inactive, can be activated manually
                client.UpdatedDate = DateTime.UtcNow;

                await _context.SaveChangesAsync();
                _logger.LogInformation("Successfully restored client profile with ID: {ClientId}", id);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error restoring client profile with ID: {ClientId}", id);
                throw;
            }
        }

        public async Task<bool> PermanentDeleteClientAsync(int id)
        {
            try
            {
                _logger.LogWarning("Permanently deleting client profile with ID: {ClientId}", id);

                var client = await _context.ClientProfiles.FindAsync(id);
                if (client == null)
                {
                    return false;
                }

                _context.ClientProfiles.Remove(client);
                await _context.SaveChangesAsync();
                _logger.LogWarning("Permanently deleted client profile with ID: {ClientId}", id);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error permanently deleting client profile with ID: {ClientId}", id);
                throw;
            }
        }

        public async Task<ClientDeleteValidationResult> ValidateDeleteAsync(int id)
        {
            var result = new ClientDeleteValidationResult { CanDelete = true };

            try
            {
                var client = await _context.ClientProfiles
                    .Include(c => c.Invoices)
                    .Include(c => c.PhotoShoots)
                    .Include(c => c.BookingRequests)
                    .Include(c => c.Contracts)
                    .FirstOrDefaultAsync(c => c.Id == id);

                if (client == null)
                {
                    result.CanDelete = false;
                    result.BlockingReasons.Add("Client not found");
                    return result;
                }

                // Check for unpaid invoices
                var unpaidInvoices = client.Invoices
                    .Where(i => i.Status == InvoiceStatus.Pending || i.Status == InvoiceStatus.Overdue)
                    .ToList();

                if (unpaidInvoices.Any())
                {
                    result.HasUnpaidInvoices = true;
                    result.UnpaidInvoicesCount = unpaidInvoices.Count;
                    result.UnpaidAmount = unpaidInvoices.Sum(i => i.Amount);
                    result.CanDelete = false;
                    result.BlockingReasons.Add($"{unpaidInvoices.Count} unpaid invoice(s) totaling {result.UnpaidAmount:C}");
                }

                // Check for active bookings
                var activeBookings = client.BookingRequests
                    .Where(br => br.Status == BookingStatus.Pending || br.Status == BookingStatus.Confirmed)
                    .ToList();

                if (activeBookings.Any())
                {
                    result.HasActiveBookings = true;
                    result.ActiveBookingsCount = activeBookings.Count;
                    result.CanDelete = false;
                    result.BlockingReasons.Add($"{activeBookings.Count} active booking(s)");
                }

                // Check for scheduled photo shoots
                var scheduledShoots = client.PhotoShoots
                    .Where(ps => ps.Status == PhotoShootStatus.Scheduled)
                    .ToList();

                if (scheduledShoots.Any())
                {
                    result.HasScheduledShoots = true;
                    result.CanDelete = false;
                    result.BlockingReasons.Add($"{scheduledShoots.Count} scheduled photo shoot(s)");
                }

                // Check for unsigned contracts
                var unsignedContracts = client.Contracts
                    .Where(c => c.Status == ContractStatus.Draft || c.Status == ContractStatus.PendingSignature)
                    .ToList();

                if (unsignedContracts.Any())
                {
                    result.HasUnsignedContracts = true;
                    result.CanDelete = false;
                    result.BlockingReasons.Add($"{unsignedContracts.Count} unsigned contract(s)");
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating delete for client ID: {ClientId}", id);
                throw;
            }
        }

        // Legacy method - kept for backwards compatibility
        [Obsolete("Use SoftDeleteClientAsync instead")]
        public async Task<bool> DeleteClientAsync(int id)
        {
            var result = await SoftDeleteClientAsync(id, force: false);
            return result.Success;
        }

        #endregion

        #region Search and Filtering

        public async Task<IEnumerable<ClientProfile>> SearchClientsAsync(string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
                return await GetAllClientsAsync();

            var lowerSearchTerm = searchTerm.ToLower();

            return await _context.ClientProfiles
                .AsNoTracking()
                .Include(c => c.Invoices)
                .Include(c => c.User)
                .Where(c => !c.IsDeleted &&
                    (c.User.FirstName.ToLower().Contains(lowerSearchTerm) ||
                     c.User.LastName.ToLower().Contains(lowerSearchTerm) ||
                     (c.User.Email != null && c.User.Email.ToLower().Contains(lowerSearchTerm)) ||
                     (c.PhoneNumber != null && c.PhoneNumber.Contains(searchTerm))))
                .OrderBy(c => c.User.LastName)
                .ThenBy(c => c.User.FirstName)
                .ToListAsync();
        }

        public async Task<IEnumerable<ClientProfile>> GetClientsByStatusAsync(ClientStatus status)
        {
            return await _context.ClientProfiles
                .AsNoTracking()
                .Include(c => c.User)
                .Include(c => c.Invoices)
                .Where(c => c.Status == status && !c.IsDeleted)
                .OrderBy(c => c.User.LastName)
                .ThenBy(c => c.User.FirstName)
                .ToListAsync();
        }

        public async Task<IEnumerable<ClientProfile>> GetClientsByCategoryAsync(ClientCategory category)
        {
            return await _context.ClientProfiles
                .AsNoTracking()
                .Include(c => c.User)
                .Include(c => c.Invoices)
                .Where(c => c.Category == category && !c.IsDeleted)
                .OrderBy(c => c.User.LastName)
                .ThenBy(c => c.User.FirstName)
                .ToListAsync();
        }

        public async Task<IEnumerable<ClientProfile>> SearchClientsAdvancedAsync(ClientSearchFilter filter)
        {
            var query = _context.ClientProfiles
                .AsNoTracking()
                .Include(c => c.User)
                .Include(c => c.Invoices)
                .Include(c => c.PhotoShoots)
                .AsQueryable();

            // Apply filters
            if (!filter.IncludeDeleted)
            {
                query = query.Where(c => !c.IsDeleted);
            }

            if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
            {
                var term = filter.SearchTerm.ToLower();
                query = query.Where(c =>
                    c.User.FirstName.ToLower().Contains(term) ||
                    c.User.LastName.ToLower().Contains(term) ||
                    (c.User.Email != null && c.User.Email.ToLower().Contains(term)));
            }

            if (filter.Status.HasValue)
            {
                query = query.Where(c => c.Status == filter.Status.Value);
            }

            if (filter.Category.HasValue)
            {
                query = query.Where(c => c.Category == filter.Category.Value);
            }

            if (filter.ReferralSource.HasValue)
            {
                query = query.Where(c => c.ReferralSource == filter.ReferralSource.Value);
            }

            if (filter.CreatedAfter.HasValue)
            {
                query = query.Where(c => c.CreatedDate >= filter.CreatedAfter.Value);
            }

            if (filter.CreatedBefore.HasValue)
            {
                query = query.Where(c => c.CreatedDate <= filter.CreatedBefore.Value);
            }

            if (filter.HasUnpaidInvoices.HasValue)
            {
                if (filter.HasUnpaidInvoices.Value)
                {
                    query = query.Where(c => c.Invoices.Any(i =>
                        i.Status == InvoiceStatus.Pending || i.Status == InvoiceStatus.Overdue));
                }
                else
                {
                    query = query.Where(c => !c.Invoices.Any(i =>
                        i.Status == InvoiceStatus.Pending || i.Status == InvoiceStatus.Overdue));
                }
            }

            var clients = await query
                .OrderBy(c => c.User.LastName)
                .ThenBy(c => c.User.FirstName)
                .ToListAsync();

            // Filter by lifetime value in memory (computed property)
            if (filter.MinLifetimeValue.HasValue)
            {
                clients = clients.Where(c => c.LifetimeValue >= filter.MinLifetimeValue.Value).ToList();
            }

            if (filter.MaxLifetimeValue.HasValue)
            {
                clients = clients.Where(c => c.LifetimeValue <= filter.MaxLifetimeValue.Value).ToList();
            }

            return clients;
        }

        #endregion

        #region Status and Category Management

        public async Task<bool> UpdateClientStatusAsync(int id, ClientStatus status)
        {
            try
            {
                var client = await _context.ClientProfiles.FindAsync(id);
                if (client == null)
                {
                    return false;
                }

                client.Status = status;
                client.UpdatedDate = DateTime.UtcNow;

                // If archiving, also mark as deleted
                if (status == ClientStatus.Archived)
                {
                    client.IsDeleted = true;
                    client.DeletedDate = DateTime.UtcNow;
                }
                else if (client.IsDeleted && status != ClientStatus.Archived)
                {
                    // If un-archiving, restore
                    client.IsDeleted = false;
                    client.DeletedDate = null;
                }

                await _context.SaveChangesAsync();
                _logger.LogInformation("Updated client {ClientId} status to {Status}", id, status);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating status for client ID: {ClientId}", id);
                throw;
            }
        }

        public async Task<bool> UpdateClientCategoryAsync(int id, ClientCategory category)
        {
            try
            {
                var client = await _context.ClientProfiles.FindAsync(id);
                if (client == null)
                {
                    return false;
                }

                client.Category = category;
                client.UpdatedDate = DateTime.UtcNow;

                await _context.SaveChangesAsync();
                _logger.LogInformation("Updated client {ClientId} category to {Category}", id, category);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating category for client ID: {ClientId}", id);
                throw;
            }
        }

        #endregion

        #region Duplicate Detection

        public async Task<IEnumerable<ClientProfile>> FindPotentialDuplicatesAsync(string email, string? phone = null)
        {
            if (string.IsNullOrWhiteSpace(email) && string.IsNullOrWhiteSpace(phone))
            {
                return Enumerable.Empty<ClientProfile>();
            }

            var query = _context.ClientProfiles
                .AsNoTracking()
                .Include(c => c.User)
                .Where(c => !c.IsDeleted);

            var emailLower = email?.ToLower();
            var phoneCleaned = phone?.Replace(" ", "").Replace("-", "").Replace("(", "").Replace(")", "");

            if (!string.IsNullOrWhiteSpace(emailLower))
            {
                query = query.Where(c =>
                    c.User.Email != null && c.User.Email.ToLower() == emailLower);
            }

            var results = await query.ToListAsync();

            // Additional phone matching in memory
            if (!string.IsNullOrWhiteSpace(phoneCleaned))
            {
                var phoneMatches = await _context.ClientProfiles
                    .AsNoTracking()
                    .Include(c => c.User)
                    .Where(c => !c.IsDeleted && c.PhoneNumber != null)
                    .ToListAsync();

                var matchingByPhone = phoneMatches
                    .Where(c => c.PhoneNumber != null &&
                        c.PhoneNumber.Replace(" ", "").Replace("-", "").Replace("(", "").Replace(")", "") == phoneCleaned);

                results = results.Union(matchingByPhone).Distinct().ToList();
            }

            return results;
        }

        public async Task<IEnumerable<ClientProfile>> FindDuplicatesByEmailAsync(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                return Enumerable.Empty<ClientProfile>();
            }

            var emailLower = email.ToLower();

            return await _context.ClientProfiles
                .AsNoTracking()
                .Include(c => c.User)
                .Where(c => !c.IsDeleted &&
                    c.User.Email != null &&
                    c.User.Email.ToLower() == emailLower)
                .ToListAsync();
        }

        #endregion

        #region Analytics

        public async Task<decimal> GetClientLifetimeValueAsync(int id)
        {
            try
            {
                var paidAmount = await _context.Invoices
                    .Where(i => i.ClientProfileId == id && i.Status == InvoiceStatus.Paid)
                    .SumAsync(i => (decimal?)i.Amount) ?? 0;

                return paidAmount;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating lifetime value for client ID: {ClientId}", id);
                throw;
            }
        }

        public async Task<ClientStatsSummary> GetClientStatsAsync()
        {
            try
            {
                var now = DateTime.UtcNow;
                var startOfMonth = new DateTime(now.Year, now.Month, 1);

                // Get counts by status using SQL
                var statusCounts = await _context.ClientProfiles
                    .Where(c => !c.IsDeleted)
                    .GroupBy(c => c.Status)
                    .Select(g => new { Status = g.Key, Count = g.Count() })
                    .ToListAsync();

                // Get counts by category using SQL
                var categoryCounts = await _context.ClientProfiles
                    .Where(c => !c.IsDeleted)
                    .GroupBy(c => c.Category)
                    .Select(g => new { Category = g.Key, Count = g.Count() })
                    .ToListAsync();

                // Get total and average lifetime value
                var lifetimeValues = await _context.Invoices
                    .Where(i => i.Status == InvoiceStatus.Paid && !i.ClientProfile.IsDeleted)
                    .GroupBy(i => i.ClientProfileId)
                    .Select(g => g.Sum(i => i.Amount))
                    .ToListAsync();

                var newClientsThisMonth = await _context.ClientProfiles
                    .CountAsync(c => !c.IsDeleted && c.CreatedDate >= startOfMonth);

                return new ClientStatsSummary
                {
                    TotalClients = await _context.ClientProfiles.CountAsync(c => !c.IsDeleted),
                    ActiveClients = statusCounts.FirstOrDefault(s => s.Status == ClientStatus.Active)?.Count ?? 0,
                    InactiveClients = statusCounts.FirstOrDefault(s => s.Status == ClientStatus.Inactive)?.Count ?? 0,
                    ArchivedClients = await _context.ClientProfiles.CountAsync(c => c.IsDeleted),
                    ProspectCount = categoryCounts.FirstOrDefault(c => c.Category == ClientCategory.Prospect)?.Count ?? 0,
                    RegularCount = categoryCounts.FirstOrDefault(c => c.Category == ClientCategory.Regular)?.Count ?? 0,
                    VIPCount = categoryCounts.FirstOrDefault(c => c.Category == ClientCategory.VIP)?.Count ?? 0,
                    CorporateCount = categoryCounts.FirstOrDefault(c => c.Category == ClientCategory.Corporate)?.Count ?? 0,
                    TotalLifetimeValue = lifetimeValues.Sum(),
                    AverageLifetimeValue = lifetimeValues.Any() ? lifetimeValues.Average() : 0,
                    NewClientsThisMonth = newClientsThisMonth
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving client statistics");
                throw;
            }
        }

        public async Task<IEnumerable<ClientProfile>> GetTopClientsByValueAsync(int count = 10)
        {
            try
            {
                // Get client IDs with their lifetime values from paid invoices
                var clientValues = await _context.Invoices
                    .Where(i => i.Status == InvoiceStatus.Paid && !i.ClientProfile.IsDeleted)
                    .GroupBy(i => i.ClientProfileId)
                    .Select(g => new { ClientProfileId = g.Key, TotalValue = g.Sum(i => i.Amount) })
                    .OrderByDescending(x => x.TotalValue)
                    .Take(count)
                    .ToListAsync();

                var clientIds = clientValues.Select(cv => cv.ClientProfileId).ToList();

                var clients = await _context.ClientProfiles
                    .Include(c => c.User)
                    .Include(c => c.Invoices)
                    .Where(c => clientIds.Contains(c.Id))
                    .ToListAsync();

                // Order by lifetime value
                return clients.OrderByDescending(c => c.LifetimeValue);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving top clients by value");
                throw;
            }
        }

        public async Task<IEnumerable<ClientProfile>> GetRecentClientsAsync(int days = 30)
        {
            var cutoffDate = DateTime.UtcNow.AddDays(-days);

            return await _context.ClientProfiles
                .AsNoTracking()
                .Include(c => c.User)
                .Where(c => !c.IsDeleted && c.CreatedDate >= cutoffDate)
                .OrderByDescending(c => c.CreatedDate)
                .ToListAsync();
        }

        #endregion

        #region Gallery Access

        public async Task<IEnumerable<GalleryAccess>> GetClientGalleryAccessesAsync(int clientProfileId)
        {
            return await _context.GalleryAccesses
                .Include(ga => ga.Gallery)
                .Where(ga => ga.ClientProfileId == clientProfileId && ga.IsActive)
                .OrderByDescending(ga => ga.GrantedDate)
                .ToListAsync();
        }

        #endregion
    }
}
