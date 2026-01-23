using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using MyPhotoBiz.Data;
using MyPhotoBiz.Enums;
using MyPhotoBiz.Helpers;
using MyPhotoBiz.Models;
using MyPhotoBiz.ViewModels;

namespace MyPhotoBiz.Services
{
    /// <summary>
    /// Service for aggregating dashboard statistics and analytics.
    /// Features: In-memory caching, revenue tracking,
    /// booking statistics, and photographer performance metrics.
    /// </summary>
    public class DashboardService : IDashboardService
    {
        private readonly ApplicationDbContext _context;
        private readonly IMemoryCache _cache;
        private const string DashboardCacheKey = "DashboardData";

        public DashboardService(ApplicationDbContext context, IMemoryCache cache)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _cache = cache ?? throw new ArgumentNullException(nameof(cache));
        }

        #region Private Utilization Helpers

        /// <summary>
        /// Calculates photographer utilization metrics for the current month
        /// </summary>
        private async Task<PhotographerUtilization> GetUtilizationMetricsAsync()
        {
            var today = DateTime.Today;
            var monthStart = new DateTime(today.Year, today.Month, 1);
            var monthEnd = monthStart.AddMonths(1).AddDays(-1);
            var next7Days = today.AddDays(7);

            // Get all scheduled/in-progress shoots for this month
            var monthShoots = await _context.PhotoShoots
                .Where(p => !p.IsDeleted &&
                           p.ScheduledDate >= monthStart &&
                           p.ScheduledDate <= monthEnd &&
                           (p.Status == PhotoShootStatus.Scheduled || p.Status == PhotoShootStatus.InProgress || p.Status == PhotoShootStatus.Completed))
                .Select(p => p.ScheduledDate.Date)
                .ToListAsync();

            // Get shoots for next 7 days
            var next7DaysShoots = await _context.PhotoShoots
                .Where(p => !p.IsDeleted &&
                           p.ScheduledDate >= today &&
                           p.ScheduledDate < next7Days &&
                           (p.Status == PhotoShootStatus.Scheduled || p.Status == PhotoShootStatus.InProgress))
                .Select(p => p.ScheduledDate.Date)
                .ToListAsync();

            // Calculate booked days (unique dates with shoots)
            var bookedDates = monthShoots.Distinct().ToList();
            var totalDays = (monthEnd - monthStart).Days + 1;
            var bookedDays = bookedDates.Count;

            // Build daily breakdown for the current month
            var dailyBreakdown = new List<DayUtilization>();
            for (var date = monthStart; date <= monthEnd; date = date.AddDays(1))
            {
                var shootCount = monthShoots.Count(d => d == date);
                dailyBreakdown.Add(new DayUtilization
                {
                    Date = date,
                    ShootCount = shootCount,
                    IsToday = date == today,
                    IsWeekend = date.DayOfWeek == DayOfWeek.Saturday || date.DayOfWeek == DayOfWeek.Sunday
                });
            }

            var next7DaysBookedCount = next7DaysShoots.Distinct().Count();

            return new PhotographerUtilization
            {
                TotalDaysInMonth = totalDays,
                BookedDays = bookedDays,
                AvailableDays = totalDays - bookedDays,
                UtilizationPercent = totalDays > 0 ? Math.Round((double)bookedDays / totalDays * 100, 1) : 0,
                Next7DaysTotal = 7,
                Next7DaysBooked = next7DaysBookedCount,
                Next7DaysAvailable = 7 - next7DaysBookedCount,
                DailyBreakdown = dailyBreakdown
            };
        }

        #endregion

        #region Private Revenue Helpers

        /// <summary>
        /// Calculates revenue for a specific month/year
        /// </summary>
        private async Task<decimal> GetRevenueForMonthAsync(int month, int year)
        {
            return await _context.Invoices
                .Where(i => i.InvoiceDate.Month == month &&
                            i.InvoiceDate.Year == year &&
                            i.Status == InvoiceStatus.Paid)
                .SumAsync(i => i.Amount + i.Tax);
        }

        /// <summary>
        /// Calculates revenue from a start date
        /// </summary>
        private async Task<decimal> GetRevenueSinceAsync(DateTime startDate)
        {
            return await _context.Invoices
                .Where(i => i.InvoiceDate >= startDate && i.Status == InvoiceStatus.Paid)
                .SumAsync(i => i.Amount + i.Tax);
        }

