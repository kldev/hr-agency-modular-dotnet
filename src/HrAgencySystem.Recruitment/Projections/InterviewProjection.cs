using HrAgencySystem.Recruitment.Domain.Interviews;
using HrAgencySystem.Recruitment.Events.Interviews;
using HrAgencySystem.SharedKernel.Snapshots;

namespace HrAgencySystem.Recruitment.Projections;

public sealed record InterviewProjection(
    Guid InterviewId,
    Guid OrgId,
    Guid ApplicationId,
    Guid CandidateId,
    InterviewStatus Status,
    DateTimeOffset ScheduleAt,
    string Timezone,
    Guid InterviewerId,
    UserSnapshot Interviewer,
    InterviewFormat Format,
    InterviewType InterviewType,
    Guid CreatedByUserId,
    UserSnapshot CreatedBy,
    string Note,
    Guid? ModifiedByUserId,
    UserSnapshot? ModifiedBy,
    DateTimeOffset CreatedAt,
    DateTimeOffset? ModifiedAt)
{
    public static InterviewProjection Create(InterviewCreated @event)
    {
        return new InterviewProjection(
            @event.InterviewId,
            @event.OrganizationId,
            @event.JobApplicationId,
            @event.CandidateId,
            InterviewStatus.Planned,
            @event.ScheduleAt,
            @event.Timezone,
            @event.Interviewer.Id,
            @event.Interviewer,
            @event.Format,
            @event.InterviewType,
            @event.Author.Id,
            @event.Author,
            @event.Note,
            null, null, @event.OccurredAt, null
        );
    }
    
    public InterviewProjection Apply(InterviewFormatChanged @event)
    {
        return this with
        {
            Format = @event.NewFormat,
            ModifiedBy = @event.Author,
            ModifiedByUserId = @event.Author.Id,
            ModifiedAt = @event.OccurredAt
        };
    }
    
    public InterviewProjection Apply(InterviewStatusChanged @event)
    {
        return this with
        {
            Status = @event.NewStatus,
            ModifiedBy = @event.Author,
            ModifiedByUserId = @event.Author.Id,
            ModifiedAt = @event.OccurredAt
        };
    }
    
    public InterviewProjection Apply(InterviewerChanged @event)
    {
        return this with
        {
            Interviewer = @event.NewInterviewer,
            InterviewId = @event.NewInterviewer.Id,
            ModifiedBy = @event.Author,
            ModifiedByUserId = @event.Author.Id,
            ModifiedAt = @event.OccurredAt
        };
    }
}