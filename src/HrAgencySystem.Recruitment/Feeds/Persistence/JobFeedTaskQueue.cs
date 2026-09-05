using HrAgencySystem.Recruitment.Feeds.Model;
using HrAgencySystem.Recruitment.Feeds.Port;
using Npgsql;

namespace HrAgencySystem.Recruitment.Feeds.Persistence;

internal sealed class JobFeedTaskQueue(NpgsqlDataSource dataSource) : IJobFeedTaskQueue
{
    private const string Sql = """
                               UPDATE jobs.job_feed_tasks
                               SET
                                   status = 'PROCESSING',
                                   attempts = attempts + 1,
                                   started_at = now()
                               WHERE id IN (
                                   SELECT id
                                   FROM jobs.job_feed_tasks
                                   WHERE status = 'PENDING'
                                     AND completed_at IS NULL
                                   ORDER BY created_at
                                   LIMIT @batchSize
                                   FOR UPDATE SKIP LOCKED
                               )
                               RETURNING
                                   id,
                                   organization_id,
                                   status,
                                   attempts,
                                   created_at,
                                   started_at,
                                   completed_at,
                                   error_message
                               """;

    public async Task<IReadOnlyList<JobFeedTask>> Fetch(
        int batchSize,
        CancellationToken ct)
    {
        await using var connection =
            await dataSource.OpenConnectionAsync(ct);

        await using var command = connection.CreateCommand();
        command.CommandText = Sql;

        command.Parameters.AddWithValue("batchSize", batchSize);

        var tasks = new List<JobFeedTask>();

        await using var reader =
            await command.ExecuteReaderAsync(ct);

        while (await reader.ReadAsync(ct))
        {
            var status = Enum.Parse<JobFeedTaskStatus>(
                reader.GetString(2),
                ignoreCase: true);
            
            tasks.Add(new JobFeedTask(
                reader.GetGuid(0),
                reader.GetGuid(1),
                status,
                reader.GetInt32(3),
                reader.GetFieldValue<DateTimeOffset>(4),
                reader.GetFieldValue<DateTimeOffset?>(5),
                reader.GetFieldValue<DateTimeOffset?>(6),
                reader.IsDBNull(7)
                    ? null
                    : reader.GetString(7)));
        }

        return [.. tasks.OrderBy(x => x.CreatedAt)];
    }
}