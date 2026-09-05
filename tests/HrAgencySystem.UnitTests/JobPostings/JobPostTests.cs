using HrAgencySystem.Recruitment.Domain.Posting;
using HrAgencySystem.Recruitment.Events.JobPosting;
using HrAgencySystem.SharedKernel.Snapshots;
using HrAgencySystem.SharedKernel.ValueObjects;
using D = HrAgencySystem.Recruitment.Domain.Posting;

namespace HrAgencySystem.UnitTests.JobPostings;

public sealed class JobPostTests
{
    [Fact]
    public void Apply_events_should_build_and_update_job_posting_correctly()
    {
        var createdAt = new DateTimeOffset(
            2026, 9, 3, 10, 0, 0, TimeSpan.Zero);

        var updatedAt = createdAt.AddHours(2);

        var recruiterId = Guid.NewGuid();
        var creatorId = Guid.NewGuid();
        var modifierId = Guid.NewGuid();

        var jobPostingId = Guid.NewGuid();
        var jobDescriptionId = Guid.NewGuid();
        var organizationId = Guid.NewGuid();
        var companyId = Guid.NewGuid();

        var posting = D.JobPost.Empty();

        // Create
        ApplyCreated(
            posting,
            jobPostingId,
            jobDescriptionId,
            organizationId,
            companyId,
            recruiterId,
            creatorId,
            createdAt);

        AssertCreatedState(
            posting,
            jobPostingId,
            jobDescriptionId,
            organizationId,
            companyId,
            recruiterId,
            creatorId,
            createdAt);

        // Update
        ApplyUpdated(
            posting,
            updatedAt,
            modifierId);

        AssertUpdatedState(
            posting,
            modifierId,
            updatedAt);

        // Publish
        ApplyPublished(
            posting,
            updatedAt.AddMinutes(10),
            modifierId);

        Assert.Equal(
            JobPostStatus.Published,
            posting.Status);

        Assert.Equal(
            updatedAt.AddMinutes(10),
            posting.UpdatedAt);

        Assert.Equal(
            modifierId,
            posting.ModifiedBy);

        // Publish to channel
        var channelPublishedAt = updatedAt.AddMinutes(20);

        ApplyToChannel(
            posting,
            channelPublishedAt,
            modifierId,
            PostingChannelType.PracujPl);

        Assert.Equal(
            JobPostStatus.Published,
            posting.Status);

        Assert.Single(posting.Posts);

        var post = posting.Posts[0];

        Assert.Equal(
            PostingChannelType.PracujPl,
            post.ChannelType);

        Assert.Equal(
            channelPublishedAt,
            post.PublishedAt);

        Assert.Equal(
            channelPublishedAt,
            posting.UpdatedAt);

        Assert.Equal(
            modifierId,
            posting.ModifiedBy);
    }

    [Fact]
    public void Apply_to_channel_should_not_be_allowed_for_final_status()
    {
        var posting = D.JobPost.Empty();

        var createdAt = DateTimeOffset.UtcNow;

        ApplyCreated(
            posting,
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            createdAt);

        ApplyClosed(
            posting,
            createdAt.AddHours(1),
            Guid.NewGuid());

        Assert.Equal(
            JobPostStatus.Closed,
            posting.Status);

        Assert.Throws<InvalidOperationException>(() =>
            ApplyToChannel(
                posting,
                createdAt.AddHours(2),
                Guid.NewGuid(),
                PostingChannelType.Linkedin));

        Assert.Empty(posting.Posts);
    }

    [Fact]
    public void Apply_to_channel_should_not_be_allowed_for_archived_status()
    {
        var posting = D.JobPost.Empty();

        var createdAt = DateTimeOffset.UtcNow;

        ApplyCreated(
            posting,
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            createdAt);

        ApplyArchived(
            posting,
            createdAt.AddHours(1),
            Guid.NewGuid());

        Assert.Equal(
            JobPostStatus.Archived,
            posting.Status);

        Assert.Throws<InvalidOperationException>(() =>
            ApplyToChannel(
                posting,
                createdAt.AddHours(2),
                Guid.NewGuid(),
                PostingChannelType.Linkedin));

        Assert.Empty(posting.Posts);
    }

