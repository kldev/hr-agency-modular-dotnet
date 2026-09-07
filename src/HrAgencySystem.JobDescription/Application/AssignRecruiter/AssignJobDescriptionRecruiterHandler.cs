using HrAgencySystem.JobDescription.Events;
using HrAgencySystem.SharedKernel.Exception;
using HrAgencySystem.SharedKernel.Snapshots;
using HrAgencySystem.SharedKernel.Tenant;
using HrAgencySystem.SharedKernel.Time;
using Wolverine.Marten;

namespace HrAgencySystem.JobDescription.Application.AssignRecruiter;

public static class AssignJobDescriptionRecruiterHandler
{
    [AggregateHandler]
    public static async Task<(JobDescriptionRecruiterAssigned,Wolverine.Marten.Events)> Handle(
        AssignJobDescriptionRecruiter command,
        Domain.JobDescription aggregate,
        IUserSnapshotRepository snapshotRepository,
        IClock clock,
        CancellationToken ct)
    {
        if (aggregate == null) throw new NotFoundException("Job description", command.JobDescriptionId);
        
        var recruiter = await GetRecruiter(command, snapshotRepository, ct);

        var modifiedBy = await GetModifiedBy(command, snapshotRepository, ct);

        ValidateOrganization(command, aggregate);

        var @event = new JobDescriptionRecruiterAssigned(recruiter, modifiedBy, clock.UtcNow);

        return (@event, [@event]);
    }

    private static void ValidateOrganization(AssignJobDescriptionRecruiter command, Domain.JobDescription aggregate)
    {
        if (aggregate.OrganizationId.Value != command.OrganizationId)
            throw new BusinessRuleException(OrganizationId.OrganizationNotMatchMessage);
    }

    private static async Task<UserSnapshot> GetModifiedBy(AssignJobDescriptionRecruiter command,
        IUserSnapshotRepository snapshotRepository, CancellationToken ct)
    {
        var modifiedBy = await snapshotRepository.GetUserAsync(command.ModifiedBy, ct);
        return modifiedBy ?? throw new BusinessRuleException(IUserSnapshotRepository.NotFoundMessage);
    }

    private static async Task<UserSnapshot> GetRecruiter(AssignJobDescriptionRecruiter command,
        IUserSnapshotRepository snapshotRepository, CancellationToken ct)
    {
        if (command.RecruiterId == Guid.Empty)
            throw new InValidValueException("Recruiter id has invalid value");
        
        var recruiter = await snapshotRepository.GetUserAsync(command.RecruiterId, ct);
        return recruiter ?? throw new BusinessRuleException(IUserSnapshotRepository.NotFoundMessage);
    }
}