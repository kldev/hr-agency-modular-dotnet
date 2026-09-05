using HrAgencySystem.Recruitment.Application.Port;
using HrAgencySystem.Recruitment.Documents;
using HrAgencySystem.Recruitment.Domain.Applications;
using HrAgencySystem.Recruitment.Domain.Candidates;
using HrAgencySystem.Recruitment.Events.JobApplication;
using HrAgencySystem.SharedKernel.Snapshots;

namespace HrAgencySystem.Recruitment.Projections;

public sealed record JobApplicationProjection(
    // ReSharper disable once NotAccessedPositionalProperty.Global
    Guid Id,
    // ReSharper disable once NotAccessedPositionalProperty.Global
    Guid OrgId,
    // ReSharper disable once NotAccessedPositionalProperty.Global
    string JobPostTitle,
    // ReSharper disable once NotAccessedPositionalProperty.Global
    string ApplicantEmail,
    // ReSharper disable once NotAccessedPositionalProperty.Global
    string ApplicantPhone,
    // ReSharper disable once NotAccessedPositionalProperty.Global
    string ApplicantFullName,
    // ReSharper disable once NotAccessedPositionalProperty.Global
    Guid CandidateId,
    // ReSharper disable once NotAccessedPositionalProperty.Global
    CandidateInfo CandidateInfo,
    // ReSharper disable once NotAccessedPositionalProperty.Global
    CandidateSource Source,
    // ReSharper disable once NotAccessedPositionalProperty.Global
    JobApplicationStatus Status,
    // ReSharper disable once NotAccessedPositionalProperty.Global
    Guid? LatestInterviewId,
    // ReSharper disable once NotAccessedPositionalProperty.Global
    Guid? ModifiedById,
    // ReSharper disable once NotAccessedPositionalProperty.Global
    UserSnapshot? ModifiedBy,
    // ReSharper disable once NotAccessedPositionalProperty.Global
    DateTimeOffset CreatedAt,
    // ReSharper disable once NotAccessedPositionalProperty.Global
    DateTimeOffset UpdatedAt,
    // ReSharper disable once NotAccessedPositionalProperty.Global
    IReadOnlyList<Tag> Tags,
    // ReSharper disable once NotAccessedPositionalProperty.Global
    IReadOnlyList<Guid> TagsIds,
    // ReSharper disable once NotAccessedPositionalProperty.Global
    Guid CompanyId,
    // ReSharper disable once NotAccessedPositionalProperty.Global
    CompanySnapshot Company)
{
    public static JobApplicationProjection Create(JobApplicationCreated @event)
    {
        return new JobApplicationProjection(
            @event.JobApplicationId,
            @event.OrganizationId,
            @event.JobPostTitle,
            @event.ApplicantEmail ?? @event.CandidateInfo.Email,
            @event.ApplicantPhone ?? "",
            @event.FullName,
            @event.CandidateInfo.CandidateId,
            @event.CandidateInfo,
            @event.Source,
            JobApplicationStatus.Applied,
            null,
            null,
            null,
            @event.CreatedAt,
            @event.CreatedAt,
            [],
            [],
            @event.Company.Id,
            @event.Company);
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
    
    public JobApplicationProjection Apply(JobApplicationTagged @event)
    {
        if (TagsIds.Contains(@event.Tag.Id)) return this;
        
        var tags = Tags
            .Append(@event.Tag)
            .ToArray();
        var tagIds = TagsIds.Append(@event.Tag.Id).ToArray();
        
        return this with
        {
            ModifiedBy = @event.Author,
            Tags = tags,
            TagsIds = tagIds
        };
    }

    public JobApplicationProjection Apply(JobApplicationTagRemoved @event)
    {
        var tags = Tags.Where(z => z.Id != @event.Tag.Id).ToArray();
        var tagIds = tags.Select(t => t.Id).ToArray();
        return this with
        {
            ModifiedBy = @event.RemovedBy,
            Tags = tags,
            TagsIds = tagIds
        };
    }
}