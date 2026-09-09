using HrAgencySystem.Recruitment.Application.JobApplications.Queries;
using HrAgencySystem.Recruitment.Application.Port;
using HrAgencySystem.Recruitment.Domain.Applications;
using HrAgencySystem.Recruitment.Events.Applications;
using HrAgencySystem.Recruitment.Services;
using HrAgencySystem.SharedKernel.Exception;
using HrAgencySystem.SharedKernel.Port;
using HrAgencySystem.SharedKernel.Snapshots;
using HrAgencySystem.SharedKernel.Time;
using HrAgencySystem.SharedKernel.ValueObjects;
using Wolverine.Marten;

namespace HrAgencySystem.Recruitment.Application.JobApplications.ChangeStatus;

// ReSharper disable once UnusedType.Global
public static class ChangeJobApplicationStatusHandler
{
    [AggregateHandler]
    public static async Task<(ChangeJobApplicationStatusResult, Wolverine.Marten.Events)> Handle(
        ChangeJobApplicationStatus command,
        JobApplication aggregate,
        IRecruitmentService service,
        INoteRepository noteRepository,
        IClock clock,
        CancellationToken ct
    )
    {
        var oldStatus = aggregate.Status;
        var now = clock.UtcNow;

        if (aggregate.OrganizationId.Value != command.OrganizationId)
            throw new BusinessRuleException(IOrganizationChecker.OrganizationCheckMessage);
        
        var user = await  service.GetUserAsync(command.ModifiedBy, ct);
        var concreteEvent = GetConcreteEvent(command, now, user);
        var result = new ChangeJobApplicationStatusResult(oldStatus, command.Status);

        var newStatus = Enum.Parse<JobApplicationStatus>(command.Status.ToString());

        ValidatePolicy(aggregate, newStatus);

        var @event = new JobApplicationStatusChanged(aggregate.Id.Value, aggregate.CandidateId.Value, now, oldStatus,
            newStatus, user);

        var events = new List<IJobApplicationEvent>
        {
            concreteEvent,
            @event
        };

        if (string.IsNullOrEmpty(command.Note)) return (result, [.. events]);

        var noteAddedEvent = await CreateApplicationNoteAddedEvent(command, aggregate, noteRepository, user, now);
        events.Add(noteAddedEvent);

        return (result, [..events]);
    }

    private static void ValidatePolicy(JobApplication aggregate, JobApplicationStatus newStatus)
    {
        var changeAllowed = JobApplicationStatusChangePolicy.Allow(aggregate.Status, newStatus);
        if (!changeAllowed)
            throw new BusinessRuleException($"Not allowed to change job application status form {aggregate.Status} to {newStatus}");
    }

    private static async Task<JobApplicationNoteAdded> CreateApplicationNoteAddedEvent(ChangeJobApplicationStatus command, JobApplication aggregate,
        INoteRepository noteRepository, UserSnapshot user, DateTimeOffset now)
    {
        var (shortNote, error) = ShortNote.TryCreate(command.Note);
        if (error != null) throw new ValidationException(error);

        var saveNote = new CreateNoteDocument(command.JobApplicationId, command.OrganizationId, aggregate.CandidateId.Value,
            shortNote!);
        await noteRepository.CreateNoteAsync(saveNote, user);
        
        var noteAddedEvent = new JobApplicationNoteAdded(aggregate.Id.Value, aggregate.CandidateId.Value, now,
            command.Note, user);
        return noteAddedEvent;
    }
    
    private static IJobApplicationEvent GetConcreteEvent(ChangeJobApplicationStatus command, DateTimeOffset now,
        UserSnapshot user)
    {
        var newStatus = Enum.Parse<JobApplicationStatus>(command.Status.ToString());
        
        if (newStatus == JobApplicationStatus.Interview && !command.InterviewId.HasValue)
            throw new BusinessRuleException("Interview id must be specified.");

        return newStatus switch
        {
            JobApplicationStatus.Screening => new JobApplicationScreeningStarted(command.JobApplicationId, now, user),
            JobApplicationStatus.Interview => new JobApplicationInterviewScheduled(command.JobApplicationId, now, user,
                command.InterviewId!.Value),
            JobApplicationStatus.Assessment => new JobApplicationAssessmentStarted(command.JobApplicationId, now, user),
            JobApplicationStatus.Offer => new JobApplicationOfferMade(command.JobApplicationId, now, user),
            JobApplicationStatus.Rejected => new JobApplicationRejected(command.JobApplicationId, now, user),
            JobApplicationStatus.Withdrawn => new JobApplicationWithdrawn(command.JobApplicationId, now, user),
            JobApplicationStatus.Hired => new JobApplicationHired(command.JobApplicationId, now, user),
            JobApplicationStatus.Applied => throw new BusinessRuleException(
                "A job application cannot return to its initial status."),
            _ => throw new BusinessRuleException("Unexpected job application status: " + command.Status)
        };
    }
}

public record ChangeJobApplicationStatusResult(JobApplicationStatus OldStatus, JobApplicationUpdateStatus NewStatus);