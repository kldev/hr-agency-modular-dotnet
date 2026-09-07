using HrAgencySystem.Recruitment.Domain.Candidates;
using HrAgencySystem.Recruitment.Domain.JobPostings;
using HrAgencySystem.Recruitment.Events.Applications;
using HrAgencySystem.SharedKernel.Snapshots;
using HrAgencySystem.SharedKernel.Tenant;
using HrAgencySystem.SharedKernel.ValueObjects;

namespace HrAgencySystem.Recruitment.Domain.Applications;

public sealed class JobApplication
{
    private JobApplication()
    {
    }

    public static JobApplication Empty()
    {
        return new JobApplication();
    }

    public JobApplicationId Id { get; private set; } = default!;
    public OrganizationId OrganizationId { get; private set; }
    public JobPostId JobPostId { get; private set; } = default!;
    public CandidateId CandidateId { get; private set; } = default!;

    public JobApplicationStatus Status { get; private set; }
    public CandidateSource Source { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; }

    public Email Email { get; private set; } = null!;

    public Guid? LastModifiedByUserId { get; private set; }
    public UserSnapshot? LastModifiedByUser { get; private set; }
    
    public Guid? LatestInterviewId { get; private set; }

    public void Apply(JobApplicationCreated @event)
    {
        Id = JobApplicationId.From(@event.JobApplicationId);
        OrganizationId = OrganizationId.From(@event.OrganizationId);
        JobPostId = JobPostId.From(@event.JobPostId);
        CandidateId = CandidateId.From(@event.CandidateInfo.CandidateId);
        Status = JobApplicationStatus.Applied;
        Source = @event.Source;
        CreatedAt = @event.CreatedAt;
        UpdatedAt = @event.CreatedAt;
        Email = Email.Create(@event.CandidateInfo.Email);
    }

    public void Apply(JobApplicationScreeningStarted @event)
    {
        CheckStatusChangeAllowed(JobApplicationStatus.Screening);
        Status = JobApplicationStatus.Screening;
        ApplyCommon(@event);
    }

    public void Apply(JobApplicationAssessmentStarted @event)
    {
        CheckStatusChangeAllowed(JobApplicationStatus.Assessment);
        
        Status = JobApplicationStatus.Assessment;
        ApplyCommon(@event);
    }
    
    public void Apply(JobApplicationInterviewScheduled @event)
    {
        if (Status == JobApplicationStatus.Interview)
        {
            LatestInterviewId = @event.InterviewId;
            ApplyCommon(@event);
            return;
        }
        
        CheckStatusChangeAllowed(JobApplicationStatus.Interview);

        Status = JobApplicationStatus.Interview;
        LatestInterviewId = @event.InterviewId;
        ApplyCommon(@event);
    }

    public void Apply(JobApplicationOfferMade @event)
    {
        CheckStatusChangeAllowed(JobApplicationStatus.Offer);

        Status = JobApplicationStatus.Offer;
        ApplyCommon(@event);
    }

    public void Apply(JobApplicationHired @event)
    {
        CheckStatusChangeAllowed(JobApplicationStatus.Hired);
        Status = JobApplicationStatus.Hired;
     
        ApplyCommon(@event);
    }

    public void Apply(JobApplicationRejected @event)
    {
        CheckStatusChangeAllowed(JobApplicationStatus.Rejected);

        Status = JobApplicationStatus.Rejected;
        ApplyCommon(@event);
    }

    public void Apply(JobApplicationWithdrawn @event)
    {
        CheckStatusChangeAllowed(JobApplicationStatus.Withdrawn);

        Status = JobApplicationStatus.Withdrawn;
        
        ApplyCommon(@event);
    }
    
    public void Apply(JobApplicationReactivated @event)
    {
        Status = JobApplicationStatus.Screening;
        ApplyCommon(@event);
    }


    private void CheckStatusChangeAllowed(JobApplicationStatus newStatus)
    {
        if (!JobApplicationStatusChangePolicy.Allow(Status, newStatus))
        {
            throw new InvalidOperationException($"Not allowed to change job application status form {Status} to {newStatus}");
        }
            
    }
    
    private void ApplyCommon(IJobApplicationEvent @event)
    {
        UpdatedAt = @event.OccurredAt;
        LastModifiedByUserId = @event.Author.Id;
        LastModifiedByUser = @event.Author;
    }
}