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

            // One document per person and per sheet; both streams are derived from what they are
            // about, so a snapshot keyed by the stream id is already keyed by the right thing.
            options.Projections.Snapshot<AgencyEmploymentProjection>(SnapshotLifecycle.Async);

            options
                .Schema.For<AgencyEmploymentProjection>()
                .DatabaseSchemaName(SchemaName)
                .Index(x => new { x.OrganizationId, x.UserId });

            options.Projections.Snapshot<TimeSheetProjection>(SnapshotLifecycle.Async);

            options
                .Schema.For<TimeSheetProjection>()
                .DatabaseSchemaName(SchemaName)
                .Index(x => new
                {
                    x.OrganizationId,
                    x.Year,
                    x.Month,
                })
                .Index(x => new
                {
                    x.OrganizationId,
                    x.UserId,
                    x.Year,
                    x.Month,
                });
        }
    }
}
