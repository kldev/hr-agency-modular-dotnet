using HrAgencySystem.Sales.Projections;
using JasperFx.Events.Projections;
using Marten;

namespace HrAgencySystem.Sales.Infrastructure.Configuration;

internal static class  SalesProjectionConfiguration
{
    private const string SchemaName = "sales";

    extension(StoreOptions options)
    {
        public void ConfigureSalesProjections()
        {
            ConfigureActivityProjection(options);
            ConfigureOpportunityProjection(options);
        }
    }

    private static void ConfigureActivityProjection(
        StoreOptions options)
    {
        options.Projections.Snapshot<SalesActivityProjection>(
            SnapshotLifecycle.Async);

        options.Schema
            .For<SalesActivityProjection>()
            .DatabaseSchemaName(SchemaName)
            .Index(x => new { x.OrgId })
            .Index(x => new { x.OrgId, x.CreatedAt  })
            .Index(x => new { x.OrgId, x.CompanyId  });

    }
    
    private static void ConfigureOpportunityProjection(
        StoreOptions options)
    {
        options.Projections.Snapshot<SalesOpportunityProjection>(
            SnapshotLifecycle.Async);

        options.Schema
            .For<SalesOpportunityProjection>()
            .DatabaseSchemaName(SchemaName)
            .Index(x => new { x.OrgId })
            .Index(x => new { x.OrgId, x.CreatedAt })
            .Index(x => new { x.OrgId, x.CompanyId })
            .Index(x => new { x.OrgId, x.Stage })
            .Index(x => new { x.OrgId, x.SalesOwnerId });
    }
}