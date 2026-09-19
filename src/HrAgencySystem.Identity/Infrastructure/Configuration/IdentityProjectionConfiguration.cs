using HrAgencySystem.Identity.Projections;
using JasperFx.Events.Projections;
using Marten;

namespace HrAgencySystem.Identity.Infrastructure.Configuration;

internal static class IdentityProjectionConfiguration
{
    private const string SchemaName = "identity";

    extension(StoreOptions options)
    {
        public void ConfigureProjections()
        {
            options.Projections.Snapshot<UserProjection>(SnapshotLifecycle.Async);

            options
                .Schema.For<UserProjection>()
                .DatabaseSchemaName(SchemaName)
                .Index(x => new { x.OrganizationId })
                .Index(x => new
                {
                    x.OrganizationId,
                    x.Email,
                    x.Id,
                })
                .Index(z => new { z.OrganizationId, z.CreatedAt })
                .Index(z => new { z.OrganizationId, z.Role });

            options.Projections.Snapshot<OwnerProjection>(SnapshotLifecycle.Async);
            options
                .Schema.For<OwnerProjection>()
                .DatabaseSchemaName(SchemaName)
                .Index(x => new { x.Email });
        }
    }
}
