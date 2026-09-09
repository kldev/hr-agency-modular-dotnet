using HrAgencySystem.IntegrationTests.Infrastructure;
using HrAgencySystem.Sales.Domain.Opportunity;
using HrAgencySystem.Sales.Events.Opportunity;
using HrAgencySystem.Sales.Projections;
using HrAgencySystem.SharedKernel.Snapshots;
using HrAgencySystem.SharedKernel.ValueObjects;
using Marten;
using Microsoft.Extensions.DependencyInjection;
using Xunit.Abstractions;

namespace HrAgencySystem.IntegrationTests.SalesOpportunities.Projections;

[Collection(IntegrationCollection.Name)]
public sealed class PipelineProjectionTests(
    IntegrationEnvironment env,
    ITestOutputHelper output)
    : BaseIntegrationTest(env, output)
{
    [Fact]
    public async Task Should_create_pipeline_bucket_when_opportunity_is_created()
    {
        // Arrange
        var organizationId = Guid.NewGuid();
        var opportunityId = Guid.NewGuid();

        await StoreAsync(
            new OpportunityCreated(
                opportunityId,
                organizationId,
                CreateCompanySnapshot(),
                "Opportunity",
                "Description",
                OpportunityStage.New,
                10_000m,
                CurrencyCode.EUR,
                false,
                null,
                CreateUserSnapshot(),
                DateTimeOffset.UtcNow,
                CreateUserSnapshot()));

        // Act
        await WaitForProjectionAsync();

        // Assert
        var summary = await GetSummaryAsync(
            organizationId,
            OpportunityStage.New,
            CurrencyCode.EUR);

        Assert.NotNull(summary);
        Assert.Equal(organizationId, summary.OrgId);
        Assert.Equal(OpportunityStage.New, summary.Stage);
        Assert.Equal(CurrencyCode.EUR, summary.CurrencyCode);
        Assert.Equal(1, summary.OpportunityCount);
        Assert.Equal(10_000m, summary.TotalExpectedValue);
    }

    [Fact]
    public async Task Should_update_total_value_when_opportunity_is_updated()
    {
        // Arrange
        var organizationId = Guid.NewGuid();
        var opportunityId = Guid.NewGuid();

        await StoreAsync(
            new OpportunityCreated(
                opportunityId,
                organizationId,
                CreateCompanySnapshot(),
                "Opportunity",
                "Description",
                OpportunityStage.New,
                10_000m,
                CurrencyCode.EUR,
                false,
                null,
                CreateUserSnapshot(),
                DateTimeOffset.UtcNow,
                CreateUserSnapshot()));

        await WaitForProjectionAsync();

        // Act
        await StoreAsync(
            new OpportunityUpdated(
                opportunityId,
                organizationId,
                OpportunityStage.New,
                "Updated opportunity",
                "Updated description",
                PreviousExpectedValue: 10_000m,
                ExpectedValue: 15_000m,
                IsHotLead: true,
                PreviousCurrency: CurrencyCode.EUR,
                Currency: CurrencyCode.EUR,
                ExpectedCloseDate: DateTimeOffset.UtcNow.AddDays(30),
                ModifiedBy: CreateUserSnapshot(),
                ModifiedAt: DateTimeOffset.UtcNow));

        await WaitForProjectionAsync();

        // Assert
        var summary = await GetSummaryAsync(
            organizationId,
            OpportunityStage.New,
            CurrencyCode.EUR);

        Assert.NotNull(summary);
        Assert.Equal(1, summary.OpportunityCount);
        Assert.Equal(15_000m, summary.TotalExpectedValue);
    }

    [Fact]
    public async Task Should_move_opportunity_between_stages_when_stage_changes()
    {
        // Arrange
        var organizationId = Guid.NewGuid();
        var opportunityId = Guid.NewGuid();

        await StoreAsync(
            new OpportunityCreated(
                opportunityId,
                organizationId,
                CreateCompanySnapshot(),
                "Opportunity",
                "Description",
                OpportunityStage.New,
                10_000m,
                CurrencyCode.EUR,
                false,
                null,
                CreateUserSnapshot(),
                DateTimeOffset.UtcNow,
                CreateUserSnapshot()));

        await WaitForProjectionAsync();

        // Act
        await StoreAsync(
            new StageChanged(
                opportunityId,
                organizationId,
                PreviousStage: OpportunityStage.New,
                Stage: OpportunityStage.Qualified,
                ChangedBy: CreateUserSnapshot(),
                ChangedAt: DateTimeOffset.UtcNow,
                LostReason: string.Empty,
                ExpectedValue: 10_000m,
                CurrencyCode: CurrencyCode.EUR));

        await WaitForProjectionAsync();

        // Assert
        var previousStage = await GetSummaryAsync(
            organizationId,
            OpportunityStage.New,
            CurrencyCode.EUR);

        var newStage = await GetSummaryAsync(
            organizationId,
            OpportunityStage.Qualified,
            CurrencyCode.EUR);

        Assert.NotNull(previousStage);
        Assert.Equal(0, previousStage.OpportunityCount);
        Assert.Equal(0m, previousStage.TotalExpectedValue);

        Assert.NotNull(newStage);
        Assert.Equal(1, newStage.OpportunityCount);
        Assert.Equal(10_000m, newStage.TotalExpectedValue);
    }

    [Fact]
    public async Task Should_move_opportunity_between_currencies_when_currency_changes()
    {
        // Arrange
        var organizationId = Guid.NewGuid();
        var opportunityId = Guid.NewGuid();

        await StoreAsync(
            new OpportunityCreated(
                opportunityId,
                organizationId,
                CreateCompanySnapshot(),
                "Opportunity",
                "Description",
                OpportunityStage.New,
                10_000m,
                CurrencyCode.EUR,
                false,
                null,
                CreateUserSnapshot(),
                DateTimeOffset.UtcNow,
                CreateUserSnapshot()));

        await WaitForProjectionAsync();

        // Act
        await StoreAsync(
            new OpportunityUpdated(
                opportunityId,
                organizationId,
                OpportunityStage.New,
                "Opportunity",
                "Description",
                PreviousExpectedValue: 10_000m,
                ExpectedValue: 12_000m,
                IsHotLead: true,
                PreviousCurrency: CurrencyCode.EUR,
                Currency: CurrencyCode.PLN,
                ExpectedCloseDate: DateTimeOffset.UtcNow.AddDays(30),
                ModifiedBy: CreateUserSnapshot(),
                ModifiedAt: DateTimeOffset.UtcNow));

        await WaitForProjectionAsync();

        // Assert
        var eur = await GetSummaryAsync(
            organizationId,
            OpportunityStage.New,
            CurrencyCode.EUR);

        var pln = await GetSummaryAsync(
            organizationId,
            OpportunityStage.New,
            CurrencyCode.PLN);

        Assert.NotNull(eur);
        Assert.Equal(0, eur.OpportunityCount);
        Assert.Equal(0m, eur.TotalExpectedValue);

        Assert.NotNull(pln);
        Assert.Equal(1, pln.OpportunityCount);
        Assert.Equal(12_000m, pln.TotalExpectedValue);
    }

    [Fact]
    public async Task Should_update_value_without_changing_bucket_when_currency_is_unchanged()
    {
        // Arrange
        var organizationId = Guid.NewGuid();
        var opportunityId = Guid.NewGuid();

        await StoreAsync(
            new OpportunityCreated(
                opportunityId,
                organizationId,
                CreateCompanySnapshot(),
                "Opportunity",
                "Description",
                OpportunityStage.New,
                10_000m,
                CurrencyCode.EUR,
                false,
                null,
                CreateUserSnapshot(),
                DateTimeOffset.UtcNow,
                CreateUserSnapshot()));

        await WaitForProjectionAsync();

        // Act
        await StoreAsync(
            new OpportunityUpdated(
                opportunityId,
                organizationId,
                OpportunityStage.New,
                "Updated opportunity",
                "Updated description",
                PreviousExpectedValue: 10_000m,
                ExpectedValue: 7_500m,
                IsHotLead: true,
                PreviousCurrency: CurrencyCode.EUR,
                Currency: CurrencyCode.EUR,
                ExpectedCloseDate: DateTimeOffset.UtcNow.AddDays(60),
                ModifiedBy: CreateUserSnapshot(),
                ModifiedAt: DateTimeOffset.UtcNow));

        await WaitForProjectionAsync();

        // Assert
        var summary = await GetSummaryAsync(
            organizationId,
            OpportunityStage.New,
            CurrencyCode.EUR);

        Assert.NotNull(summary);
        Assert.Equal(1, summary.OpportunityCount);
        Assert.Equal(7_500m, summary.TotalExpectedValue);
    }

    [Fact]
    public async Task Should_move_opportunity_when_stage_and_currency_are_changed()
    {
        // Arrange
        var organizationId = Guid.NewGuid();
        var opportunityId = Guid.NewGuid();

        await StoreAsync(
            new OpportunityCreated(
                opportunityId,
                organizationId,
                CreateCompanySnapshot(),
                "Opportunity",
                "Description",
                OpportunityStage.New,
                10_000m,
                CurrencyCode.EUR,
                false,
                null,
                CreateUserSnapshot(),
                DateTimeOffset.UtcNow,
                CreateUserSnapshot()));

        await WaitForProjectionAsync();

        // Act
        await StoreAsync(
            new StageChanged(
                opportunityId,
                organizationId,
                PreviousStage: OpportunityStage.New,
                Stage: OpportunityStage.Qualified,
                ChangedBy: CreateUserSnapshot(),
                ChangedAt: DateTimeOffset.UtcNow,
                LostReason: string.Empty,
                ExpectedValue: 10_000m,
                CurrencyCode: CurrencyCode.EUR));

        await WaitForProjectionAsync();

        await StoreAsync(
            new OpportunityUpdated(
                opportunityId,
                organizationId,
                OpportunityStage.Qualified,
                "Updated opportunity",
                "Updated description",
                PreviousExpectedValue: 10_000m,
                ExpectedValue: 25_000m,
                IsHotLead: true,
                PreviousCurrency: CurrencyCode.EUR,
                Currency: CurrencyCode.PLN,
                ExpectedCloseDate: DateTimeOffset.UtcNow.AddDays(30),
                ModifiedBy: CreateUserSnapshot(),
                ModifiedAt: DateTimeOffset.UtcNow));

        await WaitForProjectionAsync();

        // Assert
        var oldBucket = await GetSummaryAsync(
            organizationId,
            OpportunityStage.Qualified,
            CurrencyCode.EUR);

        var newBucket = await GetSummaryAsync(
            organizationId,
            OpportunityStage.Qualified,
            CurrencyCode.PLN);

        Assert.NotNull(oldBucket);
        Assert.Equal(0, oldBucket.OpportunityCount);
        Assert.Equal(0m, oldBucket.TotalExpectedValue);

        Assert.NotNull(newBucket);
        Assert.Equal(1, newBucket.OpportunityCount);
        Assert.Equal(25_000m, newBucket.TotalExpectedValue);
    }

    private async Task StoreAsync<TEvent>(TEvent @event)
    {
        await using var scope = Services.CreateAsyncScope();

        var store = scope.ServiceProvider
            .GetRequiredService<IDocumentStore>();

        await using var session = store.LightweightSession();

        session.Events.Append(
            Guid.NewGuid(),
            @event!);

        await session.SaveChangesAsync();
    }

    private async Task<PipelineStageSummary?> GetSummaryAsync(
        Guid organizationId,
        OpportunityStage stage,
        CurrencyCode currency)
    {
        await using var scope = Services.CreateAsyncScope();

        var store = scope.ServiceProvider
            .GetRequiredService<IDocumentStore>();

        await using var session = store.QuerySession();

        var id = $"{organizationId:N}:{stage}:{currency}";

        return await session.LoadAsync<PipelineStageSummary>(id);
    }

    private async Task WaitForProjectionAsync()
    {
        await Task.Delay(3000);
    }

    private static CompanySnapshot CreateCompanySnapshot()
    {
        return new CompanySnapshot(
            Guid.NewGuid(),
            "Test Company", "Tax Id");
    }

    private static UserSnapshot CreateUserSnapshot()
    {
        return new UserSnapshot(
            Guid.NewGuid(),
            "John",
            "Doe",
            "john.doe@test.com");
    }
}