    private static void ApplyCreated(
        Recruitment.Domain.Posting.JobPost post,
        Guid jobPostingId,
        Guid jobDescriptionId,
        Guid organizationId,
        Guid companyId,
        Guid recruiterId,
        Guid creatorId,
        DateTimeOffset createdAt)
    {
        post.Apply(
            new JobPostCreated(
                JobPostId: jobPostingId,
                JobDescriptionId: jobDescriptionId,
                OrganizationId: organizationId,
                CompanyId: companyId,
                Title: "Senior .NET Developer",
                Summary: "Senior developer position.",
                Description: "Development of recruitment platform.",
                Responsibilities:
                [
                    "Develop backend services",
                    "Review code",
                    "Design solutions"
                ],
                Requirements:
                [
                    "C#",
                    ".NET",
                    "PostgreSQL"
                ],
                Skills:
                [
                    "Clean Architecture",
                    "DDD",
                    "Marten"
                ],
                Location: "Opole",
                CountryCode: "PL",
                EmploymentType: EmploymentType.FullTime,
                WorkMode: WorkMode.Hybrid,
                CurrencyCode: CurrencyCode.PLN,
                SalaryMin: 15000,
                SalaryMax: 22000,
                Recruiter: new UserSnapshot(
                    recruiterId,
                    "John",
                    "Recruiter",
                    "john@example.com"),
                CreatedBy: new UserSnapshot(
                    creatorId,
                    "Jane",
                    "Creator",
                    "jane@example.com"),
                Company: new CompanySnapshot(
                    companyId,
                    "Example Company",
                    "1234567890"),
                LanguageCode: "PL",
                OrgSlug: "example-organization",
                PostingSlug: "senior-dotnet-developer",
                CreatedAt: createdAt));
    }

    private static void AssertCreatedState(
        Recruitment.Domain.Posting.JobPost post,
        Guid jobPostingId,
        Guid jobDescriptionId,
        Guid organizationId,
        Guid companyId,
        Guid recruiterId,
        Guid creatorId,
        DateTimeOffset createdAt)
    {
        Assert.Equal(
            jobPostingId,
            post.Id.Value);

        Assert.Equal(
            jobDescriptionId,
            post.DescriptionId.Value);

        Assert.Equal(
            organizationId,
            post.OrganizationId.Value);

        Assert.Equal(
            companyId,
            post.CompanyId.Value);

        Assert.Equal(
            "Senior .NET Developer",
            post.Title.Value);

        Assert.Equal(
            "Senior developer position.",
            post.Summary.Value);

        Assert.Equal(
            "Development of recruitment platform.",
            post.Description.Value);

        Assert.Equal(
            ["Develop backend services", "Review code", "Design solutions"],
            post.Responsibilities.Select(x => x.Value));

        Assert.Equal(
            ["C#", ".NET", "PostgreSQL"],
            post.Requirements.Select(x => x.Value));

        Assert.Equal(
            ["Clean Architecture", "DDD", "Marten"],
            post.Skills.Select(x => x.Value));

        Assert.Equal(
            "Opole",
            post.Location.Value);

        Assert.Equal(
            "PL",
            post.CountryCode.Value);

        Assert.Equal(
            "PL",
            post.LanguageCode.Value);

        Assert.Equal(
            EmploymentType.FullTime,
            post.EmploymentType);

        Assert.Equal(
            WorkMode.Hybrid,
            post.WorkMode);

        Assert.Equal(
            JobPostStatus.Draft,
            post.Status);

        Assert.Equal(
            recruiterId,
            post.RecruiterId);

        Assert.Equal(
            creatorId,
            post.CreatedBy);

        Assert.Null(post.ModifiedBy);

        Assert.Equal(
            createdAt,
            post.CreatedAt);

        Assert.Equal(
            createdAt,
            post.UpdatedAt);

        Assert.Empty(post.Posts);
    }

