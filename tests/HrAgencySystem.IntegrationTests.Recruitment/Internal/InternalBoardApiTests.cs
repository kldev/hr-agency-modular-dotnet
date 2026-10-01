using System.Net;
using System.Net.Http.Json;
using HrAgencySystem.Api.Auth;
using HrAgencySystem.Api.Endpoints.Internal.Maps;
using HrAgencySystem.Api.Endpoints.Owner.Maps;
using HrAgencySystem.Identity.Application.ApiKeys.Issue;
using HrAgencySystem.IntegrationTests.Infrastructure;
using HrAgencySystem.IntegrationTests.JobPosts;
using HrAgencySystem.IntegrationTests.Organization;
using HrAgencySystem.Recruitment.Domain.JobPostings;
using HrAgencySystem.Recruitment.Projections;
using Xunit.Abstractions;

namespace HrAgencySystem.IntegrationTests.Internal;

/// <summary>
/// The routes the public job board uses, end to end. The tests that matter most are the two 401s:
/// no key, and a user's credentials instead of one - the schemes must not mix in either direction.
/// </summary>
[Collection(IntegrationCollection.Name)]
public class InternalBoardApiTests(IntegrationEnvironment env, ITestOutputHelper outputHelper)
    : BaseIntegrationTest(env, outputHelper)
{
    protected override async Task BeforeEachAsync()
    {
        await Cleaner.CleanApiKeys();
        await Cleaner.CleanOrganizations();
        await Cleaner.CleanCandidates();
        await Cleaner.CleanJobApplications();
    }

    [Fact]
    public async Task Without_a_key_every_internal_route_is_401()
    {
        var board = await BoardAsync();
        var anonymous = Env.CreateClient();

        foreach (var url in Routes(board))
            Assert.Equal(HttpStatusCode.Unauthorized, (await anonymous.GetAsync(url)).StatusCode);

        Assert.Equal(HttpStatusCode.Unauthorized, (await Apply(anonymous, board)).StatusCode);
    }

    /// <summary>A signed-in agency admin is somebody - but not a program holding a key.</summary>
    [Fact]
    public async Task A_users_credentials_do_not_open_internal_routes()
    {
        var board = await BoardAsync();
        var user = Env.CreateClient().AsOrganizationRoles();
        user.WithOrganizationId(board.OrganizationId);

        foreach (var url in Routes(board))
            Assert.Equal(HttpStatusCode.Unauthorized, (await user.GetAsync(url)).StatusCode);
    }

    [Fact]
    public async Task A_revoked_key_is_401()
    {
        var board = await BoardAsync();
        var (id, value) = await IssueKeyAsync();

        var owner = Env.CreateClient().AsOwner();
        (await owner.DeleteAsync($"/api/owners/api-keys/{id}")).EnsureSuccessStatusCode();

        var response = await WithKey(value).GetAsync($"/api/internal/boards/{board.Slug}");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task With_a_key_the_board_and_its_post_are_readable()
    {
        var board = await BoardAsync();
        var (_, value) = await IssueKeyAsync();
        var client = WithKey(value);

        var agency = await client.GetFromJsonAsync<MapGetBoard.BoardResponse>(
            $"/api/internal/boards/{board.Slug}"
        );
        Assert.Equal(board.Name, agency?.Name);

        var post = await (
            await client.GetAsync($"/api/internal/boards/{board.Slug}/posts/{board.PostSlug}")
        ).ReadWithJson<MapGetPost.BoardPostResponse>(OutputHelper);
        Assert.Equal("Senior .NET Developer", post?.Title);
    }

    [Fact]
    public async Task An_unknown_slug_is_404()
    {
        var (_, value) = await IssueKeyAsync();
        var client = WithKey(value);

        Assert.Equal(
            HttpStatusCode.NotFound,
            (await client.GetAsync("/api/internal/boards/nobody-here")).StatusCode
        );
        Assert.Equal(
            HttpStatusCode.NotFound,
            (await client.GetAsync("/api/internal/boards/nobody-here/posts/anything")).StatusCode
        );
    }

    /// <summary>The application lands exactly where one from the panel would.</summary>
    [Fact]
    public async Task Applying_through_the_board_creates_the_application()
    {
        var board = await BoardAsync();
        var (_, value) = await IssueKeyAsync();

        var response = await Apply(WithKey(value), board);
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var created = await response.ReadWithJson<MapApply.BoardApplicationResponse>(OutputHelper);
        Assert.NotNull(created);

        Client.WithOrganizationId(board.OrganizationId);

        await Eventually.AssertAsync(async () =>
        {
            var application = await (
                await Client.GetAsync($"/api/recruitment/job-applications/{created.ApplicationId}")
            ).ReadWithJson<JobApplicationProjection>();

            Assert.NotNull(application);
            Assert.Equal("candidate@board.test", application.ApplicantEmail);
            Assert.Equal(board.PostId, application.JobPostId);
        });
    }

    private sealed record Board(
        Guid OrganizationId,
        string Slug,
        string Name,
        Guid PostId,
        string PostSlug
    );

    /// <summary>
    /// The slug is fixed, not random: a post takes its organization's slug from
    /// <c>FakeModuleService.GetOrganizationSlug</c>, which always answers "Slug", and the board
    /// finds a post by that slug (case-insensitively).
    /// </summary>
    private const string BoardSlug = "slug";

    private async Task<Board> BoardAsync()
    {
        const string slug = BoardSlug;
        var organizations = new OrganizationTestClient(Env.CreateClient().AsOwner(), OutputHelper);
        var organization = await organizations.CreateAsync("Board Agency", slug);

        JobPostingClient.WithOrganizationId(organization.OrganizationId);
        var post = await JobPostingClient.CreateAsync(JobPostingTestData.CreateRequest());
        await JobPostingClient.ChangeStatusAsync(post.JobPostId, JobPostStatusApi.Published);

        JobPostProjection? projection = null;
        var (_, key) = await IssueKeyAsync();

        await Eventually.AssertAsync(async () =>
        {
            projection = await JobPostingClient.GetSingle(post.JobPostId);
            Assert.Equal(JobPostStatus.Published, projection.Status);

            // The organization read model catches up on its own schedule, too.
            var agency = await WithKey(key).GetAsync($"/api/internal/boards/{slug}");
            Assert.Equal(HttpStatusCode.OK, agency.StatusCode);
        });

        return new Board(
            organization.OrganizationId,
            slug,
            "Board Agency",
            post.JobPostId,
            projection!.PostingSlug.Split('/').Last()
        );
    }

    private async Task<(Guid Id, string Value)> IssueKeyAsync()
    {
        var owner = Env.CreateClient().AsOwner();

        var response = await owner.PostAsJsonAsync(
            "/api/owners/api-keys",
            new MapIssueApiKey.IssueServiceApiKeyRequest("public job board")
        );
        response.EnsureSuccessStatusCode();

        var issued = await response.ReadWithJson<ServiceApiKeyIssued>(OutputHelper);
        Assert.NotNull(issued);

        return (issued.Id, issued.Value);
    }

    private HttpClient WithKey(string value)
    {
        var client = Env.CreateClient();
        client.DefaultRequestHeaders.Add(ApiKeyAuthenticationHandler.HeaderName, value);

        return client;
    }

    private static string[] Routes(Board board) =>
        [
            $"/api/internal/boards/{board.Slug}",
            $"/api/internal/boards/{board.Slug}/feed.json",
            $"/api/internal/boards/{board.Slug}/posts/{board.PostSlug}",
        ];

    private static Task<HttpResponseMessage> Apply(HttpClient client, Board board) =>
        client.PostAsJsonAsync(
            $"/api/internal/boards/{board.Slug}/posts/{board.PostSlug}/applications",
            new MapApply.BoardApplicationRequest(
                "Anna",
                "Kowalska",
                "candidate@board.test",
                "+48 600 000 000"
            )
        );
}
