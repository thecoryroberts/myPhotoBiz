using System.Text.RegularExpressions;

namespace MyPhotoBiz.Services
{
    /// <summary>
    /// Service for calculating WCAG 2.1 color contrast ratios.
    /// </summary>

    public class ColorContrastService : IColorContrastService
    {
        // WCAG thresholds
        private const double WcagAA_NormalText = 4.5;
        private const double WcagAA_LargeText = 3.0;
        private const double WcagAA_UIComponents = 3.0;
        private const double WcagAAA_NormalText = 7.0;

        public double GetContrastRatio(string color1, string color2)
        {
            var lum1 = GetRelativeLuminance(color1);
            var lum2 = GetRelativeLuminance(color2);

            var lighter = Math.Max(lum1, lum2);
            var darker = Math.Min(lum1, lum2);

            return (lighter + 0.05) / (darker + 0.05);
        }

        public bool MeetsWcagAA(string foreground, string background)
            => GetContrastRatio(foreground, background) >= WcagAA_NormalText;

        public bool MeetsWcagAALargeText(string foreground, string background)
            => GetContrastRatio(foreground, background) >= WcagAA_LargeText;

        public bool MeetsWcagAAUIComponents(string foreground, string background)
            => GetContrastRatio(foreground, background) >= WcagAA_UIComponents;

        public bool MeetsWcagAAA(string foreground, string background)
            => GetContrastRatio(foreground, background) >= WcagAAA_NormalText;

        public WcagLevel GetWcagLevel(double contrastRatio)
        {
            if (contrastRatio >= WcagAAA_NormalText) return WcagLevel.AAA;
            if (contrastRatio >= WcagAA_NormalText) return WcagLevel.AA;
            if (contrastRatio >= WcagAA_LargeText) return WcagLevel.AALargeOnly;
            return WcagLevel.Fail;
        }

        public ColorContrastValidationResult ValidateBrandingColors(
            string primaryColor,
            string secondaryColor,
            string accentColor,
            string successColor,
            string warningColor,
            string dangerColor,
            string backgroundColor = "#ffffff",
            string textColor = "#1f2937")
        {
            var result = new ColorContrastValidationResult();

            // Standard backgrounds to test against
            var lightBg = "#ffffff";
            var darkBg = "#1f2937";

            // Validate each color
            result.PrimaryContrast = ValidateColor("Primary", primaryColor, lightBg, darkBg, textColor);
            result.SecondaryContrast = ValidateColor("Secondary", secondaryColor, lightBg, darkBg, textColor);
            result.AccentContrast = ValidateColor("Accent", accentColor, lightBg, darkBg, textColor);
            result.SuccessContrast = ValidateColor("Success", successColor, lightBg, darkBg, textColor);
            result.WarningContrast = ValidateColor("Warning", warningColor, lightBg, darkBg, textColor);
            result.DangerContrast = ValidateColor("Danger", dangerColor, lightBg, darkBg, textColor);

            // Calculate overall validity
            result.IsValid = !result.AllContrasts.Any(c => c.HasCriticalFailure);
            result.HasWarnings = result.AllContrasts.Any(c => c.Warnings.Count > 0);

            return result;
        }

        private ColorContrastResult ValidateColor(string name, string color, string lightBg, string darkBg, string textColor)
        {
            var result = new ColorContrastResult { ColorName = name, ColorValue = color };

            // Contrast with white (for button text)
            var contrastWithWhite = GetContrastRatio(color, "#ffffff");
            result.ContrastWithWhite = contrastWithWhite;
            result.WhiteTextLevel = GetWcagLevel(contrastWithWhite);

            // Contrast with dark text
            var contrastWithDark = GetContrastRatio(color, "#000000");
            result.ContrastWithBlack = contrastWithDark;
            result.BlackTextLevel = GetWcagLevel(contrastWithDark);

            // Contrast with light background (for text/icons)
            var contrastWithLightBg = GetContrastRatio(color, lightBg);
            result.ContrastWithLightBg = contrastWithLightBg;
            result.LightBgLevel = GetWcagLevel(contrastWithLightBg);

            // Determine best text color for this as background
            result.RecommendedTextColor = contrastWithWhite >= contrastWithDark ? "#ffffff" : "#000000";

            // Generate warnings
            if (contrastWithLightBg < WcagAA_UIComponents)
            {
                result.Warnings.Add($"{name} color fails WCAG AA for UI components on light backgrounds (ratio: {contrastWithLightBg:F2}:1, requires 3:1)");
                result.HasCriticalFailure = true;
            }
            else if (contrastWithLightBg < WcagAA_NormalText)
            {
                result.Warnings.Add($"{name} color passes for large text/UI only on light backgrounds (ratio: {contrastWithLightBg:F2}:1)");
            }

            // Check if neither white nor black text works well on this color
            if (Math.Max(contrastWithWhite, contrastWithDark) < WcagAA_NormalText)
            {
                result.Warnings.Add($"{name} color may have poor text readability when used as a background");
            }

            // Suggest fix if needed
            if (result.HasCriticalFailure)
            {
                result.SuggestedFix = SuggestAccessibleColor(color, lightBg, WcagAA_UIComponents);
            }

            return result;
        }

