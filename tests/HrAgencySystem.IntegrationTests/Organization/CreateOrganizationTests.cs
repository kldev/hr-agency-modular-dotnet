using System.Net;
using System.Net.Http.Json;
using HrAgencySystem.Api.Common.Errors;
using HrAgencySystem.IntegrationTests.Infrastructure;
using HrAgencySystem.Organization.Application.Commands;
using HrAgencySystem.Organization.Domain.ValueObjects;
using HrAgencySystem.Organization.Events;
using HrAgencySystem.SharedKernel.Exception;
using JasperFx;
using Microsoft.AspNetCore.Mvc;
using Npgsql;
using Xunit.Abstractions;

namespace HrAgencySystem.IntegrationTests.Organization;

[Collection(IntegrationCollection.Name)]
public sealed class CreateOrganizationTests : BaseIntegrationTest
{
    public CreateOrganizationTests(IntegrationEnvironment env, ITestOutputHelper outputHelper) : base(env, outputHelper) 
    {
        Cleaner.CleanOrganizationReservation().Wait();
        Client.AsOwner();
    }

    private static CreateOrganization CreateOrganizationRequest(
        string name = "HR Agency",
        string slug = "hr-agency")
    {
        return new CreateOrganization(
            name,
            slug,
            Guid.NewGuid());
    }
    
    [Fact]
    public async Task Post_valid_organization_creates_organization()
    {
        var request = CreateOrganizationRequest();

        var response = await Client.PostAsJsonAsync(
            "/api/organization",
            request);

        var result = await response.ReadWithJson<OrganizationCreated>(
            OutputHelper);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        Assert.NotNull(result);
        Assert.Equal(request.Name, result.Name);
        Assert.Equal(request.Slug, result.Slug);
        Assert.NotEqual(default, result.CreatedAt);
    }

    [Fact]
    public async Task Post_organization_without_name_returns_bad_request()
    {
        var request = CreateOrganizationRequest(
            " ",
            "hr-agency");

        var response = await Client.PostAsJsonAsync(
            "/api/organization",
            request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var result = await response.ReadWithJson<BadRequestDetails>();

        Assert.NotNull(result);
        Assert.Single(result.ValidationErrors);

        Assert.Equal(
            OrganizationName.RequiredMessage,
            result.ValidationErrors.First());
    }

    [Fact]
    public async Task Post_organization_without_slug_returns_bad_request()
    {
        var request = CreateOrganizationRequest(
            "HR Agency",
            " ");

        var response = await Client.PostAsJsonAsync(
            "/api/organization",
            request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var result = await response.ReadWithJson<BadRequestDetails>();

        Assert.NotNull(result);
        Assert.Single(result.ValidationErrors);

        Assert.Equal(
            OrganizationSlug.RequiredMessage,
            result.ValidationErrors.First());
    }

    [Fact]
    public async Task Post_organization_with_invalid_fields_returns_all_validation_errors()
    {
        var request = CreateOrganizationRequest(
            " ",
            " ");

        var response = await Client.PostAsJsonAsync(
            "/api/organization",
            request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var result = await response.ReadWithJson<BadRequestDetails>();

        Assert.NotNull(result);
        Assert.Equal(2, result.ValidationErrors.Count);

        Assert.Contains(
            OrganizationName.RequiredMessage,
            result.ValidationErrors);

        Assert.Contains(
            OrganizationSlug.RequiredMessage,
            result.ValidationErrors);
    }

    [Fact]
    public async Task Post_organization_normalizes_name_and_slug()
    {
        var request = CreateOrganizationRequest(
            "  HR Agency  ",
            "  HR-AGENCY  ");

        var response = await Client.PostAsJsonAsync(
            "/api/organization",
            request);

        var result = await response.ReadWithJson<OrganizationCreated>(
            OutputHelper);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        Assert.NotNull(result);
        Assert.Equal("HR Agency", result.Name);
        Assert.Equal("hr-agency", result.Slug);
    }

    [Fact]
    public async Task Post_organization_with_name_exceeding_max_length_returns_bad_request()
    {
        var request = CreateOrganizationRequest(
            new string('A', 251)
            );

        var response = await Client.PostAsJsonAsync(
            "/api/organization",
            request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var result = await response.ReadWithJson<BadRequestDetails>();

        Assert.NotNull(result);
        Assert.Single(result.ValidationErrors);

        Assert.Equal(
            OrganizationName.MaxLengthMessage,
            result.ValidationErrors.First());
    }

    [Fact]
    public async Task Post_organization_with_slug_exceeding_max_length_returns_bad_request()
    {
        var request = CreateOrganizationRequest(
            "HR Agency",
            new string('a', 101));

        var response = await Client.PostAsJsonAsync(
            "/api/organization",
            request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var result = await response.ReadWithJson<BadRequestDetails>();

        Assert.NotNull(result);
        Assert.Single(result.ValidationErrors);

        Assert.Equal(
            OrganizationSlug.MaxLengthMessage,
            result.ValidationErrors.First());
    }

    [Fact]
    public async Task Post_organization_with_duplicate_slug_returns_bad_request()
    {
        var request = CreateOrganizationRequest();

        var responseFirst = await Client.PostAsJsonAsync(
            "/api/organization",
            request);

        Assert.Equal(
            HttpStatusCode.Created,
            responseFirst.StatusCode);

        var response = await Client.PostAsJsonAsync(
            "/api/organization",
            request);
        
        Assert.Equal(
            HttpStatusCode.BadRequest,
            response.StatusCode);
    }
    
    [Fact]
    public async Task Post_concurrent_calls_with_same_tax_id_allow_only_one_organization()
    {
        var request =
            CreateOrganizationRequest(slug: "flex-jobs", name: "Flex Jobs");

        var tasks = new[]
        {
            Client.PostAsJsonAsync("/api/organization", request),
            Client.PostAsJsonAsync("/api/organization", request)
        };

        var responses = await Task.WhenAll(tasks);

        Assert.Equal(2, responses.Length);

        Assert.Contains(
            responses,
            response => response.IsSuccessStatusCode);

        Assert.Contains(
            responses,
            response => response.StatusCode == HttpStatusCode.Conflict);

        var conflict = responses.Single(x =>
            x.StatusCode == HttpStatusCode.Conflict);

        var problem = await conflict.Content
            .ReadFromJsonAsync<ProblemDetails>();

        Assert.NotNull(problem);

        switch (problem.Type)
        {
            case nameof(DocumentAlreadyExistsException):
                OutputHelper.WriteLine("Catch database unique constrain");
                Assert.Equal(
                    nameof(DocumentAlreadyExistsException),
                    problem.Type);
                break;
            case nameof(BusinessRuleException):
                OutputHelper.WriteLine("Catch with exists query");
                Assert.Equal(
                    nameof(BusinessRuleException),
                    problem.Type);
                break;
        }
    }
}
