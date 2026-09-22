namespace HrAgencySystem.EmailTemplates.Contracts.Agency;

/// <summary>
/// A month came back to be filled in again. The one mail here that somebody has to act on, which is
/// why the reason travels with it - the domain refuses a return without one.
/// </summary>
/// <param name="ReturnedByRole">
/// "Supervisor" or "Payroll". A string because this project has no dependencies, and two very
/// different situations for whoever reads it: a manager disagreeing with the hours, or payroll
/// handing back something it had already been given.
/// </param>
public sealed record SendTimeSheetReturnedForCorrection(
    Guid EventId,
    string Source,
    string RecipientEmail,
    string RecipientFullname,
    int Year,
    int Month,
    string Period,
    string ReturnedByFullname,
    string ReturnedByRole,
    string Reason
) : IEmailTemplateContract;
