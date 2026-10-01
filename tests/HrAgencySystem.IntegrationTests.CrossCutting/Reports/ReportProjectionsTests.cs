using Dapper;
using HrAgencySystem.IntegrationTests.Infrastructure;
using HrAgencySystem.IntegrationTests.Organization;
using HrAgencySystem.IntegrationTests.Timeline;
using HrAgencySystem.Projects.Domain;
using HrAgencySystem.Recruitment.Domain.Applications;
using HrAgencySystem.Recruitment.Domain.Interviews;
using HrAgencySystem.Reports.ReadModel;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using Xunit.Abstractions;

namespace HrAgencySystem.IntegrationTests.Reports;

/// <summary>
/// The reporting tables are filled by projections living in three modules; these tests drive the
/// real endpoints and read the rows back with SQL, the way the reports service will.
/// </summary>
[Collection(IntegrationCollection.Name)]
public sealed class ReportProjectionsTests(IntegrationEnvironment env, ITestOutputHelper output)
    : BaseIntegrationTest(env, output)
{
    private static readonly TimeSpan ProjectionTimeout = TimeSpan.FromSeconds(15);

    private readonly Guid _organizationId = Guid.NewGuid();

    private TimelineTestClient Recruitment => new(Client);

    protected override async Task BeforeEachAsync()
    {
        await Cleaner.CleanReports();
    }

    [Fact]
    public async Task Application_RecordsEveryStageItReached()
    {
        // Arrange
        var jobPostId = await Recruitment.PublishedJobPostAsync(_organizationId);
        var application = await Recruitment.ApplyAsync(
            _organizationId,
            jobPostId,
            $"reports-{Guid.NewGuid():N}@test.io"
        );

        // Act
        await Recruitment.ChangeStatusAsync(
            _organizationId,
            application.JobApplicationId,
            JobApplicationUpdateStatus.Screening,
            "screening"
        );
        var interview = await Recruitment.ScheduleInterviewAsync(
            _organizationId,
            application.JobApplicationId,
            DateTime.UtcNow.Date.AddDays(7).AddHours(10)
        );
        await Recruitment.ChangeInterviewStatusAsync(
            _organizationId,
            interview.InterviewId,
            InterviewStatus.Completed
        );
        await Recruitment.ChangeStatusAsync(
            _organizationId,
            application.JobApplicationId,
            JobApplicationUpdateStatus.Offer,
            "offer"
        );
        await Recruitment.ChangeStatusAsync(
            _organizationId,
            application.JobApplicationId,
            JobApplicationUpdateStatus.Hired,
            "hired"
        );

        // Assert
        await Eventually.AssertAsync(
            async () =>
            {
                var row = await QueryRow<ApplicationRow>(
                    $"""
                    select organization_id, job_post_id, status, created_at, screening_at,
                           interview_at, offer_at, hired_at, rejected_at
                    from reports.{ReportsSchema.Tables.Applications}
                    where id = @id
                    """,
                    application.JobApplicationId
                );

                Assert.NotNull(row);
                Assert.Equal(_organizationId, row.organization_id);
                Assert.Equal(jobPostId, row.job_post_id);
                Assert.Equal(nameof(JobApplicationStatus.Hired), row.status);
                Assert.NotNull(row.screening_at);
                Assert.NotNull(row.interview_at);
                Assert.NotNull(row.offer_at);
                Assert.NotNull(row.hired_at);
                Assert.Null(row.rejected_at);
                Assert.True(row.screening_at <= row.interview_at);
                Assert.True(row.offer_at <= row.hired_at);

                var post = await QueryRow<JobPostRow>(
                    $"""
                    select is_published, first_published_at
                    from reports.{ReportsSchema.Tables.JobPosts}
                    where id = @id
                    """,
                    jobPostId
                );

                Assert.NotNull(post);
                Assert.True(post.is_published);
                Assert.NotNull(post.first_published_at);

                var held = await QueryRow<InterviewRow>(
                    $"""
                    select status, completed_at
                    from reports.{ReportsSchema.Tables.Interviews}
                    where id = @id
                    """,
                    interview.InterviewId
                );

                Assert.NotNull(held);
                Assert.Equal(nameof(InterviewStatus.Completed), held.status);
                Assert.NotNull(held.completed_at);
            },
            ProjectionTimeout,
            output: OutputHelper
        );
    }

    [Fact]
    public async Task Project_RecordsWhenItWentLive()
    {
        // Arrange
        var project = await ProjectClient.CreateReadyToGoLiveAsync(_organizationId);

        // Act
        await ProjectClient.ChangeStatusAsync(
            _organizationId,
            project.ProjectId,
            ProjectStatus.Active
        );

        // Assert
        await Eventually.AssertAsync(
            async () =>
            {
                var row = await QueryRow<ProjectRow>(
                    $"""
                    select organization_id, status, went_live_at
                    from reports.{ReportsSchema.Tables.Projects}
                    where id = @id
                    """,
                    project.ProjectId
                );

                Assert.NotNull(row);
                Assert.Equal(_organizationId, row.organization_id);
                Assert.Equal(nameof(ProjectStatus.Active), row.status);
                Assert.NotNull(row.went_live_at);
            },
            ProjectionTimeout,
            output: OutputHelper
        );
    }

    [Fact]
    public async Task Organization_IsListedUnderItsCurrentName()
    {
        // Arrange
        var slug = $"reports-{Guid.NewGuid():N}"[..20];
        var organizations = new OrganizationTestClient(Env.CreateClient().AsOwner(), OutputHelper);

        // Act
        var created = await organizations.CreateAsync("Reporting Agency", slug);

        // Assert
        await Eventually.AssertAsync(
            async () =>
            {
                var row = await QueryRow<OrganizationRow>(
                    $"""
                    select name, slug
                    from reports.{ReportsSchema.Tables.Organizations}
                    where id = @id
                    """,
                    created.OrganizationId
                );

                Assert.NotNull(row);
                Assert.Equal("Reporting Agency", row.name);
                Assert.Equal(slug, row.slug);
            },
            ProjectionTimeout,
            output: OutputHelper
        );
    }

    private async Task<T?> QueryRow<T>(string sql, Guid id)
    {
        var dataSource = Services.GetRequiredService<NpgsqlDataSource>();
        await using var connection = await dataSource.OpenConnectionAsync();

        return await connection.QuerySingleOrDefaultAsync<T>(sql, new { id });
    }

    // Column-shaped on purpose: this is what the reports service will read, not the EF row type.
    // ReSharper disable InconsistentNaming, UnusedAutoPropertyAccessor.Local, UnusedMember.Local
    private sealed class ApplicationRow
    {
        public Guid organization_id { get; init; }
        public Guid job_post_id { get; init; }
        public string status { get; init; } = "";
        public DateTimeOffset created_at { get; init; }
        public DateTimeOffset? screening_at { get; init; }
        public DateTimeOffset? interview_at { get; init; }
        public DateTimeOffset? offer_at { get; init; }
        public DateTimeOffset? hired_at { get; init; }
        public DateTimeOffset? rejected_at { get; init; }
    }

    private sealed class JobPostRow
    {
        public bool is_published { get; init; }
        public DateTimeOffset? first_published_at { get; init; }
    }

    private sealed class InterviewRow
    {
        public string status { get; init; } = "";
        public DateTimeOffset? completed_at { get; init; }
    }

    private sealed class ProjectRow
    {
        public Guid organization_id { get; init; }
        public string status { get; init; } = "";
        public DateTimeOffset? went_live_at { get; init; }
    }

    private sealed class OrganizationRow
    {
        public string name { get; init; } = "";
        public string slug { get; init; } = "";
    }

    // ReSharper restore InconsistentNaming, UnusedAutoPropertyAccessor.Local, UnusedMember.Local
}
