using HrAgencySystem.Sales.Documents;
using Marten;

namespace HrAgencySystem.Sales.Infrastructure.Configuration;

internal static class SalesDocumentConfiguration
{
    private const string SchemaName = "sales";

    extension(StoreOptions options)
    {
        public void ConfigureSalesDocuments()
        {
            ConfigureFollowUpAction(options);
        }
    }
    
    private static void ConfigureFollowUpAction(StoreOptions options)
    {
        options.Schema
            .For<FollowUpAction>()
            .DatabaseSchemaName(SchemaName)
            .Index(x => x.OrganizationId)
            .Index(x => x.OpportunityId)
            .Index(x => x.CreatedBy.Id)
            .Index(x => x.FollowDateTime)
            .Index(x => new { x.OpportunityId, x.FollowDateTime },
                idx => { idx.IsUnique = true; });
    }
}