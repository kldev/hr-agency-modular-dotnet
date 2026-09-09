using HrAgencySystem.IntegrationTests.Infrastructure;
using HrAgencySystem.Sales.Domain.Activity;
using Xunit.Abstractions;

namespace HrAgencySystem.IntegrationTests.SalesActivity;

[Collection(IntegrationCollection.Name)]
public sealed class SalesActivityTests(
    IntegrationEnvironment env,
    ITestOutputHelper output)
    : BaseIntegrationTest(env, output)
{
    private readonly Guid _organizationId = Guid.NewGuid();
    private readonly Guid _opportunityId = Guid.NewGuid();

    protected override async Task BeforeEachAsync()
    {
        await Cleaner.CleanSales();
    }

    [Fact]
    public async Task ShouldCreateActivityLog()
    {
        var createdBy = Guid.NewGuid();
        var result = await SalesActivityTestClient.Create(
            organizationId: _organizationId, 
            opportunityId: _opportunityId,
            type: SalesActivityType.Meeting,
            note: "Meeting was successful",
            createdById: createdBy);
        
        Assert.Equal(_organizationId, result.OrganizationId );
        Assert.Equal(_opportunityId, result.SalesOpportunityId);
        Assert.Equal(SalesActivityType.Meeting, result.ActivityType);
        Assert.Equal("Meeting was successful", result.Note);
        Assert.Equal(createdBy, result.CreatedBy.Id);
    }
    
    [Fact]
    public async Task ShouldGetAllActivities()
    {
        await SalesActivityTestClient.Create(
            organizationId: _organizationId 
        );
        
        await SalesActivityTestClient.Create(
            organizationId: _organizationId 
        );

        await SalesActivityTestClient.Create(
            organizationId: Guid.NewGuid() 
        );

        await Eventually.AssertAsync(async () =>
        {
            var slice = await SalesActivityTestClient.GetSliceAsync(organizationId: _organizationId);
            
            Assert.Equal(2, slice.Content.Count);
        });
    }
    
    [Fact]
    public async Task ShouldNotGetOtherOrganizationActivities()
    {
        var otherOrganization = Guid.NewGuid();
        
        await SalesActivityTestClient.Create(
            organizationId: otherOrganization 
        );
        
        await SalesActivityTestClient.Create(
            organizationId: otherOrganization 
        );

        await SalesActivityTestClient.Create(
            organizationId: _organizationId
        );

        await Eventually.AssertAsync(async () =>
        {
            var slice = await SalesActivityTestClient.GetSliceAsync(organizationId: _organizationId);

            Assert.Single(slice.Content);
            var activity = slice.Content[0];
            Assert.Equal(_organizationId, activity.OrgId);
        });
    }
    
    [Fact]
    public async Task ShouldNotGetOtherOrganizationActivitiesWithOpportunityId()
    {
        var otherOrganization = Guid.NewGuid();
        
        await SalesActivityTestClient.Create(
            organizationId: _organizationId,
            opportunityId: _opportunityId
        );
        
        await Eventually.AssertAsync(async () =>
        {
            var slice = await SalesActivityTestClient.GetSliceAsync(organizationId: otherOrganization, 
                opportunityId: _opportunityId);

            Assert.Empty(slice.Content);
        });
    }
    
    [Fact]
    public async Task ShouldNotGetOtherOrganizationActivitiesWithCompanyId()
    {
        var otherOrganization = Guid.NewGuid();
        
        var created = await SalesActivityTestClient.Create(
            organizationId: _organizationId,
            opportunityId: _opportunityId
        );
        
        await Eventually.AssertAsync(async () =>
        {
            var slice = await SalesActivityTestClient.GetSliceAsync(organizationId: otherOrganization, 
                companyId: created.Company.Id);

            Assert.Empty(slice.Content);
        });
    }
    
    [Fact]
    public async Task ShouldGetActivitiesByOpportunityId()
    {
        await SalesActivityTestClient.Create(
            organizationId: _organizationId,
            opportunityId: _opportunityId
        );
        
        await SalesActivityTestClient.Create(
            organizationId: _organizationId,
            opportunityId: _opportunityId
        );

        await SalesActivityTestClient.Create(
            organizationId: Guid.NewGuid() 
        );
        
        await SalesActivityTestClient.Create(
            organizationId: _organizationId 
        );

        await Eventually.AssertAsync(async () =>
        {
            var slice = await SalesActivityTestClient.GetSliceAsync(organizationId: _organizationId,
                opportunityId: _opportunityId);
            
            Assert.Equal(2, slice.Content.Count);
            
            Assert.All(slice.Content, p =>
            {
                Assert.Equal(p.OpportunityId, _opportunityId);
            });
        });
    }
    
    [Fact]
    public async Task ShouldGetActivitiesByCompanyId()
    {
        var result = await SalesActivityTestClient.Create(
            organizationId: _organizationId,
            opportunityId:_opportunityId
        );
        
        await Eventually.AssertAsync(async () =>
        {
            var slice = await SalesActivityTestClient.GetSliceAsync(organizationId: _organizationId,
                companyId: result.Company.Id);

            Assert.Single(slice.Content);
            
            Assert.All(slice.Content, p =>
            {
                Assert.Equal(p.CompanyId, result.Company.Id);
            });
        });
    }
    
    [Fact]
    public async Task ShouldGetActivitiesOrderByCreatedAt()
    {
        var first = await SalesActivityTestClient.Create(
            organizationId: _organizationId,
            opportunityId:_opportunityId
        );
        
        var second = await SalesActivityTestClient.Create(
            organizationId: _organizationId,
            opportunityId:_opportunityId
        );

        var third = await SalesActivityTestClient.Create(
            organizationId: _organizationId,
            opportunityId:_opportunityId
        );

        await Eventually.AssertAsync(async () =>
        {
            var slice = await SalesActivityTestClient.GetSliceAsync(organizationId: _organizationId);
            
            Assert.Equal(3, slice.Content.Count);
            
           Assert.Equal(third.SalesActivityId,  slice.Content[0].Id);
           Assert.Equal(second.SalesActivityId,  slice.Content[1].Id);
           Assert.Equal(first.SalesActivityId,  slice.Content[2].Id);
        });
    }
}