using HrAgencySystem.Recruitment.Events.JobPostings;
using HrAgencySystem.SharedKernel.ValueObjects;

namespace HrAgencySystem.Recruitment.Feeds.ReadModel;

/// <summary>
/// Flat read model behind the public job feeds. Written by <see cref="JobPostFeedProjection"/>
/// into a relational table, read with plain SQL - deliberately independent of the projection
/// the application UI reads.
/// </summary>
public sealed class JobPostFeedRow
{
    public Guid Id { get; set; }

    public Guid OrganizationId { get; set; }

    public bool IsPublished { get; set; }

    public string Title { get; set; } = "";

    public string Summary { get; set; } = "";

    public string Description { get; set; } = "";

    public string[] Responsibilities { get; set; } = [];

    public string[] Requirements { get; set; } = [];

    public string[] Skills { get; set; } = [];

    public string Location { get; set; } = "";

    public string LanguageCode { get; set; } = "";

    public string CountryCode { get; set; } = "";

    public EmploymentType EmploymentType { get; set; }

    public WorkMode WorkMode { get; set; }

    public CurrencyCode CurrencyCode { get; set; }

    public decimal SalaryMin { get; set; }

    public decimal SalaryMax { get; set; }

    /// <summary>
    /// Raw slug in the form <c>{organizationSlug}/{postSlug}</c>. The feed URL is prepended
    /// during serialization, so this read model does not depend on configuration.
    /// </summary>
    public string PostingSlug { get; set; } = "";

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }

    /*
     * Deliberately not named Create/Apply: Marten discovers conventional Create/Apply methods
     * on the projected document, and combining them with the DetermineActionAsync override that
     * EfCoreSingleStreamProjection uses is rejected at startup.
     */
    public static JobPostFeedRow From(JobPostCreated @event)
    {
        return new JobPostFeedRow
        {
            Id = @event.JobPostId,
            OrganizationId = @event.OrganizationId,
            IsPublished = false,
            Title = @event.Title,
            Summary = @event.Summary,
            Description = @event.Description,
            Responsibilities = [.. @event.Responsibilities],
            Requirements = [.. @event.Requirements],
            Skills = [.. @event.Skills],
            Location = @event.Location,
            LanguageCode = @event.LanguageCode,
            CountryCode = @event.CountryCode,
            EmploymentType = @event.EmploymentType,
            WorkMode = @event.WorkMode,
            CurrencyCode = @event.CurrencyCode,
            SalaryMin = @event.SalaryMin,
            SalaryMax = @event.SalaryMax,
            PostingSlug = @event.OrgSlug + "/" + @event.PostingSlug,
            CreatedAt = @event.CreatedAt,
            UpdatedAt = @event.CreatedAt
        };
    }

    public void Update(JobPostUpdated @event)
    {
        Title = @event.Title;
        Summary = @event.Summary ?? "";
        Description = @event.Description;
        Responsibilities = [.. @event.Responsibilities];
        Requirements = [.. @event.Requirements];
        Skills = [.. @event.Skills];
        Location = @event.Location;
        LanguageCode = @event.LanguageCode;
        CountryCode = @event.CountryCode;
        EmploymentType = @event.EmploymentType;
        WorkMode = @event.WorkMode;
        CurrencyCode = @event.CurrencyCode;
        SalaryMin = @event.SalaryMin;
        SalaryMax = @event.SalaryMax;
        UpdatedAt = @event.OccurredAt;
    }
}
