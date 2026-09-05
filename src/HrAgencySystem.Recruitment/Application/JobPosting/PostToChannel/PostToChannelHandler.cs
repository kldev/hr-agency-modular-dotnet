using HrAgencySystem.Recruitment.Domain.JobPostings;
using HrAgencySystem.Recruitment.Events.JobPostings;
using HrAgencySystem.SharedKernel.Exception;
using HrAgencySystem.SharedKernel.Port;
using HrAgencySystem.SharedKernel.Snapshots;
using HrAgencySystem.SharedKernel.Time;
using Wolverine.Marten;

namespace HrAgencySystem.Recruitment.Application.JobPosting.PostToChannel;

// ReSharper disable once UnusedType.Global
public static class PostToChannelHandler
{
    [AggregateHandler]
    public static async Task<(JobPostedToChannel, Wolverine.Marten.Events)> Handle(
        PostToChannel command, JobPost aggregate, 
        IUserSnapshotRepository snapshotRepository,
        IOrganizationChecker checker, 
        IClock clock,
        CancellationToken ct)
    {
        await ValidateOrganization(command, checker, ct);
        var user = await GetModifiedBy(snapshotRepository, command.ModifiedBy, ct);

        var @event = new JobPostedToChannel(command.JobPostId, command.Channel, clock.UtcNow, user);

        return (@event, [@event]);
    }

    private static async Task ValidateOrganization(PostToChannel command, IOrganizationChecker checker,
        CancellationToken ct)
    {
        var exits = await checker.Exists(command.OrganizationId, ct);
        if (!exits) throw new BusinessRuleException(IOrganizationChecker.OrganizationCheckMessage);
    }
    
    private static async Task<UserSnapshot> GetModifiedBy(IUserSnapshotRepository repository, Guid modifiedBy,
        CancellationToken ct)
    {
        var createdBy = await repository.GetUserAsync(modifiedBy, ct);
        return createdBy ?? throw new BusinessRuleException(IUserSnapshotRepository.NotFoundMessage);
    }
}