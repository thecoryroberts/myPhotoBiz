using MyPhotoBiz.Enums;
using MyPhotoBiz.Models;
using MyPhotoBiz.ViewModels;

namespace MyPhotoBiz.Services
{
    /// <summary>
    /// Defines the contract service contract for managing contracts.
    /// Handles CRUD operations, status transitions, signature processing,
    /// and auto-expiration of pending contracts.
    /// </summary>
    public interface IContractService
    {
        // Read operations
        Task<List<Contract>> GetAllContractsAsync(bool includeDeleted = false);
        Task<Contract?> GetContractByIdAsync(int id);
        Task<Contract?> GetContractForSigningAsync(int id);

        // CRUD operations
        Task<Contract> CreateContractAsync(
            CreateContractViewModel model,
            string? pdfPath);
        Task<Contract?> UpdateContractAsync(
            int id,
            EditContractViewModel model,
            string? newPdfPath);
        Task<bool> SoftDeleteContractAsync(int id);

        // Status transitions
        Task<(bool Success, string? ErrorMessage)> SendForSignatureAsync(int id);
        Task<(bool Success, string? ErrorMessage, string? BadgeName)> SignContractAsync(
            int id,
            string signatureBase64);

        // Auto-expiration (lazy, on-access)
        Task ExpireStaleContractsAsync();

        // Template and selection helpers
        Task<List<ContractTemplateSelectionViewModel>> GetContractTemplatesAsync();
        Task<object?> GetContractTemplateContentAsync(int id);
        Task<List<BadgeSelectionViewModel>> GetBadgeSelectionsAsync();

        // File management
        Task<string> SavePdfFileAsync(IFormFile pdfFile);
        void DeletePdfFile(string? pdfPath);
        string SaveSignature(string signatureBase64);
    }
}
