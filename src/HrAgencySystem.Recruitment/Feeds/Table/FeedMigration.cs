using HrAgencySystem.SharedKernel.Port;
using Npgsql;
using Weasel.Postgresql;
using Weasel.Postgresql.Tables;

namespace HrAgencySystem.Recruitment.Feeds.Table;

public  sealed class FeedMigration(NpgsqlDataSource ds) : ISeeder
{
    public async Task SeedAsync(CancellationToken ct)
    {
        var table = GetFeedTable();
        await using var conn = await ds.OpenConnectionAsync(ct);
        await table.MigrateAsync(conn);
    }

    private static Weasel.Postgresql.Tables.Table GetFeedTable()
    {
        var table = new Weasel.Postgresql.Tables.Table("jobs.job_feed_tasks");
        table.AddColumn<Guid>("id").AsPrimaryKey();
        table.AddColumn<Guid>("organization_id");
        var status = new TableColumn("status", "varchar(40)")
        {
            AllowNulls = false
        };
        table.AddColumn(status);
        table.AddColumn<int>("attempts").NotNull().DefaultValue(0);
        table.AddColumn<DateTimeOffset>("created_at").NotNull();
        table.AddColumn<DateTimeOffset>("started_at").AllowNulls();
        table.AddColumn<DateTimeOffset>("completed_at").AllowNulls();
        table.AddColumn<string>("error_message").AllowNulls();

        
        table.Indexes.Add(new IndexDefinition("idx_job_feed_tasks_pending").AgainstColumns("status", "created_at"));
        var taskActiveIndex = new IndexDefinition("ux_job_feed_tasks_active").AgainstColumns("organization_id");
        taskActiveIndex.IsUnique = true;
        taskActiveIndex.Predicate = "status IN ('PENDING', 'PROCESSING')";
        
        table.Indexes.Add(taskActiveIndex);

        return table;
    }
}