using Dapper;
using HrAgencySystem.IntegrationTests.Infrastructure;
using HrAgencySystem.IntegrationTests.JobPosts;
using HrAgencySystem.Recruitment.Domain.JobPostings;
using HrAgencySystem.Feeds.Application.GetJobFeed;
using HrAgencySystem.Feeds.ReadModel;
using HrAgencySystem.SharedKernel.ValueObjects;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using Xunit.Abstractions;

namespace HrAgencySystem.IntegrationTests.Feeds.ReadModel;

[Collection(IntegrationCollection.Name)]
public sealed class JobPostFeedProjectionTests(
    IntegrationEnvironment env,
    ITestOutputHelper output)
    : BaseIntegrationTest(env, output)
{
    protected override async Task BeforeEachAsync()
    {
        await Cleaner.CleanJobPostFeedRows();
    }

    [Fact]
    public async Task ShouldCreateUnpublishedRow_WhenJobPostIsCreated()
    {
        // Arrange
        var organizationId = Guid.NewGuid();
        JobPostingClient.WithOrganizationId(organizationId);

        // Act
        var created = await JobPostingClient.CreateAsync(
            JobPostingTestData.CreateRequest());

        // Assert
        await Eventually.AssertAsync(async () =>
        {
            var row = await GetRow(created.JobPostId);

            Assert.NotNull(row);
            Assert.Equal(organizationId, row.OrganizationId);
            Assert.False(row.IsPublished);

            Assert.Equal("Senior .NET Developer", row.Title);
            Assert.Equal("Opole", row.Location);
            Assert.Equal("PL", row.CountryCode);
            Assert.Equal("PL", row.LanguageCode);
            Assert.Equal(EmploymentType.FullTime, row.EmploymentType);
            Assert.Equal(WorkMode.Hybrid, row.WorkMode);
            Assert.Equal(CurrencyCode.PLN, row.CurrencyCode);
            Assert.Equal(15_000, row.SalaryMin);
            Assert.Equal(22_000, row.SalaryMax);
            Assert.Equal(4, row.Skills.Length);
            Assert.Equal(3, row.Requirements.Length);
            Assert.Equal(3, row.Responsibilities.Length);
            Assert.Contains("/", row.PostingSlug);
        }, output: OutputHelper);
    }

    [Fact]
    public async Task ShouldPublishAndUnpublishRow_WhenStatusChanges()
    {
        // Arrange
        var organizationId = Guid.NewGuid();
        JobPostingClient.WithOrganizationId(organizationId);

        var created = await JobPostingClient.CreateAsync(
            JobPostingTestData.CreateRequest());

        // Act
        await JobPostingClient.ChangeStatusAsync(
            created.JobPostId,
            JobPostStatusApi.Published);

        // Assert
        await Eventually.AssertAsync(async () =>
        {
            var row = await GetRow(created.JobPostId);

            Assert.NotNull(row);
            Assert.True(row.IsPublished);
        }, output: OutputHelper);

        // Act
        await JobPostingClient.ChangeStatusAsync(
            created.JobPostId,
            JobPostStatusApi.Closed);

        // Assert
        await Eventually.AssertAsync(async () =>
        {
            var row = await GetRow(created.JobPostId);

            Assert.NotNull(row);
            Assert.False(row.IsPublished);
        }, output: OutputHelper);
    }

    [Fact]
    public async Task ShouldApplyUpdate_WhenJobPostIsUpdated()
    {
        // Arrange
        var organizationId = Guid.NewGuid();
        JobPostingClient.WithOrganizationId(organizationId);

        var created = await JobPostingClient.CreateAsync(
            JobPostingTestData.CreateRequest());

        var update = JobPostingTestData.UpdateRequest();

        // Act
        await JobPostingClient.UpdateAsync(created.JobPostId, update);

        // Assert
        await Eventually.AssertAsync(async () =>
        {
            var row = await GetRow(created.JobPostId);

            Assert.NotNull(row);
            Assert.Equal(update.Title, row.Title);
            Assert.Equal(update.Description, row.Description);
            Assert.True(row.UpdatedAt > row.CreatedAt);
        }, output: OutputHelper);
    }

    [Fact]
    public async Task ReaderShouldReturnOnlyPublishedPostsOfOrganization()
    {
        // Arrange
        var organizationId = Guid.NewGuid();
        var otherOrganizationId = Guid.NewGuid();

        JobPostingClient.WithOrganizationId(organizationId);

        var published = await JobPostingClient.CreateAsync(
            JobPostingTestData.CreateRequest());

        var draft = await JobPostingClient.CreateAsync(
            JobPostingTestData.CreateRequest());

        await JobPostingClient.ChangeStatusAsync(
            published.JobPostId,
            JobPostStatusApi.Published);

        JobPostingClient.WithOrganizationId(otherOrganizationId);

        var otherOrganizationPost = await JobPostingClient.CreateAsync(
            JobPostingTestData.CreateRequest());

        await JobPostingClient.ChangeStatusAsync(
            otherOrganizationPost.JobPostId,
            JobPostStatusApi.Published);

        // Act + Assert
        using var scope = Services.CreateScope();
        var reader = scope.ServiceProvider.GetRequiredService<IJobFeedReader>();

        await Eventually.AssertAsync(async () =>
        {
            var rows = await reader.GetJobsFeed(
                organizationId,
                CancellationToken.None);

            var row = Assert.Single(rows);

            Assert.Equal(published.JobPostId, row.Id);
            Assert.NotEqual(draft.JobPostId, row.Id);
        }, output: OutputHelper);
    }

    private async Task<JobPostFeedRow?> GetRow(Guid jobPostId)
    {
        var dataSource = Services.GetRequiredService<NpgsqlDataSource>();

        await using var connection = await dataSource.OpenConnectionAsync();

        return await connection.QuerySingleOrDefaultAsync<JobPostFeedRow>(
            """
            select id               as "Id",
                   organization_id  as "OrganizationId",
                   is_published     as "IsPublished",
                   title            as "Title",
                   summary          as "Summary",
                   description      as "Description",
                   responsibilities as "Responsibilities",
                   requirements     as "Requirements",
                   skills           as "Skills",
                   location         as "Location",
                   language_code    as "LanguageCode",
                   country_code     as "CountryCode",
                   employment_type  as "EmploymentType",
                   work_mode        as "WorkMode",
                   currency_code    as "CurrencyCode",
                   salary_min       as "SalaryMin",
                   salary_max       as "SalaryMax",
                   posting_slug     as "PostingSlug",
                   created_at       as "CreatedAt",
                   updated_at       as "UpdatedAt"
            from feeds.job_posts
            where id = @jobPostId
            """,
            new { jobPostId });
    }
}
