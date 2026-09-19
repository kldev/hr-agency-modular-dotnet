namespace HrAgencySystem.EmailTemplates.Sending;

/// <summary>
/// Delivery of an already rendered message. Rendering stays with
/// <see cref="Rendering.IEmailTemplateProvider"/> — this side only puts the html on the wire.
/// </summary>
public interface ISendEmail
{
    Task SendAsync(EmailMessage message, CancellationToken ct = default);
}

public sealed record EmailMessage(
    string RecipientEmail,
    string RecipientName,
    string Subject,
    string HtmlBody
);
