using HrAgencySystem.Agency.Domain;
using JetBrains.Annotations;

namespace HrAgencySystem.Agency.Application.OrgUnits.Archive;

public sealed record ArchiveOrgUnit(Guid OrganizationId, Guid UnitId, Guid ModifiedBy)
{
    /// <summary>The stream this command loads: the chart's, derived from the organization's id.</summary>
    [UsedImplicitly] // Wolverine's [AggregateHandler] loads the stream by this id.
    public Guid Id => OrgStructureId.For(OrganizationId);
}
