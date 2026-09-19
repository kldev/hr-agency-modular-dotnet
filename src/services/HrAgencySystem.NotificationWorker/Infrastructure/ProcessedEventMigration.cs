using Npgsql;
using Weasel.Postgresql;
using Weasel.Postgresql.Tables;

namespace HrAgencySystem.NotificationWorker.Infrastructure;

/// <summary>
/// The table that makes mail delivery idempotent: the unique event id is what a redelivery collides
/// with, so the same notification cannot leave the building twice.
/// </summary>
public sealed class ProcessedEventMigration(NpgsqlDataSource ds)
{
    public async Task MigrateAsync(CancellationToken ct)
    {
        var table = GetProcessedEventsTable();

        await using var conn = await ds.OpenConnectionAsync(ct);
        await table.MigrateAsync(conn);
    }

    private static Table GetProcessedEventsTable()
    {
        var table = new Table("notifications.processed_events");
        table.AddColumn<Guid>("event_id").AsPrimaryKey();

        var messageType = new TableColumn("message_type", "varchar(200)") { AllowNulls = false };
        table.AddColumn(messageType);

        table.AddColumn<DateTimeOffset>("processed_at").NotNull().DefaultValueByExpression("now()");

        // The primary key is the unique constraint on the event id — a second unique index on the
        // same column would only be another btree to maintain on every insert.
        return table;
    }
}
