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

            // The refresh request carries nothing but the token, so the organization is read off the
            // row instead of being supplied - this is the one lookup in the codebase that cannot
            // lead with OrganizationId. The unique index is also what makes a hash collision or a
            // double insert fail loudly rather than hand out two live tokens.
            options
                .Schema.For<RefreshToken>()
                .DatabaseSchemaName(SchemaName)
                .Index(
                    x => x.TokenHash,
                    idx =>
                    {
                        idx.IsUnique = true;
                    }
                )
                .Index(x => x.FamilyId)
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
