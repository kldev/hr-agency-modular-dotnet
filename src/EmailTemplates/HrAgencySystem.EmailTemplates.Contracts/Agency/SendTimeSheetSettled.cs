namespace HrAgencySystem.EmailTemplates.Contracts.Agency;

/// <summary>
/// Payroll closed the month. Nothing is expected of the recipient - this one exists so that the
/// last step is not the only one nobody hears about.
/// </summary>
public sealed record SendTimeSheetSettled(
    Guid EventId,
    string Source,
    string RecipientEmail,
    string RecipientFullname,
    int Year,
    int Month,
    string Period,
    string SettledByFullname
) : IEmailTemplateContract;
