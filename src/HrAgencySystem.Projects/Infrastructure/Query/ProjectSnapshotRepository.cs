using HrAgencySystem.Projects.Domain;
using HrAgencySystem.Projects.Projections;
using HrAgencySystem.SharedKernel.Snapshots;
using HrAgencySystem.SharedKernel.Tenant;
using Marten;

namespace HrAgencySystem.Projects.Infrastructure.Query;

/// <summary>
/// What other modules are allowed to learn about a project. The workers module reads this to put
/// somebody on a delivery; it never sees the contract, the compliance list or the contacts.
/// </summary>
public sealed class ProjectSnapshotRepository(IDocumentSession session) : IProjectSnapshotRepository
{
    public async Task<ProjectSnapshot?> GetProjectAsync(
        Guid projectId,
        OrganizationId organizationId,
        CancellationToken ct
    )
    {
        var projection = await session
            .Query<ProjectProjection>()
            .Where(p => p.Id == projectId && p.OrganizationId == organizationId.Value)
            .FirstOrDefaultAsync(ct);

        if (projection is not null)
            return Describe(
                projection.Id,
                projection.Name,
                projection.CompanyId,
                projection.CompanyName,
                projection.DeliveringEntity.LegalEntityId,
                projection.DeliveringEntity.Name,
                projection.WorkCountry,
                projection.StartsOn,
                projection.EndsOn,
                projection.Status
            );

        // Replayed rather than read off ProjectCreated, because the status and the period are the
        // two things that decide whether somebody may be assigned, and both change over the stream.
        // The same fallback CompanySnapshotRepository makes, for the same reason: the read model is
        // a daemon behind, and a project set up a moment ago is exactly when people get planned onto
        // it.
        var project = await session.Events.AggregateStreamAsync<Project>(projectId, token: ct);

        if (project is null || project.OrganizationId.Value != organizationId.Value)
            return null;

        return Describe(
            project.Id.Value,
            project.Name.Value,
            project.Company.Id,
            project.Company.Name,
            project.DeliveringEntity.LegalEntityId,
            project.DeliveringEntity.Name,
            project.Placement.WorkCountry,
            project.Placement.StartsOn,
            project.Placement.EndsOn,
            project.Status
        );
    }

    private static ProjectSnapshot Describe(
        Guid id,
        string name,
        Guid companyId,
        string companyName,
        Guid deliveringEntityId,
        string deliveringEntityName,
        string workCountry,
        DateOnly startsOn,
        DateOnly? endsOn,
        ProjectStatus status
    ) =>
        new(
            id,
            name,
            companyId,
            companyName,
            deliveringEntityId,
            deliveringEntityName,
            workCountry,
            startsOn,
            endsOn,
            // A draft accepts people: crews are planned before the work starts, and that is the
            // point of planning. A finished or cancelled delivery does not - it is over.
            !ProjectStatusChangePolicy.IsFinal(status)
        );
}
