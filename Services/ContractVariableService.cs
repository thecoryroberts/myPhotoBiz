using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using MyPhotoBiz.Data;
using MyPhotoBiz.Models;

namespace MyPhotoBiz.Services
{
    /// <summary>
    /// Service for processing contract template variables.
    /// Replaces {{VariableName}} placeholders with actual values from
    /// client profiles, photo shoots, and custom variable definitions.
    /// </summary>
    public partial class ContractVariableService : IContractVariableService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<ContractVariableService> _logger;

        // Regex to match {{VariableName}} placeholders
        [GeneratedRegex(@"\{\{(\w+)\}\}", RegexOptions.Compiled)]
        private static partial Regex VariablePlaceholderRegex();

        // System variable definitions
        private static readonly Dictionary<string, string> SystemVariables = new(StringComparer.OrdinalIgnoreCase)
        {
            ["ClientName"] = "Client's full name",
            ["ClientFirstName"] = "Client's first name",
            ["ClientLastName"] = "Client's last name",
            ["ClientEmail"] = "Client's email address",
            ["ClientPhone"] = "Client's phone number",
            ["ClientAddress"] = "Client's address",
            ["ShootTitle"] = "Photo shoot title",
            ["ShootDate"] = "Photo shoot scheduled date",
            ["ShootTime"] = "Photo shoot scheduled time",
            ["EventDate"] = "Event/shoot date (alias for ShootDate)",
            ["Location"] = "Photo shoot location",
            ["PhotographerName"] = "Assigned photographer's name",
            ["PhotographerEmail"] = "Assigned photographer's email",
            ["PhotographerPhone"] = "Assigned photographer's phone",
            ["CurrentDate"] = "Current date when contract is created",
            ["CurrentYear"] = "Current year",
            ["Duration"] = "Photo shoot duration",
            ["DurationHours"] = "Photo shoot duration in hours",
            ["Price"] = "Photo shoot price",
            ["Notes"] = "Photo shoot notes"
        };

        public ContractVariableService(
            ApplicationDbContext context,
            ILogger<ContractVariableService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<string> ReplaceVariablesAsync(
            string templateContent,
            ClientProfile? clientProfile,
            PhotoShoot? photoShoot,
            Dictionary<int, string>? customVariableOverrides = null)
        {
            if (string.IsNullOrEmpty(templateContent))
                return templateContent;

            // Build the replacement dictionary
            var replacements = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            // Add system variables
            AddSystemVariables(replacements, clientProfile, photoShoot);

            // Add custom variables
            await AddCustomVariablesAsync(replacements, customVariableOverrides);

            // Perform replacements
            var result = VariablePlaceholderRegex().Replace(templateContent, match =>
            {
                var variableName = match.Groups[1].Value;
                if (replacements.TryGetValue(variableName, out var replacement))
                {
                    return replacement;
                }

                // Leave unmatched variables as-is (or could replace with empty string)
                _logger.LogWarning("Unrecognized variable in contract template: {VariableName}", variableName);
                return match.Value;
            });

            return result;
        }

        public async Task<string> GeneratePreviewAsync(string templateContent)
        {
            if (string.IsNullOrEmpty(templateContent))
                return templateContent;

            // Build preview replacements with sample data
            var replacements = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                ["ClientName"] = "Jane Doe",
                ["ClientFirstName"] = "Jane",
                ["ClientLastName"] = "Doe",
                ["ClientEmail"] = "jane.doe@example.com",
                ["ClientPhone"] = "(555) 123-4567",
                ["ClientAddress"] = "123 Main Street, Anytown, ST 12345",
                ["ShootTitle"] = "Sample Photo Session",
                ["ShootDate"] = DateTime.Today.AddDays(14).ToString("MMMM dd, yyyy"),
                ["ShootTime"] = "2:00 PM",
                ["EventDate"] = DateTime.Today.AddDays(14).ToString("MMMM dd, yyyy"),
                ["Location"] = "Sample Studio, 456 Photo Lane",
                ["PhotographerName"] = "John Photographer",
                ["PhotographerEmail"] = "photographer@example.com",
                ["PhotographerPhone"] = "(555) 987-6543",
                ["CurrentDate"] = DateTime.Today.ToString("MMMM dd, yyyy"),
                ["CurrentYear"] = DateTime.Today.Year.ToString(),
                ["Duration"] = "2 hours",
                ["DurationHours"] = "2",
                ["Price"] = "$500.00",
                ["Notes"] = "Sample notes for the photo session."
            };

            // Add custom variables with their default values or sample text
            var customVariables = await GetActiveCustomVariablesAsync();
            foreach (var variable in customVariables)
            {
                if (!replacements.ContainsKey(variable.Name))
                {
                    replacements[variable.Name] = !string.IsNullOrEmpty(variable.DefaultValue)
                        ? variable.DefaultValue
                        : $"[{variable.Name}]";
                }
            }

            // Perform replacements
            var result = VariablePlaceholderRegex().Replace(templateContent, match =>
            {
                var variableName = match.Groups[1].Value;
                if (replacements.TryGetValue(variableName, out var replacement))
                {
                    return replacement;
                }
                return $"[Unknown: {variableName}]";
            });

            return result;
        }

