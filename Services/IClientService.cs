using MyPhotoBiz.Enums;
using MyPhotoBiz.Models;
using MyPhotoBiz.ViewModels;

namespace MyPhotoBiz.Services
{
    /// <summary>
    /// Defines the client service contract.
    /// </summary>
    public interface IClientService
    {
        // Basic CRUD Operations
        Task<IEnumerable<ClientProfile>> GetAllClientsAsync(bool includeDeleted = false);
        Task<ClientProfile?> GetClientByIdAsync(int id);
        Task<ClientProfile?> GetClientByUserIdAsync(string userId);
        Task<ClientProfile> CreateClientAsync(ClientProfile clientProfile);
        Task<ClientProfile> UpdateClientAsync(ClientProfile clientProfile);

        // Soft Delete Operations
        Task<(bool Success, string? ErrorMessage)> SoftDeleteClientAsync(int id, bool force = false);
        Task<bool> RestoreClientAsync(int id);
        Task<bool> PermanentDeleteClientAsync(int id);
        Task<ClientDeleteValidationResult> ValidateDeleteAsync(int id);

        // Search and Filtering
        Task<IEnumerable<ClientProfile>> SearchClientsAsync(string searchTerm);
        Task<IEnumerable<ClientProfile>> GetClientsByStatusAsync(ClientStatus status);
        Task<IEnumerable<ClientProfile>> GetClientsByCategoryAsync(ClientCategory category);
        Task<IEnumerable<ClientProfile>> SearchClientsAdvancedAsync(ClientSearchFilter filter);

        // Status and Category Management
        Task<bool> UpdateClientStatusAsync(int id, ClientStatus status);
        Task<bool> UpdateClientCategoryAsync(int id, ClientCategory category);

        // Duplicate Detection
        Task<IEnumerable<ClientProfile>> FindPotentialDuplicatesAsync(string email, string? phone = null);
        Task<IEnumerable<ClientProfile>> FindDuplicatesByEmailAsync(string email);

        // Analytics
        Task<decimal> GetClientLifetimeValueAsync(int id);
        Task<ClientStatsSummary> GetClientStatsAsync();
        Task<IEnumerable<ClientProfile>> GetTopClientsByValueAsync(int count = 10);
        Task<IEnumerable<ClientProfile>> GetRecentClientsAsync(int days = 30);

        // Gallery Access
        Task<IEnumerable<GalleryAccess>> GetClientGalleryAccessesAsync(int clientProfileId);

        // Selection ViewModels (for dropdowns, forms, etc.)
        Task<List<ClientSelectionViewModel>> GetClientSelectionsAsync(CancellationToken cancellationToken = default);

        // Legacy method kept for backwards compatibility
        [Obsolete("Use SoftDeleteClientAsync instead")]
        Task<bool> DeleteClientAsync(int id);
    }

    /// <summary>
    /// Result of validating whether a client can be deleted
    /// </summary>
    public class ClientDeleteValidationResult
    {
        public bool CanDelete { get; set; }
        public bool HasActiveBookings { get; set; }
        public bool HasUnpaidInvoices { get; set; }
        public bool HasScheduledShoots { get; set; }
        public bool HasUnsignedContracts { get; set; }
        public int ActiveBookingsCount { get; set; }
        public int UnpaidInvoicesCount { get; set; }
        public decimal UnpaidAmount { get; set; }
        public List<string> BlockingReasons { get; set; } = new();
    }

    /// <summary>
    /// Filter criteria for advanced client search
    /// </summary>
    public class ClientSearchFilter
    {
        public string? SearchTerm { get; set; }
        public ClientStatus? Status { get; set; }
        public ClientCategory? Category { get; set; }
        public ReferralSource? ReferralSource { get; set; }
        public DateTime? CreatedAfter { get; set; }
        public DateTime? CreatedBefore { get; set; }
        public decimal? MinLifetimeValue { get; set; }
        public decimal? MaxLifetimeValue { get; set; }
        public bool? HasUnpaidInvoices { get; set; }
        public bool IncludeDeleted { get; set; } = false;
    }

    /// <summary>
    /// Summary statistics for clients
    /// </summary>
    public class ClientStatsSummary
    {
        public int TotalClients { get; set; }
        public int ActiveClients { get; set; }
        public int InactiveClients { get; set; }
        public int ArchivedClients { get; set; }
        public int ProspectCount { get; set; }
        public int RegularCount { get; set; }
        public int VIPCount { get; set; }
        public int CorporateCount { get; set; }
        public decimal TotalLifetimeValue { get; set; }
        public decimal AverageLifetimeValue { get; set; }
        public int NewClientsThisMonth { get; set; }
    }
}
