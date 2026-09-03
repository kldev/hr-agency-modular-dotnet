using HrAgencySystem.Company.Application.Model;
using HrAgencySystem.IntegrationTests.Infrastructure;
using Xunit.Abstractions;

namespace HrAgencySystem.IntegrationTests.Suggestion;

[Collection(IntegrationCollection.Name)]
public class GetCompaniesTests(IntegrationEnvironment environment, ITestOutputHelper output)
    : BaseIntegrationTest(environment, output)
{
    protected override async Task BeforeEachAsync()
    {
        await Cleaner.CleanCompany();
        await SetupCompanies();
    }

    private readonly Guid OrganizationId = Guid.NewGuid();
    private readonly Guid OtherOrganizationId = Guid.NewGuid();

    private async Task SetupCompanies()
    {
        await CompanyClient.CreateAsync(OrganizationId, name: "Almec");
        await CompanyClient.CreateAsync(OrganizationId, name: "SkyNet", countryCode:"uk", taxId: "SK-100");
        await CompanyClient.CreateAsync(OrganizationId, name: "HR Agency", registrationNumber: "HR-200");

        await CompanyClient.CreateAsync(OtherOrganizationId);
        await CompanyClient.CreateAsync(OtherOrganizationId);
        await CompanyClient.CreateAsync(OtherOrganizationId, name: "Flex Jobs", taxId: "SK-101", registrationNumber: "HR-201");
    }

    private async Task<IReadOnlyList<CompanySuggestion>> GetSuggestions(string search = "", string countryCode = "")
    {
        var url =$"/api/suggestion/companies?search={search}&countryCode={countryCode}" ;
        var response = await Client.GetAsync(url);
        var result = (await response.ReadWithJson<IReadOnlyList<CompanySuggestion>>(OutputHelper))!;
        response.EnsureSuccessStatusCode();
        return result;
    }
    
    [Fact]
    public async Task ShouldGetCompaniesFromOrganization()
    {
        Client.WithOrganizationId(OrganizationId);

        await Eventually.AssertAsync(async () =>
        {
            var result = await GetSuggestions();

            Assert.Equal(3, result.Count);
            Assert.Contains(result, x => x.Name == "Almec");
            Assert.Contains(result, x => x.Name == "SkyNet");
            Assert.Contains(result, x => x.Name == "HR Agency");
            
            Assert.DoesNotContain(result, x => x.Name == "Flex Jobs");
        });
    }
    
    [Fact]
    public async Task ShouldGetCompaniesFilterBySearch()
    {
        Client.WithOrganizationId(OrganizationId);

        await Eventually.AssertAsync(async () =>
        {
            var result = await GetSuggestions(search: "alm");

            Assert.Single(result);
            Assert.Contains(result, x => x.Name == "Almec");
        });
    }
    
    [Fact]
    public async Task ShouldGetCompaniesFilterByTax()
    {
        Client.WithOrganizationId(OrganizationId);

        await Eventually.AssertAsync(async () =>
        {
            var result = await GetSuggestions(search: "SK-10");

            Assert.Single(result);
            Assert.Contains(result, x => x.Name == "SkyNet");
        });
    }
    
    [Fact]
    public async Task ShouldGetCompaniesFilterByCountryCode()
    {
        Client.WithOrganizationId(OrganizationId);

        await Eventually.AssertAsync(async () =>
        {
            var result = await GetSuggestions(countryCode: "uk");

            Assert.Single(result);
            Assert.Contains(result, x => x.Name == "SkyNet");
        });
    }
    
    [Fact]
    public async Task ShouldNotGetCompaniesFromOtherOrganizationWithSearchQuery()
    {
        Client.WithOrganizationId(OrganizationId);

        await Eventually.AssertAsync(async () =>
        {
            var result = await GetSuggestions(search: "Jobs");

            Assert.Empty(result);
        });
    }
}