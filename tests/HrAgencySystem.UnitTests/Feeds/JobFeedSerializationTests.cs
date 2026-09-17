using System.Text.Json;
using System.Xml.Linq;
using HrAgencySystem.Recruitment.Feeds.Application.GenerateJobFeed;
using HrAgencySystem.Recruitment.Feeds.ReadModel;
using HrAgencySystem.SharedKernel.ValueObjects;

namespace HrAgencySystem.UnitTests.Feeds;

public sealed class JobFeedSerializationTests
{
    private const string FeedUrl = "http://localhost:5050";

    [Fact]
    public void SerializeJson_ShouldMapRowToFeedItem()
    {
        // Arrange
        var row = CreateRow();

        // Act
        var json = JobFeedGenerator.SerializeJson([row], FeedUrl);

        // Assert
        using var document = JsonDocument.Parse(json);

        var job = Assert.Single(
            document.RootElement.GetProperty("jobs").EnumerateArray());

        Assert.Equal(row.Id, job.GetProperty("id").GetGuid());
        Assert.Equal(row.Title, job.GetProperty("title").GetString());
        Assert.Equal(row.Summary, job.GetProperty("summary").GetString());
        Assert.Equal(row.Description, job.GetProperty("description").GetString());
        Assert.Equal(row.Location, job.GetProperty("location").GetString());
        Assert.Equal(row.LanguageCode, job.GetProperty("languageCode").GetString());
        Assert.Equal(row.CountryCode, job.GetProperty("countryCode").GetString());
        Assert.Equal(row.SalaryMin, job.GetProperty("salaryMin").GetDecimal());
        Assert.Equal(row.SalaryMax, job.GetProperty("salaryMax").GetDecimal());

        Assert.Equal(
            row.Skills,
            job.GetProperty("skills").EnumerateArray().Select(x => x.GetString()).ToArray());
    }

    [Fact]
    public void SerializeJson_ShouldPrefixApplyUrlWithFeedUrl()
    {
        // Arrange
        var row = CreateRow();

        // Act
        var json = JobFeedGenerator.SerializeJson([row], FeedUrl);

        // Assert
        using var document = JsonDocument.Parse(json);

        var job = Assert.Single(
            document.RootElement.GetProperty("jobs").EnumerateArray());

        Assert.Equal(
            $"{FeedUrl}/acme/senior-net-developer",
            job.GetProperty("applyUrl").GetString());
    }

    [Fact]
    public void SerializeXml_ShouldPrefixApplyUrlWithFeedUrl()
    {
        // Arrange
        var row = CreateRow();

        // Act
        var xml = JobFeedGenerator.SerializeXml([row], FeedUrl);

        // Assert
        var document = XDocument.Parse(xml);

        var job = Assert.Single(document.Root!.Elements("job"));

        Assert.Equal(
            $"{FeedUrl}/acme/senior-net-developer",
            job.Element("applyUrl")!.Value);

        Assert.Equal(row.Title, job.Element("title")!.Value);
        Assert.Equal("FullTime", job.Element("employmentType")!.Value);
        Assert.Equal("PLN", job.Element("currencyCode")!.Value);
    }

    [Fact]
    public void SerializeJson_ShouldReturnEmptyFeed_WhenThereAreNoPublishedPosts()
    {
        // Act
        var json = JobFeedGenerator.SerializeJson([], FeedUrl);

        // Assert
        using var document = JsonDocument.Parse(json);

        Assert.Empty(document.RootElement.GetProperty("jobs").EnumerateArray());
    }

    private static JobPostFeedRow CreateRow()
    {
        return new JobPostFeedRow
        {
            Id = Guid.NewGuid(),
            OrganizationId = Guid.NewGuid(),
            IsPublished = true,
            Title = "Senior .NET Developer",
            Summary = "Senior .NET Developer position",
            Description = "We are looking for an experienced .NET Developer.",
            Responsibilities = ["Design and develop applications", "Review code"],
            Requirements = ["5+ years of experience", "Good knowledge of C#"],
            Skills = ["C#", ".NET", "PostgreSQL"],
            Location = "Opole",
            LanguageCode = "PL",
            CountryCode = "PL",
            EmploymentType = EmploymentType.FullTime,
            WorkMode = WorkMode.Hybrid,
            CurrencyCode = CurrencyCode.PLN,
            SalaryMin = 15_000,
            SalaryMax = 22_000,
            PostingSlug = "acme/senior-net-developer",
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };
    }
}
