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
            ConfigureSalesPipelineStageSummaryProjection(options);
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
        options.Projections.Snapshot<OpportunityProjection>(
            SnapshotLifecycle.Async);

        options.Schema
            .For<OpportunityProjection>()
            .DatabaseSchemaName(SchemaName)
            .Index(x => new { x.OrganizationId })
            .Index(x => new { x.OrganizationId, ca = x.CreatedAt })
            .Index(x => new { x.OrganizationId, c = x.CompanyId })
            .Index(x => new { x.OrganizationId, s = x.Stage })
            .Index(x => new {  x.OrganizationId, o = x.SalesOwnerId })
            .Index(x => new { x.OrganizationId,
                s = x.Stage, cc = x.CurrencyCode, v = x.ExpectedValue }, idx => { idx.Name = "idx_sop_exp_value";});
    }
    
    private static void ConfigureSalesPipelineStageSummaryProjection(StoreOptions options)
    {
        options.Projections.Add<SalesPipelineProjection>(ProjectionLifecycle.Async);

        options.Schema.For<SalesPipelineStageSummary>()
            .DatabaseSchemaName(SchemaName)
            .Index(x => new { OrganizationId = x.OrgId })
            .Index(x => new { OrganizationId = x.OrgId, x.Stage });
    }
}