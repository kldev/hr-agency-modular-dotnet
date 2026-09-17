using HrAgencySystem.SharedKernel.ValueObjects;

namespace HrAgencySystem.Feeds.ReadModel;

/// <summary>
/// Flat read model behind the public job feeds: written by a projection owned by the module that
/// owns the events, read here with plain SQL. This row is the whole contract between the two -
/// this project knows nothing about job post events or the projection the application UI reads.
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
}
