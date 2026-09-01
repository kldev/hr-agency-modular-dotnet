using HrAgencySystem.JobDescription.Domain;
using HrAgencySystem.JobDescription.Domain.ValueObjects;
using HrAgencySystem.JobDescription.Events;


namespace HrAgencySystem.JobDescription.Projections;

public sealed record JobDescriptionProjection(
    Guid Id,
    Guid OrganizationId,
    Guid CompanyId,
    string Title,
    string? Summary,
    string? Description,
    IReadOnlyList<string> Responsibilities,
    IReadOnlyList<string> Requirements,
    IReadOnlyList<string> Skills,
    string? Location,
    string CountryCode,
    EmploymentType EmploymentType,
    WorkMode WorkMode,
    SalaryRange? SalaryRange,
    JobDescriptionStatus Status,
    Guid RecruiterId,
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
            @event.Title.Value,
            @event.Summary?.Value,
            @event.Description?.Value,
            @event.Responsibilities.Select(x => x.Value).ToList(),
            @event.Requirements.Select(x => x.Value).ToList(),
            @event.Skills.Select(x => x.Value).ToList(),
            @event.Location,
            @event.CountryCode,
            @event.EmploymentType,
            @event.WorkMode,
            @event.SalaryRange,
            JobDescriptionStatus.Draft,
            @event.RecruiterId,
            @event.CreatedAt,
            @event.CreatedAt);
    }

    public JobDescriptionProjection Apply(
        JobDescriptionUpdated @event)
    {
        return this with
        {
            Title = @event.Title.Value,
            Summary = @event.Summary?.Value,
            Description = @event.Description?.Value,
            Responsibilities =
            [
                .. @event.Responsibilities
                    .Select(x => x.Value)
            ],
            Requirements =
            [
                .. @event.Requirements
                    .Select(x => x.Value)
            ],
            Skills =
            [
                .. @event.Skills
                    .Select(x => x.Value)
            ],
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
}