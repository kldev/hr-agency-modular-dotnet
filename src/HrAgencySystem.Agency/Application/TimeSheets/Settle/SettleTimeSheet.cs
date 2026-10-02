using HrAgencySystem.Agency.Domain;
using JetBrains.Annotations;

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
    [UsedImplicitly] // Wolverine's [AggregateHandler] loads the stream by this id.
    public Guid Id => AgencyStreamId.ForTimeSheet(OrganizationId, UserId, Year, Month);
}
