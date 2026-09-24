using HrAgencySystem.Tasks.Events;
using HrAgencySystem.Tasks.Projections;
using JasperFx.Events.Projections;
using Marten;

namespace HrAgencySystem.Tasks.Infrastructure.Configuration;

internal static class TasksMartenConfiguration
{
    public const string SchemaName = "tasks";

    extension(StoreOptions options)
    {
        public void ConfigureEvents()
        {
            options.Events.AddEventType<TaskItemCreated>();
            options.Events.AddEventType<TaskItemUpdated>();
            options.Events.AddEventType<TaskItemCompleted>();
            options.Events.AddEventType<TaskItemReopened>();
        }

        public void ConfigureProjections()
        {
            options.Projections.Snapshot<TaskItemProjection>(SnapshotLifecycle.Async);

            // Named by hand: the generated names run past Postgres' 63 characters.
            options
                .Schema.For<TaskItemProjection>()
                .DatabaseSchemaName(SchemaName)
                .Index(
                    x => new { x.OrganizationId, x.AssigneeId, x.Status, x.DueAt },
                    index => index.Name = "mt_doc_taskitem_idx_org_assignee_due"
                )
                .Index(
                    x => new { x.OrganizationId, x.AssigneeId, x.Status, x.CompletedAt },
                    index => index.Name = "mt_doc_taskitem_idx_org_assignee_done"
                )
                .Index(
                    x => new { x.OrganizationId, x.CompanyId },
                    index => index.Name = "mt_doc_taskitem_idx_org_company"
                );
        }
    }
}
