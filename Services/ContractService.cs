using Microsoft.EntityFrameworkCore;
using MyPhotoBiz.Data;
using MyPhotoBiz.Enums;
using MyPhotoBiz.Models;
using MyPhotoBiz.ViewModels;

namespace MyPhotoBiz.Services
{
    /// <summary>
    /// Service for managing contracts: CRUD, status transitions, signature processing,
    /// badge awards, and lazy auto-expiration of stale PendingSignature contracts.
    /// </summary>
    public class ContractService : IContractService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<ContractService> _logger;
        private readonly IContractVariableService _contractVariableService;
        private readonly INotificationService _notificationService;

        public ContractService(
            ApplicationDbContext context,
            ILogger<ContractService> logger,
            IContractVariableService contractVariableService,
            INotificationService notificationService)
        {
            _context = context;
            _logger = logger;
            _contractVariableService = contractVariableService;
            _notificationService = notificationService;
        }

        #region Read Operations

        public async Task<List<Contract>> GetAllContractsAsync(bool includeDeleted = false)
        {
            // Expire stale contracts lazily before returning
            await ExpireStaleContractsAsync();

            var query = _context.Contracts
                .AsNoTracking()
                .Include(c => c.ClientProfile).ThenInclude(cp => cp!.User)
                .Include(c => c.PhotoShoot)
                .AsQueryable();

            if (!includeDeleted)
                query = query.Where(c => !c.IsDeleted);

            return await query
                .OrderByDescending(c => c.CreatedDate)
                .ToListAsync();
        }

        public async Task<Contract?> GetContractByIdAsync(int id)
        {
            // Expire stale contracts lazily before returning
            await ExpireStaleContractsAsync();

            return await _context.Contracts
                .AsNoTracking()
                .Include(c => c.ClientProfile).ThenInclude(cp => cp!.User)
                .Include(c => c.PhotoShoot)
                .FirstOrDefaultAsync(c => c.Id == id && !c.IsDeleted);
        }

        public async Task<Contract?> GetContractForSigningAsync(int id)
        {
            // Expire stale contracts lazily before returning
            await ExpireStaleContractsAsync();

            return await _context.Contracts
                .Include(c => c.ClientProfile).ThenInclude(cp => cp!.User)
                .Include(c => c.BadgeToAward)
                .FirstOrDefaultAsync(c => c.Id == id && !c.IsDeleted);
        }

        #endregion

        #region CRUD Operations

        public async Task<Contract> CreateContractAsync(
            CreateContractViewModel model,
            string? pdfPath)
        {
            // Get client and photoshoot for variable replacement
            ClientProfile? clientProfile = null;
            PhotoShoot? photoShoot = null;

            if (model.ClientId.HasValue)
            {
                clientProfile = await _context.ClientProfiles
                    .Include(c => c.User)
                    .FirstOrDefaultAsync(c => c.Id == model.ClientId.Value);
            }

            if (model.PhotoShootId.HasValue)
            {
                photoShoot = await _context.PhotoShoots
                    .Include(p => p.PhotographerProfile)
                        .ThenInclude(pp => pp!.User)
                    .FirstOrDefaultAsync(p => p.Id == model.PhotoShootId.Value);
            }

            // Build custom variable overrides from form input
            var customVariableOverrides = model.CustomVariables?
                .Where(cv => !string.IsNullOrEmpty(cv.Value))
                .ToDictionary(cv => cv.VariableId, cv => cv.Value ?? "");

            // Process the content to replace all variables
            var processedContent = await _contractVariableService.ReplaceVariablesAsync(
                model.Content ?? "",
                clientProfile,
                photoShoot,
                customVariableOverrides);

            var contract = new Contract
            {
                Title = model.Title,
                Content = processedContent,
                PdfFilePath = pdfPath,
                ClientProfileId = model.ClientId,
                PhotoShootId = model.PhotoShootId,
                CreatedDate = DateTime.UtcNow,
                Status = ContractStatus.Draft,
                AwardBadgeOnSign = model.AwardBadgeOnSign,
                BadgeToAwardId = model.BadgeToAwardId
            };

            using var transaction = await _context.Database.BeginTransactionAsync();

            _context.Contracts.Add(contract);
            await _context.SaveChangesAsync();

            // Save custom variable values for audit/reference
            if (customVariableOverrides != null && customVariableOverrides.Any())
            {
                foreach (var kvp in customVariableOverrides)
                {
                    _context.ContractVariableValues.Add(new ContractVariableValue
                    {
                        ContractId = contract.Id,
                        ContractVariableId = kvp.Key,
                        Value = kvp.Value
                    });
                }
            }
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            _logger.LogInformation("Contract {ContractId} created: {Title}", contract.Id, contract.Title);
            return contract;
        }

