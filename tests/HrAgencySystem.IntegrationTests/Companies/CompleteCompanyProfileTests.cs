using System.Net;
using System.Net.Http.Json;
using HrAgencySystem.Api.Endpoints.Company.Maps;
using HrAgencySystem.Api.Common.Errors;
using HrAgencySystem.Company.Events;
using HrAgencySystem.Company.Projections;
using HrAgencySystem.IntegrationTests.Infrastructure;
using HrAgencySystem.SharedKernel.ValueObjects;
using HrAgencySystem.SharedKernel.Web.Common;
using Xunit.Abstractions;

namespace HrAgencySystem.IntegrationTests.Company;

[Collection(IntegrationCollection.Name)]
public class CompleteCompanyProfileTests(IntegrationEnvironment env, ITestOutputHelper outputHelper)
    : BaseIntegrationTest(env, outputHelper)
{
    protected override async Task BeforeEachAsync()
    {
        await Cleaner.CleanCompany();
        await Cleaner.CleanCompanyTaxIds();
    }

    [Fact]
    public async Task Put_profile_marks_the_company_as_complete()
    {
        var organizationId = Guid.NewGuid();
        var company = await CompanyClient.CreateAsync(organizationId);
        Client.WithOrganizationId(organizationId);

        var response = await Client.PutAsJsonAsync(
            $"/api/companies/{company.CompanyId}/profile",
            Request()
        );

        var result = await response.ReadWithJson<CompanyProfileUpdated>(OutputHelper);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(result);
        Assert.Equal("ACME Corporation sp. z o.o.", result.Profile.LegalName);

        await Eventually.AssertAsync(async () =>
        {
            var projection = await GetCompany(company.CompanyId);

            Assert.NotNull(projection);
            Assert.True(projection.IsProfileComplete);
            Assert.NotNull(projection.ProfileCompletedAt);
            Assert.Equal("Warszawa", projection.Profile.RegisteredAddress!.City);
            Assert.Equal("PL1234567890", projection.Profile.VatNumber);
        });
    }

    [Fact]
    public async Task Company_created_without_a_profile_is_valid_but_incomplete()
    {
        // The lead path must stay cheap: a name and a tax id are enough to get a company into the
        // pipeline, and nothing about the paperwork may be demanded there.
        var organizationId = Guid.NewGuid();
        var company = await CompanyClient.CreateAsync(organizationId);
        Client.WithOrganizationId(organizationId);

        await Eventually.AssertAsync(async () =>
        {
            var projection = await GetCompany(company.CompanyId);

            Assert.NotNull(projection);
            Assert.False(projection.IsProfileComplete);
            Assert.Null(projection.ProfileCompletedAt);
            Assert.Null(projection.Profile.LegalName);
        });
    }

    [Fact]
    public async Task Put_partial_address_returns_bad_request()
    {
        var organizationId = Guid.NewGuid();
        var company = await CompanyClient.CreateAsync(organizationId);
        Client.WithOrganizationId(organizationId);

        var response = await Client.PutAsJsonAsync(
            $"/api/companies/{company.CompanyId}/profile",
            Request() with
            {
                City = null,
            }
        );

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var problem = await response.ReadWithJson<BadRequestDetails>(OutputHelper);
        Assert.NotNull(problem);
        Assert.Contains(PostalAddress.PartialMessage, problem.ValidationErrors);
    }

    [Fact]
    public async Task Put_profile_from_another_organization_is_forbidden()
    {
        var company = await CompanyClient.CreateAsync(Guid.NewGuid());

        Client.WithOrganizationId(Guid.NewGuid());

        var response = await Client.PutAsJsonAsync(
            $"/api/companies/{company.CompanyId}/profile",
            Request()
        );

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    private async Task<CompanyProjection?> GetCompany(Guid companyId)
    {
        var response = await Client.GetAsync($"/api/companies/{companyId}");
        response.EnsureSuccessStatusCode();

        return await response.ReadWithJson<CompanyProjection>();
    }

    private static MapCompleteProfile.CompleteCompanyProfileRequest Request() =>
        new(
            "ACME Corporation sp. z o.o.",
            "Prosta",
            "51",
            "12",
            "00-838",
            "Warszawa",
            "pl",
            "PL1234567890",
            "PL61 1090 1014 0000 0712 1981 2874",
            "WBKPPLPP",
            new ContactPerson("jan@acme.example.com", "Jan", "Kowalski", "CEO", "+48 600 100 200")
        );
}
