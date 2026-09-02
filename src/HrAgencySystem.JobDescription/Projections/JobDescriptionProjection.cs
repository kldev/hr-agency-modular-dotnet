using HrAgencySystem.JobDescription.Domain;
using HrAgencySystem.JobDescription.Domain.ValueObjects;
using HrAgencySystem.JobDescription.Events;
using HrAgencySystem.SharedKernel.Snapshots;


namespace HrAgencySystem.JobDescription.Projections;

public sealed record JobDescriptionProjection(
    Guid Id,
    Guid OrganizationId,
    Guid CompanyId,
    string Title,
    string Summary,
    string Description,
    IReadOnlyList<string> Responsibilities,
    IReadOnlyList<string> Requirements,
    IReadOnlyList<string> Skills,
    string? Location,
    string CountryCode,
    EmploymentType EmploymentType,
    WorkMode WorkMode,
    SalaryRange SalaryRange,
    JobDescriptionStatus Status,
    Guid RecruiterId,
    UserSnapshot Recruiter,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt)
{
    public static JobDescriptionProjection Create(
        JobDescriptionCreated @event)
    {
        return new JobDescriptionProjection(
            @event.JobDescriptionId,
            @event.OrganizationId,
            @event.CompanyId,
            @event.Title,
            @event.Summary,
            @event.Description,
            @event.Responsibilities,
            @event.Requirements,
            @event.Skills,
            @event.Location,
            @event.CountryCode,
            @event.EmploymentType,
            @event.WorkMode,
            @event.SalaryRange,
            JobDescriptionStatus.Draft,
            @event.Recruiter.Id,
            @event.Recruiter,
            @event.CreatedAt,
            @event.CreatedAt);
    }

    public JobDescriptionProjection Apply(
        JobDescriptionUpdated @event)
    {
        return this with
        {
            Title = @event.Title,
            Summary = @event.Summary,
            Description = @event.Description,
            Responsibilities = @event.Responsibilities,
            Requirements = @event.Requirements,
            Skills = @event.Skills,
            Location = @event.Location,
            CountryCode = @event.CountryCode,
            EmploymentType = @event.EmploymentType,
            WorkMode = @event.WorkMode,
            SalaryRange = @event.SalaryRange,
            UpdatedAt = @event.UpdatedAt
        };
    }

    public JobDescriptionProjection Apply(
        JobDescriptionOpened @event)
    {
        return this with
        {
            Status = JobDescriptionStatus.Open,
            UpdatedAt = @event.OccurredAt
        };
    }

    public JobDescriptionProjection Apply(
        JobDescriptionPutOnHold @event)
    {
        return this with
        {
            Status = JobDescriptionStatus.OnHold,
            UpdatedAt = @event.OccurredAt
        };
    }

    public JobDescriptionProjection Apply(
        JobDescriptionClosed @event)
    {
        return this with
        {
            Status = JobDescriptionStatus.Closed,
            UpdatedAt = @event.OccurredAt
        };
    }

    public JobDescriptionProjection Apply(
        JobDescriptionCancelled @event)
    {
        return this with
        {
            Status = JobDescriptionStatus.Cancelled,
            UpdatedAt = @event.OccurredAt
        };
    }
    
    public JobDescriptionProjection Apply(
        JobDescriptionRecruiterAssigned @event)
    {
        return this with
        {
            RecruiterId = @event.Recruiter.Id,
            Recruiter = @event.Recruiter,
            UpdatedAt = @event.OccurredAt
        };
    }
}