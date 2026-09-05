using System.Xml.Serialization;
using HrAgencySystem.Recruitment.Projections;
using HrAgencySystem.SharedKernel.ValueObjects;

namespace HrAgencySystem.Recruitment.Feeds.Serialization;

[XmlRoot("jobs")]
public sealed class JobFeedXml
{
    [XmlElement("job")]
    public List<JobFeedXmlItem> Jobs { get; set; } = [];
}

public sealed class JobFeedXmlItem
{
    [XmlElement("id")]
    public Guid Id { get; set; }
    
    [XmlElement("title")]
    public string Title { get; set; } = string.Empty;

    [XmlElement("summary")]
    public string Summary { get; set; } = string.Empty;

    [XmlElement("description")]
    public string Description { get; set; } = string.Empty;

    [XmlArray("responsibilities")]
    [XmlArrayItem("responsibility")]
    public List<string> Responsibilities { get; set; } = [];

    [XmlArray("requirements")]
    [XmlArrayItem("requirement")]
    public List<string> Requirements { get; set; } = [];

    [XmlArray("skills")]
    [XmlArrayItem("skill")]
    public List<string> Skills { get; set; } = [];

    [XmlElement("location")]
    public string Location { get; set; } = string.Empty;

    [XmlElement("languageCode")]
    public string LanguageCode { get; set; } = string.Empty;

    [XmlElement("countryCode")]
    public string CountryCode { get; set; } = string.Empty;

    [XmlElement("employmentType")]
    public EmploymentType EmploymentType { get; set; }

    [XmlElement("workMode")]
    public WorkMode WorkMode { get; set; }

    [XmlElement("currencyCode")]
    public CurrencyCode CurrencyCode { get; set; }

    [XmlElement("salaryMin")]
    public decimal SalaryMin { get; set; }

    [XmlElement("salaryMax")]
    public decimal SalaryMax { get; set; }
    
    [XmlElement("createdAt")]
    public DateTimeOffset CreatedAt { get; set; }

    [XmlElement("updatedAt")]
    public DateTimeOffset UpdatedAt { get; set; }

    [XmlElement("applyUrl")]
    public string PostingSlug { get; set; } = string.Empty;

    public static JobFeedXmlItem FromProjection(JobPostProjection projection)
    {
        return new JobFeedXmlItem
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
            
            PostingSlug = projection.PostingSlug
        };
    }
}
