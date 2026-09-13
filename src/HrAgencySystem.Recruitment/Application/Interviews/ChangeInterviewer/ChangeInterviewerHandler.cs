using HrAgencySystem.Recruitment.Domain.Interviews;
using HrAgencySystem.Recruitment.Events.Applications;
using HrAgencySystem.Recruitment.Events.Interviews;
using HrAgencySystem.Recruitment.Services;
using HrAgencySystem.SharedKernel.Exception;
using HrAgencySystem.SharedKernel.Time;
using HrAgencySystem.SharedKernel.ValueObjects;
using Marten;
using Wolverine.Marten;

namespace HrAgencySystem.Recruitment.Application.Interviews.ChangeInterviewer;

public static class ChangeInterviewerHandler
{
    [AggregateHandler]
    public static async Task<(InterviewerChanged, Wolverine.Marten.Events)> Handle(
        ChangeInterviewer command,
        Interview aggregate,
        IDocumentSession session,
        IRecruitmentService service,
        IClock clock,
        CancellationToken ct)
    {
        service.ValidateAggregateUpdate(aggregate, command.OrganizationId);

        var user = await service.GetUserAsync(command.ModifiedBy, ct);
        var interviewer = await service.GetUserAsync(command.InterviewerId, ct);

        var @event =
            new InterviewerChanged(command.InterviewId, 
                aggregate.Interviewer, 
                interviewer, 
                user, 
                clock.UtcNow);

        if (!string.IsNullOrEmpty(command.Note)) return (@event, [@event]);
        
        var applicationInfo = await service.GetApplicationAsync(aggregate.JobApplicationId.Value, command.OrganizationId, ct);
        var (shortNote, error) = ShortNote.TryCreate(command.Note ??"", false);
        if (error != null) throw new ValidationException(error);
        var noteEvent = new JobApplicationNoteAdded(aggregate.JobApplicationId.Value, applicationInfo.CandidateId,
            clock.UtcNow, shortNote!.Value, user);

        session.Events.Append(aggregate.JobApplicationId.Value, noteEvent);

        return (@event, [@event]);
    }
}