        /// <summary>
        /// Gets monthly revenue data for the last N months
        /// </summary>
        private async Task<Dictionary<string, decimal>> GetMonthlyRevenueDataAsync(int months = 12)
        {
            var startDate = DateTime.Now.AddMonths(-(months - 1));
            startDate = new DateTime(startDate.Year, startDate.Month, 1);

            var rawData = await _context.Invoices
                .Where(inv => inv.InvoiceDate >= startDate && inv.Status == InvoiceStatus.Paid)
                .GroupBy(inv => new { inv.InvoiceDate.Year, inv.InvoiceDate.Month })
                .Select(g => new
                {
                    g.Key.Year,
                    g.Key.Month,
                    Total = g.Sum(inv => inv.Amount + inv.Tax)
                })
                .ToListAsync();

            var result = new Dictionary<string, decimal>();
            for (int i = months - 1; i >= 0; i--)
            {
                var month = DateTime.Now.AddMonths(-i);
                var monthKey = month.ToString("MMM yyyy");
                var data = rawData.FirstOrDefault(r => r.Year == month.Year && r.Month == month.Month);
                result[monthKey] = data?.Total ?? 0m;
            }

            return result;
        }

        /// <summary>
        /// Gets photoshoot status counts
        /// </summary>
        private async Task<Dictionary<string, int>> GetPhotoshootStatusCountsAsync()
        {
            var statusCounts = await _context.PhotoShoots
                .Where(p => !p.IsDeleted)
                .GroupBy(p => p.Status)
                .Select(g => new { Status = g.Key, Count = g.Count() })
                .ToListAsync();

            return new Dictionary<string, int>
            {
                ["Scheduled"] = statusCounts.FirstOrDefault(s => s.Status == PhotoShootStatus.Scheduled)?.Count ?? 0,
                ["Completed"] = statusCounts.FirstOrDefault(s => s.Status == PhotoShootStatus.Completed)?.Count ?? 0,
                ["Cancelled"] = statusCounts.FirstOrDefault(s => s.Status == PhotoShootStatus.Cancelled)?.Count ?? 0,
                ["InProgress"] = statusCounts.FirstOrDefault(s => s.Status == PhotoShootStatus.InProgress)?.Count ?? 0
            };
        }

        /// <summary>
        /// Maps PhotoShoot to PhotoShootViewModel
        /// </summary>
        private static PhotoShootViewModel MapToViewModel(PhotoShoot ps)
        {
            return new PhotoShootViewModel
            {
                Id = ps.Id,
                Title = ps.Title,
                ClientId = ps.ClientProfileId,
                Location = ps.Location ?? string.Empty,
                ScheduledDate = ps.ScheduledDate,
                EndTime = ps.EndTime,
                UpdatedDate = ps.UpdatedDate,
                Status = ps.Status,
                Price = ps.Price,
                Notes = ps.Notes,
                DurationHours = ps.DurationHours,
                DurationMinutes = ps.DurationMinutes,
                ClientProfile = ps.ClientProfile
            };
        }

        #endregion

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
            if (_cache.TryGetValue(DashboardCacheKey, out DashboardViewModel? cachedData) && cachedData != null)
            {
                return cachedData;
            }

            // Fetch all data using helper methods
            var now = DateTime.Now;
            var currentMonth = now;
            var lastMonth = now.AddMonths(-1);
            var yearStart = new DateTime(now.Year, 1, 1);

            // Run independent queries in parallel for better performance
            var totalClientsTask = GetClientsCountAsync();
            var pendingPhotoShootsTask = GetPendingPhotoShootsCountAsync();
            var completedPhotoShootsTask = _context.PhotoShoots.CountAsync(p => p.Status == PhotoShootStatus.Completed);
            var outstandingInvoicesTask = GetOutstandingInvoicesAsync();
            var upcomingPhotoShootsTask = GetUpcomingPhotoShootsAsync(5);
            var recentInvoicesTask = GetRecentInvoicesAsync(5);
            var monthlyRevenueDataTask = GetMonthlyRevenueDataAsync(12);
            var photoshootStatusDataTask = GetPhotoshootStatusCountsAsync();
            var pendingBookingsTask = GetPendingBookingsAsync(5);
            var pendingBookingsCountTask = _context.BookingRequests.CountAsync(br => br.Status == BookingStatus.Pending);
            var contractsAwaitingTask = GetContractsAwaitingSignatureAsync(5);
            var contractsCountTask = _context.Contracts.CountAsync(c => c.Status == ContractStatus.PendingSignature);
            var overdueInvoicesTask = GetOverdueInvoicesAsync();
            var overdueAgingTask = GetOverdueAgingBreakdownAsync();
            var overdueAmountTask = _context.Invoices
                .Where(i => i.Status == InvoiceStatus.Overdue && !i.IsDeleted)
                .SumAsync(i => i.Amount + i.Tax);
            var todaysScheduleTask = GetTodaysScheduleAsync();
            var recentActivitiesTask = GetRecentActivitiesAsync(10);
            var currentMonthRevenueTask = GetRevenueForMonthAsync(currentMonth.Month, currentMonth.Year);
            var lastMonthRevenueTask = GetRevenueForMonthAsync(lastMonth.Month, lastMonth.Year);
            var yearlyRevenueTask = GetRevenueSinceAsync(yearStart);
            var recentClientsTask = _context.ClientProfiles
                .Include(c => c.User)
                .OrderByDescending(c => c.Id)
                .Take(5)
                .ToListAsync();
            var utilizationTask = GetUtilizationMetricsAsync();

