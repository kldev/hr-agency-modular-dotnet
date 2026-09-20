using HrAgencySystem.Workers.Infrastructure.Persistence;
using Marten;

namespace HrAgencySystem.Workers.Infrastructure.Configuration;

internal static class WorkersDocumentConfiguration
{
    private const string SchemaName = "workers";

    extension(StoreOptions options)
    {
        public void ConfigureDocuments()
        {
            // The unique index is the actual invariant; the handler checks first only so the
            // ordinary case reads as a sentence instead of a 409.
            options
                .Schema.For<WorkerIdentityDocumentReservation>()
                .DatabaseSchemaName(SchemaName)
                .Index(
                    x => new
                    {
                        x.OrganizationId,
                        x.IssuingCountry,
                        x.Number,
                    },
                    idx =>
                    {
                        idx.IsUnique = true;
                        idx.Name = "mt_idx_worker_document_unique";
                    }
                )
                // Named by hand: what Marten would generate from this type and these columns runs
                // past Postgres' 63 character identifier limit.
                .Index(
                    x => new { x.OrganizationId, x.WorkerId },
                    idx =>
                    {
                        idx.Name = "mt_idx_worker_reservation_owner";
                    }
                );

            options
                .Schema.For<WorkerEmailReservation>()
                .DatabaseSchemaName(SchemaName)
                .Index(
                    x => new { x.OrganizationId, x.Email },
                    idx =>
                    {
                        idx.IsUnique = true;
                        idx.Name = "mt_idx_worker_email_unique";
                    }
                )
                .Index(
                    x => new { x.OrganizationId, x.WorkerId },
                    idx =>
                    {
                        idx.Name = "mt_idx_worker_email_owner";
                    }
                );
        }
    }
}