        public async Task<Contract?> UpdateContractAsync(
            int id,
            EditContractViewModel model,
            string? newPdfPath)
        {
            var contract = await _context.Contracts.FindAsync(id);
            if (contract == null || contract.IsDeleted)
                return null;

            // Handle PDF replacement
            if (newPdfPath != null)
            {
                if (!string.IsNullOrEmpty(contract.PdfFilePath))
                    DeletePdfFile(contract.PdfFilePath);
                contract.PdfFilePath = newPdfPath;
            }

            contract.Title = model.Title;
            contract.Content = model.Content;
            contract.ClientProfileId = model.ClientId;
            contract.PhotoShootId = model.PhotoShootId;
            contract.Status = model.Status;
            contract.AwardBadgeOnSign = model.AwardBadgeOnSign;
            contract.BadgeToAwardId = model.BadgeToAwardId;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Contract {ContractId} updated", id);
            return contract;
        }

        public async Task<bool> SoftDeleteContractAsync(int id)
        {
            var contract = await _context.Contracts.FindAsync(id);
            if (contract == null)
                return false;

            contract.IsDeleted = true;
            contract.DeletedDate = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            _logger.LogInformation("Contract {ContractId} soft-deleted", id);
            return true;
        }

        #endregion

        #region Status Transitions

        public async Task<(bool Success, string? ErrorMessage)> SendForSignatureAsync(int id)
        {
            var contract = await _context.Contracts.FindAsync(id);
            if (contract == null || contract.IsDeleted)
                return (false, "Contract not found.");

            if (contract.Status != ContractStatus.Draft)
                return (false, "Only draft contracts can be sent for signature.");

            contract.Status = ContractStatus.PendingSignature;
            contract.SentDate = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Contract {ContractId} sent for signature", id);
            return (true, null);
        }

        public async Task<(bool Success, string? ErrorMessage, string? BadgeName)> SignContractAsync(
            int id,
            string signatureBase64)
        {
            var contract = await _context.Contracts
                .Include(c => c.ClientProfile).ThenInclude(cp => cp!.User)
                .Include(c => c.BadgeToAward)
                .FirstOrDefaultAsync(c => c.Id == id && !c.IsDeleted);

            if (contract == null)
                return (false, "Contract not found.", null);

            if (contract.Status == ContractStatus.Signed)
                return (false, "This contract has already been signed.", null);

            if (contract.Status == ContractStatus.Expired)
                return (false, "This contract has expired and cannot be signed.", null);

            if (contract.Status != ContractStatus.PendingSignature)
                return (false, "This contract is not ready for signature.", null);

            // Validate signature
            if (string.IsNullOrWhiteSpace(signatureBase64))
                return (false, "Signature is required.", null);

            if (!IsValidSignatureBase64(signatureBase64))
                return (false, "Invalid signature format. Please draw your signature again.", null);

            var signaturePath = SaveSignature(signatureBase64);
            contract.SignatureImagePath = signaturePath;
            contract.SignedDate = DateTime.UtcNow;
            contract.Status = ContractStatus.Signed;

            await _context.SaveChangesAsync();

            // Award badge if configured
            string? badgeName = null;
            if (contract.AwardBadgeOnSign && contract.BadgeToAwardId.HasValue && contract.ClientProfileId.HasValue)
            {
                await AwardBadgeToClientAsync(contract.ClientProfileId.Value, contract.BadgeToAwardId.Value, contract.Id);
                badgeName = contract.BadgeToAward?.Name;
            }

            _logger.LogInformation("Contract {ContractId} signed", id);
            return (true, null, badgeName);
        }

        #endregion

        #region Auto-Expiration

        public async Task ExpireStaleContractsAsync()
        {
            var now = DateTime.UtcNow;
            var staleContracts = await _context.Contracts
                .Where(c => c.Status == ContractStatus.PendingSignature
                    && c.ExpirationDate.HasValue
                    && c.ExpirationDate.Value < now
                    && !c.IsDeleted)
                .Include(c => c.ClientProfile)
                .ToListAsync();

            if (!staleContracts.Any())
                return;

            foreach (var contract in staleContracts)
            {
                contract.Status = ContractStatus.Expired;
                _logger.LogInformation("Contract {ContractId} auto-expired (expiration date: {ExpirationDate})",
                    contract.Id, contract.ExpirationDate);

                // Notify the client if one is assigned
                if (contract.ClientProfile?.UserId != null)
                {
                    try
                    {
                        await _notificationService.CreateNotificationAsync(new Notification
                        {
                            UserId = contract.ClientProfile.UserId,
                            Title = "Contract Expired",
                            Message = $"The contract \"{contract.Title}\" has expired and can no longer be signed.",
                            Type = NotificationType.Warning,
                            CreatedDate = DateTime.UtcNow
                        });
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Failed to create expiration notification for contract {ContractId}", contract.Id);
                    }
                }
            }

            await _context.SaveChangesAsync();
        }

