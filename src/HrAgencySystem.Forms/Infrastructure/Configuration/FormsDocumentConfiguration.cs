using HrAgencySystem.Forms.Documents;
using Marten;

namespace HrAgencySystem.Forms.Infrastructure.Configuration;

internal static class FormsDocumentConfiguration
{
    internal const string SchemaName = "forms";

    extension(StoreOptions options)
    {
        public void ConfigureDocuments()
        {
            // Both ids are derived from what the document is about - (form, version) and
            // (organization, subject) - so the primary key already makes a second one impossible.
            options
                .Schema.For<FormVersion>()
                .DatabaseSchemaName(SchemaName)
                .Index(x => new { x.OrganizationId, x.FormId });

            options.Schema.For<SubjectProfile>().DatabaseSchemaName(SchemaName);

            // The unique index is what stops two concurrent creates taking the same code.
            options
                .Schema.For<FormCodeReservation>()
                .DatabaseSchemaName(SchemaName)
                .UniqueIndex(x => x.OrganizationId, x => x.Code);
        }
    }
}
