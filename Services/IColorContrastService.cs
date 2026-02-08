using MyPhotoBiz.Enums;
using MyPhotoBiz.Models;
using MyPhotoBiz.ViewModels;

namespace MyPhotoBiz.Services
{
    public interface IColorContrastService
    {
        /// <summary>
        /// Calculate the contrast ratio between two colors.
        /// </summary>
        double GetContrastRatio(string color1, string color2);

        /// <summary>
        /// Check if contrast meets WCAG AA for normal text (4.5:1).
        /// </summary>
        bool MeetsWcagAA(string foreground, string background);

        /// <summary>
        /// Check if contrast meets WCAG AA for large text (3:1).
        /// </summary>
        bool MeetsWcagAALargeText(string foreground, string background);

        /// <summary>
        /// Check if contrast meets WCAG AA for UI components (3:1).
        /// </summary>
        bool MeetsWcagAAUIComponents(string foreground, string background);

        /// <summary>
        /// Check if contrast meets WCAG AAA for normal text (7:1).
        /// </summary>
        bool MeetsWcagAAA(string foreground, string background);

        /// <summary>
        /// Validate all branding colors and return warnings.
        /// </summary>
        ColorContrastValidationResult ValidateBrandingColors(
            string primaryColor,
            string secondaryColor,
            string accentColor,
            string successColor,
            string warningColor,
            string dangerColor,
            string backgroundColor = "#ffffff",
            string textColor = "#1f2937");

        /// <summary>
        /// Get the WCAG compliance level for a contrast ratio.
        /// </summary>
        WcagLevel GetWcagLevel(double contrastRatio);

        /// <summary>
        /// Suggest an adjusted color that meets the minimum contrast.
        /// </summary>
        string SuggestAccessibleColor(string color, string background, double minContrast = 4.5);
    }
}
     