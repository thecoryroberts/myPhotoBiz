using MyPhotoBiz.Models;

namespace MyPhotoBiz.Services
{
    /// <summary>
    /// Service for processing contract template variables.
    /// Handles replacement of system variables (ClientName, ShootDate, etc.)
    /// and custom variables defined by administrators.
    /// </summary>
    public interface IContractVariableService
    {
        /// <summary>
        /// Replaces all variables in the template content with actual values.
        /// System variables are populated from the client/photoshoot context.
        /// Custom variables use provided overrides or their default values.
        /// </summary>
        /// <param name="templateContent">The template content with {{Variable}} placeholders</param>
        /// <param name="clientProfile">The client profile (optional)</param>
        /// <param name="photoShoot">The photo shoot (optional)</param>
        /// <param name="customVariableOverrides">Dictionary of custom variable overrides (variableId -> value)</param>
        /// <returns>The processed content with all variables replaced</returns>
        Task<string> ReplaceVariablesAsync(
            string templateContent,
            ClientProfile? clientProfile,
            PhotoShoot? photoShoot,
            Dictionary<int, string>? customVariableOverrides = null);

        /// <summary>
        /// Generates a preview of the template with sample/placeholder data.
        /// Useful for template editing to see what the result will look like.
        /// </summary>
        /// <param name="templateContent">The template content with {{Variable}} placeholders</param>
        /// <returns>The content with sample values for preview</returns>
        Task<string> GeneratePreviewAsync(string templateContent);

        /// <summary>
        /// Gets all active custom variables that can be used in templates.
        /// </summary>
        /// <returns>List of active contract variables</returns>
        Task<List<ContractVariable>> GetActiveCustomVariablesAsync();

        /// <summary>
        /// Extracts all variable placeholders from a template.
        /// Useful for determining which custom variables need values.
        /// </summary>
        /// <param name="templateContent">The template content</param>
        /// <returns>List of variable names found in the template</returns>
        List<string> ExtractVariableNames(string templateContent);

        /// <summary>
        /// Gets the system variable definitions with their descriptions.
        /// </summary>
        /// <returns>Dictionary of system variable name -> description</returns>
        Dictionary<string, string> GetSystemVariableDefinitions();
    }
}
