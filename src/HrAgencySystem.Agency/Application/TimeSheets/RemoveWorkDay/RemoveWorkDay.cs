using HrAgencySystem.Agency.Domain;
using JetBrains.Annotations;

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
    [UsedImplicitly] // Wolverine's [AggregateHandler] loads the stream by this id.
    public Guid Id => AgencyStreamId.ForTimeSheet(OrganizationId, UserId, Year, Month);
}
