using HrAgencySystem.Recruitment.Application.Port;
using HrAgencySystem.Recruitment.Events.Applications;
using HrAgencySystem.SharedKernel.Exception;
using HrAgencySystem.SharedKernel.Snapshots;
using HrAgencySystem.SharedKernel.Time;
using Wolverine.Marten;
namespace HrAgencySystem.Recruitment.Application.JobApplication.TagApplication;

// ReSharper disable once UnusedType.Global
public static class TagApplicationHandler
{
    [AggregateHandler]
    public static async Task<(JobApplicationTagged, Wolverine.Marten.Events)> Handle(
        TagApplication command, 
        Domain.Applications.JobApplication aggregate,
        ITagRepository tagRepository,
        IUserSnapshotRepository snapshotRepository,
        IClock clock,
        CancellationToken ct)
    {
        var tag = await tagRepository.GetTag(command.TagId, ct);
        var user = await GetCreatedBy(snapshotRepository, command.CreatedBy, ct);

        var @event = new JobApplicationTagged(command.JobApplicationId, tag, user, clock.UtcNow);
        
        return (@event, [@event]);
    }

    private static async Task<UserSnapshot> GetCreatedBy(IUserSnapshotRepository snapshotRepository, Guid createdById, 
        CancellationToken ct)
    {
        var user = await snapshotRepository.GetUserAsync(createdById, ct);
        return user ?? throw new NotFoundException(IUserSnapshotRepository.NotFoundMessage);
    }
}