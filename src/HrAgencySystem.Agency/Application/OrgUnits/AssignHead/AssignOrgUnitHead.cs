using HrAgencySystem.Agency.Domain;
using JetBrains.Annotations;

namespace HrAgencySystem.Agency.Application.OrgUnits.AssignHead;

public sealed record AssignOrgUnitHead(
    Guid OrganizationId,
    Guid UnitId,
    Guid HeadUserId,
    Guid ModifiedBy
)
{
    /// <summary>The stream this command loads: the chart's, derived from the organization's id.</summary>
    [UsedImplicitly] // Wolverine's [AggregateHandler] loads the stream by this id.
    public Guid Id => OrgStructureId.For(OrganizationId);
}
