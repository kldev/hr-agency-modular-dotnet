using HrAgencySystem.JobDescription.Application.Commands;
using HrAgencySystem.JobDescription.Events;
using HrAgencySystem.SharedKernel.Exception;
using HrAgencySystem.SharedKernel.Snapshots;
using HrAgencySystem.SharedKernel.Time;
using Wolverine.Marten;

namespace HrAgencySystem.JobDescription.Application.Handlers;

public static class UpdateJobDescriptionHandler
{
    [AggregateHandler]
    public static async Task<(JobDescriptionUpdated, Wolverine.Marten.Events)> Handle(
        UpdateJobDescription command,
        Domain.JobDescription aggregate,
        IUserSnapshotRepository snapshotRepository,
        IClock clock,
        CancellationToken ct)
    {
        if (aggregate == null) throw new NotFoundException("Not found " + command.JobDescriptionId);
        
        var (title, summary, description,
            location, responsibilities,
            requirements, skills, salaryRange, countryCode) = JobDescriptionDataFactory.Create(command);

        var modifiedBy = await snapshotRepository.GetUserAsync(command.ModifiedBy, ct);
        if (modifiedBy == null)
            throw new BusinessRuleException(IUserSnapshotRepository.NotFoundMessage);

        var @event = new JobDescriptionUpdated(
            title.Value,
            summary.Value,
            description.Value,
            [.. responsibilities.Select(z => z.Value)],
            [.. requirements.Select(x => x.Value)],
            [.. skills.Select(x => x.Value)],
            location.Value,
            countryCode.Value,
            command.EmploymentType,
            command.WorkMode,
            salaryRange.Currency,
            salaryRange.Min,
            salaryRange.Max,
            modifiedBy,
            clock.UtcNow
        );
        
        return (@event, [@event]);
    }
}