using HrAgencySystem.JobDescription.Domain;
using HrAgencySystem.JobDescription.Domain.ValueObjects;
using HrAgencySystem.JobDescription.Events;
using HrAgencySystem.SharedKernel.Snapshots;
using HrAgencySystem.SharedKernel.ValueObjects;


namespace HrAgencySystem.JobDescription.Projections;

public sealed record JobDescriptionProjection(
    Guid Id,
    Guid OrgId,
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
    CurrencyCode CurrencyCode,
    decimal SalaryMin,
    decimal SalaryMax,
    JobDescriptionStatus Status,
    Guid RecruiterId,
    UserSnapshot Recruiter,
    Guid CreatedById,
    UserSnapshot CreatedBy,
    Guid? ModifiedById,
    UserSnapshot? ModifiedBy,
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
            @event.CurrencyCode,
            @event.SalaryMin,
            @event.SalaryMax,
            JobDescriptionStatus.Draft,
            @event.Recruiter.Id,
            @event.Recruiter,
            @event.CreatedBy.Id,
            @event.CreatedBy, null, null,
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
            CurrencyCode = @event.CurrencyCode,
            SalaryMin = @event.SalaryMin,
            SalaryMax = @event.SalaryMax,
            UpdatedAt = @event.UpdatedAt,
            ModifiedById = @event.ModifiedBy.Id,
            ModifiedBy = @event.ModifiedBy
        };
    }

    public JobDescriptionProjection Apply(
        JobDescriptionOpened @event)
    {
        return this with
        {
            Status = JobDescriptionStatus.Open,
            UpdatedAt = @event.OccurredAt,
            ModifiedById = @event.ModifiedBy.Id,
            ModifiedBy = @event.ModifiedBy
        };
    }

    public JobDescriptionProjection Apply(
        JobDescriptionPutOnHold @event)
    {
        return this with
        {
            Status = JobDescriptionStatus.OnHold,
            UpdatedAt = @event.OccurredAt,
            ModifiedById = @event.ModifiedBy.Id,
            ModifiedBy = @event.ModifiedBy
        };
    }

    public JobDescriptionProjection Apply(
        JobDescriptionClosed @event)
    {
        return this with
        {
            Status = JobDescriptionStatus.Closed,
            UpdatedAt = @event.OccurredAt,
            ModifiedById = @event.ModifiedBy.Id,
            ModifiedBy = @event.ModifiedBy
        };
    }

    public JobDescriptionProjection Apply(
        JobDescriptionCancelled @event)
    {
        return this with
        {
            Status = JobDescriptionStatus.Cancelled,
            UpdatedAt = @event.OccurredAt,
            ModifiedById = @event.ModifiedBy.Id,
            ModifiedBy = @event.ModifiedBy
        };
    }
    
    public JobDescriptionProjection Apply(
        JobDescriptionRecruiterAssigned @event)
    {
        return this with
        {
            RecruiterId = @event.Recruiter.Id,
            Recruiter = @event.Recruiter,
            UpdatedAt = @event.OccurredAt,
            ModifiedById = @event.ModifiedBy.Id,
            ModifiedBy = @event.ModifiedBy
        };
    }
}