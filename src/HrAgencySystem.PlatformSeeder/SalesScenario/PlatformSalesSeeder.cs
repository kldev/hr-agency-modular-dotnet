using System.Diagnostics;
using Bogus;
using HrAgencySystem.Company.Projections;
using HrAgencySystem.Identity.Projections;
using HrAgencySystem.Sales.Application.Activities.Create;
using HrAgencySystem.Sales.Application.Opportunities.ChangeStage;
using HrAgencySystem.Sales.Application.Opportunities.Create;
using HrAgencySystem.Sales.Domain.Activity;
using HrAgencySystem.Sales.Domain.Opportunity;
using HrAgencySystem.Sales.Events.Opportunity;
using HrAgencySystem.Sales.Services;
using HrAgencySystem.SharedKernel.Tenant;
using HrAgencySystem.SharedKernel.Time;
using HrAgencySystem.SharedKernel.ValueObjects;
using JasperFx.Events;
using Marten;
using Microsoft.Extensions.Logging;

namespace HrAgencySystem.PlatformSeeder.SalesScenario;

// ReSharper disable once ClassNeverInstantiated.Global
public sealed class PlatformSalesSeeder(
    IDocumentSession session,
    ISalesService service,
    ILogger<PlatformSalesSeeder> logger
    ) : IPlatformSalesSeeder
{
    private static readonly Faker Faker = new();
    private CancellationToken Ct { get; set; }
    private int Total { get; set; } = 0;
    private int AggregateLoadTotal { get; set; } = 0;

    public async Task Seed(
        int opportunityCount = 500,
        string slug = "hr-agency", CancellationToken cts = default)
    {
        Ct = cts;
        var stopwatch = Stopwatch.StartNew();

        logger.LogInformation("Starting HR Agency sales seeding");
        
        var organizationId = await service.GetBySlugAsync(slug, Ct);

        var userIds = await GetUserIds(organizationId);
        var companyIds = await GetCompanies(organizationId);

        if (userIds.Count == 0)
            throw new InvalidOperationException(
                $"No users found for organization {organizationId.Value}.");

        if (companyIds.Count == 0)
            throw new InvalidOperationException(
                $"No companies found for organization {organizationId.Value}.");

        for (var i = 0; i < opportunityCount; i++)
        {
            var createdBy = userIds[Random.Shared.Next(userIds.Count)];
            var companyId = companyIds[Random.Shared.Next(companyIds.Count)];

            await CreateOpportunityWithActivities(
                organizationId,
                createdBy,
                companyId,
                userIds);
        }
        
        logger.LogInformation("Total added: {Total}.", Total);
        logger.LogInformation("Aggregate loaded: {AggregateLoadTotal}.", AggregateLoadTotal);
        
        logger.LogInformation(
            "HR Agency sales seeding completed in {Elapsed}",
            stopwatch.Elapsed);
    }

    private async Task<IReadOnlyList<Guid>> GetUserIds(
        OrganizationId organizationId)
    {
        return await session.Query<UserProjection>()
            .Where(x => x.OrganizationId == organizationId.Value)
            .Select(x => x.Id)
            .ToListAsync(Ct);
    }

    private async Task<IReadOnlyList<Guid>> GetCompanies(
        OrganizationId organizationId)
    {
        return await session.Query<CompanyProjection>()
            .Where(x => x.OrganizationId == organizationId.Value)
            .Select(x => x.Id)
            .ToListAsync(Ct);
    }

    private async Task CreateOpportunityWithActivities(
        OrganizationId organizationId,
        Guid createdBy,
        Guid companyId,
        IReadOnlyList<Guid> userIds)
    {
        var now = DateTimeOffset.UtcNow;

        var createdAt = RandomDate(
            now.AddMonths(-3),
            now);

        var clock = new FixedClock(createdAt);

        var responsibleId = Random.Shared.NextDouble() < 0.15
            ? (Guid?)null
            : userIds[Random.Shared.Next(userIds.Count)];

        var command = new CreateOpportunity(
            OrganizationId: organizationId.Value,
            CompanyId: companyId,
            Title: GenerateOpportunityTitle(),
            Description: GenerateOpportunityDescription(),
            ExpectedValue: GenerateExpectedValue(),
            IsHotLead: Random.Shared.NextDouble() < 0.2,
            Currency: RandomCurrency(),
            ExpectedCloseDate: GenerateExpectedCloseDate(createdAt),
            ResponsibleId: responsibleId,
            CreatedBy: createdBy);

        await session.SaveChangesAsync(Ct);

        try
        {
            var opportunity = await CreateOpportunityHandler.Handle(
                command,
                service,
                session,
                clock,
                Ct);

            await session.SaveChangesAsync(Ct);

            /*
             * Najpierw generujemy historię zmian pipeline.
             *
             * OpportunityCreated = New
             *
             * Następnie np.:
             *
             * New
             *   ↓
             * Viewed
             *   ↓
             * Contacted
             *   ↓
             * Qualified
             *   ↓
             * Proposal
             *   ↓
             * Won
             */
            await CreateStageHistory(
                organizationId,
                opportunity,
                createdAt,
                now,
                createdBy);

            await session.SaveChangesAsync(Ct);

            await CreateActivities(
                organizationId,
                opportunity,
                userIds,
                createdAt,
                now);

            await session.SaveChangesAsync(Ct);
            Total++;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, ex.Message);
        }
    }

    private async Task CreateStageHistory(
        OrganizationId organizationId,
        OpportunityCreated opportunity,
        DateTimeOffset createdAt,
        DateTimeOffset now,
        Guid modifiedBy)
    {
        var stages = GeneratePipelineStages();

        var previousDate = createdAt;

        foreach (var stage in stages)
        {
            /*
             * Każda zmiana musi nastąpić po poprzedniej.
             *
             * Minimum 1 godzina, maksimum kilka/kilkanaście dni.
             */
            var stageDate = RandomDate(
                previousDate.AddHours(1),
                now);

            /*
             * Nie pozwalamy, żeby losowanie daty cofnęło historię.
             */
            if (stageDate <= previousDate)
                stageDate = previousDate.AddHours(1);

            /*
             * Jeżeli wyszliśmy poza "now", kończymy historię.
             */
            if (stageDate > now)
                break;

            var lostReason = stage == OpportunityStage.Lost
                ? GenerateLostReason()
                : string.Empty;

            var command = new ChangeOpportunityStage(
                OpportunityId: opportunity.OpportunityId,
                OrganizationId: organizationId.Value,
                Stage: stage,
                LostReason: lostReason,
                ModifiedBy: modifiedBy);

            var clock = new FixedClock(stageDate);

            var allEvents = await session.Events.FetchStreamAsync(command.OpportunityId, token: Ct );

            logger.LogInformation(
                "Opportunity {OpportunityId} has {EventCount} events. Last event: {LastEvent}",
                command.OpportunityId,
                allEvents.Count,
                allEvents.LastOrDefault()?.Data?.GetType().Name);
            
            var opportunityAggregate = SalesOpportunity.Empty();
            foreach (var eventData in allEvents)
            {
                switch (eventData.Data)
                {
                    case OpportunityCreated created:
                        opportunityAggregate.Apply(created);
                        break;
                    
                    case OpportunityUpdated updated:
                        opportunityAggregate.Apply(updated);
                        break;

                    case StageChanged stageChanged:
                        opportunityAggregate.Apply(stageChanged);
                        break;

                    case ResponsiblePersonChanged responsibleChanged:
                        opportunityAggregate.Apply(responsibleChanged);
                        break;

                    // kolejne eventy agregatu...
                }
            }
            
            logger.LogInformation("Aggregate state: " + opportunityAggregate);
            //
            // if (sales == null)
            // {
            //     logger.LogInformation("Could not load aggregate with id " + command.OpportunityId);
            //     
            //     continue;
            // }
            //
            logger.LogInformation("Aggregate loaded successfully with id " + command.OpportunityId);
            AggregateLoadTotal++;
            
           var result = await ChangeOpportunityStageHandler.Handle(
                command,
                opportunityAggregate,
                service,
                clock,
                Ct);

            previousDate = stageDate;
            
            session.Events.Append(
                command.OpportunityId,
                result.Item1);

            await session.SaveChangesAsync(Ct);
            /*
             * Won/Lost są stanami końcowymi.
             */
            if (stage is OpportunityStage.Won or OpportunityStage.Lost)
                break;
        }
    }

    private static IReadOnlyList<OpportunityStage> GeneratePipelineStages()
    {
        /*
         * Część opportunity zostaje na wcześniejszych etapach.
         *
         * Dzięki temu dashboard będzie wyglądał bardziej realistycznie:
         *
         * New       ~15%
         * Viewed    ~15%
         * Contacted ~15%
         * Qualified ~15%
         * Proposal  ~15%
         * Won       ~15%
         * Lost      ~10%
         */

        var targetStage = Random.Shared.Next(0, 7);

        return targetStage switch
        {
            0 =>
            [
                OpportunityStage.Viewed
            ],

            1 =>
            [
                OpportunityStage.Viewed,
                OpportunityStage.Contacted
            ],

            2 =>
            [
                OpportunityStage.Viewed,
                OpportunityStage.Contacted,
                OpportunityStage.Qualified
            ],

            3 =>
            [
                OpportunityStage.Viewed,
                OpportunityStage.Contacted,
                OpportunityStage.Qualified,
                OpportunityStage.Proposal
            ],

            4 =>
            [
                OpportunityStage.Viewed,
                OpportunityStage.Contacted,
                OpportunityStage.Qualified,
                OpportunityStage.Proposal,
                OpportunityStage.Won
            ],

            5 =>
            [
                OpportunityStage.Viewed,
                OpportunityStage.Contacted,
                OpportunityStage.Lost
            ],

            _ =>
            [
                OpportunityStage.Viewed,
                OpportunityStage.Contacted,
                OpportunityStage.Qualified,
                OpportunityStage.Lost
            ]
        };
    }

    private async Task CreateActivities(
        OrganizationId organizationId,
        OpportunityCreated opportunity,
        IReadOnlyList<Guid> userIds,
        DateTimeOffset opportunityCreatedAt,
        DateTimeOffset now)
    {
        var activityCount = Random.Shared.Next(0, 7);

        if (activityCount == 0)
            return;

        for (var i = 0; i < activityCount; i++)
        {
            var activityDate = RandomDate(
                opportunityCreatedAt,
                now);

            var createdBy = userIds[Random.Shared.Next(userIds.Count)];

            var command = new CreateActivity(
                OrganizationId: organizationId.Value,
                SalesOpportunityId: opportunity.OpportunityId,
                ActivityType: RandomActivityType(),
                Note: GenerateActivityNote(),
                CreatedBy: createdBy);

            var clock = new FixedClock(activityDate);

            await CreateActivityHandler.Handle(
                command,
                service,
                session,
                clock,
                Ct);
        }
    }

    private static DateTimeOffset RandomDate(
        DateTimeOffset from,
        DateTimeOffset to)
    {
        var range = to - from;

        if (range <= TimeSpan.Zero)
            return from;

        var randomTicks = (long)(
            Random.Shared.NextDouble() * range.Ticks);

        return from.AddTicks(randomTicks);
    }

    private static DateTimeOffset GenerateExpectedCloseDate(
        DateTimeOffset opportunityCreatedAt)
    {
        var days = Random.Shared.Next(7, 61);

        return opportunityCreatedAt
            .AddDays(days)
            .Date
            .AddHours(Random.Shared.Next(8, 18));
    }

    private static decimal GenerateExpectedValue()
    {
        var values = new[]
        {
            1_500m,
            2_500m,
            5_000m,
            7_500m,
            10_000m,
            15_000m,
            20_000m,
            30_000m,
            32_000m,
            35_000m,
            40_000m,
        };

        return values[Random.Shared.Next(values.Length)];
    }

    private static CurrencyCode RandomCurrency()
    {
        var currencies = new[]
        {
            CurrencyCode.PLN,
            CurrencyCode.EUR,
            CurrencyCode.GBP,
            CurrencyCode.USD
        };

        return currencies[Random.Shared.Next(currencies.Length)];
    }

    private static SalesActivityType RandomActivityType()
    {
        var values = Enum.GetValues<SalesActivityType>();

        return values[Random.Shared.Next(values.Length)];
    }

    private static string GenerateOpportunityTitle()
    {
        var company = Faker.Company.CompanyName();
        var product = Faker.Commerce.ProductName();

        return $"{product} - {company}";
    }

    private static string GenerateOpportunityDescription()
    {
        return Faker.Lorem.Sentence(Random.Shared.Next(8, 18));
    }

    private static string GenerateActivityNote()
    {
        var notes = new[]
        {
            "Initial contact with the customer.",
            "Customer requested additional information.",
            "Follow-up call completed.",
            "Proposal sent to the customer.",
            "Customer requested a revised offer.",
            "Meeting scheduled with the decision maker.",
            "Customer confirmed interest in the offer.",
            "Waiting for customer feedback.",
            "Price negotiation in progress.",
            "Follow-up email sent.",
            "Customer asked about implementation timeline.",
            "Next meeting agreed with the customer.",
            "Discussed customer requirements and business needs.",
            "Technical requirements reviewed with the customer.",
            "Customer requested a detailed pricing breakdown.",
            "Updated proposal prepared and sent.",
            "Customer is reviewing the contract.",
            "Customer requested changes to the contract.",
            "Waiting for management approval.",
            "Customer postponed the decision.",
            "Customer expressed strong interest in moving forward.",
            "Customer needs additional time to make a decision.",
            "Customer asked about integration capabilities.",
            "Customer requested references from similar projects.",
            "References provided to the customer.",
            "Customer requested a follow-up meeting.",
            "Discussed possible implementation dates.",
            "Customer asked for additional pricing options.",
            "Proposal reviewed together with the customer.",
            "Customer requested clarification regarding the scope.",
            "Internal review completed before customer follow-up.",
            "Customer confirmed the main requirements.",
            "Discussed next steps with the customer.",
            "Customer is waiting for internal approval.",
            "Decision postponed due to budget constraints."
        };

        return notes[Random.Shared.Next(notes.Length)];
    }

    private static string GenerateLostReason()
    {
        var reasons = new[]
        {
            "Customer chose a competitor.",
            "Customer decided not to proceed with the project.",
            "Budget was not approved.",
            "Project was postponed indefinitely.",
            "Customer's requirements changed.",
            "Customer decided to handle the project internally.",
            "Price was too high for the customer's budget.",
            "Customer stopped responding.",
            "Project was cancelled by the customer.",
            "Customer selected another provider.",
            "No agreement was reached on commercial terms.",
            "Customer postponed the decision until next year."
        };

        return reasons[Random.Shared.Next(reasons.Length)];
    }
}