using HrAgencySystem.SharedKernel.Tenant;
using HrAgencySystem.Tasks.Application.Port;
using HrAgencySystem.Tasks.Domain;
using HrAgencySystem.Tasks.Projections;
using Marten;

namespace HrAgencySystem.Tasks.Infrastructure.Query;

// ReSharper disable once ClassNeverInstantiated.Global
public sealed class TaskItemsQueryRepository(IQuerySession session) : ITaskItemsQueryRepository
{
    public async Task<TaskBoard> GetBoard(
        OrganizationId organizationId,
        TaskBoardQuery query,
        CancellationToken ct
    )
    {
        var mine = Mine(organizationId.Value, query);
        var (from, to) = (query.Range.From, query.Range.To);

        var active = await Mine(organizationId.Value, query)
            .Where(t => t.Status == TaskItemStatus.Open && t.DueAt < to)
            .OrderBy(t => t.DueAt)
            .ThenBy(t => t.Id)
            .Take(ITaskItemsQueryRepository.SectionLimit)
            .ToListAsync(ct);

        var completed = await mine.Where(t =>
                t.Status == TaskItemStatus.Done && t.CompletedAt >= from && t.CompletedAt < to
            )
            .OrderByDescending(t => t.CompletedAt)
            .ThenBy(t => t.Id)
            .Take(ITaskItemsQueryRepository.SectionLimit)
            .ToListAsync(ct);

        return new TaskBoard(
            from,
            to,
            [.. active.Select(t => TaskItemRow.From(t, query.Now))],
            [.. completed.Select(t => TaskItemRow.From(t, query.Now))]
        );
    }

    public Task<TaskItemProjection?> GetTask(
        OrganizationId organizationId,
        Guid taskId,
        CancellationToken ct
    ) =>
        session
            .Query<TaskItemProjection>()
            .Where(t => t.OrganizationId == organizationId.Value && t.Id == taskId)
            .FirstOrDefaultAsync(ct);

    private IQueryable<TaskItemProjection> Mine(Guid organizationId, TaskBoardQuery query)
    {
        var tasks = session
            .Query<TaskItemProjection>()
            .Where(t => t.OrganizationId == organizationId && t.AssigneeId == query.AssigneeId);

        return query.CompanyId is { } companyId ? tasks.Where(t => t.CompanyId == companyId) : tasks;
    }
}
