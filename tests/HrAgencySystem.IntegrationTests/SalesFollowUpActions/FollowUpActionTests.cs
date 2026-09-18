using System.Net;
using HrAgencySystem.Api.Common.Errors;
using HrAgencySystem.IntegrationTests.Infrastructure;
using HrAgencySystem.Sales.Documents;
using HrAgencySystem.SharedKernel.ValueObjects;
using Xunit.Abstractions;

namespace HrAgencySystem.IntegrationTests.SalesFollowUpActions;

[Collection(IntegrationCollection.Name)]
public sealed class FollowUpActionTests(
    IntegrationEnvironment env,
    ITestOutputHelper output)
    : BaseIntegrationTest(env, output)
{
    private static readonly DateTimeOffset Tomorrow = new(2026, 10, 1, 9, 0, 0, TimeSpan.Zero);
    private static readonly DateTimeOffset NextWeek = new(2026, 10, 7, 9, 0, 0, TimeSpan.Zero);
    private static readonly DateTimeOffset NextMonth = new(2026, 11, 2, 9, 0, 0, TimeSpan.Zero);

    private readonly Guid _organizationId = Guid.NewGuid();
    private readonly Guid _companyId = Guid.NewGuid();

    protected override async Task BeforeEachAsync()
    {
        await Cleaner.CleanSales();
    }

    private async Task<Guid> AnOpportunity(Guid? organizationId = null)
    {
        var opportunity = await OpportunityTestClient.Create(
            organizationId: organizationId ?? _organizationId,
            companyId: _companyId);

        return opportunity.OpportunityId;
    }

    [Fact]
    public async Task ShouldCreateFollowUpAction()
    {
        var opportunityId = await AnOpportunity();
        var createdBy = Guid.NewGuid();

        var result = await FollowUpActionTestClient.Create(
            organizationId: _organizationId,
            opportunityId: opportunityId,
            content: "Send the offer and call back",
            followDateTime: Tomorrow,
            createdById: createdBy);

        Assert.NotEqual(Guid.Empty, result.FollowUpActionId);
        Assert.Equal(_organizationId, result.OrganizationId);
        Assert.Equal(opportunityId, result.OpportunityId);
        Assert.Equal("Send the offer and call back", result.Content);
        Assert.Equal(Tomorrow, result.FollowDateTime);
        Assert.Equal(createdBy, result.CreatedBy.Id);
    }

    [Fact]
    public async Task ShouldStoreFollowUpActionDocument()
    {
        var opportunityId = await AnOpportunity();

        var created = await FollowUpActionTestClient.Create(
            organizationId: _organizationId,
            opportunityId: opportunityId,
            content: "Send the offer",
            followDateTime: Tomorrow);

        var document = await FollowUpActionTestClient.Get(_organizationId, created.FollowUpActionId);

        Assert.Equal(created.FollowUpActionId, document.Id);
        Assert.Equal(opportunityId, document.OpportunityId);
        Assert.Equal(_organizationId, document.OrganizationId);
        Assert.Equal("Send the offer", document.Content);
        Assert.Equal(Tomorrow, document.FollowDateTime);
        Assert.Equal(_companyId, document.Company.Id);
    }

    [Fact]
    public async Task ShouldNotCreateFollowUpActionWithoutContent()
    {
        var opportunityId = await AnOpportunity();

        var response = await FollowUpActionTestClient.CreateResponse(
            organizationId: _organizationId,
            opportunityId: opportunityId,
            content: "  ",
            followDateTime: Tomorrow);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var problem = await response.ReadWithJson<BadRequestDetails>(OutputHelper);

        Assert.NotNull(problem);
        Assert.Single(problem.ValidationErrors);
        Assert.Equal(LongText.FieldIsRequired(FollowUpAction.ContentFieldName),
            problem.ValidationErrors.First());
    }

    [Fact]
    public async Task ShouldUpdateContentAndFollowDateTime()
    {
        var opportunityId = await AnOpportunity();
        var modifiedBy = Guid.NewGuid();

        var created = await FollowUpActionTestClient.Create(
            organizationId: _organizationId,
            opportunityId: opportunityId,
            content: "Call the client",
            followDateTime: Tomorrow);

        var updated = await FollowUpActionTestClient.Update(
            organizationId: _organizationId,
            followUpActionId: created.FollowUpActionId,
            content: "Client asked to postpone the call",
            followDateTime: NextWeek,
            modifiedById: modifiedBy);

        Assert.Equal(created.FollowUpActionId, updated.FollowUpActionId);
        Assert.Equal(opportunityId, updated.OpportunityId);
        Assert.Equal(_organizationId, updated.OrganizationId);
        Assert.Equal("Client asked to postpone the call", updated.Content);
        Assert.Equal(NextWeek, updated.FollowDateTime);
        Assert.Equal(modifiedBy, updated.ModifiedBy.Id);

        var document = await FollowUpActionTestClient.Get(_organizationId, created.FollowUpActionId);

        Assert.Equal("Client asked to postpone the call", document.Content);
        Assert.Equal(NextWeek, document.FollowDateTime);
        Assert.Equal(created.CreatedAt, document.CreatedAt);
    }

    [Fact]
    public async Task ShouldNotUpdateNotExistingFollowUpAction()
    {
        var response = await FollowUpActionTestClient.UpdateResponse(
            organizationId: _organizationId,
            followUpActionId: Guid.NewGuid(),
            content: "Call the client",
            followDateTime: Tomorrow);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task ShouldNotUpdateFollowUpActionOfOtherOrganization()
    {
        var opportunityId = await AnOpportunity();

        var created = await FollowUpActionTestClient.Create(
            organizationId: _organizationId,
            opportunityId: opportunityId,
            followDateTime: Tomorrow);

        var response = await FollowUpActionTestClient.UpdateResponse(
            organizationId: Guid.NewGuid(),
            followUpActionId: created.FollowUpActionId,
            content: "Hijacked",
            followDateTime: NextWeek);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task ShouldNotReadFollowUpActionOfOtherOrganization()
    {
        var opportunityId = await AnOpportunity();

        var created = await FollowUpActionTestClient.Create(
            organizationId: _organizationId,
            opportunityId: opportunityId,
            followDateTime: Tomorrow);

        var response = await FollowUpActionTestClient.GetResponse(Guid.NewGuid(), created.FollowUpActionId);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task ShouldGetFollowUpActionsByOpportunityOrderedByFollowDateTime()
    {
        var opportunityId = await AnOpportunity();
        var otherOpportunityId = await AnOpportunity();

        var first = await FollowUpActionTestClient.Create(
            organizationId: _organizationId, opportunityId: opportunityId, followDateTime: Tomorrow);

        var second = await FollowUpActionTestClient.Create(
            organizationId: _organizationId, opportunityId: opportunityId, followDateTime: NextMonth);

        await FollowUpActionTestClient.Create(
            organizationId: _organizationId, opportunityId: otherOpportunityId, followDateTime: NextWeek);

        var slice = await FollowUpActionTestClient.GetSliceAsync(
            organizationId: _organizationId, opportunityId: opportunityId);

        Assert.Equal(2, slice.Content.Count);
        Assert.Equal(second.FollowUpActionId, slice.Content[0].Id);
        Assert.Equal(first.FollowUpActionId, slice.Content[1].Id);
    }

    [Fact]
    public async Task ShouldNotGetFollowUpActionsOfOtherOrganization()
    {
        var opportunityId = await AnOpportunity();

        await FollowUpActionTestClient.Create(
            organizationId: _organizationId, opportunityId: opportunityId, followDateTime: Tomorrow);

        var slice = await FollowUpActionTestClient.GetSliceAsync(organizationId: Guid.NewGuid());

        Assert.Empty(slice.Content);
    }

    [Fact]
    public async Task ShouldGetFollowUpActionsByCompanyId()
    {
        var opportunityId = await AnOpportunity();

        await FollowUpActionTestClient.Create(
            organizationId: _organizationId, opportunityId: opportunityId, followDateTime: Tomorrow);

        var slice = await FollowUpActionTestClient.GetSliceAsync(
            organizationId: _organizationId, companyId: _companyId);

        Assert.Single(slice.Content);
        Assert.Equal(_companyId, slice.Content[0].Company.Id);
    }

    [Fact]
    public async Task ShouldTrackLatestFollowUpActionOnOpportunity()
    {
        var opportunityId = await AnOpportunity();

        var created = await FollowUpActionTestClient.Create(
            organizationId: _organizationId,
            opportunityId: opportunityId,
            content: "Call the client",
            followDateTime: Tomorrow);

        await Eventually.AssertAsync(async () =>
        {
            var opportunity = await OpportunityTestClient.Get(_organizationId, opportunityId);

            Assert.Equal(created.FollowUpActionId, opportunity.FollowUpActionId);
            Assert.Equal("Call the client", opportunity.FollowUpContent);
            Assert.Equal(Tomorrow, opportunity.FollowUpDateTime);
        }, output: OutputHelper);
    }

    [Fact]
    public async Task ShouldKeepTheEntryWithTheLatestFollowDateTimeOnOpportunity()
    {
        var opportunityId = await AnOpportunity();

        var latest = await FollowUpActionTestClient.Create(
            organizationId: _organizationId,
            opportunityId: opportunityId,
            content: "Sign the contract",
            followDateTime: NextMonth);

        await FollowUpActionTestClient.Create(
            organizationId: _organizationId,
            opportunityId: opportunityId,
            content: "An entry added for an earlier date",
            followDateTime: Tomorrow);

        await Eventually.AssertAsync(async () =>
        {
            var opportunity = await OpportunityTestClient.Get(_organizationId, opportunityId);

            Assert.Equal(latest.FollowUpActionId, opportunity.FollowUpActionId);
            Assert.Equal("Sign the contract", opportunity.FollowUpContent);
            Assert.Equal(NextMonth, opportunity.FollowUpDateTime);
        }, output: OutputHelper);
    }

    [Fact]
    public async Task ShouldRefreshOpportunityWhenTheTrackedEntryIsEdited()
    {
        var opportunityId = await AnOpportunity();

        var created = await FollowUpActionTestClient.Create(
            organizationId: _organizationId,
            opportunityId: opportunityId,
            content: "Call the client",
            followDateTime: Tomorrow);

        await FollowUpActionTestClient.Update(
            organizationId: _organizationId,
            followUpActionId: created.FollowUpActionId,
            content: "Client asked to postpone the call",
            followDateTime: NextWeek);

        await Eventually.AssertAsync(async () =>
        {
            var opportunity = await OpportunityTestClient.Get(_organizationId, opportunityId);

            Assert.Equal(created.FollowUpActionId, opportunity.FollowUpActionId);
            Assert.Equal("Client asked to postpone the call", opportunity.FollowUpContent);
            Assert.Equal(NextWeek, opportunity.FollowUpDateTime);
        }, output: OutputHelper);
    }

    [Fact]
    public async Task ShouldTakeOverOpportunityWhenAnOlderEntryIsMovedForward()
    {
        var opportunityId = await AnOpportunity();

        var older = await FollowUpActionTestClient.Create(
            organizationId: _organizationId,
            opportunityId: opportunityId,
            content: "Call the client",
            followDateTime: Tomorrow);

        var newer = await FollowUpActionTestClient.Create(
            organizationId: _organizationId,
            opportunityId: opportunityId,
            content: "Sign the contract",
            followDateTime: NextWeek);

        await FollowUpActionTestClient.Update(
            organizationId: _organizationId,
            followUpActionId: older.FollowUpActionId,
            content: "Rescheduled after the signature",
            followDateTime: NextMonth);

        await Eventually.AssertAsync(async () =>
        {
            var opportunity = await OpportunityTestClient.Get(_organizationId, opportunityId);

            Assert.NotEqual(newer.FollowUpActionId, opportunity.FollowUpActionId);
            Assert.Equal(older.FollowUpActionId, opportunity.FollowUpActionId);
            Assert.Equal("Rescheduled after the signature", opportunity.FollowUpContent);
            Assert.Equal(NextMonth, opportunity.FollowUpDateTime);
        }, output: OutputHelper);
    }
}