            // Await all tasks
            await Task.WhenAll(
                totalClientsTask, pendingPhotoShootsTask, completedPhotoShootsTask,
                outstandingInvoicesTask, upcomingPhotoShootsTask, recentInvoicesTask,
                monthlyRevenueDataTask, photoshootStatusDataTask, pendingBookingsTask,
                pendingBookingsCountTask, contractsAwaitingTask, contractsCountTask,
                overdueInvoicesTask, overdueAgingTask, overdueAmountTask,
                todaysScheduleTask, recentActivitiesTask, currentMonthRevenueTask,
                lastMonthRevenueTask, yearlyRevenueTask, recentClientsTask, utilizationTask
            );

            // Get results
            var currentMonthRevenue = await currentMonthRevenueTask;
            var lastMonthRevenue = await lastMonthRevenueTask;
            var overdueInvoices = (await overdueInvoicesTask).ToList();

            var revenueChangePercent = lastMonthRevenue > 0
                ? ((currentMonthRevenue - lastMonthRevenue) / lastMonthRevenue) * 100
                : (currentMonthRevenue > 0 ? 100 : 0);

            var todaysScheduleViewModels = (await todaysScheduleTask).Select(MapToViewModel).ToList();

            var dashboardData = new DashboardViewModel
            {
                TotalClients = await totalClientsTask,
                UpcomingPhotoshoots = await pendingPhotoShootsTask,
                CompletedPhotoshoots = await completedPhotoShootsTask,
                MonthlyRevenue = currentMonthRevenue,
                YearlyRevenue = await yearlyRevenueTask,
                PendingInvoiceAmount = await outstandingInvoicesTask,
                RecentPhotoshoots = (await upcomingPhotoShootsTask).Select(MapToViewModel).ToList(),
                RecentInvoices = (await recentInvoicesTask).ToList(),
                RecentClients = await recentClientsTask,
                MonthlyRevenueData = await monthlyRevenueDataTask,
                PhotoshootStatusData = await photoshootStatusDataTask,
                PendingBookings = (await pendingBookingsTask).ToList(),
                PendingBookingsCount = await pendingBookingsCountTask,
                ContractsAwaitingSignature = (await contractsAwaitingTask).ToList(),
                ContractsAwaitingSignatureCount = await contractsCountTask,
                OverdueInvoices = overdueInvoices,
                OverdueInvoicesCount = overdueInvoices.Count,
                OverdueAmount = await overdueAmountTask,
                OverdueAging = await overdueAgingTask,
                TodaysSchedule = todaysScheduleViewModels,
                TodaysShootsCount = todaysScheduleViewModels.Count,
                RecentActivities = (await recentActivitiesTask).ToList(),
                LastMonthRevenue = lastMonthRevenue,
                RevenueChangePercent = revenueChangePercent,
                Utilization = await utilizationTask
            };

            // Cache the dashboard data
            var cacheOptions = new MemoryCacheEntryOptions()
                .SetSlidingExpiration(TimeSpan.FromMinutes(AppConstants.Cache.DashboardCacheMinutes))
                .SetAbsoluteExpiration(TimeSpan.FromMinutes(AppConstants.Cache.DashboardCacheMinutes * 2));

            _cache.Set(DashboardCacheKey, dashboardData, cacheOptions);

            return dashboardData;
        }

        /// <summary>
        /// Clears the dashboard cache - useful after data changes
        /// </summary>
        public void ClearDashboardCache()
        {
            _cache.Remove(DashboardCacheKey);
        }
    }
}