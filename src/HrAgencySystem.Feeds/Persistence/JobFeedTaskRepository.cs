using HrAgencySystem.Feeds.Model;
using HrAgencySystem.Feeds.Port;
using Npgsql;
using Weasel.Postgresql;

namespace HrAgencySystem.Feeds.Persistence;

internal class JobFeedTaskRepository(NpgsqlDataSource ds) : IJobFeedTaskRepository
{
    private const string InsertSql = """
        INSERT INTO jobs.job_feed_tasks (
            id,
            organization_id,
            status,
            attempts,
            created_at
        )
        VALUES (:id, :orgId, 'PENDING', 0, now())
        ON CONFLICT DO NOTHING
        """;

    private const string MarkFailedSql = """
        UPDATE jobs.job_feed_tasks
        SET status =  CASE 
                              WHEN attempts + 1 >= 3 THEN 'FAILED'
                              WHEN status = 'PROCESSING' THEN 'PENDING'
                              ELSE status
        END,
                  attempts = attempts + 1,
                  error_message = :error
              WHERE id = :id AND status = 'PROCESSING'
        """;

    private const string MarkCompletedSql = """
        UPDATE jobs.job_feed_tasks
              SET status = 'COMPLETED',
                  completed_at = now()
              WHERE id = :id AND status = 'PROCESSING'
        """;

    public async Task Save(JobFeedTask task, CancellationToken ct)
    {
        await using var conn = await ds.OpenConnectionAsync(ct);
        var cmd = conn.CreateCommand(InsertSql);
        cmd.AddNamedParameter("id", task.Id);
        cmd.AddNamedParameter("orgId", task.OrganizationId);
        await cmd.ExecuteNonQueryAsync(ct);
    }

    public async Task BatchSave(IReadOnlyList<JobFeedTask> tasks, CancellationToken ct)
    {
        await using var conn = await ds.OpenConnectionAsync(ct);
        foreach (var task in tasks)
        {
            var cmd = ds.CreateCommand(InsertSql);
            cmd.AddNamedParameter("id", task.Id);
            cmd.AddNamedParameter("orgId", task.OrganizationId);
            await cmd.ExecuteNonQueryAsync(ct);
        }
    }

    public async Task MarkFailed(Guid id, string errorMessage, CancellationToken ct)
    {
        await using var conn = await ds.OpenConnectionAsync(ct);
        var cmd = conn.CreateCommand(MarkFailedSql);
        cmd.AddNamedParameter("id", id);
        cmd.AddNamedParameter("error", errorMessage);
        await cmd.ExecuteNonQueryAsync(ct);
    }

    public async Task MarkCompleted(Guid id, CancellationToken ct)
    {
        await using var conn = await ds.OpenConnectionAsync(ct);
        var cmd = conn.CreateCommand(MarkCompletedSql);
        cmd.AddNamedParameter("id", id);
        await cmd.ExecuteNonQueryAsync(ct);
    }
}
