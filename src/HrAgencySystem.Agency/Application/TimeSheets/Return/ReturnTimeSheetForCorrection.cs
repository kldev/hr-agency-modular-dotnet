using HrAgencySystem.Agency.Domain;
using JetBrains.Annotations;

namespace HrAgencySystem.Agency.Application.TimeSheets.Return;

/// <summary>
/// <paramref name="Reason"/> is not optional. A month handed back without saying what is wrong with
/// it is an invitation to hand it back a second time.
/// </summary>
public sealed record ReturnTimeSheetForCorrection(
    Guid OrganizationId,
    Guid UserId,
    int Year,
    int Month,
    string Reason,
    bool ActingAsPayroll,
    Guid ReturnedBy
)
{
    [UsedImplicitly] // Wolverine's [AggregateHandler] loads the stream by this id.
    public Guid Id => AgencyStreamId.ForTimeSheet(OrganizationId, UserId, Year, Month);
}
