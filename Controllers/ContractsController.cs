using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyPhotoBiz.Enums;
using MyPhotoBiz.Models;
using MyPhotoBiz.Services;
using MyPhotoBiz.ViewModels;

namespace MyPhotoBiz.Controllers
{
    /// <summary>
    /// Controller for managing contracts with clients.
    /// Delegates business logic to IContractService; handles views, redirects, and form binding.
    /// </summary>
    [Authorize]
    public class ContractsController : Controller
    {
        private readonly IContractService _contractService;
        private readonly ILogger<ContractsController> _logger;
        private readonly IClientService _clientService;
        private readonly IPhotoShootService _photoShootService;
        private readonly IContractVariableService _contractVariableService;

        public ContractsController(
            IContractService contractService,
            ILogger<ContractsController> logger,
            IClientService clientService,
            IPhotoShootService photoShootService,
            IContractVariableService contractVariableService)
        {
            _contractService = contractService;
            _logger = logger;
            _clientService = clientService;
            _photoShootService = photoShootService;
            _contractVariableService = contractVariableService;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                var contracts = await _contractService.GetAllContractsAsync();
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
            var customVariables = await _contractVariableService.GetActiveCustomVariablesAsync();

            var viewModel = new CreateContractViewModel
            {
                AvailableTemplates = await _contractService.GetContractTemplatesAsync(),
                AvailableClients = await _clientService.GetClientSelectionsAsync(),
                AvailablePhotoShoots = await _photoShootService.GetPhotoShootSelectionsAsync(),
                AvailableBadges = await _contractService.GetBadgeSelectionsAsync(),
                CustomVariables = customVariables.Select(v => new CustomVariableInputViewModel
                {
                    VariableId = v.Id,
                    VariableName = v.Name,
                    Description = v.Description,
                    DefaultValue = v.DefaultValue,
                    Value = v.DefaultValue
                }).ToList()
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
                        pdfPath = await _contractService.SavePdfFileAsync(model.PdfFile);

                    var contract = await _contractService.CreateContractAsync(model, pdfPath);

                    TempData["Success"] = "Contract created successfully!";
                    return RedirectToAction(nameof(Details), new { id = contract.Id });
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error creating contract");
                    ModelState.AddModelError("", "An error occurred while creating the contract.");
                }
            }

            await PopulateCreateViewModelAsync(model);
            return View(model);
        }

        public async Task<IActionResult> Edit(int id)
        {
            var contract = await _contractService.GetContractByIdAsync(id);
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
                AvailableBadges = await _contractService.GetBadgeSelectionsAsync()
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
                    string? newPdfPath = null;
                    if (model.PdfFile != null && model.PdfFile.Length > 0)
                        newPdfPath = await _contractService.SavePdfFileAsync(model.PdfFile);

                    var contract = await _contractService.UpdateContractAsync(id, model, newPdfPath);
                    if (contract == null)
                        return NotFound();

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
            model.AvailableBadges = await _contractService.GetBadgeSelectionsAsync();
            return View(model);
        }

        public async Task<IActionResult> Details(int id)
        {
            var contract = await _contractService.GetContractByIdAsync(id);
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
            var (success, errorMessage) = await _contractService.SendForSignatureAsync(id);

            if (!success)
            {
                TempData["Error"] = errorMessage;
                return RedirectToAction(nameof(Details), new { id });
            }

            TempData["Success"] = "Contract sent for signature successfully!";
            return RedirectToAction(nameof(Details), new { id });
        }

        [AllowAnonymous]
        public async Task<IActionResult> Sign(int id)
        {
            var contract = await _contractService.GetContractForSigningAsync(id);
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
            var (success, errorMessage, badgeName) = await _contractService.SignContractAsync(id, signatureBase64);

            if (!success)
            {
                var isSignatureError = errorMessage != null &&
                    (errorMessage.Contains("Signature is required") || errorMessage.Contains("Invalid signature format"));

                TempData["Error"] = errorMessage;
                return RedirectToAction(isSignatureError ? nameof(Sign) : nameof(Details), new { id });
            }

            TempData["Success"] = badgeName != null
                ? $"Contract signed successfully! Badge '{badgeName}' awarded!"
                : "Contract signed successfully!";

            return RedirectToAction(nameof(Details), new { id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var deleted = await _contractService.SoftDeleteContractAsync(id);
                if (!deleted)
                    return NotFound();

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
                var template = await _contractService.GetContractTemplateContentAsync(id);
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

        /// <summary>
        /// Previews contract content with variable replacement.
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> PreviewContent([FromBody] PreviewContractRequest request)
        {
            try
            {
                if (string.IsNullOrEmpty(request.Content))
                    return Json(new { preview = "" });

                var preview = await _contractVariableService.GeneratePreviewAsync(request.Content);
                return Json(new { preview });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating contract preview");
                return StatusCode(500, new { error = "Failed to generate preview" });
            }
        }

        /// <summary>
        /// Gets all available variables for the template editor.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetAvailableVariables()
        {
            try
            {
                var systemVariables = _contractVariableService.GetSystemVariableDefinitions()
                    .Select(kvp => new
                    {
                        Name = kvp.Key,
                        Placeholder = $"{{{{{kvp.Key}}}}}",
                        Description = (string?)kvp.Value,
                        IsSystem = true
                    });

                var customVariables = (await _contractVariableService.GetActiveCustomVariablesAsync())
                    .Select(v => new
                    {
                        v.Name,
                        Placeholder = v.Placeholder,
                        v.Description,
                        IsSystem = false
                    });

                var allVariables = systemVariables.Concat(customVariables)
                    .OrderBy(v => v.Name)
                    .ToList();

                return Json(allVariables);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving available variables");
                return StatusCode(500, new { error = "Failed to load variables" });
            }
        }

        #region Private Helpers

        private async Task PopulateCreateViewModelAsync(CreateContractViewModel model)
        {
            model.AvailableTemplates = await _contractService.GetContractTemplatesAsync();
            model.AvailableClients = await _clientService.GetClientSelectionsAsync();
            model.AvailablePhotoShoots = await _photoShootService.GetPhotoShootSelectionsAsync();
            model.AvailableBadges = await _contractService.GetBadgeSelectionsAsync();
            var customVariables = await _contractVariableService.GetActiveCustomVariablesAsync();
            var existingValues = model.CustomVariables?.ToDictionary(cv => cv.VariableId, cv => cv.Value) ?? new Dictionary<int, string?>();
            model.CustomVariables = customVariables.Select(v => new CustomVariableInputViewModel
            {
                VariableId = v.Id,
                VariableName = v.Name,
                Description = v.Description,
                DefaultValue = v.DefaultValue,
                Value = existingValues.TryGetValue(v.Id, out var existingValue) ? existingValue : v.DefaultValue
            }).ToList();
        }

        #endregion
    }
}
