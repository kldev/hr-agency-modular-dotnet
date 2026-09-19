using HrAgencySystem.EmailTemplates.Contracts;
using Npgsql;

namespace HrAgencySystem.NotificationWorker.Infrastructure;

public sealed class ProcessedEventStore(NpgsqlDataSource ds) : IProcessedEventStore
{
    private const string InsertSql = """
        insert into notifications.processed_events (event_id, message_type, processed_at)
        values (@event_id, @message_type, @processed_at)
        """;

    public async Task<bool> TryMarkAsync(IEmailTemplateContract message, CancellationToken ct)
    {
        try
        {
            await using var conn = await ds.OpenConnectionAsync(ct);
            await using var command = new NpgsqlCommand(InsertSql, conn);

            command.Parameters.AddWithValue("event_id", message.EventId);
            command.Parameters.AddWithValue("message_type", message.GetType().Name);
            command.Parameters.AddWithValue("processed_at", DateTimeOffset.UtcNow);

            await command.ExecuteNonQueryAsync(ct);

            return true;
        }
        catch (PostgresException e) when (e.SqlState == PostgresErrorCodes.UniqueViolation)
        {
            // The unique event id is the guard itself: losing the race means somebody else sent it.
            return false;
        }
    }
}
