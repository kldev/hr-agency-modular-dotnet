using HrAgencySystem.Recruitment.Domain.Applications;
using HrAgencySystem.Recruitment.Domain.Candidates;
using HrAgencySystem.Recruitment.Domain.JobPostings.ValueObjects;
using HrAgencySystem.Recruitment.Events.Interviews;
using HrAgencySystem.SharedKernel.Snapshots;
using HrAgencySystem.SharedKernel.Tenant;
using HrAgencySystem.SharedKernel.ValueObjects;

namespace HrAgencySystem.Recruitment.Domain.Interviews;

public sealed class Interview : IOrganizationDomain
{
    private Interview()
    {
        
    }

    public static Interview Empty()
    {
        return new Interview();
    }
    
    public InterviewId Id { get; private set; }
    public OrganizationId OrganizationId { get; private set;}
    public JobApplicationId JobApplicationId { get; private set; }
    public CandidateId CandidateId { get; private set; }
    public CompanyId    CompanyId { get; private set; }
    public InterviewStatus Status { get; private set; }
    public DateTimeOffset ScheduleAt { get; private set; }
    public string Timezone { get; private set; } = "";
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; }
    public UserSnapshot Interviewer { get; private set; } = null!;
    public InterviewFormat Format { get; private set; }
    public InterviewType InterviewType { get; private set; }
    
    public UserSnapshot  CreatedByUser { get; private set; } = null!;
    public UserSnapshot? LastModifiedByUser { get; private set; }
    public ShortNote Note { get; private set; } = null!;

    public void Apply(InterviewCreated @event)
    {
        Id = InterviewId.From(@event.InterviewId);
        OrganizationId = OrganizationId.From(@event.OrganizationId);
        JobApplicationId = JobApplicationId.From(@event.JobApplicationId);
        CandidateId = CandidateId.From(@event.CandidateId);
        CompanyId = CompanyId.From(@event.CompanyId);
        ScheduleAt= @event.ScheduleAt;
        Timezone = @event.Timezone;
        Interviewer = @event.Interviewer;
        Format = @event.Format;
        InterviewType = @event.InterviewType;
        Status = InterviewStatus.Planned;
        CreatedByUser = @event.Author;
        Note = ShortNote.Create(@event.Note, false);
        CreatedAt = @event.OccurredAt;
    }

    public void Apply(InterviewFormatChanged @event)
    {
        Format = @event.NewFormat;
        ApplyCommon(@event);
    }
    
    public void Apply(InterviewStatusChanged @event)
    {
        Status = @event.NewStatus;
        ApplyCommon(@event);
    }
    
    public void Apply(InterviewerChanged @event)
    {
        Interviewer = @event.NewInterviewer;
        ApplyCommon(@event);
    }
    
    private void ApplyCommon(IInterviewEvent @event)
    {
        LastModifiedByUser = @event.Author;
        UpdatedAt = @event.OccurredAt;
    }
} 