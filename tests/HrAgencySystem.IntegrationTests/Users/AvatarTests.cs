using System.Net;
using System.Net.Http.Headers;
using System.Text;
using HrAgencySystem.Api.Endpoints.User.Maps;
using HrAgencySystem.Identity.Application.Users.Avatar.Change;
using HrAgencySystem.Identity.Application.Users.Avatar.Remove;
using HrAgencySystem.IntegrationTests.Infrastructure;
using Xunit.Abstractions;

namespace HrAgencySystem.IntegrationTests.Users;

[Collection(IntegrationCollection.Name)]
public class AvatarTests(IntegrationEnvironment env, ITestOutputHelper outputHelper)
    : BaseIntegrationTest(env, outputHelper)
{
    private const string Picture = "not really a png, but the bytes are nobody's business";

    protected override async Task BeforeEachAsync()
    {
        await Cleaner.CleanUsers();
    }

    [Fact]
    public async Task Uploading_a_picture_puts_it_on_the_profile()
    {
        var organizationId = Guid.NewGuid();
        var user = await CreateUser(organizationId);

        var uploaded = await Upload(organizationId, user);

        Assert.NotEqual(Guid.Empty, uploaded.FileId);
        Assert.Null(uploaded.PreviousFileId);

        var profile = await GetProfile(organizationId, user);

        Assert.Equal(uploaded.FileId, profile.AvatarFileId);
    }

    /// <summary>
    /// The profile is a plain document, not a projection, so the picture is on it the moment the
    /// upload answers - no waiting for a daemon.
    /// </summary>
    [Fact]
    public async Task The_picture_can_be_read_back_immediately()
    {
        var organizationId = Guid.NewGuid();
        var user = await CreateUser(organizationId);

        await Upload(organizationId, user);

        Client.WithOrganizationId(organizationId);
        Client.WithUserId(user);

        var response = await Client.GetAsync("/api/users/me/avatar");

        response.EnsureSuccessStatusCode();

        Assert.Equal("image/png", response.Content.Headers.ContentType?.MediaType);
        Assert.Equal(Picture, await response.Content.ReadAsStringAsync());
    }

    [Fact]
    public async Task Replacing_a_picture_reports_the_one_it_displaced()
    {
        var organizationId = Guid.NewGuid();
        var user = await CreateUser(organizationId);

        var first = await Upload(organizationId, user);
        var second = await Upload(organizationId, user);

        Assert.Equal(first.FileId, second.PreviousFileId);
        Assert.NotEqual(first.FileId, second.FileId);

        var profile = await GetProfile(organizationId, user);

        Assert.Equal(second.FileId, profile.AvatarFileId);
    }

    [Fact]
    public async Task Removing_a_picture_leaves_the_profile_without_one()
    {
        var organizationId = Guid.NewGuid();
        var user = await CreateUser(organizationId);

        var uploaded = await Upload(organizationId, user);

        Client.WithOrganizationId(organizationId);
        Client.WithUserId(user);

        var response = await Client.DeleteAsync("/api/users/me/avatar");

        response.EnsureSuccessStatusCode();

        var removed = await response.ReadWithJson<UserAvatarRemoved>(OutputHelper);

        Assert.NotNull(removed);
        Assert.Equal(uploaded.FileId, removed.FileId);

        var profile = await GetProfile(organizationId, user);

        Assert.Null(profile.AvatarFileId);

        var download = await Client.GetAsync("/api/users/me/avatar");

        Assert.Equal(HttpStatusCode.NotFound, download.StatusCode);
    }

    [Fact]
    public async Task Removing_a_picture_that_is_not_there_is_refused()
    {
        var organizationId = Guid.NewGuid();
        var user = await CreateUser(organizationId);

        Client.WithOrganizationId(organizationId);
        Client.WithUserId(user);

        var response = await Client.DeleteAsync("/api/users/me/avatar");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task A_picture_over_the_limit_is_refused()
    {
        var organizationId = Guid.NewGuid();
        var user = await CreateUser(organizationId);

        var oversized = new string('x', (512 * 1024) + 1);

        var response = await Send(organizationId, user, oversized, "image/png", "face.png");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Anything_that_is_not_a_png_or_jpeg_is_refused()
    {
        var organizationId = Guid.NewGuid();
        var user = await CreateUser(organizationId);

        var response = await Send(
            organizationId,
            user,
            "the signed contract",
            "application/pdf",
            "umowa.pdf"
        );

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    /// <summary>
    /// Nobody can reach anybody else's picture, because there is no route that takes a user id - the
    /// only avatar these endpoints know about is the caller's own.
    /// </summary>
    [Fact]
    public async Task Another_persons_picture_is_simply_not_there()
    {
        var organizationId = Guid.NewGuid();
        var owner = await CreateUser(organizationId, "owner@test.com");
        var other = await CreateUser(organizationId, "other@test.com");

        await Upload(organizationId, owner);

        Client.WithOrganizationId(organizationId);
        Client.WithUserId(other);

        var response = await Client.GetAsync("/api/users/me/avatar");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    /// <summary>
    /// Waits out the user read model before handing the id back. The avatar itself is a document and
    /// needs no waiting, but the profile endpoint also reports the person, and that half is a
    /// projection - without this the tests would be measuring the daemon rather than the feature.
    /// </summary>
    private async Task<Guid> CreateUser(Guid organizationId, string email = "me@test.com")
    {
        var user = await UserClient.CreateAsync(organizationId, email, "John", "Doe");

        await Eventually.AssertAsync(async () =>
        {
            Client.WithOrganizationId(organizationId);
            Client.WithUserId(user.Id);

            var response = await Client.GetAsync("/api/users/me");

            response.EnsureSuccessStatusCode();
        });

        return user.Id;
    }

    private async Task<UserAvatarChanged> Upload(Guid organizationId, Guid userId)
    {
        var response = await Send(organizationId, userId, Picture, "image/png", "face.png");

        response.EnsureSuccessStatusCode();

        var result = await response.ReadWithJson<UserAvatarChanged>(OutputHelper);

        Assert.NotNull(result);

        return result;
    }

    private async Task<HttpResponseMessage> Send(
        Guid organizationId,
        Guid userId,
        string content,
        string contentType,
        string fileName
    )
    {
        Client.WithOrganizationId(organizationId);
        Client.WithUserId(userId);

        using var form = new MultipartFormDataContent();
        var file = new ByteArrayContent(Encoding.UTF8.GetBytes(content));
        file.Headers.ContentType = new MediaTypeHeaderValue(contentType);

        form.Add(file, "file", fileName);

        return await Client.PostAsync("/api/users/me/avatar", form);
    }

    private async Task<MyProfileResponse> GetProfile(Guid organizationId, Guid userId)
    {
        Client.WithOrganizationId(organizationId);
        Client.WithUserId(userId);

        var response = await Client.GetAsync("/api/users/me");

        response.EnsureSuccessStatusCode();

        var result = await response.ReadWithJson<MyProfileResponse>(OutputHelper);

        Assert.NotNull(result);

        return result;
    }
}