        #endregion

        #region Template and Selection Helpers

        public async Task<List<ContractTemplateSelectionViewModel>> GetContractTemplatesAsync()
        {
            return await _context.ContractTemplates
                .Where(t => t.IsActive)
                .OrderBy(t => t.Category)
                .ThenBy(t => t.Name)
                .Select(t => new ContractTemplateSelectionViewModel
                {
                    Id = t.Id,
                    Name = t.Name,
                    Description = t.Description,
                    Category = t.Category
                })
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<object?> GetContractTemplateContentAsync(int id)
        {
            return await _context.ContractTemplates
                .Where(t => t.Id == id && t.IsActive)
                .Select(t => new
                {
                    t.Id,
                    t.Name,
                    t.ContentTemplate,
                    t.AwardBadgeOnSign,
                    t.BadgeToAwardId
                })
                .AsNoTracking()
                .FirstOrDefaultAsync();
        }

        public async Task<List<BadgeSelectionViewModel>> GetBadgeSelectionsAsync()
        {
            return await _context.Badges
                .AsNoTracking()
                .Where(b => b.IsActive)
                .OrderBy(b => b.Name)
                .Select(b => new BadgeSelectionViewModel
                {
                    Id = b.Id,
                    Name = b.Name,
                    Description = b.Description,
                    Icon = b.Icon,
                    Color = b.Color
                })
                .ToListAsync();
        }

        #endregion

        #region File Management

        public async Task<string> SavePdfFileAsync(IFormFile pdfFile)
        {
            var contractsDir = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "contracts");
            if (!Directory.Exists(contractsDir))
                Directory.CreateDirectory(contractsDir);

            var fileName = $"{Guid.NewGuid()}_{Path.GetFileName(pdfFile.FileName)}";
            var filePath = Path.Combine(contractsDir, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await pdfFile.CopyToAsync(stream);
            }

            return $"/uploads/contracts/{fileName}";
        }

        public void DeletePdfFile(string? pdfPath)
        {
            try
            {
                if (!string.IsNullOrEmpty(pdfPath))
                {
                    var physicalPath = Path.Combine(
                        Directory.GetCurrentDirectory(), "wwwroot",
                        pdfPath.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));

                    if (File.Exists(physicalPath))
                    {
                        File.Delete(physicalPath);
                        _logger.LogInformation("Deleted PDF file: {Path}", physicalPath);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting PDF file: {PdfPath}", pdfPath);
            }
        }

        public string SaveSignature(string base64)
        {
            var signaturesDir = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "signatures");
            if (!Directory.Exists(signaturesDir))
                Directory.CreateDirectory(signaturesDir);

            var base64Data = base64.Contains(',') ? base64.Split(',')[1] : base64;
            var bytes = Convert.FromBase64String(base64Data);
            var fileName = $"{Guid.NewGuid()}.png";
            var filePath = Path.Combine(signaturesDir, fileName);

            File.WriteAllBytes(filePath, bytes);

            return $"/signatures/{fileName}";
        }

        #endregion

        #region Private Helpers

        private bool IsValidSignatureBase64(string base64)
        {
            try
            {
                if (!base64.StartsWith("data:image/png;base64,") &&
                    !base64.StartsWith("data:image/jpeg;base64,"))
                {
                    if (base64.Contains(','))
                        base64 = base64.Split(',')[1];
                }
                else
                {
                    base64 = base64.Split(',')[1];
                }

                var bytes = Convert.FromBase64String(base64);
                return bytes.Length > 100;
            }
            catch
            {
                return false;
            }
        }

        private async Task AwardBadgeToClientAsync(int clientProfileId, int badgeId, int? contractId = null)
        {
            var existingBadge = await _context.ClientBadges
                .FirstOrDefaultAsync(cb => cb.ClientProfileId == clientProfileId && cb.BadgeId == badgeId);

            if (existingBadge == null)
            {
                var clientBadge = new ClientBadge
                {
                    ClientProfileId = clientProfileId,
                    BadgeId = badgeId,
                    ContractId = contractId,
                    EarnedDate = DateTime.UtcNow,
                    Notes = contractId.HasValue ? "Awarded by contract signature" : null
                };

                _context.ClientBadges.Add(clientBadge);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Badge {BadgeId} awarded to client profile {ClientProfileId}", badgeId, clientProfileId);
            }
        }

        #endregion
    }
}
