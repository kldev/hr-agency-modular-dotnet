using HrAgencySystem.Agency.Projections;
using JasperFx.Events.Projections;
using Marten;

namespace HrAgencySystem.Agency.Infrastructure.Configuration;

internal static class AgencyProjectionsConfiguration
{
    private const string SchemaName = "agency";

    extension(StoreOptions options)
    {
        public void ConfigureProjections()
        {
            // One document per organization, keyed by the organization id - which is also the
            // stream id, so a plain snapshot is the whole read model.
            options.Projections.Snapshot<OrgStructureProjection>(SnapshotLifecycle.Async);

            options
                .Schema.For<OrgStructureProjection>()
                .DatabaseSchemaName(SchemaName)
                .Index(x => new { x.OrganizationId });
        }
    }
}
