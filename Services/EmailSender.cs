using Microsoft.AspNetCore.Identity.UI.Services;

namespace MyPhotoBiz.Services
{
    /// <summary>
    /// Email service implementation - currently logs emails to console for development.
    /// Production deployment requires integration with email provider (SendGrid, AWS SES, etc.)
    /// </summary>
    public class EmailSender : IEmailSender
    {
        private readonly ILogger<EmailSender> _logger;

        public EmailSender(ILogger<EmailSender> logger)
        {
            _logger = logger;
        }

        public Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            // Development mode: Log email to console instead of sending
            _logger.LogInformation("========== EMAIL ==========");
            _logger.LogInformation("To: {Email}", email);
            _logger.LogInformation("Subject: {Subject}", subject);
            _logger.LogInformation("Message: {Message}", htmlMessage);
            _logger.LogInformation("===========================");

            return Task.CompletedTask;
        }
    }
}
