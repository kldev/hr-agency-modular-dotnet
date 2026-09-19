using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;
using MimeKit.Text;

namespace HrAgencySystem.EmailTemplates.Sending;

public sealed class MailKitEmailSender(
    IOptions<SmtpConfig> options,
    ILogger<MailKitEmailSender> logger
) : ISendEmail
{
    private readonly SmtpConfig _config = options.Value;

    public async Task SendAsync(EmailMessage message, CancellationToken ct = default)
    {
        var mail = new MimeMessage
        {
            Subject = message.Subject,
            Body = new TextPart(TextFormat.Html) { Text = message.HtmlBody },
        };

        mail.From.Add(new MailboxAddress(_config.FromName, _config.FromEmail));
        mail.To.Add(new MailboxAddress(message.RecipientName, message.RecipientEmail));

        using var client = new SmtpClient();

        // Mailpit speaks plain SMTP on 1025; SecureSocketOptions.Auto would try STARTTLS first.
        var security = _config.UseSsl ? SecureSocketOptions.StartTls : SecureSocketOptions.None;

        await client.ConnectAsync(_config.Host, _config.Port, security, ct);

        if (!string.IsNullOrWhiteSpace(_config.Username))
        {
            await client.AuthenticateAsync(_config.Username, _config.Password, ct);
        }

        await client.SendAsync(mail, ct);
        await client.DisconnectAsync(true, ct);

        logger.LogInformation(
            "Sent \"{Subject}\" to {RecipientEmail} through {Host}:{Port}",
            message.Subject,
            message.RecipientEmail,
            _config.Host,
            _config.Port
        );
    }
}
