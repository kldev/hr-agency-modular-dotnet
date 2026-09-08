using HrAgencySystem.Company.Projections;
using JasperFx.Events.Projections;
using Marten;

namespace HrAgencySystem.Company.Infrastructure.Configuration;

internal static class CompanyProjectionsConfiguration
{
    private const string SchemaName = "company";

    extension(StoreOptions options)
    {
        public void ConfigureProjections( bool skipSnapshots = false)
        {
            options.ConfigureCompanyProjections(skipSnapshots);
        }
        
        public void ConfigureCompanyProjections(bool skipSnapshots = false)
        {
            if (!skipSnapshots)
            {
                options.Projections.Snapshot<CompanyProjection>(SnapshotLifecycle.Async);
            }

            options.Schema.For<CompanyProjection>().DatabaseSchemaName(SchemaName)
                .Index(x => new { x.OrganizationId })
                .Index(x => new { x.OrganizationId, x.Name })
                .Index(x => new { x.OrganizationId, x.CreatedId })
                .Index(x => new { x.OrganizationId, x.CountryCode });
        }
    }
    
  
}