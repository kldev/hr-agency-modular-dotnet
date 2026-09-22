using HrAgencySystem.Agency.Domain;

namespace HrAgencySystem.Agency.Application.TimeSheets.Settle;

public sealed record SettleTimeSheet(
    Guid OrganizationId,
    Guid UserId,
    int Year,
    int Month,
    bool ActingAsPayroll,
    Guid SettledBy
)
{
    public Guid Id => AgencyStreamId.ForTimeSheet(OrganizationId, UserId, Year, Month);
}
