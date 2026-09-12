using HrAgencySystem.Company.Documents;
using HrAgencySystem.Company.Infrastructure.Persistence;
using Marten;

namespace HrAgencySystem.Company.Infrastructure.Configuration;

internal static class CompanyDocumentsConfiguration
{
    private const string SchemaName = "company";
    
    extension(StoreOptions options)
    {
        public void ConfigureDocuments()
        {
            ConfigureReservation(options);
            ConfigureContacts(options);
        }
    }
    
    private static void ConfigureReservation(StoreOptions options)
    {
        options.Schema.For<CompanyTaxIdReservation>().DatabaseSchemaName(SchemaName)
            .Index(x => new { x.OrganizationId, x.TaxId },
                idx => { idx.IsUnique = true; });
    }
    
    private static void ConfigureContacts(StoreOptions options)
    {
        options.Schema.For<CompanyContact>().DatabaseSchemaName(SchemaName)
            .Index(x => new { x.OrganizationId, x.CompanyId })
            .Index(x => new { x.OrganizationId, x.Contact.Email, x.CompanyId }, 
                idx => { idx.IsUnique = true;
                idx.Name = "idx_contact_uq";
            })
            .Index(x => new { x.OrganizationId, x.Contact.FirstName })
            .Index(x => new { x.OrganizationId, x.Contact.LastName })
            .Index(x => new { x.OrganizationId, x.CreatedAt })
            .Index(x => new { x.OrganizationId, x.CompanyName });
    }
}