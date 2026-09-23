using HrAgencySystem.Projects.Domain;
using HrAgencySystem.Projects.Events;
using HrAgencySystem.Reports.ReadModel;
using JasperFx.Events;
using Marten;
using Marten.EntityFrameworkCore;

namespace HrAgencySystem.Projects.Projections;

/// <summary>
/// Projects the project stream into the reporting table: status, engagement, country and the first
/// time it went live. Contract, contacts, documents and compliance only move its activity date.
/// </summary>
public sealed class ProjectReportProjection
    : EfCoreSingleStreamProjection<ProjectReportRow, Guid, ProjectsReportDbContext>
{
    public override ProjectReportRow? ApplyEvent(
        ProjectReportRow? snapshot,
        Guid identity,
        IEvent @event,
        ProjectsReportDbContext dbContext,
        IQuerySession session
    )
    {
        switch (@event.Data)
        {
            case ProjectCreated created:
                return new ProjectReportRow
                {
                    Id = created.ProjectId,
                    OrganizationId = created.OrganizationId,
                    CompanyId = created.Company.Id,
                    EngagementType = created.EngagementType.ToString(),
                    CountryCode = created.Placement.WorkCountry,
                    Status = nameof(ProjectStatus.Draft),
                    CreatedAt = created.CreatedAt,
                    UpdatedAt = created.CreatedAt,
                };

            case ProjectUpdated updated when snapshot is not null:
                snapshot.CountryCode = updated.Placement.WorkCountry;
                return Touch(snapshot, updated.ModifiedAt);

            case ProjectStatusChanged changed when snapshot is not null:
                snapshot.Status = changed.Status.ToString();

                if (changed.Status == ProjectStatus.Active)
                {
                    snapshot.WentLiveAt ??= changed.ChangedAt;
                }

                return Touch(snapshot, changed.ChangedAt);

            case not null when snapshot is not null:
                return Touch(snapshot, @event.Timestamp);

            default:
                return snapshot;
        }
    }

    private static ProjectReportRow Touch(ProjectReportRow row, DateTimeOffset at)
    {
        if (at > row.UpdatedAt)
        {
            row.UpdatedAt = at;
        }

        return row;
    }
}
