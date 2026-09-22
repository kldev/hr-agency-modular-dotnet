using HrAgencySystem.Agency.Domain;

namespace HrAgencySystem.Agency.Application.TimeSheets.Submit;

/// <param name="Comment">
/// Optional note sent along with the month - "three days off sick", "the 14th is a correction".
/// Optional for the same reason approving is: a month that speaks for itself needs nothing said
/// about it, and the person handing it over is the one who knows which is which.
/// </param>
public sealed record SubmitTimeSheet(
    Guid OrganizationId,
    Guid UserId,
    int Year,
    int Month,
    Guid SubmittedBy,
    string? Comment = null
)
{
    public Guid Id => AgencyStreamId.ForTimeSheet(OrganizationId, UserId, Year, Month);
}
