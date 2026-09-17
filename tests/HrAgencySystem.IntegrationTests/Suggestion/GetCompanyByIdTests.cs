using System.Net;
using HrAgencySystem.Company.Application.Suggestion;
using HrAgencySystem.IntegrationTests.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Xunit.Abstractions;

namespace HrAgencySystem.IntegrationTests.Suggestion;

[Collection(IntegrationCollection.Name)]
public sealed class GetCompanyByIdTests(IntegrationEnvironment environment, ITestOutputHelper output)
    : BaseIntegrationTest(environment, output)
{
    private readonly Guid OrganizationId = Guid.NewGuid();
    private readonly Guid OtherOrganizationId = Guid.NewGuid();

    protected override async Task BeforeEachAsync()
    {
        await Cleaner.CleanCompany();
    }

    private async Task<HttpResponseMessage> GetSuggestion(Guid companyId)
    {
        return await Client.GetAsync($"/api/suggestion/companies/{companyId}");
    }

    [Fact]
    public async Task ShouldGetCompanySuggestionById()
    {
        var created = await CompanyClient.CreateAsync(OrganizationId, name: "Almec", taxId: "TX-100",
            countryCode: "pl");

        Client.WithOrganizationId(OrganizationId);

        await Eventually.AssertAsync(async () =>
        {
            var response = await GetSuggestion(created.CompanyId);

            response.EnsureSuccessStatusCode();

            var suggestion = await response.ReadWithJson<CompanySuggestion>(OutputHelper);

            Assert.NotNull(suggestion);
            Assert.Equal(created.CompanyId, suggestion.Id);
            Assert.Equal("Almec", suggestion.Name);
            Assert.Equal("TX-100", suggestion.TaxNumber);
        });
    }

    [Fact]
    public async Task ShouldNotGetCompanySuggestionFromAnotherOrganization()
    {
        var created = await CompanyClient.CreateAsync(OtherOrganizationId, name: "Flex Jobs");

        // wait until the projection is there, otherwise the 404 below would pass for the wrong reason
        Client.WithOrganizationId(OtherOrganizationId);

        await Eventually.AssertAsync(async () =>
        {
            var response = await GetSuggestion(created.CompanyId);

            response.EnsureSuccessStatusCode();
        });

        Client.WithOrganizationId(OrganizationId);

        var notFound = await GetSuggestion(created.CompanyId);

        Assert.Equal(HttpStatusCode.NotFound, notFound.StatusCode);

        var problem = await notFound.ReadWithJson<ProblemDetails>();

        Assert.NotNull(problem);
        Assert.Equal((int)HttpStatusCode.NotFound, problem.Status);
    }

    [Fact]
    public async Task ShouldReturnNotFoundForUnknownCompanyId()
    {
        Client.WithOrganizationId(OrganizationId);

        var response = await GetSuggestion(Guid.NewGuid());

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}
