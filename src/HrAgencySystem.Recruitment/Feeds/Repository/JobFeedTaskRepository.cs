using HrAgencySystem.Recruitment.Feeds.Model;
using HrAgencySystem.Recruitment.Feeds.Port;
using Npgsql;
using Weasel.Postgresql;

namespace HrAgencySystem.Recruitment.Feeds.Repository;

internal class JobFeedTaskRepository(NpgsqlDataSource ds) : IJobFeedTaskRepository
{
    private const string SelectBatchSql = """
                                              select id,
                                                    organization_id,
                                                    status,
                                                    attempts,
                                                    created_at,
                                                    started_at,
                                                    completed_at,
                                                   error_message
                                             from  jobs.job_feed_tasks where completed_at is null
                                                        and status = 'PENDING'
                                                     order by created_at asc
                                                     limit :batchSize
                                                   FOR UPDATE SKIP LOCKED
                                             """;

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
                                               SET status = 'COMPLETED'
                                               WHERE id = :id AND status = 'PROCESSING'
                                         """;
    
    
    public async Task Save(JobFeedTask task, CancellationToken ct)
    {
        await using var conn = await ds.OpenConnectionAsync(ct);
        var cmd = conn.CreateCommand(InsertSql);
        cmd.AddNamedParameter("id", task.Id);
        cmd.AddNamedParameter("orgId",task.OrganizationId);
        await cmd.ExecuteNonQueryAsync(ct);

    }

    public async Task<IReadOnlyList<JobFeedTask>> FindPendingForUpdate(int batchSize, CancellationToken ct)
    {
        await using var conn = await ds.OpenConnectionAsync(ct);
        var cmd = conn.CreateCommand(SelectBatchSql);
        cmd.AddNamedParameter("batchSize", batchSize);

        var cmb = new CommandBuilder(cmd);
        return await conn.FetchListAsync<JobFeedTask>(cmb, (r, tok) =>
        {
            var result = new JobFeedTask(r.GetGuid(0),
                r.GetGuid(1),
                r.GetFieldValue<JobFeedTaskStatus>(2),
                r.GetFieldValue<int>(3),
                r.GetFieldValue<DateTimeOffset>(4),
                r.GetFieldValue<DateTimeOffset?>(5),
                r.GetFieldValue<DateTimeOffset?>(6),
                r.GetFieldValue<string>(7));
            return Task.FromResult(result);
        }, ct);

    }

    public async Task BatchSave(IReadOnlyList<JobFeedTask> tasks,  CancellationToken ct)
    {
        await using var conn = await ds.OpenConnectionAsync(ct);
        foreach (var task in tasks)
        {
            var cmd = ds.CreateCommand(InsertSql);
            cmd.AddNamedParameter("id", task.Id);
            cmd.AddNamedParameter("orgId",task.OrganizationId);
            await cmd.ExecuteNonQueryAsync(ct);
        }
    }

    public async Task MarkFailed(Guid id, string errorMessage, CancellationToken ct)
    {
        await using var conn = await ds.OpenConnectionAsync(ct);
        var cmd = conn.CreateCommand(MarkFailedSql);
        cmd.AddNamedParameter("id", id);
        cmd.AddNamedParameter("error",errorMessage);
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