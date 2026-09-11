using Amazon.Util.Internal.PlatformServices;
using HrAgencySystem.Recruitment.Application.Port;
using HrAgencySystem.Recruitment.Domain.Interviews;
using HrAgencySystem.Recruitment.Events.Interviews;
using HrAgencySystem.SharedKernel.Snapshots;

namespace HrAgencySystem.Recruitment.Projections;

public sealed record InterviewProjection(
    Guid Id,
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
    DateTimeOffset? ModifiedAt,
    CandidateInfo ApplicantInfo)
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
            null, 
            null, 
            @event.OccurredAt, 
            null,
            @event.Candidate
        );
    }
    
    private static InterviewProjection ApplyCommon(
        InterviewProjection projection,
        IInterviewEvent @event)
    {
        return projection with
        {
            ModifiedByUserId = @event.Author.Id,
            ModifiedBy = @event.Author,
            ModifiedAt = @event.OccurredAt
        };
    }
    
    public InterviewProjection Apply(InterviewFormatChanged @event)
    {
        return ApplyCommon(this, @event) with
        {
            Format = @event.NewFormat,
        };
    }
    
    public InterviewProjection Apply(InterviewStatusChanged @event)
    {
        return ApplyCommon(this, @event) with
        {
            Status = @event.NewStatus,
        };
    }
    
    public InterviewProjection Apply(InterviewerChanged @event)
    {
        return ApplyCommon(this, @event) with
        {
            Interviewer = @event.NewInterviewer,
        };
    }
}