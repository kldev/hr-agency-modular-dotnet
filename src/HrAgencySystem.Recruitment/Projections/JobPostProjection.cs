using HrAgencySystem.Recruitment.Domain.Posting;
using HrAgencySystem.Recruitment.Events.JobPosting;
using HrAgencySystem.SharedKernel.Snapshots;
using HrAgencySystem.SharedKernel.ValueObjects;

namespace HrAgencySystem.Recruitment.Projections;

public sealed record JobPostProjection(
    Guid Id,
    Guid JobDescriptionId,
    Guid OrgId,
    Guid CompanyId,
    string Title,
    string Summary,
    string Description,
    IReadOnlyList<string> Responsibilities,
    IReadOnlyList<string> Requirements,
    IReadOnlyList<string> Skills,
    string Location,
    string LanguageCode,
    string CountryCode,
    EmploymentType EmploymentType,
    WorkMode WorkMode,
    CurrencyCode CurrencyCode,
    decimal SalaryMin,
    decimal SalaryMax,
    JobPostStatus Status,
    Guid RecruiterId,
    UserSnapshot Recruiter,
    Guid CreatedById,
    UserSnapshot CreatedBy,
    Guid? ModifiedById,
    UserSnapshot? ModifiedBy,
    CompanySnapshot Company,
    IReadOnlyList<ChannelPost> Posts,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt,
    string SearchText)
{
    public static JobPostProjection Create(
        JobPostCreated @event)
    {
        return new JobPostProjection(
            @event.JobPostId,
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
            @event.LanguageCode,
            @event.CountryCode,
            @event.EmploymentType,
            @event.WorkMode,
            @event.CurrencyCode,
            @event.SalaryMin,
            @event.SalaryMax,
            JobPostStatus.Draft,
            @event.Recruiter.Id,
            @event.Recruiter,
            @event.CreatedBy.Id,
            @event.CreatedBy,
            null,
            null,
            @event.Company,
            [],
            @event.CreatedAt,
            @event.CreatedAt,
            string.Join(",",@event.Responsibilities)
            + string.Join(",",@event.Requirements)
            + string.Join(",",@event.Skills));
    }

    public JobPostProjection Apply(
        JobPostUpdated @event)
    {
        var searchText = string.Join(",", @event.Responsibilities)
                         + string.Join(",", @event.Requirements)
                         + string.Join(",", @event.Skills);
        
        return this with
        {
            Title = @event.Title,
            Summary = @event.Summary ?? "",
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
            UpdatedAt = @event.OccurredAt,
            ModifiedById = @event.Author.Id,
            ModifiedBy = @event.Author,
            SearchText = searchText
        };
    }

    public JobPostProjection Apply(
        JobPostToChannel @event)
    {
        var posts = Posts
            .Append(new ChannelPost(
                @event.ChannelType,
                @event.OccurredAt))
            .ToArray();

        return this with
        {
            Status = JobPostStatus.Published,
            Posts = posts,
            UpdatedAt = @event.OccurredAt,
            ModifiedById = @event.Author.Id,
            ModifiedBy = @event.Author
        };
    }

    public JobPostProjection Apply(
        JobPostPublished @event)
    {
        return this with
        {
            Status = JobPostStatus.Published,
            UpdatedAt = @event.OccurredAt,
            ModifiedById = @event.Author.Id,
            ModifiedBy = @event.Author
        };
    }

    public JobPostProjection Apply(
        JobPostClosed @event)
    {
        return this with
        {
            Status = JobPostStatus.Closed,
            UpdatedAt = @event.OccurredAt,
            ModifiedById = @event.Author.Id,
            ModifiedBy = @event.Author
        };
    }

    public JobPostProjection Apply(
        JobPostArchived @event)
    {
        return this with
        {
            Status = JobPostStatus.Archived,
            UpdatedAt = @event.OccurredAt,
            ModifiedById = @event.Author.Id,
            ModifiedBy = @event.Author
        };
    }
}