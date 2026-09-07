using HrAgencySystem.Recruitment.Domain.JobPostings;
using HrAgencySystem.Recruitment.Events.JobPostings;
using HrAgencySystem.SharedKernel.Exception;
using HrAgencySystem.SharedKernel.Port;
using HrAgencySystem.SharedKernel.Snapshots;
using HrAgencySystem.SharedKernel.Time;
using Wolverine.Marten;

namespace HrAgencySystem.Recruitment.Application.JobPosting.ChangeStatus;

public static class ChangeJobPostStatusHandler
{
    [AggregateHandler]
    public static async Task<(JobPostStatusChanged, Wolverine.Marten.Events)> Handle(
        ChangeJobPostStatus command,
        JobPost aggregate,
        IUserSnapshotRepository snapshotRepository,
        IClock clock,
        CancellationToken ct
    )
    {
        var oldStatus = aggregate.Status;
        var now = clock.UtcNow;

        if (aggregate.OrganizationId.Value != command.OrganizationId)
            throw new BusinessRuleException(IOrganizationChecker.OrganizationCheckMessage);

        var user = await GetModifiedBy(snapshotRepository, command.ModifiedBy, ct);
        var concreteEvent = GetConcreteEvent(command, now, user);

        var newStatus = command.Status.ToDomain();
       // var result = new ChangeJobPostStatusResult(oldStatus, newStatus);

        ValidatePolicy(aggregate, newStatus);

        var @event = new JobPostStatusChanged(aggregate.Id.Value, aggregate.CompanyId.Value,
            aggregate.OrganizationId.Value, oldStatus,
            newStatus, clock.UtcNow, user);

        return (@event, [concreteEvent, @event]);
    }

    private static async Task<UserSnapshot> GetModifiedBy(IUserSnapshotRepository repository, Guid modifiedById,
        CancellationToken ct)
    {
        var user = await repository.GetUserAsync(modifiedById, ct);
        return user ?? throw new BusinessRuleException(IUserSnapshotRepository.NotFoundMessage);
    }

    private static void ValidatePolicy(JobPost aggregate, JobPostStatus newStatus)
    {
        var changeAllowed = JobPostStatusChangePolicy.Allow(aggregate.Status, newStatus);
        if (!changeAllowed)
            throw new BusinessRuleException(
                $"Not allowed to change job post status form {aggregate.Status} to {newStatus}");
    }

    private static IJobPostEvent GetConcreteEvent(ChangeJobPostStatus command, DateTimeOffset now,
        UserSnapshot user)
    {
        var newStatus = command.Status.ToDomain();

        return newStatus switch
        {
            JobPostStatus.Published => new JobPostPublished(command.JobPostId, now, user),
            JobPostStatus.Archived => new JobPostArchived(command.JobPostId, now, user),
            JobPostStatus.Closed => new JobPostClosed(command.JobPostId, now, user),
            JobPostStatus.Draft =>
                throw new BusinessRuleException("A job application cannot return to its initial status."),
            _ => throw new BusinessRuleException("Unexpected job application status: " + command.Status)
        };
    }
}

// public record ChangeJobPostStatusResult(JobPostStatus OldStatus, JobPostStatus NewStatus);