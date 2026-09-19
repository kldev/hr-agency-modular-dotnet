using HrAgencySystem.Identity.Infrastructure.Persistence;
using HrAgencySystem.Identity.Sagas;
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

            // The saga is a Marten document like any other; Wolverine only needs it to be storable.
            options
                .Schema.For<PasswordResetSaga>()
                .DatabaseSchemaName(SchemaName)
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
