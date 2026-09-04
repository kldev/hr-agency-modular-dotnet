using System.Net;
using System.Net.Http.Json;
using HrAgencySystem.Api.Common.Errors;
using HrAgencySystem.Api.Endpoints.Company.Maps;
using HrAgencySystem.Company.Application.Handlers;
using HrAgencySystem.Company.Domain.ValueObjects;
using HrAgencySystem.Company.Events;
using HrAgencySystem.IntegrationTests.Infrastructure;
using HrAgencySystem.SharedKernel.Exception;
using HrAgencySystem.SharedKernel.ValueObjects;
using JasperFx;
using Microsoft.AspNetCore.Mvc;
using Xunit.Abstractions;

namespace HrAgencySystem.IntegrationTests.Company;

[Collection(IntegrationCollection.Name)]
public class CreateCompanyTests(IntegrationEnvironment env, ITestOutputHelper outputHelper)
    : BaseIntegrationTest(env, outputHelper)
{
    protected override async Task BeforeEachAsync()
    {
        await Cleaner.CleanCompanyTaxIds();
    }

    private static MapCreate.CreateCompanyRequest CreateCompanyRequest(
        string name = "Acme z.o.o",
        string countryCode = "pl",
        string taxId = "TX101-101",
        string registrationNumber = "KRS-200"
    )
    {
        return new MapCreate.CreateCompanyRequest(name, countryCode, taxId,
            registrationNumber);
    }


    [Fact]
    public async Task Post_valid_company_creates_company()
    {
        Guid organizationId = Guid.NewGuid();
        Client.WithOrganizationId(organizationId);
        
        var request =
            CreateCompanyRequest();

        var response = await Client.PostAsJsonAsync("/api/companies", request);

        var result = await response.ReadWithJson<CompanyCreated>(OutputHelper);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.NotNull(result);
        Assert.Equal(organizationId, result.OrganizationId);
        Assert.Equal(request.Name, result.Name);
        Assert.Equal(request.TaxId, result.TaxId);
        Assert.Equal(request.RegistrationNumber, result.RegistrationNumber);
    }

    [Fact]
    public async Task Post_company_without_name_returns_bad_request()
    {
        var request =
            CreateCompanyRequest( " ");

        var response = await Client.PostAsJsonAsync("/api/companies", request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var result = await response.ReadWithJson<BadRequestDetails>();

        Assert.NotNull(result);
        Assert.Single(result.ValidationErrors);

        Assert.Equal(CompanyName.RequiredMessage, result.ValidationErrors.First());
    }

    [Fact]
    public async Task Post_company_with_invalid_fields_returns_all_validation_errors()
    {
        var request =
            CreateCompanyRequest(" ", "wrong country", " ", "");

        var response = await Client.PostAsJsonAsync("/api/companies", request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var result = await response.ReadWithJson<BadRequestDetails>();

        Assert.NotNull(result);
        Assert.Equal(4, result.ValidationErrors.Count);

        Assert.Equal(CompanyName.RequiredMessage, result.ValidationErrors.First());

        Assert.Contains(
            CompanyName.RequiredMessage,
            result.ValidationErrors);

        Assert.Contains(
            CountryCode.InvalidFormatMessage,
            result.ValidationErrors);

        Assert.Contains(
            TaxId.RequiredMessage,
            result.ValidationErrors);

        Assert.Contains(
            RegistrationNumber.RequiredMessage,
            result.ValidationErrors);
    }

    [Fact]
    public async Task Post_company_with_duplicate_tax_id_returns_bad_request()
    {
        var request =
            CreateCompanyRequest();
        
       var responseFirst = await Client.PostAsJsonAsync("/api/companies", request);
        
        OutputHelper.WriteLine(await responseFirst.Content.ReadAsStringAsync());
        
        Assert.Equal(HttpStatusCode.Created, responseFirst.StatusCode);
        
        var response = await Client.PostAsJsonAsync("/api/companies", request);
        
        var result = await response.ReadWithJson<ProblemDetails>(OutputHelper);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        Assert.NotNull(result);
        Assert.Equal(CreateCompanyHandler.TaxIdAlreadyExistsMessage, result.Detail);
        Assert.Equal(nameof(BusinessRuleException), result.Type);
    }

    [Fact]
    public async Task Post_same_tax_id_is_allowed_for_different_tenants()
    {
        var organizationIdA = Guid.NewGuid();
        var organizationIdB = Guid.NewGuid();

        Client.WithOrganizationId(organizationIdA);
        var responseA = await Client.PostAsJsonAsync("/api/companies", CreateCompanyRequest());
        
        Client.WithOrganizationId(organizationIdB);
        var responseB = await Client.PostAsJsonAsync("/api/companies", CreateCompanyRequest());

        Assert.Equal(HttpStatusCode.Created, responseA.StatusCode);
        Assert.Equal(HttpStatusCode.Created, responseB.StatusCode);
    }


    [Fact]
    public async Task Post_concurrent_calls_with_same_tax_id_allow_only_one_company()
    {
        var organizationId = Guid.NewGuid();
        Client.WithOrganizationId(organizationId);
        
        var request =
            CreateCompanyRequest();

        var tasks = new[]
        {
            Client.PostAsJsonAsync("/api/companies", request),
            Client.PostAsJsonAsync("/api/companies", request)
        };

        var responses = await Task.WhenAll(tasks);

        Assert.Equal(2, responses.Length);

        Assert.Contains(
            responses,
            response => response.IsSuccessStatusCode);

        Assert.Contains(
            responses,
            response => response.StatusCode is HttpStatusCode.Conflict or HttpStatusCode.BadRequest);

        var conflict = responses.Single(x =>
            x.StatusCode is HttpStatusCode.Conflict or HttpStatusCode.BadRequest);

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