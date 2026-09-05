using System.Text.Json.Serialization;
using HrAgencySystem.Recruitment.Projections;
using HrAgencySystem.SharedKernel.ValueObjects;

namespace HrAgencySystem.Recruitment.Feeds.Serialization;

public sealed class JobFeedJson
{
    [JsonPropertyName("jobs")]
    public List<JobJson> Jobs { get; set; } = [];
}

public sealed class JobJson
{
    [JsonPropertyName("id")]
    public Guid Id { get; set; }
    
    [JsonPropertyName("title")]
    public string Title { get; set; } = string.Empty;

    [JsonPropertyName("summary")]
    public string Summary { get; set; } = string.Empty;

    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;

    [JsonPropertyName("responsibilities")]
    public List<string> Responsibilities { get; set; } = [];

    [JsonPropertyName("requirements")]
    public List<string> Requirements { get; set; } = [];

    [JsonPropertyName("skills")]
    public List<string> Skills { get; set; } = [];

    [JsonPropertyName("location")]
    public string Location { get; set; } = string.Empty;

    [JsonPropertyName("languageCode")]
    public string LanguageCode { get; set; } = string.Empty;

    [JsonPropertyName("countryCode")]
    public string CountryCode { get; set; } = string.Empty;

    [JsonPropertyName("employmentType")]
    public EmploymentType EmploymentType { get; set; }

    [JsonPropertyName("workMode")]
    public WorkMode WorkMode { get; set; }

    [JsonPropertyName("currencyCode")]
    public CurrencyCode CurrencyCode { get; set; }

    [JsonPropertyName("salaryMin")]
    public decimal SalaryMin { get; set; }

    [JsonPropertyName("salaryMax")]
    public decimal SalaryMax { get; set; }

    [JsonPropertyName("createdAt")]
    public DateTimeOffset CreatedAt { get; set; }

    [JsonPropertyName("updatedAt")]
    public DateTimeOffset UpdatedAt { get; set; }

    [JsonPropertyName("applyUrl")]
    public string PostingSlug { get; set; } = string.Empty;

    public static JobJson FromProjection(JobPostProjection projection)
    {
        return new JobJson
        {
            Id = projection.Id,
            
            Title = projection.Title,
            Summary = projection.Summary,
            Description = projection.Description,

            Responsibilities = [.. projection.Responsibilities],
            Requirements = [.. projection.Requirements],
            Skills = [.. projection.Skills],

            Location = projection.Location,
            LanguageCode = projection.LanguageCode,
            CountryCode = projection.CountryCode,

            EmploymentType = projection.EmploymentType,
            WorkMode = projection.WorkMode,
            CurrencyCode = projection.CurrencyCode,

            SalaryMin = projection.SalaryMin,
            SalaryMax = projection.SalaryMax,

            CreatedAt = projection.CreatedAt,
            UpdatedAt = projection.UpdatedAt,

            PostingSlug = projection.PostingSlug
        };
    }
}