using System.Net;
using System.Net.Http.Json;
using HrAgencySystem.Api.Common.Errors;
using HrAgencySystem.Api.Endpoints.Organization.Maps;
using HrAgencySystem.IntegrationTests.Infrastructure;
using HrAgencySystem.Organization.Domain.ValueObjects;
using HrAgencySystem.Organization.Events;
using Microsoft.AspNetCore.Mvc;
using Xunit.Abstractions;

namespace HrAgencySystem.IntegrationTests.Organization;

[Collection(IntegrationCollection.Name)]
public sealed class UpdateOrganizationSlugTests : BaseIntegrationTest
{
    private OrganizationTestClient _testClient => new (Client, OutputHelper);
    public UpdateOrganizationSlugTests(
        IntegrationEnvironment env,
        ITestOutputHelper outputHelper)
        : base(env, outputHelper)
    {
       Cleaner.CleanOrganizationReservation().Wait();
       Client.AsOwner();
    }

    private static MapUpdateSlug.UpdateSlug UpdateSlugRequest(string slug = "new-slug")
    {
        return new MapUpdateSlug.UpdateSlug(slug);
    }

    [Fact]
    public async Task Put_valid_slug_updates_organization_slug()
    {
        var organization = await _testClient.CreateAsync(
            slug: "old-slug");

        var request = UpdateSlugRequest("new-slug");

        var response = await Client.PutAsJsonAsync(
            $"/api/organization/{organization.OrganizationId}/slug",
            request);

        var result = await response.ReadWithJson<OrganizationSlugUpdated>(
            OutputHelper);

        Assert.Equal(
            HttpStatusCode.OK,
            response.StatusCode);

        Assert.NotNull(result);
        Assert.Equal(
            organization.OrganizationId,
            result.OrganizationId);

        Assert.Equal(
            "new-slug",
            result.Slug);
    }

    [Fact]
    public async Task Put_empty_slug_returns_bad_request()
    {
        var organization = await _testClient.CreateAsync(
            slug: "old-slug");

        var request = UpdateSlugRequest(" ");

        var response = await Client.PutAsJsonAsync(
            $"/api/organization/{organization.OrganizationId}/slug",
            request);

        Assert.Equal(
            HttpStatusCode.BadRequest,
            response.StatusCode);

        var result = await response.ReadWithJson<BadRequestDetails>();

        Assert.NotNull(result);
        Assert.Single(result.ValidationErrors);

        Assert.Equal(
            OrganizationSlug.RequiredMessage,
            result.ValidationErrors.First());
    }

    [Fact]
    public async Task Put_slug_exceeding_max_length_returns_bad_request()
    {
        var organization = await _testClient.CreateAsync(
            slug: "old-slug");

        var request = UpdateSlugRequest(
            new string('a', 101));

        var response = await Client.PutAsJsonAsync(
            $"/api/organization/{organization.OrganizationId}/slug",
            request);

        Assert.Equal(
            HttpStatusCode.BadRequest,
            response.StatusCode);

        var result = await response.ReadWithJson<BadRequestDetails>();

        Assert.NotNull(result);
        Assert.Single(result.ValidationErrors);

        Assert.Equal(
            OrganizationSlug.MaxLengthMessage,
            result.ValidationErrors.First());
    }

    [Fact]
    public async Task Put_slug_for_non_existing_organization_returns_not_found()
    {
        var organizationId = Guid.NewGuid();

        var request = UpdateSlugRequest("new-slug");

        var response = await Client.PutAsJsonAsync(
            $"/api/organization/{organizationId}/slug",
            request);

        Assert.Equal(
            HttpStatusCode.NotFound,
            response.StatusCode);

        var result = await response.ReadWithJson<ProblemDetails>();

        Assert.NotNull(result);
    }

    [Fact]
    public async Task Put_duplicate_slug_returns_bad_request()
    {
        var firstOrganization = await _testClient.CreateAsync(
            slug: "existing-slug");

        var secondOrganization = await _testClient.CreateAsync(
            name: "Second Agency",
            slug: "second-slug");

        var request = UpdateSlugRequest(
            "existing-slug");

        var response = await Client.PutAsJsonAsync(
            $"/api/organization/{secondOrganization.OrganizationId}/slug",
            request);

        Assert.Equal(
            HttpStatusCode.BadRequest,
            response.StatusCode);
    }

    [Fact]
    public async Task Put_slug_normalizes_slug()
    {
        var organization = await _testClient.CreateAsync(
            slug: "old-slug");

        var request = UpdateSlugRequest(
            "  NEW-SLUG  ");

        var response = await Client.PutAsJsonAsync(
            $"/api/organization/{organization.OrganizationId}/slug",
            request);

        var result = await response.ReadWithJson<OrganizationSlugUpdated>(
            OutputHelper);

        Assert.Equal(
            HttpStatusCode.OK,
            response.StatusCode);

        Assert.NotNull(result);
        Assert.Equal(
            "new-slug",
            result.Slug);
        Assert.Equal(
            organization.OrganizationId,
            result.OrganizationId);
    }
}