using MyPhotoBiz.Models;

namespace MyPhotoBiz.Services
{
    /// <summary>
    /// Service for orchestrating multi-step business workflows
    /// Simplifies complex operations into single method calls
    /// </summary>
    public interface IWorkflowService
    {
        #region Client Workflow

        /// <summary>
        /// Creates a client with all associated resources (user account, folder, profile)
        /// </summary>
        Task<WorkflowResult<ClientProfile>> CreateClientWithResourcesAsync(
            string firstName,
            string lastName,
            string email,
            string? phone = null,
            string? address = null);

        #endregion

        #region Booking Workflow

        /// <summary>
        /// Approves a booking request and creates associated photoshoot and contract
        /// </summary>
        Task<WorkflowResult<PhotoShoot>> ApproveBookingAsync(int bookingId, string approvedBy);

        /// <summary>
        /// Rejects a booking request with a reason
        /// </summary>
        Task<WorkflowResult<BookingRequest>> RejectBookingAsync(int bookingId, string reason, string rejectedBy);

        #endregion

        #region Photoshoot Workflow

        /// <summary>
        /// Marks a photoshoot as complete and creates album/gallery
        /// </summary>
        Task<WorkflowResult<Album>> CompletePhotoshootAsync(int photoshootId, bool createGallery = true);

        /// <summary>
        /// Marks a photoshoot as delivered after final payment
        /// </summary>
        Task<WorkflowResult<PhotoShoot>> DeliverPhotoshootAsync(int photoshootId);

        #endregion

        #region Invoice Workflow

        /// <summary>
        /// Generates an invoice from a completed photoshoot
        /// </summary>
        Task<WorkflowResult<Invoice>> GenerateInvoiceFromPhotoshootAsync(int photoshootId);

        /// <summary>
        /// Marks an invoice as paid and triggers delivery workflow if fully paid
        /// </summary>
        Task<WorkflowResult<Invoice>> MarkInvoicePaidAsync(int invoiceId, string paymentMethod, string? transactionId = null);

        #endregion

        #region Gallery Workflow

        /// <summary>
        /// Creates a gallery from an album with sharing settings
        /// </summary>
        Task<WorkflowResult<Gallery>> CreateGalleryFromAlbumAsync(
            int albumId,
            string name,
            DateTime expiryDate,
            bool notifyClient = true);

        #endregion
    }

    /// <summary>
    /// Result wrapper for workflow operations
    /// </summary>
    public class WorkflowResult<T>
    {
        public bool Success { get; set; }
        public T? Data { get; set; }
        public string? ErrorMessage { get; set; }
        public List<string> Warnings { get; set; } = new();

        public static WorkflowResult<T> Succeeded(T data, params string[] warnings)
        {
            return new WorkflowResult<T>
            {
                Success = true,
                Data = data,
                Warnings = warnings.ToList()
            };
        }

        public static WorkflowResult<T> Failed(string error)
        {
            return new WorkflowResult<T>
            {
                Success = false,
                ErrorMessage = error
            };
        }
    }
}
