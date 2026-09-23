using HrAgencySystem.Forms.Projections;
using JasperFx.Events.Projections;
using Marten;

namespace HrAgencySystem.Forms.Infrastructure.Configuration;

internal static class FormsProjectionsConfiguration
{
    extension(StoreOptions options)
    {
        public void ConfigureProjections()
        {
            options.Projections.Snapshot<FormDefinitionProjection>(SnapshotLifecycle.Async);

            options
                .Schema.For<FormDefinitionProjection>()
                .DatabaseSchemaName(FormsDocumentConfiguration.SchemaName)
                .Index(x => new { x.OrganizationId, x.Status })
                .Index(x => new { x.OrganizationId, x.Code });

            options.Projections.Snapshot<FormResponseProjection>(SnapshotLifecycle.Async);

            // The first GIN index in the codebase: "who answered X" is a containment query over the
            // answers, and this is the index that serves it (plan 028 §C).
            options
                .Schema.For<FormResponseProjection>()
                .DatabaseSchemaName(FormsDocumentConfiguration.SchemaName)
                // Named by hand: the generated names run past Postgres' 63 characters.
                .Index(
                    x => new { x.OrganizationId, x.SubjectKind, x.SubjectId },
                    index => index.Name = "mt_doc_formresponse_idx_org_subject"
                )
                .Index(
                    x => new { x.OrganizationId, x.FormId, x.Status },
                    index => index.Name = "mt_doc_formresponse_idx_org_form_status"
                )
                .GinIndexJsonData();
        }
    }
}
