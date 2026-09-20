using HrAgencySystem.Workers.Projections;
using JasperFx.Events.Projections;
using Marten;

namespace HrAgencySystem.Workers.Infrastructure.Configuration;

internal static class WorkersProjectionsConfiguration
{
    private const string SchemaName = "workers";

    extension(StoreOptions options)
    {
        public void ConfigureProjections()
        {
            // A multi stream projection rather than a snapshot: the person's row carries facts that
            // live on their assignments' streams - see WorkerProjector.
            options.Projections.Add<WorkerProjector>(ProjectionLifecycle.Async);
            options.Projections.Snapshot<AssignmentProjection>(SnapshotLifecycle.Async);

            // OrganizationId leads every index: it is the first filter of every query here, and the
            // rule the rest of the codebase follows.
            options
                .Schema.For<WorkerProjection>()
                .DatabaseSchemaName(SchemaName)
                .Index(x => new { x.OrganizationId })
                .Index(x => new { x.OrganizationId, x.Status })
                .Index(
                    x => new { x.OrganizationId, x.CurrentWorkCountry },
                    idx =>
                    {
                        idx.Name = "mt_idx_worker_work_country";
                    }
                )
                .Index(
                    x => new { x.OrganizationId, x.Citizenship },
                    idx =>
                    {
                        idx.Name = "mt_idx_worker_citizenship";
                    }
                );

            options
                .Schema.For<AssignmentProjection>()
                .DatabaseSchemaName(SchemaName)
                .Index(x => new { x.OrganizationId })
                .Index(x => new { x.OrganizationId, x.Status })
                .Index(
                    x => new { x.OrganizationId, x.WorkerId },
                    idx =>
                    {
                        idx.Name = "mt_idx_assignment_worker";
                    }
                )
                .Index(
                    x => new { x.OrganizationId, x.ProjectId },
                    idx =>
                    {
                        idx.Name = "mt_idx_assignment_project";
                    }
                );
        }
    }
}
