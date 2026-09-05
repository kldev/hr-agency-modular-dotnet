using HrAgencySystem.Recruitment.Feeds.Model;
using HrAgencySystem.Recruitment.Feeds.Port;
using Npgsql;
using Weasel.Postgresql;

namespace HrAgencySystem.Recruitment.Feeds.Repository;

internal class JobFeedTaskRepository(NpgsqlDataSource ds) : IJobFeedTaskRepository
{
    private const string SELECT_BATCH_SQL = """
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

    private const string INSERT_SQL = """
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
    
    public async Task Save(JobFeedTask task, CancellationToken ct)
    {
        await using var conn = await ds.OpenConnectionAsync(ct);
        var cmd = conn.CreateCommand(INSERT_SQL);
        cmd.AddNamedParameter("id", task.Id);
        cmd.AddNamedParameter("orgId",task.OrganizationId);
        await cmd.ExecuteNonQueryAsync(ct);

    }

    public async Task<IReadOnlyList<JobFeedTask>> FindPendingForUpdate(int batchSize, CancellationToken ct)
    {
        await using var conn = await ds.OpenConnectionAsync(ct);
        var cmd = conn.CreateCommand(SELECT_BATCH_SQL);
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
            var cmd = ds.CreateCommand(INSERT_SQL);
            cmd.AddNamedParameter("id", task.Id);
            cmd.AddNamedParameter("orgId",task.OrganizationId);
            await cmd.ExecuteNonQueryAsync(ct);
        }
    }
}