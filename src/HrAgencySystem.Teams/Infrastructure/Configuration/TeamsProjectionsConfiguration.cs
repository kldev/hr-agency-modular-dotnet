using HrAgencySystem.Teams.Projections;
using JasperFx.Events.Projections;
using Marten;

namespace HrAgencySystem.Teams.Infrastructure.Configuration;

internal static class TeamsProjectionsConfiguration
{
    private const string SchemaName = "teams";

    extension(StoreOptions options)
    {
        public void ConfigureProjections()
        {
            options.Projections.Snapshot<TeamProjection>(SnapshotLifecycle.Async);

            options
                .Schema.For<TeamProjection>()
                .DatabaseSchemaName(SchemaName)
                .Index(x => new { x.OrganizationId })
                .Index(x => new { x.OrganizationId, x.Name });
        }
    }
}
