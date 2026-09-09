using HrAgencySystem.Recruitment.Domain.JobPostings;
using HrAgencySystem.Recruitment.Events.JobPostings;
using HrAgencySystem.SharedKernel.Exception;
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
    // ReSharper disable once NotAccessedPositionalProperty.Global
    Guid CreatedById,
    UserSnapshot CreatedBy,
    // ReSharper disable once NotAccessedPositionalProperty.Global
    Guid? ModifiedById,
    // ReSharper disable once NotAccessedPositionalProperty.Global
    UserSnapshot? ModifiedBy,
    CompanySnapshot Company,
    IReadOnlyList<ChannelPost> Posts,
    DateTimeOffset CreatedAt,
    // ReSharper disable once NotAccessedPositionalProperty.Global
    DateTimeOffset UpdatedAt,
    string PostingSlug,
    string SearchText)
{

    public JobPostProjection UpdatePostSlug(string appUrl)
    {
        return this with
        {
            PostingSlug = appUrl + "/" + PostingSlug
        };
    }
    
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
            @event.OrgSlug + "/" + @event.PostingSlug,
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
        JobPostedToChannel @event)
    {
        
        var posts = Posts
            .Append(new ChannelPost(
                @event.ChannelType,
                @event.OccurredAt))
            .ToArray();

        return ApplyCommon(this, @event) with
        {
            Posts = posts,
        };
    }

    public JobPostProjection Apply(
        JobPostPublished @event)
    {
        return ApplyCommon(this, @event) with
        {
            Status = JobPostStatus.Published,
        };
    }

    public JobPostProjection Apply(
        JobPostClosed @event)
    {
        return ApplyCommon(this,@event) with
        {
            Status = JobPostStatus.Closed,
        };
    }

    public JobPostProjection Apply(
        JobPostArchived @event)
    {
        return ApplyCommon(this, @event) with
        {
            Status = JobPostStatus.Archived,
        };
    }

    public JobPostProjection Apply(
        JobPostRecruiterChanged @event)
    {
        return ApplyCommon(this, @event) with
        {
            RecruiterId = @event.Recruiter.Id,
            Recruiter = @event.Recruiter,
        };
    }
    
    public JobPostProjection Apply(
        JobPostStatusChanged @event)
    {
        return ApplyCommon(this, @event) with
        {
            Status = @event.NewStatus,
        };
    }

    private JobPostProjection ApplyCommon(JobPostProjection post, IJobPostEvent @event)
    {
        return post with
        {
            UpdatedAt = @event.OccurredAt,
            ModifiedById = @event.Author.Id,
            ModifiedBy = @event.Author
        };
    }
}