        public string SuggestAccessibleColor(string color, string background, double minContrast = 4.5)
        {
            var bgLuminance = GetRelativeLuminance(background);

            // Convert to HSL for better hue preservation
            var (h, s, l) = RgbToHsl(color);

            // For light backgrounds (luminance > 0.5), we need darker colors (lower lightness)
            // For dark backgrounds, we need lighter colors (higher lightness)
            bool needsDarker = bgLuminance > 0.179; // 0.179 is roughly the threshold for light vs dark

            // Binary search for the right lightness value
            double minL = needsDarker ? 0.0 : l;
            double maxL = needsDarker ? l : 1.0;

            string bestColor = color;
            double bestContrast = GetContrastRatio(color, background);

            for (int i = 0; i < 50; i++)
            {
                double testL = (minL + maxL) / 2;
                var testColor = HslToRgb(h, s, testL);
                var contrast = GetContrastRatio(testColor, background);

                if (contrast >= minContrast)
                {
                    bestColor = testColor;
                    bestContrast = contrast;

                    // Try to get closer to original lightness while still passing
                    if (needsDarker)
                        minL = testL;
                    else
                        maxL = testL;
                }
                else
                {
                    // Need more contrast, go further from original
                    if (needsDarker)
                        maxL = testL;
                    else
                        minL = testL;
                }

                // Stop if we're close enough
                if (Math.Abs(maxL - minL) < 0.001)
                    break;
            }

            // If binary search failed, try extreme values
            if (bestContrast < minContrast)
            {
                var darkest = HslToRgb(h, s, 0.15);
                var lightest = HslToRgb(h, s, 0.85);

                var darkContrast = GetContrastRatio(darkest, background);
                var lightContrast = GetContrastRatio(lightest, background);

                if (darkContrast >= minContrast)
                    return darkest;
                if (lightContrast >= minContrast)
                    return lightest;

                // Return whichever is better
                return darkContrast > lightContrast ? darkest : lightest;
            }

            return bestColor;
        }

        private (double h, double s, double l) RgbToHsl(string hex)
        {
            var (r, g, b) = HexToRgb(hex);
            double rd = r / 255.0;
            double gd = g / 255.0;
            double bd = b / 255.0;

            double max = Math.Max(rd, Math.Max(gd, bd));
            double min = Math.Min(rd, Math.Min(gd, bd));
            double l = (max + min) / 2;
            double h = 0, s = 0;

            if (max != min)
            {
                double d = max - min;
                s = l > 0.5 ? d / (2 - max - min) : d / (max + min);

                if (max == rd)
                    h = ((gd - bd) / d + (gd < bd ? 6 : 0)) / 6;
                else if (max == gd)
                    h = ((bd - rd) / d + 2) / 6;
                else
                    h = ((rd - gd) / d + 4) / 6;
            }

            return (h, s, l);
        }

        private string HslToRgb(double h, double s, double l)
        {
            double r, g, b;

            if (s == 0)
            {
                r = g = b = l;
            }
            else
            {
                double q = l < 0.5 ? l * (1 + s) : l + s - l * s;
                double p = 2 * l - q;
                r = HueToRgb(p, q, h + 1.0 / 3);
                g = HueToRgb(p, q, h);
                b = HueToRgb(p, q, h - 1.0 / 3);
            }

            return RgbToHex(
                (int)Math.Round(r * 255),
                (int)Math.Round(g * 255),
                (int)Math.Round(b * 255));
        }