        public async Task<List<ContractVariable>> GetActiveCustomVariablesAsync()
        {
            return await _context.ContractVariables
                .AsNoTracking()
                .Where(v => v.IsActive && !v.IsSystemVariable)
                .OrderBy(v => v.Name)
                .ToListAsync();
        }

        public List<string> ExtractVariableNames(string templateContent)
        {
            if (string.IsNullOrEmpty(templateContent))
                return new List<string>();

            var matches = VariablePlaceholderRegex().Matches(templateContent);
            return matches
                .Select(m => m.Groups[1].Value)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(v => v)
                .ToList();
        }

        public Dictionary<string, string> GetSystemVariableDefinitions()
        {
            return new Dictionary<string, string>(SystemVariables);
        }

        private void AddSystemVariables(
            Dictionary<string, string> replacements,
            ClientProfile? clientProfile,
            PhotoShoot? photoShoot)
        {
            // Current date/time variables (always available)
            replacements["CurrentDate"] = DateTime.Today.ToString("MMMM dd, yyyy");
            replacements["CurrentYear"] = DateTime.Today.Year.ToString();

            // Client variables
            if (clientProfile?.User != null)
            {
                var user = clientProfile.User;
                var fullName = $"{user.FirstName} {user.LastName}".Trim();

                replacements["ClientName"] = !string.IsNullOrEmpty(fullName) ? fullName : "";
                replacements["ClientFirstName"] = user.FirstName ?? "";
                replacements["ClientLastName"] = user.LastName ?? "";
                replacements["ClientEmail"] = user.Email ?? "";
                replacements["ClientPhone"] = clientProfile.PhoneNumber ?? user.PhoneNumber ?? "";
                replacements["ClientAddress"] = clientProfile.Address ?? "";
            }
            else
            {
                // Set empty values for client variables if no client
                replacements["ClientName"] = "";
                replacements["ClientFirstName"] = "";
                replacements["ClientLastName"] = "";
                replacements["ClientEmail"] = "";
                replacements["ClientPhone"] = "";
                replacements["ClientAddress"] = "";
            }

            // PhotoShoot variables
            if (photoShoot != null)
            {
                replacements["ShootTitle"] = photoShoot.Title ?? "";
                replacements["ShootDate"] = photoShoot.ScheduledDate.ToString("MMMM dd, yyyy");
                replacements["ShootTime"] = photoShoot.ScheduledDate.ToString("h:mm tt");
                replacements["EventDate"] = photoShoot.ScheduledDate.ToString("MMMM dd, yyyy");
                replacements["Location"] = photoShoot.Location ?? "";
                replacements["Notes"] = photoShoot.Notes ?? "";
                replacements["Price"] = photoShoot.Price.ToString("C");

                // Duration
                var totalMinutes = (photoShoot.DurationHours * 60) + photoShoot.DurationMinutes;
                if (totalMinutes > 0)
                {
                    var hours = totalMinutes / 60;
                    var minutes = totalMinutes % 60;
                    replacements["Duration"] = minutes > 0
                        ? $"{hours} hour{(hours != 1 ? "s" : "")} {minutes} minute{(minutes != 1 ? "s" : "")}"
                        : $"{hours} hour{(hours != 1 ? "s" : "")}";
                    replacements["DurationHours"] = hours.ToString();
                }
                else
                {
                    replacements["Duration"] = "";
                    replacements["DurationHours"] = "";
                }

                // Photographer variables
                if (photoShoot.PhotographerProfile?.User != null)
                {
                    var photographer = photoShoot.PhotographerProfile.User;
                    replacements["PhotographerName"] = $"{photographer.FirstName} {photographer.LastName}".Trim();
                    replacements["PhotographerEmail"] = photographer.Email ?? "";
                    replacements["PhotographerPhone"] = photographer.PhoneNumber ?? "";
                }
                else
                {
                    replacements["PhotographerName"] = "";
                    replacements["PhotographerEmail"] = "";
                    replacements["PhotographerPhone"] = "";
                }
            }
            else
            {
                // Set empty values for shoot variables if no photoshoot
                replacements["ShootTitle"] = "";
                replacements["ShootDate"] = "";
                replacements["ShootTime"] = "";
                replacements["EventDate"] = "";
                replacements["Location"] = "";
                replacements["Notes"] = "";
                replacements["Price"] = "";
                replacements["Duration"] = "";
                replacements["DurationHours"] = "";
                replacements["PhotographerName"] = "";
                replacements["PhotographerEmail"] = "";
                replacements["PhotographerPhone"] = "";
            }
        }

        private async Task AddCustomVariablesAsync(
            Dictionary<string, string> replacements,
            Dictionary<int, string>? customVariableOverrides)
        {
            var customVariables = await GetActiveCustomVariablesAsync();

            foreach (var variable in customVariables)
            {
                // Check for override value first
                if (customVariableOverrides != null &&
                    customVariableOverrides.TryGetValue(variable.Id, out var overrideValue) &&
                    !string.IsNullOrEmpty(overrideValue))
                {
                    replacements[variable.Name] = overrideValue;
                }
                // Otherwise use default value
                else if (!string.IsNullOrEmpty(variable.DefaultValue))
                {
                    replacements[variable.Name] = variable.DefaultValue;
                }
                // If no default, leave empty
                else
                {
                    replacements[variable.Name] = "";
                }
            }
        }
    }
}
