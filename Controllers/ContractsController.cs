using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyPhotoBiz.Data;
using MyPhotoBiz.Enums;
using MyPhotoBiz.Models;
using MyPhotoBiz.Services;
using MyPhotoBiz.ViewModels;

namespace MyPhotoBiz.Controllers
{
#pragma warning disable CS8602
    /// <summary>
    /// Controller for managing contracts with clients.
    /// Supports contract templates, PDF upload/replacement, and client assignment.
    /// </summary>
    /// <summary>
    /// Handles HTTP requests for contracts.
    /// </summary>
    [Authorize]
    public class ContractsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<ContractsController> _logger;
        private readonly IClientService _clientService;
        private readonly IPhotoShootService _photoShootService;
        private readonly IBadgeService _badgeService;

        public ContractsController(
            ApplicationDbContext context,
            ILogger<ContractsController> logger,
            IClientService clientService,
            IPhotoShootService photoShootService,
            IBadgeService badgeService)
        {
            _context = context;
            _logger = logger;
            _clientService = clientService;
            _photoShootService = photoShootService;
            _badgeService = badgeService;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                var contracts = await _context.Contracts
                    .AsNoTracking()
                    .Include(c => c.ClientProfile).ThenInclude(cp => cp.User)
                    .Include(c => c.PhotoShoot)
                    .OrderByDescending(c => c.CreatedDate)
                    .ToListAsync();

                return View(contracts);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving contracts");
                TempData["Error"] = "An error occurred while loading contracts.";
                return View(new List<Contract>());
            }
        }

        public async Task<IActionResult> Create()
        {
            var viewModel = new CreateContractViewModel
            {
                AvailableTemplates = await GetContractTemplatesAsync(),
                AvailableClients = await _clientService.GetClientSelectionsAsync(),
                AvailablePhotoShoots = await _photoShootService.GetPhotoShootSelectionsAsync(),
                AvailableBadges = await _badgeService.GetBadgeSelectionsAsync()
            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateContractViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    string? pdfPath = null;
                    if (model.PdfFile != null && model.PdfFile.Length > 0)
                    {
                        pdfPath = await SavePdfFileAsync(model.PdfFile);
                    }

                    var contract = new Contract
                    {
                        Title = model.Title,
                        Content = model.Content,
                        PdfFilePath = pdfPath,
                        ClientProfileId = model.ClientId,
                        PhotoShootId = model.PhotoShootId,
                        CreatedDate = DateTime.UtcNow,
                        Status = ContractStatus.Draft,
                        AwardBadgeOnSign = model.AwardBadgeOnSign,
                        BadgeToAwardId = model.BadgeToAwardId
                    };

                    _context.Contracts.Add(contract);
                    await _context.SaveChangesAsync();

                    TempData["Success"] = "Contract created successfully!";
                    return RedirectToAction(nameof(Details), new { id = contract.Id });
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error creating contract");
                    ModelState.AddModelError("", "An error occurred while creating the contract.");
                }
            }

            model.AvailableClients = await _clientService.GetClientSelectionsAsync();
            model.AvailablePhotoShoots = await _photoShootService.GetPhotoShootSelectionsAsync();
            model.AvailableBadges = await _badgeService.GetBadgeSelectionsAsync();
            return View(model);
        }

        public async Task<IActionResult> Edit(int id)
        {
            var contract = await _context.Contracts
                .Include(c => c.ClientProfile).ThenInclude(cp => cp.User)
                .Include(c => c.PhotoShoot)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (contract == null)
                return NotFound();

            var viewModel = new EditContractViewModel
            {
                Id = contract.Id,
                Title = contract.Title,
                Content = contract.Content ?? "",
                ExistingPdfPath = contract.PdfFilePath,
                ClientId = contract.ClientProfileId,
                PhotoShootId = contract.PhotoShootId,
                Status = contract.Status,
                CreatedDate = contract.CreatedDate,
                AwardBadgeOnSign = contract.AwardBadgeOnSign,
                BadgeToAwardId = contract.BadgeToAwardId,
                AvailableClients = await _clientService.GetClientSelectionsAsync(),
                AvailablePhotoShoots = await _photoShootService.GetPhotoShootSelectionsAsync(),
                AvailableBadges = await _badgeService.GetBadgeSelectionsAsync()
            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, EditContractViewModel model)
        {
            if (id != model.Id)
                return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    var contract = await _context.Contracts.FindAsync(id);
                    if (contract == null)
                        return NotFound();

                    // Handle PDF upload
                    if (model.PdfFile != null && model.PdfFile.Length > 0)
                    {
                        // Delete old PDF if exists
                        if (!string.IsNullOrEmpty(contract.PdfFilePath))
                        {
                            DeletePdfFile(contract.PdfFilePath);
                        }

                        // Save new PDF
                        contract.PdfFilePath = await SavePdfFileAsync(model.PdfFile);
                    }

                    contract.Title = model.Title;
                    contract.Content = model.Content;
                    contract.ClientProfileId = model.ClientId;
                    contract.PhotoShootId = model.PhotoShootId;
                    contract.Status = model.Status;
                    contract.AwardBadgeOnSign = model.AwardBadgeOnSign;
                    contract.BadgeToAwardId = model.BadgeToAwardId;

                    await _context.SaveChangesAsync();

                    TempData["Success"] = "Contract updated successfully!";
                    return RedirectToAction(nameof(Details), new { id = contract.Id });
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error updating contract");
                    ModelState.AddModelError("", "An error occurred while updating the contract.");
                }
            }

            model.AvailableClients = await _clientService.GetClientSelectionsAsync();
            model.AvailablePhotoShoots = await _photoShootService.GetPhotoShootSelectionsAsync();
            model.AvailableBadges = await _badgeService.GetBadgeSelectionsAsync();
            return View(model);
        }

        public async Task<IActionResult> Details(int id)
        {
            var contract = await _context.Contracts
                .AsNoTracking()
                .Include(c => c.ClientProfile).ThenInclude(cp => cp.User)
                .Include(c => c.PhotoShoot)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (contract == null)
                return NotFound();

            var contractClientUser = contract.ClientProfile?.User;
            var viewModel = new ContractDetailsViewModel
            {
                Id = contract.Id,
                Title = contract.Title,
                Content = contract.Content,
                CreatedDate = contract.CreatedDate,
                SignedDate = contract.SignedDate,
                SentDate = contract.SentDate,
                SignatureImagePath = contract.SignatureImagePath,
                Status = contract.Status,
                ClientId = contract.ClientProfileId,
                ClientName = contractClientUser != null ? $"{contractClientUser.FirstName} {contractClientUser.LastName}" : null,
                ClientEmail = contractClientUser?.Email,
                PhotoShootId = contract.PhotoShootId,
                PhotoShootTitle = contract.PhotoShoot?.Title
            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Photographer")]
        public async Task<IActionResult> SendForSignature(int id)
        {
            var contract = await _context.Contracts.FindAsync(id);
            if (contract == null)
                return NotFound();

            // Validate status transition - can only send Draft contracts
            if (contract.Status != ContractStatus.Draft)
            {
                TempData["Error"] = "Only draft contracts can be sent for signature.";
                return RedirectToAction(nameof(Details), new { id });
            }

            contract.Status = ContractStatus.PendingSignature;
            contract.SentDate = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            TempData["Success"] = "Contract sent for signature successfully!";
            return RedirectToAction(nameof(Details), new { id });
        }

        [AllowAnonymous] // Allow clients to sign without full authentication
        public async Task<IActionResult> Sign(int id)
        {
            var contract = await _context.Contracts
                .Include(c => c.ClientProfile).ThenInclude(cp => cp.User)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (contract == null)
                return NotFound();

            if (contract.Status == ContractStatus.Signed)
            {
                TempData["Error"] = "This contract has already been signed.";
                return RedirectToAction(nameof(Details), new { id });
            }

            if (contract.Status == ContractStatus.Expired)
            {
                TempData["Error"] = "This contract has expired and cannot be signed.";
                return RedirectToAction(nameof(Details), new { id });
            }

            if (contract.Status == ContractStatus.Draft)
            {
                TempData["Error"] = "This contract has not been sent for signature yet.";
                return RedirectToAction(nameof(Details), new { id });
            }

            var signClientUser = contract.ClientProfile?.User;
            var viewModel = new SignContractViewModel
            {
                Id = contract.Id,
                Title = contract.Title,
                Content = contract.Content,
                ClientName = signClientUser != null ? $"{signClientUser.FirstName} {signClientUser.LastName}" : null,
                CreatedDate = contract.CreatedDate
            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [AllowAnonymous]
        public async Task<IActionResult> Sign(int id, string signatureBase64)
        {
            var contract = await _context.Contracts
                .Include(c => c.ClientProfile).ThenInclude(cp => cp.User)
                .Include(c => c.BadgeToAward)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (contract == null)
                return NotFound();

            // Validate status transition
            if (contract.Status == ContractStatus.Signed)
            {
                TempData["Error"] = "This contract has already been signed.";
                return RedirectToAction(nameof(Details), new { id });
            }

            if (contract.Status == ContractStatus.Expired)
            {
                TempData["Error"] = "This contract has expired and cannot be signed.";
                return RedirectToAction(nameof(Details), new { id });
            }

            if (contract.Status != ContractStatus.PendingSignature)
            {
                TempData["Error"] = "This contract is not ready for signature.";
                return RedirectToAction(nameof(Details), new { id });
            }

            // Validate signature - must be a valid base64 PNG image
            if (string.IsNullOrWhiteSpace(signatureBase64))
            {
                TempData["Error"] = "Signature is required.";
                return RedirectToAction(nameof(Sign), new { id });
            }

            if (!IsValidSignatureBase64(signatureBase64))
            {
                TempData["Error"] = "Invalid signature format. Please draw your signature again.";
                return RedirectToAction(nameof(Sign), new { id });
            }

            try
            {
                var signaturePath = SaveSignature(signatureBase64);
                contract.SignatureImagePath = signaturePath;
                contract.SignedDate = DateTime.UtcNow;
                contract.Status = ContractStatus.Signed;

                await _context.SaveChangesAsync();

                // Award badge if configured
                if (contract.AwardBadgeOnSign && contract.BadgeToAwardId.HasValue && contract.ClientProfileId.HasValue)
                {
                    await AwardBadgeToClientAsync(contract.ClientProfileId.Value, contract.BadgeToAwardId.Value, contract.Id);
                    var badgeName = contract.BadgeToAward?.Name;
                    TempData["Success"] = badgeName != null
                        ? $"Contract signed successfully! Badge '{badgeName}' awarded!"
                        : "Contract signed successfully!";
                }
                else
                {
                    TempData["Success"] = "Contract signed successfully!";
                }

                return RedirectToAction(nameof(Details), new { id });
            }
            catch (FormatException)
            {
                TempData["Error"] = "Invalid signature format. Please draw your signature again.";
                return RedirectToAction(nameof(Sign), new { id });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error signing contract");
                TempData["Error"] = "An error occurred while signing the contract.";
                return RedirectToAction(nameof(Sign), new { id });
            }
        }

        private bool IsValidSignatureBase64(string base64)
        {
            try
            {
                // Check for valid base64 image format
                if (!base64.StartsWith("data:image/png;base64,") &&
                    !base64.StartsWith("data:image/jpeg;base64,"))
                {
                    // If no prefix, try to parse as raw base64
                    if (base64.Contains(','))
                        base64 = base64.Split(',')[1];
                }
                else
                {
                    base64 = base64.Split(',')[1];
                }

                // Validate base64 format and minimum length (empty signatures are too short)
                var bytes = Convert.FromBase64String(base64);
                return bytes.Length > 100; // A valid signature image should be more than 100 bytes
            }
            catch
            {
                return false;
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var contract = await _context.Contracts.FindAsync(id);
                if (contract == null)
                    return NotFound();

                // Delete signature file if exists
                if (!string.IsNullOrEmpty(contract.SignatureImagePath))
                {
                    var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", contract.SignatureImagePath.TrimStart('/'));
                    if (System.IO.File.Exists(filePath))
                    {
                        System.IO.File.Delete(filePath);
                    }
                }

                _context.Contracts.Remove(contract);
                await _context.SaveChangesAsync();

                TempData["Success"] = "Contract deleted successfully!";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting contract");
                TempData["Error"] = "An error occurred while deleting the contract.";
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetTemplate(int id)
        {
            try
            {
                var template = await _context.ContractTemplates
                    .Where(t => t.Id == id && t.IsActive)
                    .Select(t => new
                    {
                        t.Id,
                        t.Name,
                        t.ContentTemplate,
                        t.AwardBadgeOnSign,
                        t.BadgeToAwardId
                    })
                    .FirstOrDefaultAsync();

                if (template == null)
                    return NotFound();

                return Json(template);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving template {TemplateId}", id);
                return StatusCode(500, new { error = "Failed to load template" });
            }
        }

        private string SaveSignature(string base64)
        {
            // Ensure directory exists
            var signaturesDir = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "signatures");
            if (!Directory.Exists(signaturesDir))
            {
                Directory.CreateDirectory(signaturesDir);
            }

            // Extract base64 data and save
            var base64Data = base64.Contains(',') ? base64.Split(',')[1] : base64;
            var bytes = Convert.FromBase64String(base64Data);
            var fileName = $"{Guid.NewGuid()}.png";
            var filePath = Path.Combine(signaturesDir, fileName);

            System.IO.File.WriteAllBytes(filePath, bytes);

            return $"/signatures/{fileName}";
        }

        private async Task<List<ContractTemplateSelectionViewModel>> GetContractTemplatesAsync()
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
                .ToListAsync();
        }

        private async Task<string> SavePdfFileAsync(IFormFile pdfFile)
        {
            // Ensure directory exists
            var contractsDir = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "contracts");
            if (!Directory.Exists(contractsDir))
            {
                Directory.CreateDirectory(contractsDir);
            }

            // Generate unique filename
            var fileName = $"{Guid.NewGuid()}_{Path.GetFileName(pdfFile.FileName)}";
            var filePath = Path.Combine(contractsDir, fileName);

            // Save file
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await pdfFile.CopyToAsync(stream);
            }

            return $"/uploads/contracts/{fileName}";
        }

        private void DeletePdfFile(string? pdfPath)
        {
            try
            {
                if (!string.IsNullOrEmpty(pdfPath))
                {
                    // Convert web path to physical path
                    var physicalPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", pdfPath.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));

                    if (System.IO.File.Exists(physicalPath))
                    {
                        System.IO.File.Delete(physicalPath);
                        _logger.LogInformation($"Deleted PDF file: {physicalPath}");
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting PDF file: {pdfPath}");
                // Don't throw - file deletion failure shouldn't break the update
            }
        }

        private async Task AwardBadgeToClientAsync(int clientProfileId, int badgeId, int? contractId = null)
        {
            // Check if client already has this badge
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

                _logger.LogInformation($"Badge {badgeId} awarded to client profile {clientProfileId}");
            }
        }
    }
}
