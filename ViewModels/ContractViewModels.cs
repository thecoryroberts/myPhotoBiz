using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;
using MyPhotoBiz.Enums;

namespace MyPhotoBiz.ViewModels
{
    /// <summary>
    /// Represents view model data for create contract.
    /// </summary>
    public class CreateContractViewModel : IValidatableObject
    {
        [Display(Name = "Template")]
        public int? TemplateId { get; set; }

        [Required]
        [StringLength(200)]
        [Display(Name = "Contract Title")]
        public string Title { get; set; } = string.Empty;

        [Display(Name = "Contract Content")]
        public string? Content { get; set; }

        [Display(Name = "Upload PDF Contract")]
        public IFormFile? PdfFile { get; set; }

        [Display(Name = "Client")]
        public int? ClientId { get; set; }

        [Display(Name = "Photo Shoot")]
        public int? PhotoShootId { get; set; }

        [Display(Name = "Award Badge When Signed")]
        public bool AwardBadgeOnSign { get; set; } = false;

        [Display(Name = "Badge to Award")]
        public int? BadgeToAwardId { get; set; }

        public List<ContractTemplateSelectionViewModel> AvailableTemplates { get; set; } = new();
        public List<ClientSelectionViewModel> AvailableClients { get; set; } = new();
        public List<PhotoShootSelectionViewModel> AvailablePhotoShoots { get; set; } = new();
        public List<BadgeSelectionViewModel> AvailableBadges { get; set; } = new();

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            var hasContent = !string.IsNullOrWhiteSpace(Content);
            var hasPdf = PdfFile != null && PdfFile.Length > 0;

            if (!hasContent && !hasPdf)
            {
                yield return new ValidationResult(
                    "Please provide contract content or upload a PDF file.",
                    new[] { nameof(Content), nameof(PdfFile) });
            }
        }
    }

    /// <summary>
    /// Represents view model data for edit contract.
    /// </summary>
    public class EditContractViewModel : IValidatableObject
    {
        public int Id { get; set; }

        [Required]
        [StringLength(200)]
        public string Title { get; set; } = string.Empty;

        public string? Content { get; set; }

        [Display(Name = "Upload New PDF Contract")]
        public IFormFile? PdfFile { get; set; }

        public string? ExistingPdfPath { get; set; }

        public int? ClientId { get; set; }
        public int? PhotoShootId { get; set; }

        public ContractStatus Status { get; set; }
        public DateTime CreatedDate { get; set; }

        [Display(Name = "Award Badge When Signed")]
        public bool AwardBadgeOnSign { get; set; } = false;

        [Display(Name = "Badge to Award")]
        public int? BadgeToAwardId { get; set; }

        public List<ClientSelectionViewModel> AvailableClients { get; set; } = new();
        public List<PhotoShootSelectionViewModel> AvailablePhotoShoots { get; set; } = new();
        public List<BadgeSelectionViewModel> AvailableBadges { get; set; } = new();

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            var hasContent = !string.IsNullOrWhiteSpace(Content);
            var hasPdf = PdfFile != null && PdfFile.Length > 0;
            var hasExistingPdf = !string.IsNullOrWhiteSpace(ExistingPdfPath);

            if (!hasContent && !hasPdf && !hasExistingPdf)
            {
                yield return new ValidationResult(
                    "Please provide contract content or upload a PDF file.",
                    new[] { nameof(Content), nameof(PdfFile) });
            }
        }
    }

    /// <summary>
    /// Represents view model data for contract details.
    /// </summary>
    public class ContractDetailsViewModel
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Content { get; set; }
        public string? PdfFilePath { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? SignedDate { get; set; }
        public DateTime? SentDate { get; set; }
        public string? SignatureImagePath { get; set; }
        public ContractStatus Status { get; set; }

        public bool AwardBadgeOnSign { get; set; }
        public string? BadgeToAwardName { get; set; }

        public int? ClientId { get; set; }
        public string? ClientName { get; set; }
        public string? ClientEmail { get; set; }

        public int? PhotoShootId { get; set; }
        public string? PhotoShootTitle { get; set; }
    }

    /// <summary>
    /// Represents view model data for sign contract.
    /// </summary>
    public class SignContractViewModel
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Content { get; set; }
        public string? PdfFilePath { get; set; }
        public string? ClientName { get; set; }
        public DateTime CreatedDate { get; set; }
        public bool WillAwardBadge { get; set; }
        public string? BadgeName { get; set; }
    }

    /// <summary>
    /// Represents view model data for client selection.
    /// </summary>
    public class ClientSelectionViewModel
    {
        public int Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Name => FullName; // Alias for FullName
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
        public bool IsSelected { get; set; }
    }

    /// <summary>
    /// Represents view model data for photo shoot selection.
    /// </summary>
    public class PhotoShootSelectionViewModel
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public DateTime ShootDate { get; set; }
        public string? ClientName { get; set; }
    }

    /// <summary>
    /// Represents view model data for badge selection.
    /// </summary>
    public class BadgeSelectionViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? Icon { get; set; }
        public string Color { get; set; } = "#6c757d";
    }

    /// <summary>
    /// Represents view model data for contract template selection.
    /// </summary>
    public class ContractTemplateSelectionViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? Category { get; set; }
    }
}
