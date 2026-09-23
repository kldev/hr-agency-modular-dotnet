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
            options
                .Schema.For<FormVersion>()
                .DatabaseSchemaName(SchemaName)
                .Index(x => new { x.OrganizationId, x.FormId })
                .UniqueIndex(x => x.FormId, x => x.Version);

            options
                .Schema.For<SubjectProfile>()
                .DatabaseSchemaName(SchemaName)
                .UniqueIndex(x => x.OrganizationId, x => x.SubjectKind, x => x.SubjectId);

            // The unique index is what stops two concurrent creates taking the same code.
            options
                .Schema.For<FormCodeReservation>()
                .DatabaseSchemaName(SchemaName)
                .UniqueIndex(x => x.OrganizationId, x => x.Code);
        }
    }
}
