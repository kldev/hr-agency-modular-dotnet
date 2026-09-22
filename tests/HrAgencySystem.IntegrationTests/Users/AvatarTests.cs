using System.Net;
using System.Net.Http.Headers;
using System.Text;
using HrAgencySystem.Api.Endpoints.User.Maps;
using HrAgencySystem.FileService.Contracts;
using HrAgencySystem.Identity.Application.Users.Avatar.Change;
using HrAgencySystem.Identity.Application.Users.Avatar.Remove;
using HrAgencySystem.Identity.Domain;
using HrAgencySystem.IntegrationTests.Infrastructure;
using HrAgencySystem.IntegrationTests.Infrastructure.Fakes;
using Microsoft.Extensions.DependencyInjection;
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
    /// The self-service route only ever answers about the caller. Reading somebody else's picture
    /// goes through the route that takes a user id, which is a different test below.
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

    [Fact]
    public async Task An_administrator_puts_a_picture_on_somebody_elses_profile()
    {
        var organizationId = Guid.NewGuid();
        var admin = await CreateUser(organizationId, "admin@test.com");
        var target = await CreateUser(organizationId, "target@test.com");

        var uploaded = await UploadFor(organizationId, admin, target);

        Assert.Equal(target, uploaded.UserId);

        var profile = await GetProfile(organizationId, target);

        Assert.Equal(uploaded.FileId, profile.AvatarFileId);
    }

    [Fact]
    public async Task An_administrator_takes_a_picture_off_somebody_elses_profile()
    {
        var organizationId = Guid.NewGuid();
        var admin = await CreateUser(organizationId, "admin@test.com");
        var target = await CreateUser(organizationId, "target@test.com");

        var uploaded = await UploadFor(organizationId, admin, target);

        Client.WithOrganizationId(organizationId);
        Client.WithUserId(admin);

        var response = await Client.DeleteAsync($"/api/users/{target}/avatar");

        response.EnsureSuccessStatusCode();

        var removed = await response.ReadWithJson<UserAvatarRemoved>(OutputHelper);

        Assert.NotNull(removed);
        Assert.Equal(uploaded.FileId, removed.FileId);

        var profile = await GetProfile(organizationId, target);

        Assert.Null(profile.AvatarFileId);
    }

    /// <summary>
    /// Reading is open to the whole organization on purpose - faces are drawn next to people in the
    /// user list and in the org chart, so a colleague has to be able to fetch one.
    /// </summary>
    [Fact]
    public async Task Anybody_in_the_organization_can_see_a_colleagues_picture()
    {
        var organizationId = Guid.NewGuid();
        var admin = await CreateUser(organizationId, "admin@test.com");
        var target = await CreateUser(organizationId, "target@test.com");
        var colleague = await CreateUser(organizationId, "colleague@test.com");

        await UploadFor(organizationId, admin, target);

        var recruiter = RecruiterClient(organizationId, colleague);

        var response = await recruiter.GetAsync($"/api/users/{target}/avatar");

        response.EnsureSuccessStatusCode();

        Assert.Equal("image/png", response.Content.Headers.ContentType?.MediaType);
        Assert.Equal(Picture, await response.Content.ReadAsStringAsync());
    }

    [Fact]
    public async Task Somebody_who_is_not_an_administrator_cannot_touch_another_persons_picture()
    {
        var organizationId = Guid.NewGuid();
        var admin = await CreateUser(organizationId, "admin@test.com");
        var target = await CreateUser(organizationId, "target@test.com");
        var colleague = await CreateUser(organizationId, "colleague@test.com");

        await UploadFor(organizationId, admin, target);

        var recruiter = RecruiterClient(organizationId, colleague);

        using var form = Form(Picture, "image/png", "face.png");

        var upload = await recruiter.PostAsync($"/api/users/{target}/avatar", form);
        var remove = await recruiter.DeleteAsync($"/api/users/{target}/avatar");

        Assert.Equal(HttpStatusCode.Forbidden, upload.StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, remove.StatusCode);
    }

    /// <summary>
    /// Not found rather than forbidden: a refusal would confirm that the id exists somewhere.
    /// </summary>
    [Fact]
    public async Task A_picture_belonging_to_another_organization_is_not_there()
    {
        var organizationId = Guid.NewGuid();
        var elsewhere = Guid.NewGuid();

        var admin = await CreateUser(organizationId, "admin@test.com");
        var target = await CreateUser(organizationId, "target@test.com");
        var stranger = await CreateUser(elsewhere, "stranger@test.com");

        await UploadFor(organizationId, admin, target);

        Client.WithOrganizationId(elsewhere);
        Client.WithUserId(stranger);

        var download = await Client.GetAsync($"/api/users/{target}/avatar");
        var remove = await Client.DeleteAsync($"/api/users/{target}/avatar");

        Assert.Equal(HttpStatusCode.NotFound, download.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, remove.StatusCode);
    }

    /// <summary>
    /// The target is looked up before the bytes are read, so an id that answers nothing costs no
    /// round trip to the file service - and a file nobody can reach is never left behind.
    /// </summary>
    [Fact]
    public async Task Setting_a_picture_for_somebody_who_does_not_exist_never_reaches_the_storage()
    {
        var organizationId = Guid.NewGuid();
        var admin = await CreateUser(organizationId, "admin@test.com");

        var files = (FakeFileServiceClient)Env.Services.GetRequiredService<IFileServiceClient>();
        var before = files.UploadCount;

        Client.WithOrganizationId(organizationId);
        Client.WithUserId(admin);

        using var form = Form(Picture, "image/png", "face.png");

        var response = await Client.PostAsync($"/api/users/{Guid.NewGuid()}/avatar", form);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.Equal(before, files.UploadCount);
    }

    [Fact]
    public async Task The_catalogue_reports_only_the_people_who_have_a_picture()
    {
        var organizationId = Guid.NewGuid();
        var admin = await CreateUser(organizationId, "admin@test.com");
        var withPicture = await CreateUser(organizationId, "with@test.com");
        var withoutPicture = await CreateUser(organizationId, "without@test.com");

        var uploaded = await UploadFor(organizationId, admin, withPicture);

        Client.WithOrganizationId(organizationId);
        Client.WithUserId(admin);

        var response = await Client.GetAsync("/api/users/avatars");

        response.EnsureSuccessStatusCode();

        var catalogue = await response.ReadWithJson<List<UserAvatarRef>>(OutputHelper);

        Assert.NotNull(catalogue);
        Assert.Equal(
            uploaded.FileId,
            Assert.Single(catalogue, z => z.UserId == withPicture).AvatarFileId
        );
        Assert.DoesNotContain(catalogue, z => z.UserId == withoutPicture);
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

    private async Task<UserAvatarChanged> UploadFor(
        Guid organizationId,
        Guid adminId,
        Guid targetId
    )
    {
        Client.WithOrganizationId(organizationId);
        Client.WithUserId(adminId);

        using var form = Form(Picture, "image/png", "face.png");

        var response = await Client.PostAsync($"/api/users/{targetId}/avatar", form);

        response.EnsureSuccessStatusCode();

        var result = await response.ReadWithJson<UserAvatarChanged>(OutputHelper);

        Assert.NotNull(result);

        return result;
    }

    /// <summary>A client of the same organization holding a role that is not <c>Admin</c>.</summary>
    private HttpClient RecruiterClient(Guid organizationId, Guid userId)
    {
        var client = Env.CreateClient();

        client.SetTestRoles(nameof(OrganizationRole.Recruiter));
        client.WithOrganizationId(organizationId);
        client.WithUserId(userId);

        return client;
    }

    private static MultipartFormDataContent Form(
        string content,
        string contentType,
        string fileName
    )
    {
        var form = new MultipartFormDataContent();
        var file = new ByteArrayContent(Encoding.UTF8.GetBytes(content));
        file.Headers.ContentType = new MediaTypeHeaderValue(contentType);

        form.Add(file, "file", fileName);

        return form;
    }
}
