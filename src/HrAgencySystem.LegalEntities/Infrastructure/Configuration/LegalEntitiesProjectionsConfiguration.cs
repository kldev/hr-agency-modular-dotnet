using HrAgencySystem.LegalEntities.Projections;
using JasperFx.Events.Projections;
using Marten;

namespace HrAgencySystem.LegalEntities.Infrastructure.Configuration;

internal static class LegalEntitiesProjectionsConfiguration
{
    private const string SchemaName = "legal_entities";

    extension(StoreOptions options)
    {
        public void ConfigureProjections()
        {
            options.Projections.Snapshot<LegalEntityProjection>(SnapshotLifecycle.Async);

            options
                .Schema.For<LegalEntityProjection>()
                .DatabaseSchemaName(SchemaName)
                .Index(x => new { x.OrganizationId })
                .Index(x => new { x.OrganizationId, x.Name })
                // "Which of our companies were trading then" is the one filter every list uses.
                // Named by hand: the generated name for three columns on a document with a name
                // this long goes over Postgres' 63 character limit for an identifier.
                .Index(
                    x => new
                    {
                        x.OrganizationId,
                        x.ActiveFrom,
                        x.ActiveTo,
                    },
                    idx =>
                    {
                        idx.Name = "mt_idx_legal_entity_trading";
                    }
                );
        }
    }
}
