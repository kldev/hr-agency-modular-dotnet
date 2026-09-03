using System.Net.Http.Json;
using HrAgencySystem.Api.Endpoints.Company.Maps;
using HrAgencySystem.Company.Events;
using HrAgencySystem.IntegrationTests.Infrastructure;

namespace HrAgencySystem.IntegrationTests.Company;

public sealed class CompanyTestClient(HttpClient client)
{
    private static readonly Random _random = new Random();
    public async Task<CompanyCreated> CreateAsync(
        Guid organizationId,
        string name = "",
        string countryCode = "pl",
        string taxId = "",
        string registrationNumber = "")
    {
        if (string.IsNullOrWhiteSpace(name))
            name  = "Company "  + _random.Next(9999);
        
        if (string.IsNullOrWhiteSpace(taxId))
            taxId  = "TX"  + _random.Next(9999) + "-" + _random.Next(9999);

        if (string.IsNullOrWhiteSpace(registrationNumber))
            registrationNumber  = "REG"  + _random.Next(9999);
        
        var request = new MapCreate.CreateCompanyRequest(name, countryCode, taxId, registrationNumber);
        client.WithOrganizationId(organizationId);
        var response = await client.PostAsJsonAsync(
            "/api/companies", 
            request);

        response.EnsureSuccessStatusCode();

        var result = await response.ReadWithJson<CompanyCreated>();

        Assert.NotNull(result);

        return result;
    }
    
}