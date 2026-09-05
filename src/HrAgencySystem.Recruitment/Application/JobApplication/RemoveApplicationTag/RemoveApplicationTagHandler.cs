using HrAgencySystem.Recruitment.Application.Port;
using HrAgencySystem.Recruitment.Events.JobApplication;
using HrAgencySystem.SharedKernel.Exception;
using HrAgencySystem.SharedKernel.Snapshots;
using HrAgencySystem.SharedKernel.Time;
using Wolverine.Marten;

namespace HrAgencySystem.Recruitment.Application.JobApplication.RemoveApplicationTag;

public static class RemoveApplicationTagHandler
{
    [AggregateHandler]
    public static async Task<(JobApplicationTagRemoved, Wolverine.Marten.Events)> Handle(
        RemoveApplicationTag command, 
        Domain.Applications.JobApplication aggregate,
        ITagRepository tagRepository,
        IUserSnapshotRepository snapshotRepository,
        IClock clock,
        CancellationToken ct)
    {
        var tag = await tagRepository.GetTag(command.TagId, ct);
        var user = await GetModifiedBy(snapshotRepository, command.ModifiedBy, ct);

        var @event = new JobApplicationTagRemoved(command.JobApplicationId, tag, user, clock.UtcNow);
        
        return (@event, [@event]);
    }

    private static async Task<UserSnapshot> GetModifiedBy(IUserSnapshotRepository snapshotRepository, Guid createdById, 
        CancellationToken ct)
    {
        var user = await snapshotRepository.GetUserAsync(createdById, ct);
        return user ?? throw new NotFoundException(IUserSnapshotRepository.NotFoundMessage);
    }
}