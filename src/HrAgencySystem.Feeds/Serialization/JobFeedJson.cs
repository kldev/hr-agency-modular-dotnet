using System.Text.Json.Serialization;
using HrAgencySystem.Feeds.ReadModel;
using HrAgencySystem.SharedKernel.ValueObjects;

namespace HrAgencySystem.Feeds.Serialization;

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

    [JsonPropertyName("applyUrl")]
    public string PostingSlug { get; set; } = string.Empty;

    public static JobJson FromRow(JobPostFeedRow row, string feedUrl)
    {
        return new JobJson
        {
            Id = row.Id,

            Title = row.Title,
            Summary = row.Summary,
            Description = row.Description,

            Responsibilities = [.. row.Responsibilities],
            Requirements = [.. row.Requirements],
            Skills = [.. row.Skills],

            Location = row.Location,
            LanguageCode = row.LanguageCode,
            CountryCode = row.CountryCode,

            EmploymentType = row.EmploymentType,
            WorkMode = row.WorkMode,
            CurrencyCode = row.CurrencyCode,

            SalaryMin = row.SalaryMin,
            SalaryMax = row.SalaryMax,

            CreatedAt = row.CreatedAt,

            PostingSlug = feedUrl + "/" + row.PostingSlug,
        };
    }
}
