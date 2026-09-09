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
        options.Projections.Snapshot<ActivityProjection>(
            SnapshotLifecycle.Async);

        options.Schema
            .For<ActivityProjection>()
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
            .Index(x => new { x.OrganizationId, c = x.CompanyId }, idx => { idx.Name = "idx_sop_company"; })
            .Index(x => new { x.OrganizationId, s = x.Stage }, idx => { idx.Name = "idx_sop_stage"; })
            .Index(x => new { x.OrganizationId, o = x.ResponsibleId }, idx => { idx.Name = "idx_sop_owner_id"; })
            .Index(x => new
            {
                x.OrganizationId,
                s = x.Stage, cc = x.CurrencyCode, v = x.ExpectedValue
            }, idx => { idx.Name = "idx_sop_exp_value"; });
    }

    private static void ConfigureSalesPipelineStageSummaryProjection(StoreOptions options)
    {
        options.Projections.Add<PipelineProjection>(ProjectionLifecycle.Async);

        options.Schema.For<PipelineStageSummary>()
            .DatabaseSchemaName(SchemaName)
            .Index(x => new { x.OrgId })
            .Index(x => new { x.OrgId, x.Stage });
    }
}