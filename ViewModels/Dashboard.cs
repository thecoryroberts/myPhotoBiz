using Microsoft.AspNetCore.Identity;
using MyPhotoBiz.Models;
using System.ComponentModel.DataAnnotations;

namespace MyPhotoBiz.ViewModels
{
    public class DashboardViewModel
    {
        // Counts
        public int TotalClients { get; set; } = 0;
        public int UpcomingPhotoshoots { get; set; } = 0;
        public int CompletedPhotoshoots { get; set; } = 0;
        public int TotalAlbums { get; set; } = 0;
        public int TotalPhotos { get; set; } = 0;

        // Financials
        public decimal MonthlyRevenue { get; set; } = 0m;
        public decimal YearlyRevenue { get; set; } = 0m;
        public decimal PendingInvoiceAmount { get; set; } = 0m;

        // Recent activity
        public IEnumerable<PhotoShootViewModel> RecentPhotoshoots { get; set; } = new List<PhotoShootViewModel>();
        public IEnumerable<Invoice> RecentInvoices { get; set; } = new List<Invoice>();
        public IEnumerable<ClientProfile> RecentClients { get; set; } = new List<ClientProfile>();

        // Chart data
        public IDictionary<string, decimal> MonthlyRevenueData { get; set; } = new Dictionary<string, decimal>();
        public IDictionary<string, int> PhotoshootStatusData { get; set; } = new Dictionary<string, int>();

        // Action Items - Pending bookings requiring attention
        public IEnumerable<BookingRequest> PendingBookings { get; set; } = new List<BookingRequest>();
        public int PendingBookingsCount { get; set; } = 0;

        // Contracts awaiting signature
        public IEnumerable<Contract> ContractsAwaitingSignature { get; set; } = new List<Contract>();
        public int ContractsAwaitingSignatureCount { get; set; } = 0;

        // Overdue invoices with aging breakdown
        public IEnumerable<Invoice> OverdueInvoices { get; set; } = new List<Invoice>();
        public int OverdueInvoicesCount { get; set; } = 0;
        public decimal OverdueAmount { get; set; } = 0m;
        public OverdueAgingBreakdown OverdueAging { get; set; } = new OverdueAgingBreakdown();

        // Today's schedule
        public IEnumerable<PhotoShootViewModel> TodaysSchedule { get; set; } = new List<PhotoShootViewModel>();
        public int TodaysShootsCount { get; set; } = 0;

        // Recent activity timeline
        public IEnumerable<Activity> RecentActivities { get; set; } = new List<Activity>();

        // Comparison metrics (vs last month/year)
        public decimal LastMonthRevenue { get; set; } = 0m;
        public decimal RevenueChangePercent { get; set; } = 0m;
        public int LastMonthClients { get; set; } = 0;
        public int ClientsChangePercent { get; set; } = 0;
        public int LastMonthPhotoshoots { get; set; } = 0;
        public int PhotoshootsChangePercent { get; set; } = 0;
    }

    public class OverdueAgingBreakdown
    {
        public decimal Amount1To30Days { get; set; } = 0m;
        public int Count1To30Days { get; set; } = 0;

        public decimal Amount31To60Days { get; set; } = 0m;
        public int Count31To60Days { get; set; } = 0;

        public decimal Amount61To90Days { get; set; } = 0m;
        public int Count61To90Days { get; set; } = 0;

        public decimal AmountOver90Days { get; set; } = 0m;
        public int CountOver90Days { get; set; } = 0;
    }
}