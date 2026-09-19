using Microsoft.Extensions.Logging;

namespace HrAgencySystem.EmailTemplates.Sending;

/// <summary>
/// The fallback when no mail provider is configured: the rendered message is logged instead of sent,
/// so a host without SMTP still runs end to end.
/// </summary>
public sealed class LoggingEmailSender(ILogger<LoggingEmailSender> logger) : ISendEmail
{
    public Task SendAsync(EmailMessage message, CancellationToken ct = default)
    {
        logger.LogInformation(
            "No mail provider configured, dropping \"{Subject}\" addressed to {RecipientEmail}",
            message.Subject,
            message.RecipientEmail
        );

        return Task.CompletedTask;
    }
}
