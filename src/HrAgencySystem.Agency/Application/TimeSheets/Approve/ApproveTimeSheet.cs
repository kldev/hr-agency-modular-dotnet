using HrAgencySystem.Agency.Domain;
using JetBrains.Annotations;

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
    [UsedImplicitly] // Wolverine's [AggregateHandler] loads the stream by this id.
    public Guid Id => AgencyStreamId.ForTimeSheet(OrganizationId, UserId, Year, Month);
}
