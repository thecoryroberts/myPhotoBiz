using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using MyPhotoBiz.Data;
using MyPhotoBiz.Enums;
using MyPhotoBiz.Models;
using MyPhotoBiz.ViewModels;

namespace MyPhotoBiz.Services
{
    // COMPLETED: [MEDIUM] Added in-memory caching for dashboard stats (5-minute cache)
    // TODO: [FEATURE] Add revenue forecast/trend analysis
    // TODO: [FEATURE] Add photographer utilization stats
    // TODO: [FEATURE] Add galleries expiring soon alert
    public class DashboardService : IDashboardService
    {
        private readonly ApplicationDbContext _context;
        private readonly IMemoryCache _cache;
        private const int CacheDurationMinutes = 5;

        public DashboardService(ApplicationDbContext context, IMemoryCache cache)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _cache = cache ?? throw new ArgumentNullException(nameof(cache));
        }

        public async Task<int> GetClientsCountAsync() =>
            await _context.ClientProfiles.CountAsync();

        public async Task<int> GetPhotoShootsCountAsync() =>
            await _context.PhotoShoots.CountAsync();

        public async Task<int> GetPendingPhotoShootsCountAsync() =>
            await _context.PhotoShoots
                .Where(p => p.Status == PhotoShootStatus.Scheduled)
                .CountAsync();

        public async Task<decimal> GetTotalRevenueAsync()
        {
            var amounts = await _context.Invoices
                .Where(i => i.Status == InvoiceStatus.Paid)
                .Select(i => i.Amount + i.Tax)
                .ToListAsync();

            return amounts.Sum();
        }

        public async Task<decimal> GetOutstandingInvoicesAsync()
        {
            var amounts = await _context.Invoices
                .Where(i => i.Status == InvoiceStatus.Pending || i.Status == InvoiceStatus.Overdue)
                .Select(i => i.Amount + i.Tax)
                .ToListAsync();

            return amounts.Sum();
        }

        public async Task<IEnumerable<Invoice>> GetRecentInvoicesAsync(int count = 5)
        {
            return await _context.Invoices
                .Include(i => i.ClientProfile)
                .Include(i => i.PhotoShoot)
                .OrderByDescending(i => i.InvoiceDate)
                .Take(count)
                .ToListAsync();
        }

        public async Task<IEnumerable<PhotoShoot>> GetUpcomingPhotoShootsAsync(int count = 5)
        {
            return await _context.PhotoShoots
                .Include(p => p.ClientProfile)
                    .ThenInclude(cp => cp!.User)
                .Where(p => p.ScheduledDate >= DateTime.Today && !p.IsDeleted)
                .OrderBy(p => p.ScheduledDate)
                .Take(count)
                .ToListAsync();
        }

        public async Task<IEnumerable<BookingRequest>> GetPendingBookingsAsync(int count = 5)
        {
            return await _context.BookingRequests
                .Include(br => br.ClientProfile)
                    .ThenInclude(cp => cp!.User)
                .Where(br => br.Status == BookingStatus.Pending)
                .OrderBy(br => br.PreferredDate)
                .Take(count)
                .ToListAsync();
        }

        public async Task<IEnumerable<Contract>> GetContractsAwaitingSignatureAsync(int count = 5)
        {
            return await _context.Contracts
                .Include(c => c.ClientProfile)
                    .ThenInclude(cp => cp!.User)
                .Include(c => c.PhotoShoot)
                .Where(c => c.Status == ContractStatus.PendingSignature)
                .OrderBy(c => c.CreatedDate)
                .Take(count)
                .ToListAsync();
        }

        public async Task<IEnumerable<Invoice>> GetOverdueInvoicesAsync()
        {
            return await _context.Invoices
                .Include(i => i.ClientProfile)
                    .ThenInclude(cp => cp!.User)
                .Where(i => i.Status == InvoiceStatus.Overdue && !i.IsDeleted)
                .OrderBy(i => i.DueDate)
                .ToListAsync();
        }

        public async Task<OverdueAgingBreakdown> GetOverdueAgingBreakdownAsync()
        {
            var today = DateTime.Today;
            var overdueInvoices = await _context.Invoices
                .Where(i => i.Status == InvoiceStatus.Overdue && !i.IsDeleted)
                .Select(i => new { i.DueDate, Total = i.Amount + i.Tax })
                .ToListAsync();

            var breakdown = new OverdueAgingBreakdown();

            foreach (var inv in overdueInvoices)
            {
                var daysOverdue = (today - inv.DueDate).Days;

                if (daysOverdue <= 30)
                {
                    breakdown.Amount1To30Days += inv.Total;
                    breakdown.Count1To30Days++;
                }
                else if (daysOverdue <= 60)
                {
                    breakdown.Amount31To60Days += inv.Total;
                    breakdown.Count31To60Days++;
                }
                else if (daysOverdue <= 90)
                {
                    breakdown.Amount61To90Days += inv.Total;
                    breakdown.Count61To90Days++;
                }
                else
                {
                    breakdown.AmountOver90Days += inv.Total;
                    breakdown.CountOver90Days++;
                }
            }

            return breakdown;
        }

