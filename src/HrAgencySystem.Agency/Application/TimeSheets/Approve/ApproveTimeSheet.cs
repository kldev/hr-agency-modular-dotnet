using HrAgencySystem.Agency.Domain;

namespace HrAgencySystem.Agency.Application.TimeSheets.Approve;

public sealed record ApproveTimeSheet(
    Guid OrganizationId,
    Guid UserId,
    int Year,
    int Month,
    string? Comment,
    Guid ApprovedBy
)
{
    public Guid Id => AgencyStreamId.ForTimeSheet(OrganizationId, UserId, Year, Month);
}
