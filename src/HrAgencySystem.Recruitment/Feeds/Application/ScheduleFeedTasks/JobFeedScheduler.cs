using HrAgencySystem.Recruitment.Feeds.Model;
using HrAgencySystem.Recruitment.Feeds.Port;
using HrAgencySystem.SharedKernel.Services;
using HrAgencySystem.SharedKernel.Time;

namespace HrAgencySystem.Recruitment.Feeds.Application.ScheduleFeedTasks;

internal sealed class JobFeedScheduler(
    IOrganizationService organizationService,
    IJobFeedTaskRepository repository,
    IClock clock) : IJobFeedScheduler
{
    public async Task ScheduleAsync(CancellationToken ct)
    {
        var organizations = await organizationService.GetActiveOrganizationsAsync(ct);
        var saveBatch =
            organizations.Select(o => JobFeedTask.Create(o.Id, clock.UtcNow));

        await repository.BatchSave([.. saveBatch], ct);
    }
}