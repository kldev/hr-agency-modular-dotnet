using HrAgencySystem.Organization.Events;
using HrAgencySystem.Reports.ReadModel;
using JasperFx.Events;
using Marten;
using Marten.EntityFrameworkCore;

namespace HrAgencySystem.Organization.Projections;

/// <summary>
/// Projects the organization stream into the reporting table the platform report lists tenants
/// from. The row belongs to <c>HrAgencySystem.Reports.ReadModel</c>, which knows no events.
/// </summary>
public sealed class OrganizationReportProjection
    : EfCoreSingleStreamProjection<OrganizationReportRow, Guid, OrganizationsReportDbContext>
{
    public override OrganizationReportRow? ApplyEvent(
        OrganizationReportRow? snapshot,
        Guid identity,
        IEvent @event,
        OrganizationsReportDbContext dbContext,
        IQuerySession session
    )
    {
        switch (@event.Data)
        {
            case OrganizationCreated created:
                return new OrganizationReportRow
                {
                    Id = created.OrganizationId,
                    OrganizationId = created.OrganizationId,
                    Name = created.Name,
                    Slug = created.Slug,
                    CreatedAt = created.CreatedAt,
                    UpdatedAt = created.CreatedAt,
                };

            case OrganizationUpdated updated when snapshot is not null:
                snapshot.Name = updated.Name;
                snapshot.Slug = updated.Slug;
                snapshot.UpdatedAt = updated.ModifiedAt;
                return snapshot;

            case OrganizationSlugUpdated slug when snapshot is not null:
                snapshot.Slug = slug.Slug;
                snapshot.UpdatedAt = slug.ModifiedAt;
                return snapshot;

            default:
                return snapshot;
        }
    }
}
