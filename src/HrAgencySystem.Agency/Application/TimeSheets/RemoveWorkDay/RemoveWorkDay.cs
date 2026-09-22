using HrAgencySystem.Agency.Domain;

namespace HrAgencySystem.Agency.Application.TimeSheets.RemoveWorkDay;

public sealed record RemoveWorkDay(
    Guid OrganizationId,
    Guid UserId,
    int Year,
    int Month,
    DateOnly Date,
    Guid ModifiedBy
)
{
    public Guid Id => AgencyStreamId.ForTimeSheet(OrganizationId, UserId, Year, Month);
}
