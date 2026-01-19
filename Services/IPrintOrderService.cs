using MyPhotoBiz.Enums;
using MyPhotoBiz.Models;
using MyPhotoBiz.ViewModels;

namespace MyPhotoBiz.Services
{
    /// <summary>
    /// Service for managing print order operations
    /// </summary>
    public interface IPrintOrderService
    {
        #region Order Management

        /// <summary>
        /// Creates a new print order from a gallery session
        /// </summary>
        Task<WorkflowResult<PrintOrder>> CreateOrderAsync(
            int gallerySessionId,
            PrintOrderViewModel orderModel);

        /// <summary>
        /// Gets an order by ID with all related data
        /// </summary>
        Task<PrintOrder?> GetOrderAsync(int orderId);

        /// <summary>
        /// Gets an order by order number
        /// </summary>
        Task<PrintOrder?> GetOrderByNumberAsync(string orderNumber);

        /// <summary>
        /// Gets all orders with optional filtering
        /// </summary>
        Task<List<PrintOrder>> GetOrdersAsync(
            OrderStatus? status = null,
            DateTime? fromDate = null,
            DateTime? toDate = null,
            string? searchTerm = null);

        /// <summary>
        /// Gets orders for a specific gallery session
        /// </summary>
        Task<List<PrintOrder>> GetOrdersBySessionAsync(int gallerySessionId);

        #endregion

        #region Status Management

        /// <summary>
        /// Updates order status to Processing
        /// </summary>
        Task<WorkflowResult<PrintOrder>> ProcessOrderAsync(int orderId, string? printLabOrderId = null);

        /// <summary>
        /// Updates order status to Shipped
        /// </summary>
        Task<WorkflowResult<PrintOrder>> ShipOrderAsync(int orderId, string? trackingNumber = null);

        /// <summary>
        /// Updates order status to Delivered
        /// </summary>
        Task<WorkflowResult<PrintOrder>> DeliverOrderAsync(int orderId);

        /// <summary>
        /// Cancels an order
        /// </summary>
        Task<WorkflowResult<PrintOrder>> CancelOrderAsync(int orderId, string reason);

        /// <summary>
        /// Marks an order as paid
        /// </summary>
        Task<WorkflowResult<PrintOrder>> MarkOrderPaidAsync(int orderId);

        /// <summary>
        /// Refunds an order
        /// </summary>
        Task<WorkflowResult<PrintOrder>> RefundOrderAsync(int orderId);

        #endregion

        #region Pricing

        /// <summary>
        /// Gets all print pricing options
        /// </summary>
        Task<List<PrintPricing>> GetPricingAsync();

        /// <summary>
        /// Gets price for a specific size and finish type
        /// </summary>
        Task<decimal?> GetPriceAsync(string size, string finishType);

        /// <summary>
        /// Gets shipping cost (configurable)
        /// </summary>
        decimal GetShippingCost();

        #endregion

        #region Cart

        /// <summary>
        /// Gets favorite photos for a gallery session (cart items)
        /// </summary>
        Task<List<Photo>> GetCartPhotosAsync(int gallerySessionId);

        /// <summary>
        /// Validates a gallery session is active and can accept orders
        /// </summary>
        Task<(bool IsValid, string? ErrorMessage, GallerySession? Session)> ValidateSessionAsync(string sessionToken);

        #endregion

        #region Statistics

        /// <summary>
        /// Gets order statistics for dashboard
        /// </summary>
        Task<PrintOrderStats> GetOrderStatsAsync();

        #endregion
    }

    /// <summary>
    /// Print order statistics for dashboard
    /// </summary>
    public class PrintOrderStats
    {
        public int TotalOrders { get; set; }
        public int PendingOrders { get; set; }
        public int ProcessingOrders { get; set; }
        public int ShippedOrders { get; set; }
        public int DeliveredOrders { get; set; }
        public int CancelledOrders { get; set; }
        public decimal TotalRevenue { get; set; }
        public decimal MonthlyRevenue { get; set; }
        public int OrdersThisMonth { get; set; }
    }
}
