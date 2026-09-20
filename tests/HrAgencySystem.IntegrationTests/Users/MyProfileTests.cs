using System.Net;
using System.Net.Http.Json;
using HrAgencySystem.Api.Endpoints.User.Maps;
using HrAgencySystem.Identity.Events;
using HrAgencySystem.IntegrationTests.Infrastructure;
using Xunit.Abstractions;

namespace HrAgencySystem.IntegrationTests.Users;

[Collection(IntegrationCollection.Name)]
public class MyProfileTests(IntegrationEnvironment env, ITestOutputHelper outputHelper)
    : BaseIntegrationTest(env, outputHelper)
{
    protected override async Task BeforeEachAsync()
    {
        await Cleaner.CleanUsers();
    }

    [Fact]
    public async Task The_profile_answers_with_the_signed_in_person()
    {
        var organizationId = Guid.NewGuid();
        var user = await UserClient.CreateAsync(organizationId, "me@test.com", "John", "Doe");

        await Eventually.AssertAsync(async () =>
        {
            var profile = await GetProfile(organizationId, user.Id);

            Assert.Equal(user.Id, profile.User.Id);
            Assert.Equal("me@test.com", profile.User.Email);
            Assert.Null(profile.AvatarFileId);
        });
    }

    [Fact]
    public async Task Editing_the_profile_changes_the_contact_data()
    {
        var organizationId = Guid.NewGuid();
        var user = await UserClient.CreateAsync(organizationId, "me@test.com", "John", "Doe");

        var updated = await Update(
            organizationId,
            user.Id,
            new UpdateOwnProfileRequest("Johnny", "Doe-Smith", "Lead Recruiter", "+48 600 100 200")
        );

        Assert.Equal("Johnny", updated.Contact.FirstName);
        Assert.Equal("Doe-Smith", updated.Contact.LastName);

        await Eventually.AssertAsync(async () =>
        {
            var profile = await GetProfile(organizationId, user.Id);

            Assert.Equal("Johnny", profile.User.FirstName);
            Assert.Equal("Lead Recruiter", profile.User.JobTitle);
            Assert.Equal("+48 600 100 200", profile.User.Phone);
        });
    }

    /// <summary>
    /// The phone used to be dropped on the way through the projection, so it survived a creation and
    /// then silently vanished on the first edit. Worth its own test rather than a line in another.
    /// </summary>
    [Fact]
    public async Task Editing_the_profile_keeps_the_phone_it_was_given()
    {
        var organizationId = Guid.NewGuid();
        var user = await UserClient.CreateAsync(organizationId, "me@test.com", "John", "Doe");

        await Update(
            organizationId,
            user.Id,
            new UpdateOwnProfileRequest("John", "Doe", Phone: "+48 111 222 333")
        );

        await Eventually.AssertAsync(async () =>
        {
            var profile = await GetProfile(organizationId, user.Id);

            Assert.Equal("+48 111 222 333", profile.User.Phone);
        });
    }

    /// <summary>
    /// There is no field on the request that could carry an address, so the only thing worth proving
    /// is that editing everything else leaves the login alone.
    /// </summary>
    [Fact]
    public async Task Editing_the_profile_leaves_the_address_alone()
    {
        var organizationId = Guid.NewGuid();
        var user = await UserClient.CreateAsync(organizationId, "me@test.com", "John", "Doe");

        var updated = await Update(
            organizationId,
            user.Id,
            new UpdateOwnProfileRequest("Johnny", "Doe")
        );

        Assert.Equal("me@test.com", updated.Contact.Email);

        await Eventually.AssertAsync(async () =>
        {
            var profile = await GetProfile(organizationId, user.Id);

            Assert.Equal("me@test.com", profile.User.Email);
        });
    }

    [Fact]
    public async Task A_profile_without_a_name_is_refused()
    {
        var organizationId = Guid.NewGuid();
        var user = await UserClient.CreateAsync(organizationId, "me@test.com", "John", "Doe");

        Client.WithOrganizationId(organizationId);
        Client.WithUserId(user.Id);

        var response = await Client.PutAsJsonAsync(
            "/api/users/me",
            new UpdateOwnProfileRequest("", "")
        );

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
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

    private async Task<UserUpdated> Update(
        Guid organizationId,
        Guid userId,
        UpdateOwnProfileRequest request
    )
    {
        Client.WithOrganizationId(organizationId);
        Client.WithUserId(userId);

        var response = await Client.PutAsJsonAsync("/api/users/me", request);

        response.EnsureSuccessStatusCode();

        var result = await response.ReadWithJson<UserUpdated>(OutputHelper);

        Assert.NotNull(result);

        return result;
    }
}
