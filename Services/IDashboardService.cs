using MyPhotoBiz.Models;
using MyPhotoBiz.ViewModels;

namespace MyPhotoBiz.Services
{
    /// <summary>
    /// Defines the dashboard service contract.
    /// </summary>
    public interface IDashboardService
    {
        Task<int> GetClientsCountAsync();
        Task<int> GetPhotoShootsCountAsync();
        Task<int> GetPendingPhotoShootsCountAsync();
        Task<decimal> GetTotalRevenueAsync();
        Task<decimal> GetOutstandingInvoicesAsync();
        Task<IEnumerable<Invoice>> GetRecentInvoicesAsync(int count = 5);
        Task<IEnumerable<PhotoShoot>> GetUpcomingPhotoShootsAsync(int count = 5);
        Task<DashboardViewModel> GetDashboardDataAsync();

        // New methods for enhanced dashboard
        Task<IEnumerable<BookingRequest>> GetPendingBookingsAsync(int count = 5);
        Task<IEnumerable<Contract>> GetContractsAwaitingSignatureAsync(int count = 5);
        Task<IEnumerable<Invoice>> GetOverdueInvoicesAsync();
        Task<OverdueAgingBreakdown> GetOverdueAgingBreakdownAsync();
        Task<IEnumerable<PhotoShoot>> GetTodaysScheduleAsync();
        Task<IEnumerable<Activity>> GetRecentActivitiesAsync(int count = 10);

        // Cache management
        void ClearDashboardCache();
    }
}
