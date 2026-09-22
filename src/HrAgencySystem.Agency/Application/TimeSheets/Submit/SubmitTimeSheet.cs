using HrAgencySystem.Agency.Domain;

namespace HrAgencySystem.Agency.Application.TimeSheets.Submit;

public sealed record SubmitTimeSheet(
    Guid OrganizationId,
    Guid UserId,
    int Year,
    int Month,
    Guid SubmittedBy
)
{
    public Guid Id => AgencyStreamId.ForTimeSheet(OrganizationId, UserId, Year, Month);
}
