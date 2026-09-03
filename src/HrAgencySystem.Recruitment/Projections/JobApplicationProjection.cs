using HrAgencySystem.Recruitment.Domain.Candidate;
using HrAgencySystem.Recruitment.Domain.JobApplication;
using HrAgencySystem.Recruitment.Events.JobApplication;
using HrAgencySystem.SharedKernel.Snapshots;

namespace HrAgencySystem.Recruitment.Projections;

public sealed record JobApplicationProjection(Guid Id,
    Guid OrgId, 
    Guid CandidateId, 
    string Email,
    CandidateSource Source,
    JobApplicationStatus Status,
    Guid? LatestInterviewId,
    Guid? ModifiedById,
    UserSnapshot? ModifiedBy,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt)
{
    public static JobApplicationProjection Create(JobApplicationCreated @event)
    {
        return new JobApplicationProjection(
            @event.JobApplicationId,
            @event.OrganizationId,
            @event.CandidateId,
            @event.Email,
            @event.Source,
            JobApplicationStatus.Applied,
            null,
            null,
            null,
            @event.CreatedAt,
            @event.CreatedAt);
    }
    
    public static JobApplicationProjection Apply(
        JobApplicationProjection projection,
        JobApplicationScreeningStarted @event)
    {
        return ApplyCommon(projection, @event) with
        {
            Status = JobApplicationStatus.Screening
        };
    }

    public static JobApplicationProjection Apply(
        JobApplicationProjection projection,
        JobApplicationAssessmentStarted @event)
    {
        return ApplyCommon(projection, @event) with
        {
            Status = JobApplicationStatus.Assessment
        };
    }

    public static JobApplicationProjection Apply(
        JobApplicationProjection projection,
        JobApplicationInterviewScheduled @event)
    {
        return ApplyCommon(projection, @event) with
        {
            Status = JobApplicationStatus.Interview,
            LatestInterviewId = @event.InterviewId
        };
    }

    public static JobApplicationProjection Apply(
        JobApplicationProjection projection,
        JobApplicationOfferMade @event)
    {
        return ApplyCommon(projection, @event) with
        {
            Status = JobApplicationStatus.Offer
        };
    }

    public static JobApplicationProjection Apply(
        JobApplicationProjection projection,
        JobApplicationHired @event)
    {
        return ApplyCommon(projection, @event) with
        {
            Status = JobApplicationStatus.Hired
        };
    }

    public static JobApplicationProjection Apply(
        JobApplicationProjection projection,
        JobApplicationRejected @event)
    {
        return ApplyCommon(projection, @event) with
        {
            Status = JobApplicationStatus.Rejected
        };
    }

    public static JobApplicationProjection Apply(
        JobApplicationProjection projection,
        JobApplicationWithdrawn @event)
    {
        return ApplyCommon(projection, @event) with
        {
            Status = JobApplicationStatus.Withdrawn
        };
    }
    
    private static JobApplicationProjection ApplyCommon(
        JobApplicationProjection projection,
        IJobApplicationEvent @event)
    {
        return projection with
        {
            ModifiedById = @event.AuthorId,
            ModifiedBy = @event.Author,
            UpdatedAt = @event.OccurredAt
        };
    }
}