    private static void ApplyUpdated(
        Recruitment.Domain.Posting.JobPost post,
        DateTimeOffset occurredAt,
        Guid modifierId)
    {
        post.Apply(
            new JobPostUpdated(
                JobPostId: post.Id.Value,
                Title: "Lead .NET Developer",
                Summary: "Updated job summary.",
                Description: "Updated job description.",
                Responsibilities:
                [
                    "Lead development",
                    "Review architecture"
                ],
                Requirements:
                [
                    "C#",
                    ".NET 8"
                ],
                Skills:
                [
                    "DDD",
                    "Marten"
                ],
                Location: "Wrocław",
                CountryCode: "PL",
                EmploymentType: EmploymentType.FullTime,
                LanguageCode: "pl",
                WorkMode: WorkMode.Remote,
                CurrencyCode: CurrencyCode.PLN,
                SalaryMin: 18000,
                SalaryMax: 25000,
                OccurredAt: occurredAt,
                Author: new UserSnapshot(
                    modifierId,
                    "John",
                    "Developer",
                    "john@example.com")));
    }

    private static void AssertUpdatedState(
        Recruitment.Domain.Posting.JobPost post,
        Guid modifierId,
        DateTimeOffset occurredAt)
    {
        Assert.Equal(
            "Lead .NET Developer",
            post.Title.Value);

        Assert.Equal(
            "Updated job summary.",
            post.Summary.Value);

        Assert.Equal(
            "Updated job description.",
            post.Description.Value);

        Assert.Equal(
            ["Lead development", "Review architecture"],
            post.Responsibilities.Select(x => x.Value));

        Assert.Equal(
            ["C#", ".NET 8"],
            post.Requirements.Select(x => x.Value));

        Assert.Equal(
            ["DDD", "Marten"],
            post.Skills.Select(x => x.Value));

        Assert.Equal(
            "Wrocław",
            post.Location.Value);

        Assert.Equal(
            "PL",
            post.CountryCode.Value);

        Assert.Equal(
            "PL",
            post.LanguageCode.Value);

        Assert.Equal(
            EmploymentType.FullTime,
            post.EmploymentType);

        Assert.Equal(
            WorkMode.Remote,
            post.WorkMode);

        Assert.Equal(
            JobPostStatus.Draft,
            post.Status);

        Assert.Equal(
            modifierId,
            post.ModifiedBy);

        Assert.Equal(
            occurredAt,
            post.UpdatedAt);
    }

    private static void ApplyPublished(
        Recruitment.Domain.Posting.JobPost post,
        DateTimeOffset occurredAt,
        Guid authorId)
    {
        post.Apply(
            new JobPostPublished(
                JobPostId: post.Id.Value,
                OccurredAt: occurredAt,
                Author: new UserSnapshot(
                    authorId,
                    "John",
                    "Publisher",
                    "publisher@example.com")));
    }

    private static void ApplyToChannel(
        Recruitment.Domain.Posting.JobPost post,
        DateTimeOffset occurredAt,
        Guid authorId,
        PostingChannelType channelType)
    {
        post.Apply(
            new JobPostedToChannel(
                JobPostId: post.Id.Value,
                ChannelType: channelType,
                OccurredAt: occurredAt,
                Author: new UserSnapshot(
                    authorId,
                    "John",
                    "Publisher",
                    "publisher@example.com")));
    }

    private static void ApplyClosed(
        Recruitment.Domain.Posting.JobPost post,
        DateTimeOffset occurredAt,
        Guid authorId)
    {
        post.Apply(
            new JobPostClosed(
                JobPostId: post.Id.Value,
                OccurredAt: occurredAt,
                Author: new UserSnapshot(
                    authorId,
                    "John",
                    "Closer",
                    "closer@example.com")));
    }

    private static void ApplyArchived(
        Recruitment.Domain.Posting.JobPost post,
        DateTimeOffset occurredAt,
        Guid authorId)
    {
        post.Apply(
            new JobPostArchived(
                JobPostId: post.Id.Value,
                OccurredAt: occurredAt,
                Author: new UserSnapshot(
                    authorId,
                    "John",
                    "Archiver",
                    "archiver@example.com")));
    }
}