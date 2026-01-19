using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MyPhotoBiz.Data;
using MyPhotoBiz.Enums;
using MyPhotoBiz.Models;
using MyPhotoBiz.ViewModels;

namespace MyPhotoBiz.Services
{
    /// <summary>
    /// Service for managing print order operations
    /// </summary>
    public class PrintOrderService : IPrintOrderService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<PrintOrderService> _logger;

        // Configurable shipping cost (could be moved to app settings)
        private const decimal DefaultShippingCost = 5.00m;

        public PrintOrderService(
            ApplicationDbContext context,
            ILogger<PrintOrderService> logger)
        {
            _context = context;
            _logger = logger;
        }

        #region Order Management

        public async Task<WorkflowResult<PrintOrder>> CreateOrderAsync(
            int gallerySessionId,
            PrintOrderViewModel orderModel)
        {
            try
            {
                var session = await _context.GallerySessions
                    .Include(s => s.Gallery)
                    .FirstOrDefaultAsync(s => s.Id == gallerySessionId);

                if (session == null)
                    return WorkflowResult<PrintOrder>.Failed("Gallery session not found.");

                if (!session.Gallery.IsActive || session.Gallery.ExpiryDate < DateTime.UtcNow)
                    return WorkflowResult<PrintOrder>.Failed("Gallery is no longer active.");

                if (orderModel.Items == null || orderModel.Items.Count == 0)
                    return WorkflowResult<PrintOrder>.Failed("No items in order.");

                var order = new PrintOrder
                {
                    GallerySessionId = gallerySessionId,
                    OrderNumber = GenerateOrderNumber(),
                    ClientName = orderModel.ClientName?.Trim(),
                    ClientEmail = orderModel.ClientEmail?.Trim(),
                    ClientPhone = orderModel.ClientPhone?.Trim(),
                    Status = OrderStatus.Pending,
                    CreatedDate = DateTime.UtcNow,
                    TotalPrice = 0
                };

                decimal totalPrice = 0;
                var validItems = new List<PrintItem>();

                foreach (var item in orderModel.Items)
                {
                    if (item.Quantity <= 0 || string.IsNullOrEmpty(item.Size) || string.IsNullOrEmpty(item.FinishType))
                        continue;

                    // Verify photo belongs to gallery
                    var photo = await _context.Photos
                        .Include(p => p.Album)
                            .ThenInclude(a => a.Galleries)
                        .AsNoTracking()
                        .FirstOrDefaultAsync(p => p.Id == item.PhotoId &&
                            p.Album.Galleries.Any(g => g.Id == session.GalleryId));

                    if (photo == null)
                        continue;

                    var pricing = await GetPriceAsync(item.Size, item.FinishType);
                    if (pricing == null)
                        continue;

                    var printItem = new PrintItem
                    {
                        PhotoId = item.PhotoId,
                        Size = item.Size,
                        FinishType = item.FinishType,
                        Quantity = item.Quantity,
                        UnitPrice = pricing.Value,
                        PrintOrder = order
                    };

                    validItems.Add(printItem);
                    totalPrice += pricing.Value * item.Quantity;
                }

                if (validItems.Count == 0)
                    return WorkflowResult<PrintOrder>.Failed("No valid items in order.");

                order.TotalPrice = totalPrice + GetShippingCost();
                foreach (var item in validItems)
                    order.Items.Add(item);

                _context.PrintOrders.Add(order);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Order {OrderNumber} created for {ClientEmail}",
                    order.OrderNumber, order.ClientEmail);

                return WorkflowResult<PrintOrder>.Succeeded(order);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating print order");
                return WorkflowResult<PrintOrder>.Failed("An error occurred while creating the order.");
            }
        }

        public async Task<PrintOrder?> GetOrderAsync(int orderId)
        {
            return await _context.PrintOrders
                .Include(o => o.Session)
                    .ThenInclude(s => s!.Gallery)
                .Include(o => o.Items)
                    .ThenInclude(i => i.Photo)
                .AsNoTracking()
                .FirstOrDefaultAsync(o => o.Id == orderId);
        }

        public async Task<PrintOrder?> GetOrderByNumberAsync(string orderNumber)
        {
            return await _context.PrintOrders
                .Include(o => o.Session)
                    .ThenInclude(s => s!.Gallery)
                .Include(o => o.Items)
                    .ThenInclude(i => i.Photo)
                .AsNoTracking()
                .FirstOrDefaultAsync(o => o.OrderNumber == orderNumber);
        }

        public async Task<List<PrintOrder>> GetOrdersAsync(
            OrderStatus? status = null,
            DateTime? fromDate = null,
            DateTime? toDate = null,
            string? searchTerm = null)
        {
            var query = _context.PrintOrders
                .Include(o => o.Session)
                    .ThenInclude(s => s!.Gallery)
                .Include(o => o.Items)
                .AsNoTracking()
                .AsQueryable();

            if (status.HasValue)
                query = query.Where(o => o.Status == status.Value);

            if (fromDate.HasValue)
                query = query.Where(o => o.CreatedDate >= fromDate.Value);

            if (toDate.HasValue)
                query = query.Where(o => o.CreatedDate <= toDate.Value);

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                var term = searchTerm.ToLower();
                query = query.Where(o =>
                    (o.OrderNumber != null && o.OrderNumber.ToLower().Contains(term)) ||
                    (o.ClientName != null && o.ClientName.ToLower().Contains(term)) ||
                    (o.ClientEmail != null && o.ClientEmail.ToLower().Contains(term)));
            }

            return await query
                .OrderByDescending(o => o.CreatedDate)
                .ToListAsync();
        }

        public async Task<List<PrintOrder>> GetOrdersBySessionAsync(int gallerySessionId)
        {
            return await _context.PrintOrders
                .Include(o => o.Items)
                    .ThenInclude(i => i.Photo)
                .Where(o => o.GallerySessionId == gallerySessionId)
                .OrderByDescending(o => o.CreatedDate)
                .AsNoTracking()
                .ToListAsync();
        }

        #endregion

        #region Status Management

        public async Task<WorkflowResult<PrintOrder>> ProcessOrderAsync(int orderId, string? printLabOrderId = null)
        {
            return await UpdateOrderStatusAsync(orderId, OrderStatus.Processing, order =>
            {
                if (order.Status != OrderStatus.Pending)
                    return "Order must be in Pending status to process.";

                order.PrintLabOrderId = printLabOrderId;
                return null;
            });
        }

        public async Task<WorkflowResult<PrintOrder>> ShipOrderAsync(int orderId, string? trackingNumber = null)
        {
            return await UpdateOrderStatusAsync(orderId, OrderStatus.Shipped, order =>
            {
                if (order.Status != OrderStatus.Processing)
                    return "Order must be in Processing status to ship.";

                order.ShippedDate = DateTime.UtcNow;
                // Could store tracking number in a notes field or separate property
                return null;
            });
        }

        public async Task<WorkflowResult<PrintOrder>> DeliverOrderAsync(int orderId)
        {
            return await UpdateOrderStatusAsync(orderId, OrderStatus.Delivered, order =>
            {
                if (order.Status != OrderStatus.Shipped)
                    return "Order must be in Shipped status to mark as delivered.";

                order.DeliveredDate = DateTime.UtcNow;
                return null;
            });
        }

        public async Task<WorkflowResult<PrintOrder>> CancelOrderAsync(int orderId, string reason)
        {
            return await UpdateOrderStatusAsync(orderId, OrderStatus.Cancelled, order =>
            {
                if (order.Status == OrderStatus.Shipped || order.Status == OrderStatus.Delivered)
                    return "Cannot cancel an order that has been shipped or delivered.";

                if (order.Status == OrderStatus.Cancelled)
                    return "Order is already cancelled.";

                order.CancelledDate = DateTime.UtcNow;
                _logger.LogInformation("Order {OrderNumber} cancelled. Reason: {Reason}",
                    order.OrderNumber, reason);
                return null;
            });
        }

        public async Task<WorkflowResult<PrintOrder>> MarkOrderPaidAsync(int orderId)
        {
            try
            {
                var order = await _context.PrintOrders.FindAsync(orderId);
                if (order == null)
                    return WorkflowResult<PrintOrder>.Failed("Order not found.");

                if (order.PaidDate.HasValue)
                    return WorkflowResult<PrintOrder>.Failed("Order is already marked as paid.");

                order.PaidDate = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                _logger.LogInformation("Order {OrderNumber} marked as paid", order.OrderNumber);
                return WorkflowResult<PrintOrder>.Succeeded(order);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error marking order {OrderId} as paid", orderId);
                return WorkflowResult<PrintOrder>.Failed("An error occurred while updating the order.");
            }
        }

        public async Task<WorkflowResult<PrintOrder>> RefundOrderAsync(int orderId)
        {
            try
            {
                var order = await _context.PrintOrders.FindAsync(orderId);
                if (order == null)
                    return WorkflowResult<PrintOrder>.Failed("Order not found.");

                if (!order.PaidDate.HasValue)
                    return WorkflowResult<PrintOrder>.Failed("Order has not been paid.");

                if (order.RefundedDate.HasValue)
                    return WorkflowResult<PrintOrder>.Failed("Order has already been refunded.");

                order.RefundedDate = DateTime.UtcNow;
                order.Status = OrderStatus.Cancelled;
                order.CancelledDate = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                _logger.LogInformation("Order {OrderNumber} refunded", order.OrderNumber);
                return WorkflowResult<PrintOrder>.Succeeded(order);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error refunding order {OrderId}", orderId);
                return WorkflowResult<PrintOrder>.Failed("An error occurred while processing the refund.");
            }
        }

        private async Task<WorkflowResult<PrintOrder>> UpdateOrderStatusAsync(
            int orderId,
            OrderStatus newStatus,
            Func<PrintOrder, string?> validate)
        {
            try
            {
                var order = await _context.PrintOrders.FindAsync(orderId);
                if (order == null)
                    return WorkflowResult<PrintOrder>.Failed("Order not found.");

                var validationError = validate(order);
                if (validationError != null)
                    return WorkflowResult<PrintOrder>.Failed(validationError);

                order.Status = newStatus;
                await _context.SaveChangesAsync();

                _logger.LogInformation("Order {OrderNumber} status changed to {Status}",
                    order.OrderNumber, newStatus);

                return WorkflowResult<PrintOrder>.Succeeded(order);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating order {OrderId} status to {Status}", orderId, newStatus);
                return WorkflowResult<PrintOrder>.Failed("An error occurred while updating the order.");
            }
        }

        #endregion

        #region Pricing

        public async Task<List<PrintPricing>> GetPricingAsync()
        {
            var pricing = await _context.PrintPricings
                .OrderBy(p => p.Size)
                .ThenBy(p => p.FinishType)
                .AsNoTracking()
                .ToListAsync();

            // Return defaults if no pricing configured
            if (pricing.Count == 0)
                return PrintPricing.GetDefaultPrices();

            return pricing;
        }

        public async Task<decimal?> GetPriceAsync(string size, string finishType)
        {
            var pricing = await _context.PrintPricings
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Size == size && p.FinishType == finishType);

            if (pricing != null)
                return pricing.Price;

            // Fallback to defaults
            var defaultPricing = PrintPricing.GetDefaultPrices()
                .FirstOrDefault(p => p.Size == size && p.FinishType == finishType);

            return defaultPricing?.Price;
        }

        public decimal GetShippingCost()
        {
            // Could be extended to read from app settings or database
            return DefaultShippingCost;
        }

        #endregion

        #region Cart

        public async Task<List<Photo>> GetCartPhotosAsync(int gallerySessionId)
        {
            return await _context.Proofs
                .Where(p => p.GallerySessionId == gallerySessionId && p.IsFavorite && p.Photo != null)
                .Include(p => p.Photo)
                .Select(p => p.Photo!)
                .OrderBy(p => p.DisplayOrder)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<(bool IsValid, string? ErrorMessage, GallerySession? Session)> ValidateSessionAsync(string sessionToken)
        {
            if (string.IsNullOrEmpty(sessionToken))
                return (false, "Session token is required.", null);

            var session = await _context.GallerySessions
                .Include(s => s.Gallery)
                .FirstOrDefaultAsync(s => s.SessionToken == sessionToken);

            if (session == null)
                return (false, "Session not found.", null);

            if (!session.Gallery.IsActive)
                return (false, "Gallery is no longer active.", session);

            if (session.Gallery.ExpiryDate < DateTime.UtcNow)
                return (false, "Gallery has expired.", session);

            return (true, null, session);
        }

        #endregion

        #region Statistics

        public async Task<PrintOrderStats> GetOrderStatsAsync()
        {
            var now = DateTime.UtcNow;
            var startOfMonth = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);

            var orders = await _context.PrintOrders
                .AsNoTracking()
                .ToListAsync();

            return new PrintOrderStats
            {
                TotalOrders = orders.Count,
                PendingOrders = orders.Count(o => o.Status == OrderStatus.Pending),
                ProcessingOrders = orders.Count(o => o.Status == OrderStatus.Processing),
                ShippedOrders = orders.Count(o => o.Status == OrderStatus.Shipped),
                DeliveredOrders = orders.Count(o => o.Status == OrderStatus.Delivered),
                CancelledOrders = orders.Count(o => o.Status == OrderStatus.Cancelled),
                TotalRevenue = orders
                    .Where(o => o.Status != OrderStatus.Cancelled && o.PaidDate.HasValue)
                    .Sum(o => o.TotalPrice),
                MonthlyRevenue = orders
                    .Where(o => o.CreatedDate >= startOfMonth &&
                                o.Status != OrderStatus.Cancelled &&
                                o.PaidDate.HasValue)
                    .Sum(o => o.TotalPrice),
                OrdersThisMonth = orders.Count(o => o.CreatedDate >= startOfMonth)
            };
        }

        #endregion

        #region Private Helpers

        private static string GenerateOrderNumber()
        {
            return $"ORD-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString()[..8].ToUpper()}";
        }

        #endregion
    }
}
