namespace HrAgencySystem.EmailTemplates.Contracts.Agency;

/// <summary>
/// The supervisor accepted somebody's month. Goes to the person the sheet belongs to, and only when
/// that is not the same person who approved it.
/// </summary>
/// <param name="Period">
/// Already written out - "September 2026". Liquid has no way to name a month from a year and a
/// number, so doing it here keeps one place that can get it wrong instead of one per template.
/// </param>
/// <param name="Comment">
/// What the supervisor said, or empty when they said nothing. Empty rather than null: liquid
/// renders an absent variable as nothing either way, so the template can only ask about "".
/// </param>
public sealed record SendTimeSheetApproved(
    Guid EventId,
    string Source,
    string RecipientEmail,
    string RecipientFullname,
    int Year,
    int Month,
    string Period,
    string ApprovedByFullname,
    string Comment
) : IEmailTemplateContract;
