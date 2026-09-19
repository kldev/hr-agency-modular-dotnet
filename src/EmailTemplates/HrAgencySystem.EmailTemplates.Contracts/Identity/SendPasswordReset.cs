namespace HrAgencySystem.EmailTemplates.Contracts.Identity;

public sealed record SendPasswordReset(
    Guid EventId,
    string Source,
    string RecipientFullname,
    string RecipientEmail,
    string ResetUrl,
    int ExpiresInMinutes
) : IEmailTemplateContract;
