using HrAgencySystem.Projects.Projections;
using JasperFx.Events.Projections;
using Marten;

namespace HrAgencySystem.Projects.Infrastructure.Configuration;

internal static class ProjectsProjectionsConfiguration
{
    private const string SchemaName = "projects";

    extension(StoreOptions options)
    {
        public void ConfigureProjections()
        {
            options.Projections.Snapshot<ProjectProjection>(SnapshotLifecycle.Async);

            // Positions are folded out of the project's stream into one document each, so they can
            // be listed and searched across projects.
            options.Projections.Add<ProjectPositionProjector>(ProjectionLifecycle.Async);

            // OrganizationId leads every index: it is the first filter of every query here, and the
            // rule the rest of the codebase follows.
            options
                .Schema.For<ProjectProjection>()
                .DatabaseSchemaName(SchemaName)
                .Index(x => new { x.OrganizationId })
                .Index(x => new { x.OrganizationId, x.Status })
                .Index(x => new { x.OrganizationId, x.CompanyId })
                .Index(x => new { x.OrganizationId, x.WorkCountry });

            options
                .Schema.For<ProjectPositionProjection>()
                .DatabaseSchemaName(SchemaName)
                .Index(x => new { x.OrganizationId })
                .Index(x => new { x.OrganizationId, x.ProjectId })
                .Index(x => new { x.OrganizationId, x.IsArchived });
        }
    }
}
