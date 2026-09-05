using HrAgencySystem.Recruitment.Domain.Candidate;
using HrAgencySystem.Recruitment.Domain.Posting;
using HrAgencySystem.Recruitment.Events.JobApplication;
using HrAgencySystem.SharedKernel.Snapshots;
using HrAgencySystem.SharedKernel.Tenant;
using HrAgencySystem.SharedKernel.ValueObjects;

namespace HrAgencySystem.Recruitment.Domain.JobApplication;

public sealed class JobApplication
{
    private JobApplication()
    {
    }

    public JobApplication Empty()
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
        JobPostId = JobPostId.From(@event.JobPostingId);
        CandidateId = CandidateId.From(@event.CandidateInfo.CandidateId);
        Status = JobApplicationStatus.Applied;
        Source = @event.Source;
        CreatedAt = @event.CreatedAt;
        UpdatedAt = @event.CreatedAt;
        Email = Email.Create(@event.CandidateInfo.Email);
    }

    public void Apply(JobApplicationScreeningStarted @event)
    {
        RequireStatus(JobApplicationStatus.Applied);
        Status = JobApplicationStatus.Screening;
        ApplyCommon(@event);
    }

    public void Apply(JobApplicationAssessmentStarted @event)
    {
        RequireStatus(
            JobApplicationStatus.Screening,
            JobApplicationStatus.Interview);
        
        Status = JobApplicationStatus.Assessment;
        ApplyCommon(@event);
    }
    
    public void Apply(JobApplicationInterviewScheduled @event)
    {
        RequireStatus(
            JobApplicationStatus.Screening,
            JobApplicationStatus.Assessment);

        Status = JobApplicationStatus.Interview;
        LatestInterviewId = @event.InterviewId;
        ApplyCommon(@event);
    }

    public void Apply(JobApplicationOfferMade @event)
    {
        RequireStatus(
            JobApplicationStatus.Interview,
            JobApplicationStatus.Assessment);

        Status = JobApplicationStatus.Offer;
        ApplyCommon(@event);
    }

    public void Apply(JobApplicationHired @event)
    {
        Status = JobApplicationStatus.Hired;
     
        ApplyCommon(@event);
    }

    public void Apply(JobApplicationRejected @event)
    {
        RequireNotFinal();

        Status = JobApplicationStatus.Rejected;
        ApplyCommon(@event);
    }

    public void Apply(JobApplicationWithdrawn @event)
    {
        RequireNotFinal();

        Status = JobApplicationStatus.Withdrawn;
        
        ApplyCommon(@event);
    }

    private void RequireStatus(params JobApplicationStatus[] allowedStatuses)
    {
        if (allowedStatuses.Contains(Status))
            return;

        throw new InvalidOperationException(
            $"Cannot change application status from {Status}.");
    }

    private void RequireNotFinal()
    {
        if (Status is
            JobApplicationStatus.Hired or
            JobApplicationStatus.Rejected or
            JobApplicationStatus.Withdrawn)
        {
            throw new InvalidOperationException(
                $"Application is already in final status: {Status}.");
        }
    }

    private void ApplyCommon(IJobApplicationEvent @event)
    {
        UpdatedAt = @event.OccurredAt;
        LastModifiedByUserId = @event.AuthorId;
        LastModifiedByUser = @event.Author;
    }
}