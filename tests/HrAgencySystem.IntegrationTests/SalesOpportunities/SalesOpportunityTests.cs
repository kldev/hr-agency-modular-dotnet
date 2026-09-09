using System.Net;
using System.Net.Http.Json;
using HrAgencySystem.Api.Common.Errors;
using HrAgencySystem.IntegrationTests.Infrastructure;
using HrAgencySystem.SharedKernel.Exception;
using HrAgencySystem.SharedKernel.ValueObjects;
using Microsoft.AspNetCore.Mvc;
using Xunit.Abstractions;

namespace HrAgencySystem.IntegrationTests.SalesOpportunities;

[Collection(IntegrationCollection.Name)]
public sealed class SalesOpportunityTests(
    IntegrationEnvironment env,
    ITestOutputHelper output)
    : BaseIntegrationTest(env, output)
{
    private readonly Guid _organizationId = Guid.NewGuid();
    private readonly Guid _companyId = Guid.NewGuid();
    private readonly Guid _responsibleId = Guid.NewGuid();
    
    protected override async Task BeforeEachAsync()
    {
        await Cleaner.CleanSales();
    }
    
    [Fact]
    public async Task ShouldCreateOpportunity()
    {
        var date = DateTimeOffset.UtcNow;
        var createdBy = Guid.NewGuid();
        var result = await OpportunityTestClient.Create(
            organizationId: _organizationId, 
            companyId: _companyId,
            title: "FullStack Developer",
            description: "2x fullstack Developer",
            currency: CurrencyCode.PLN,
            expectedCloseDate: date,
            expectedValue: 15_000,
            responsibleId: _responsibleId,
            createdById: createdBy);
        
        Assert.Equal(_organizationId, result.OrganizationId );
        Assert.Equal(_companyId, result.Company.Id );
        Assert.Equal("FullStack Developer", result.Title);
        Assert.Equal("2x fullstack Developer", result.Description);
        Assert.Equal(_responsibleId, result.Responsible.Id);
        Assert.Equal(CurrencyCode.PLN, result.Currency);
        Assert.Equal(15_000, result.ExpectedValue);
        Assert.Equal(date, result.ExpectedCloseDate);
        Assert.Equal(createdBy, result.CreatedBy.Id);
    }
    
    [Fact]
    public async Task ShouldCreateOpportunityWithoutResponsibleId()
    {
        var date = DateTimeOffset.UtcNow;
        var createdBy = Guid.NewGuid();
        var result = await OpportunityTestClient.Create(
            organizationId: _organizationId, 
            companyId: _companyId,
            title: "FullStack Developer",
            description: "2x fullstack Developer",
            currency: CurrencyCode.PLN,
            expectedCloseDate: date,
            expectedValue: 15_000,
            responsibleId: null,
            createdById: createdBy);
        
        Assert.Equal(_organizationId, result.OrganizationId );
        Assert.Equal(_companyId, result.Company.Id );
        Assert.Equal("FullStack Developer", result.Title);
        Assert.Equal("2x fullstack Developer", result.Description);
        Assert.Equal(createdBy, result.Responsible.Id);
        Assert.Equal(CurrencyCode.PLN, result.Currency);
        Assert.Equal(15_000, result.ExpectedValue);
        Assert.Equal(date, result.ExpectedCloseDate);
        Assert.Equal(createdBy, result.CreatedBy.Id);
    }

    [Fact]
    public async Task ShouldUpdateOpportunity()
    {
        var date = DateTimeOffset.UtcNow;
        var createdBy = Guid.NewGuid();
        var result = await OpportunityTestClient.Create(
            organizationId: _organizationId,
            companyId: _companyId,
            title: "FullStack Developer",
            description: "2x fullstack Developer",
            currency: CurrencyCode.PLN,
            expectedCloseDate: date,
            expectedValue: 15_000,
            responsibleId: _responsibleId,
            createdById: createdBy);

        var updatedDate = date.AddHours(Random.Shared.Next(120));
        var modifiedBy = Guid.NewGuid();
        
        var updatedResult = await OpportunityTestClient.Update(
            opportunityId: result.OpportunityId,
            organizationId: _organizationId,
            title: "FullStack Developer + DBA Administrator",
            description: "Needs ASAP. Hot Lead",
            currency: CurrencyCode.EUR,
            expectedCloseDate: updatedDate,
            expectedValue: 5_000,
            modifiedBy: modifiedBy);


        Assert.Equal(_organizationId, updatedResult.OrganizationId);
        Assert.Equal("FullStack Developer + DBA Administrator", updatedResult.Title);
        Assert.Equal("Needs ASAP. Hot Lead", updatedResult.Description);
        Assert.Equal(CurrencyCode.EUR, updatedResult.Currency);
        Assert.Equal(5_000, updatedResult.ExpectedValue);
        Assert.Equal(15_000, updatedResult.PreviousExpectedValue);
        Assert.Equal(updatedDate, updatedResult.ExpectedCloseDate);
        Assert.Equal(modifiedBy, updatedResult.ModifiedBy.Id);
    }
    
    [Fact]
    public async Task ShouldCreateOpportunityWithoutExpectedCloseDate()
    {
        var result = await OpportunityTestClient.Create(
            expectedCloseDate: null);
        
        Assert.Null(result.ExpectedCloseDate);
        
    }
    
    [Fact]
    public async Task ShouldReturnValidationErrorWithExpectedValueInvalid()
    {
        var request = OpportunityTestClient.CreateValidRequest() with
        {
            ExpectedValue = -1
        };

        var response = await Client.PostAsJsonAsync(OpportunityTestClient.BaseUrl, request);
        
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var problem = await response.ReadWithJson<BadRequestDetails>();

        Assert.NotNull(problem);
        Assert.Single(problem.ValidationErrors);
    }
    
    [Fact]
    public async Task ShouldNotUpdateOtherOrganizationOpportunity()
    {
        var result = await OpportunityTestClient.Create(
            organizationId: _organizationId);

        var otherOrganizationId = Guid.NewGuid();

        var request = OpportunityTestClient.CreateValidUpdateRequest();

        Client.WithOrganizationId(otherOrganizationId);
        var response = await Client.PutAsJsonAsync(OpportunityTestClient.BaseUrl + $"/{result.OpportunityId}", request);
       
        
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);

        var problem = await response.ReadWithJson<ProblemDetails>(OutputHelper);
        Assert.NotNull(problem);
        
        Assert.Equal(OrganizationAccessDeniedException.ProblemTitle, problem.Title);
        Assert.Equal(OrganizationAccessDeniedException.ProblemMessage, problem.Detail);
    }
}
