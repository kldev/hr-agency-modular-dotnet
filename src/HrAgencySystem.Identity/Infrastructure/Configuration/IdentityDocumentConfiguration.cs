using HrAgencySystem.Identity.Infrastructure.Persistence;
using Marten;

namespace HrAgencySystem.Identity.Infrastructure.Configuration;

internal static class IdentityDocumentConfiguration
{
    private const string SchemaName = "identity";

    extension(StoreOptions options)
    {
        public void ConfigureDocuments()
        {
            options
                .Schema.For<UserEmailReservation>()
                .DatabaseSchemaName(SchemaName)
                .Index(
                    x => new { x.OrganizationId, x.Email },
                    idx =>
                    {
                        idx.IsUnique = true;
                    }
                )
                .Index(x => new { x.OrganizationId, x.UserId });

            options
                .Schema.For<OwnerEmailReservation>()
                .DatabaseSchemaName(SchemaName)
                .Index(
                    x => new { x.Email },
                    idx =>
                    {
                        idx.IsUnique = true;
                    }
                );
        }
    }
}