        public async Task<IEnumerable<PhotoShoot>> GetTodaysScheduleAsync()
        {
            var today = DateTime.Today;
            var tomorrow = today.AddDays(1);

            return await _context.PhotoShoots
                .Include(p => p.ClientProfile)
                    .ThenInclude(cp => cp!.User)
                .Include(p => p.PhotographerProfile)
                    .ThenInclude(pp => pp!.User)
                .Where(p => p.ScheduledDate >= today && p.ScheduledDate < tomorrow && !p.IsDeleted)
                .OrderBy(p => p.ScheduledDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<Activity>> GetRecentActivitiesAsync(int count = 10)
        {
            return await _context.Activities
                .OrderByDescending(a => a.CreatedAt)
                .Take(count)
                .ToListAsync();
        }

        public async Task<DashboardViewModel> GetDashboardDataAsync()
        {
            // Try to get cached dashboard data
            const string cacheKey = "DashboardData";

            if (_cache.TryGetValue(cacheKey, out DashboardViewModel? cachedData) && cachedData != null)
            {
                return cachedData;
            }

            // If not cached, fetch fresh data
            var totalClients = await GetClientsCountAsync();
            var pendingPhotoShoots = await GetPendingPhotoShootsCountAsync();
            var completedPhotoShoots = await _context.PhotoShoots
                .CountAsync(p => p.Status == PhotoShootStatus.Completed);

            var totalRevenue = await GetTotalRevenueAsync();
            var outstandingInvoices = await GetOutstandingInvoicesAsync();

            var upcomingPhotoShoots = await GetUpcomingPhotoShootsAsync(5);
            var recentInvoices = await GetRecentInvoicesAsync(5);
            var recentClients = await _context.ClientProfiles
                .Include(c => c.User)
                .OrderByDescending(c => c.Id)
                .Take(5)
                .ToListAsync();

            // Calculate monthly revenue (last 12 months)
            var monthlyRevenue = new Dictionary<string, decimal>();
            for (int i = 11; i >= 0; i--)
            {
                var month = DateTime.Now.AddMonths(-i);
                var monthKey = month.ToString("MMM yyyy");
                
                // Sum on client side because SQLite provider doesn't support SUM on decimal expressions
                var monthValues = await _context.Invoices
                    .Where(inv => inv.InvoiceDate.Month == month.Month &&
                                  inv.InvoiceDate.Year == month.Year &&
                                  inv.Status == InvoiceStatus.Paid)
                    .Select(inv => inv.Amount + inv.Tax)
                    .ToListAsync();
                
                monthlyRevenue[monthKey] = monthValues.Sum();
            }

            // Calculate photoshoot status data for charts
            var photoshootStatusData = new Dictionary<string, int>
            {
                ["Scheduled"] = await _context.PhotoShoots.CountAsync(p => p.Status == PhotoShootStatus.Scheduled),
                ["Completed"] = completedPhotoShoots,
                ["Cancelled"] = await _context.PhotoShoots.CountAsync(p => p.Status == PhotoShootStatus.Cancelled),
                ["InProgress"] = await _context.PhotoShoots.CountAsync(p => p.Status == PhotoShootStatus.InProgress)
            };

            // Convert PhotoShoots to PhotoShootViewModel
            var recentPhotoshoots = upcomingPhotoShoots.Select(ps => new PhotoShootViewModel
            {
                Id = ps.Id,
                Title = ps.Title,
                ClientId = ps.ClientProfileId,
                Location = ps.Location ?? string.Empty,
                ScheduledDate = ps.ScheduledDate,
                UpdatedDate = ps.UpdatedDate,
                Status = ps.Status,
                Price = ps.Price,
                Notes = ps.Notes,
                DurationHours = ps.DurationHours,
                DurationMinutes = ps.DurationMinutes,
                ClientProfile = ps.ClientProfile
            }).ToList();

            // Get pending bookings
            var pendingBookings = await GetPendingBookingsAsync(5);
            var pendingBookingsCount = await _context.BookingRequests
                .CountAsync(br => br.Status == BookingStatus.Pending);

            // Get contracts awaiting signature
            var contractsAwaitingSignature = await GetContractsAwaitingSignatureAsync(5);
            var contractsAwaitingSignatureCount = await _context.Contracts
                .CountAsync(c => c.Status == ContractStatus.PendingSignature);

            // Get overdue invoices
            var overdueInvoices = await GetOverdueInvoicesAsync();
            var overdueAging = await GetOverdueAgingBreakdownAsync();
            var overdueAmountValues = await _context.Invoices
                .Where(i => i.Status == InvoiceStatus.Overdue && !i.IsDeleted)
                .Select(i => i.Amount + i.Tax)
                .ToListAsync();
            var overdueAmount = overdueAmountValues.Sum();

            // Get today's schedule
            var todaysSchedule = await GetTodaysScheduleAsync();
            var todaysScheduleViewModels = todaysSchedule.Select(ps => new PhotoShootViewModel
            {
                Id = ps.Id,
                Title = ps.Title,
                ClientId = ps.ClientProfileId,
                Location = ps.Location ?? string.Empty,
                ScheduledDate = ps.ScheduledDate,
                EndTime = ps.EndTime,
                Status = ps.Status,
                Price = ps.Price,
                DurationHours = ps.DurationHours,
                DurationMinutes = ps.DurationMinutes,
                ClientProfile = ps.ClientProfile
            }).ToList();

            // Get recent activities
            var recentActivities = await GetRecentActivitiesAsync(10);

            // Calculate comparison metrics
            var currentMonth = DateTime.Now;
            var lastMonth = currentMonth.AddMonths(-1);

            var currentMonthRevenueValues = await _context.Invoices
                .Where(i => i.InvoiceDate.Month == currentMonth.Month &&
                            i.InvoiceDate.Year == currentMonth.Year &&
                            i.Status == InvoiceStatus.Paid)
                .Select(i => i.Amount + i.Tax)
                .ToListAsync();
            var currentMonthRevenue = currentMonthRevenueValues.Sum();

            var lastMonthRevenueValues = await _context.Invoices
                .Where(i => i.InvoiceDate.Month == lastMonth.Month &&
                            i.InvoiceDate.Year == lastMonth.Year &&
                            i.Status == InvoiceStatus.Paid)
                .Select(i => i.Amount + i.Tax)
                .ToListAsync();
            var lastMonthRevenue = lastMonthRevenueValues.Sum();

            var revenueChangePercent = lastMonthRevenue > 0
                ? ((currentMonthRevenue - lastMonthRevenue) / lastMonthRevenue) * 100
                : (currentMonthRevenue > 0 ? 100 : 0);

            // Calculate yearly revenue properly
            var yearStart = new DateTime(currentMonth.Year, 1, 1);
            var yearlyRevenueValues = await _context.Invoices
                .Where(i => i.InvoiceDate >= yearStart &&
                            i.Status == InvoiceStatus.Paid)
                .Select(i => i.Amount + i.Tax)
                .ToListAsync();
            var yearlyRevenue = yearlyRevenueValues.Sum();

            var dashboardData = new DashboardViewModel
            {
                TotalClients = totalClients,
                UpcomingPhotoshoots = pendingPhotoShoots,
                CompletedPhotoshoots = completedPhotoShoots,
                MonthlyRevenue = currentMonthRevenue,
                YearlyRevenue = yearlyRevenue,
                PendingInvoiceAmount = outstandingInvoices,
                RecentPhotoshoots = recentPhotoshoots,
                RecentInvoices = recentInvoices.ToList(),
                RecentClients = recentClients,
                MonthlyRevenueData = monthlyRevenue,
                PhotoshootStatusData = photoshootStatusData,

                // New properties
                PendingBookings = pendingBookings.ToList(),
                PendingBookingsCount = pendingBookingsCount,
                ContractsAwaitingSignature = contractsAwaitingSignature.ToList(),
                ContractsAwaitingSignatureCount = contractsAwaitingSignatureCount,
                OverdueInvoices = overdueInvoices.ToList(),
                OverdueInvoicesCount = overdueInvoices.Count(),
                OverdueAmount = overdueAmount,
                OverdueAging = overdueAging,
                TodaysSchedule = todaysScheduleViewModels,
                TodaysShootsCount = todaysScheduleViewModels.Count,
                RecentActivities = recentActivities.ToList(),
                LastMonthRevenue = lastMonthRevenue,
                RevenueChangePercent = revenueChangePercent
            };

            // Cache the dashboard data for 5 minutes
            var cacheOptions = new MemoryCacheEntryOptions()
                .SetSlidingExpiration(TimeSpan.FromMinutes(CacheDurationMinutes))
                .SetAbsoluteExpiration(TimeSpan.FromMinutes(CacheDurationMinutes * 2));

            _cache.Set(cacheKey, dashboardData, cacheOptions);

            return dashboardData;
        }

        /// <summary>
        /// Clears the dashboard cache - useful after data changes
        /// </summary>
        public void ClearDashboardCache()
        {
            _cache.Remove("DashboardData");
        }
    }
}