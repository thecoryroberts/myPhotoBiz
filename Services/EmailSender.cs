using Microsoft.AspNetCore.Identity.UI.Services;

namespace MyPhotoBiz.Services
{
    public class EmailSender : IEmailSender
    {
        private readonly ILogger<EmailSender> _logger;

        public EmailSender(ILogger<EmailSender> logger)
        {
            _logger = logger;
        }

        public Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            // For development: log email to console instead of sending
            _logger.LogInformation("========== EMAIL ==========");
            _logger.LogInformation("To: {Email}", email);
            _logger.LogInformation("Subject: {Subject}", subject);
            _logger.LogInformation("Message: {Message}", htmlMessage);
            _logger.LogInformation("===========================");

            // TODO: In production, integrate with an actual email service (SendGrid, SMTP, etc.)
            return Task.CompletedTask;
        }
    }
}
