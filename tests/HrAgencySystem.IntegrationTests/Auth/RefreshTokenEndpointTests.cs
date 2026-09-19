using System.Net;
using System.Net.Http.Json;
using HrAgencySystem.Api.Endpoints.Auth.Maps;
using HrAgencySystem.Identity.Application.Users.Login;
using HrAgencySystem.IntegrationTests.Infrastructure;
using HrAgencySystem.Organization.Application.Create;
using HrAgencySystem.Organization.Events;
using Xunit.Abstractions;

namespace HrAgencySystem.IntegrationTests.Auth;

[Collection(IntegrationCollection.Name)]
public sealed class RefreshTokenEndpointTests(
    IntegrationEnvironment env,
    ITestOutputHelper outputHelper
) : BaseIntegrationTest(env, outputHelper)
{
    private const string Password = "Password123!";
    private const string LoginUrl = "/api/auth/login";
    private const string RefreshUrl = "/api/auth/refresh";
    private const string LogoutUrl = "/api/auth/logout";

    protected override async Task BeforeEachAsync()
    {
        await Cleaner.CleanRefreshTokens();
        await Cleaner.CleanUsers();
        await Cleaner.CleanOrganizationReservation();
    }

    [Fact]
    public async Task ShouldIssueBothTokensOnLogin()
    {
        var session = await SignIn("issue");

        Assert.NotEmpty(session.Token);
        Assert.NotEmpty(session.RefreshToken);
        Assert.NotEqual(session.Token, session.RefreshToken);

        // the two lifetimes the client is told about are the configured ones, not guesses
        Assert.InRange(
            session.RefreshTokenExpiresAt,
            DateTimeOffset.UtcNow.AddDays(29),
            DateTimeOffset.UtcNow.AddDays(31)
        );
        Assert.InRange(
            session.ExpiresAt,
            DateTimeOffset.UtcNow.AddHours(5),
            DateTimeOffset.UtcNow.AddHours(7)
        );
    }

    [Fact]
    public async Task ShouldRotateTheRefreshTokenWithoutMovingItsExpiry()
    {
        var session = await SignIn("rotate");

        var refreshed = await Refresh(session.RefreshToken);

        Assert.NotEqual(session.RefreshToken, refreshed.RefreshToken);
        Assert.NotEmpty(refreshed.Token);

        // thirty days are counted from the login, so a rotation cannot extend the session
        Assert.Equal(session.RefreshTokenExpiresAt, refreshed.RefreshTokenExpiresAt);
    }

    [Fact]
    public async Task ShouldRejectAReplayedRefreshTokenAndTakeTheWholeSessionDown()
    {
        var session = await SignIn("replay");

        var refreshed = await Refresh(session.RefreshToken);

        // the spent token turns up a second time: either a buggy client or a stolen one
        var replay = await Post(
            RefreshUrl,
            new MapRefreshToken.RefreshTokenRequest(session.RefreshToken)
        );

        Assert.Equal(HttpStatusCode.Unauthorized, replay.StatusCode);

        // and the token the legitimate client is holding dies with the rest of the family
        var afterReplay = await Post(
            RefreshUrl,
            new MapRefreshToken.RefreshTokenRequest(refreshed.RefreshToken)
        );

        Assert.Equal(HttpStatusCode.Unauthorized, afterReplay.StatusCode);
    }

    [Fact]
    public async Task ShouldRejectARefreshTokenThatWasNeverIssued()
    {
        var response = await Post(
            RefreshUrl,
            new MapRefreshToken.RefreshTokenRequest("not-a-token")
        );

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task ShouldEndTheSessionOnLogout()
    {
        var session = await SignIn("logout");

        var response = await Post(LogoutUrl, new MapLogout.LogoutRequest(session.RefreshToken));

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        var afterLogout = await Post(
            RefreshUrl,
            new MapRefreshToken.RefreshTokenRequest(session.RefreshToken)
        );

        Assert.Equal(HttpStatusCode.Unauthorized, afterLogout.StatusCode);
    }

    [Fact]
    public async Task ShouldAcceptLoggingOutWithATokenThatIsAlreadyGone()
    {
        // logging out twice, or after the window closed, is not an error - and saying otherwise
        // would tell a caller which tokens exist
        var response = await Post(LogoutUrl, new MapLogout.LogoutRequest("not-a-token"));

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    private async Task<LoginUserResult> SignIn(string slug)
    {
        var organizationId = await CreateOrganizationAsync(slug);
        var email = $"{slug}@{slug}.com";

        await UserClient.CreateAsync(organizationId, email);

        LoginUserResult? session = null;

        // the user projection is built by the async daemon, so the account is not loginable
        // the instant the command comes back
        await Eventually.AssertAsync(
            async () =>
            {
                var response = await Post(LoginUrl, new LoginUser(email, Password, slug));

                Assert.Equal(HttpStatusCode.OK, response.StatusCode);

                session = await response.ReadWithJson<LoginUserResult>(OutputHelper);
            },
            output: OutputHelper
        );

        Assert.NotNull(session);

        return session;
    }

    private async Task<LoginUserResult> Refresh(string refreshToken)
    {
        var response = await Post(
            RefreshUrl,
            new MapRefreshToken.RefreshTokenRequest(refreshToken)
        );

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await response.ReadWithJson<LoginUserResult>(OutputHelper);

        Assert.NotNull(result);

        return result;
    }

    private async Task<Guid> CreateOrganizationAsync(string slug)
    {
        Client.AsOwner();

        var request = new CreateOrganization("HR Agency", slug, Guid.NewGuid(), [$"{slug}.com"]);
        var response = await Client.PostAsJsonAsync("/api/organization", request);

        response.EnsureSuccessStatusCode();

        var created = await response.ReadWithJson<OrganizationCreated>(OutputHelper);

        Assert.NotNull(created);

        Client.AsOrganizationRoles();

        return created.OrganizationId;
    }

    private Task<HttpResponseMessage> Post<T>(string url, T body) =>
        Client.PostAsJsonAsync(url, body);
}
