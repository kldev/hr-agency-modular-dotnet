using HrAgencySystem.JobDescription.Application.Commands;
using HrAgencySystem.JobDescription.Events;
using HrAgencySystem.SharedKernel.Exception;
using HrAgencySystem.SharedKernel.Time;
using Wolverine.Marten;

namespace HrAgencySystem.JobDescription.Application.Handlers;

public static class UpdateJobDescriptionHandler
{
    [AggregateHandler]
    public static Task<(JobDescriptionUpdated, Wolverine.Marten.Events)> Handle(
        UpdateJobDescription command,
        Domain.JobDescription aggregate,
        IClock clock)
    {
        if (aggregate == null) throw new NotFoundException("Not found " + command.JobDescriptionId);

        var (title, summary, description,
            location, responsibilities,
            requirements, skills, salaryRange, countryCode) = JobDescriptionDataFactory.Create(command);

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
            salaryRange,
            clock.UtcNow
        );
        
        return Task.FromResult<(JobDescriptionUpdated, Wolverine.Marten.Events)>((@event, [@event]));
    }
}