        private double HueToRgb(double p, double q, double t)
        {
            if (t < 0) t += 1;
            if (t > 1) t -= 1;
            if (t < 1.0 / 6) return p + (q - p) * 6 * t;
            if (t < 1.0 / 2) return q;
            if (t < 2.0 / 3) return p + (q - p) * (2.0 / 3 - t) * 6;
            return p;
        }

        private double GetRelativeLuminance(string hexColor)
        {
            var (r, g, b) = HexToRgb(hexColor);

            var rSrgb = r / 255.0;
            var gSrgb = g / 255.0;
            var bSrgb = b / 255.0;

            var rLinear = rSrgb <= 0.03928 ? rSrgb / 12.92 : Math.Pow((rSrgb + 0.055) / 1.055, 2.4);
            var gLinear = gSrgb <= 0.03928 ? gSrgb / 12.92 : Math.Pow((gSrgb + 0.055) / 1.055, 2.4);
            var bLinear = bSrgb <= 0.03928 ? bSrgb / 12.92 : Math.Pow((bSrgb + 0.055) / 1.055, 2.4);

            return 0.2126 * rLinear + 0.7152 * gLinear + 0.0722 * bLinear;
        }

        private (int r, int g, int b) HexToRgb(string hex)
        {
            hex = hex.TrimStart('#');

            if (hex.Length == 3)
            {
                hex = $"{hex[0]}{hex[0]}{hex[1]}{hex[1]}{hex[2]}{hex[2]}";
            }

            if (hex.Length != 6 || !Regex.IsMatch(hex, "^[0-9A-Fa-f]{6}$"))
            {
                throw new ArgumentException($"Invalid hex color format: '{hex}'", nameof(hex));
            }

            return (
                Convert.ToInt32(hex.Substring(0, 2), 16),
                Convert.ToInt32(hex.Substring(2, 2), 16),
                Convert.ToInt32(hex.Substring(4, 2), 16)
            );
        }

        private string RgbToHex(int r, int g, int b)
        {
            return $"#{r:X2}{g:X2}{b:X2}".ToLower();
        }
    }

    public enum WcagLevel
    {
        Fail,
        AALargeOnly,
        AA,
        AAA
    }

    public class ColorContrastResult
    {
        public string ColorName { get; set; } = string.Empty;
        public string ColorValue { get; set; } = string.Empty;

        public double ContrastWithWhite { get; set; }
        public double ContrastWithBlack { get; set; }
        public double ContrastWithLightBg { get; set; }

        public WcagLevel WhiteTextLevel { get; set; }
        public WcagLevel BlackTextLevel { get; set; }
        public WcagLevel LightBgLevel { get; set; }

        public string RecommendedTextColor { get; set; } = "#ffffff";

        public List<string> Warnings { get; set; } = new();
        public bool HasCriticalFailure { get; set; }
        public string? SuggestedFix { get; set; }
    }

    public class ColorContrastValidationResult
    {
        public bool IsValid { get; set; }
        public bool HasWarnings { get; set; }

        public ColorContrastResult PrimaryContrast { get; set; } = new();
        public ColorContrastResult SecondaryContrast { get; set; } = new();
        public ColorContrastResult AccentContrast { get; set; } = new();
        public ColorContrastResult SuccessContrast { get; set; } = new();
        public ColorContrastResult WarningContrast { get; set; } = new();
        public ColorContrastResult DangerContrast { get; set; } = new();

        public IEnumerable<ColorContrastResult> AllContrasts => new[]
        {
            PrimaryContrast,
            SecondaryContrast,
            AccentContrast,
            SuccessContrast,
            WarningContrast,
            DangerContrast
        };

        public IEnumerable<string> AllWarnings => AllContrasts.SelectMany(c => c.Warnings);
        public IEnumerable<string> CriticalIssues => AllContrasts
            .Where(c => c.HasCriticalFailure)
            .SelectMany(c => c.Warnings);
    }
}
