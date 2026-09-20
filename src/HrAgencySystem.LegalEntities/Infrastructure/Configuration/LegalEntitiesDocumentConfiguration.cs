using HrAgencySystem.LegalEntities.Infrastructure.Persistence;
using Marten;

namespace HrAgencySystem.LegalEntities.Infrastructure.Configuration;

internal static class LegalEntitiesDocumentConfiguration
{
    private const string SchemaName = "legal_entities";

    extension(StoreOptions options)
    {
        public void ConfigureDocuments()
        {
            // The unique index is the actual invariant; the handler checks first only so the
            // ordinary case reads as a sentence instead of a 409.
            options
                .Schema.For<LegalEntityTaxIdReservation>()
                .DatabaseSchemaName(SchemaName)
                .Index(
                    x => new { x.OrganizationId, x.TaxId },
                    idx =>
                    {
                        idx.IsUnique = true;
                        idx.Name = "mt_idx_legal_entity_tax_id_unique";
                    }
                )
                // Named by hand for the same reason as the projection's: the generated name would
                // exceed Postgres' 63 character identifier limit.
                .Index(
                    x => new { x.OrganizationId, x.LegalEntityId },
                    idx =>
                    {
                        idx.Name = "mt_idx_legal_entity_reservation_owner";
                    }
                );
        }
    }
}
