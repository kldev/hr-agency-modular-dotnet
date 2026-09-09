using HrAgencySystem.Sales.Domain.Opportunity;
using HrAgencySystem.Sales.Events.Opportunity;
using HrAgencySystem.SharedKernel.ValueObjects;
using JasperFx.Events;
using Marten.Events.Projections;

namespace HrAgencySystem.Sales.Projections;

public sealed class SalesPipelineProjection
    : MultiStreamProjection<SalesPipelineStageSummary, string>
{
    public SalesPipelineProjection()
    {
        /*
         * Created:
         *
         * One opportunity belongs to exactly one
         * organization/stage/currency bucket.
         */
        Identity<SalesOpportunityCreated>(
            @event => CreateId(
                @event.OrganizationId,
                @event.Stage,
                @event.Currency));

        /*
         * Updated:
         *
         * The event contains the current stage and organization.
         * Currency is assumed to be immutable after creation.
         */
        Identity<SalesOpportunityUpdated>(
            @event => CreateId(
                @event.OrganizationId,
                @event.Stage,
                @event.CurrencyCode));

        /*
         * StageChanged:
         *
         * One event modifies TWO pipeline buckets:
         *
         * PreviousStage -> remove opportunity/value
         * New Stage     -> add opportunity/value
         */
        Identities<IEvent<SalesOpportunityStageChanged>>(
            @event =>
            {
                var e = @event.Data;

                return
                [
                    CreateId(
                        e.OrganizationId,
                        e.PreviousStage,
                        e.CurrencyCode),

                    CreateId(
                        e.OrganizationId,
                        e.Stage,
                        e.CurrencyCode)
                ];
            });

        Options.CacheLimitPerTenant = 1000;
    }

    /*
     * First event for a pipeline bucket.
     */
    public SalesPipelineStageSummary Create(
        SalesOpportunityCreated @event)
    {
        return new SalesPipelineStageSummary
        {
            Id = CreateId(
                @event.OrganizationId,
                @event.Stage,
                @event.Currency),

            OrgId = @event.OrganizationId,
            Stage = @event.Stage,
            CurrencyCode = @event.Currency,

            OpportunityCount = 1,
            TotalExpectedValue = @event.ExpectedValue
        };
    }

    /*
     * This is normally called when the bucket already exists.
     *
     * If the bucket does not exist while rebuilding the projection,
     * this creates the bucket with the current opportunity.
     */
    public SalesPipelineStageSummary Create(
        SalesOpportunityUpdated @event)
    {
        return new SalesPipelineStageSummary
        {
            Id = CreateId(
                @event.OrganizationId,
                @event.Stage,
                @event.CurrencyCode),

            OrgId = @event.OrganizationId,
            Stage = @event.Stage,
            CurrencyCode = @event.CurrencyCode,

            OpportunityCount = 1,
            TotalExpectedValue = @event.ExpectedValue
        };
    }

    /*
     * Created is handled only when this is the first event
     * for a bucket. Additional Created events for the same
     * bucket are applied here.
     */
    public void Apply(
        SalesPipelineStageSummary summary,
        SalesOpportunityCreated @event)
    {
        summary.OpportunityCount++;
        summary.TotalExpectedValue += @event.ExpectedValue;
    }

    /*
     * ExpectedValue changed, but the opportunity remains
     * in the same stage.
     */
    public void Apply(
        SalesPipelineStageSummary summary,
        SalesOpportunityUpdated @event)
    {
        summary.TotalExpectedValue +=
            @event.ExpectedValue - @event.PreviousExpectedValue;
    }

    /*
     * StageChanged is routed to TWO summaries:
     *
     * 1. PreviousStage
     * 2. Stage
     *
     * We determine which side we are applying by comparing
     * the summary's Stage with the event's PreviousStage.
     */
    public void Apply(
        SalesPipelineStageSummary summary,
        SalesOpportunityStageChanged @event)
    {
        if (summary.Stage == @event.PreviousStage)
        {
            summary.OpportunityCount--;
            summary.TotalExpectedValue -= @event.ExpectedValue;

            return;
        }

        if (summary.Stage == @event.Stage)
        {
            summary.OpportunityCount++;
            summary.TotalExpectedValue += @event.ExpectedValue;

            return;
        }

        throw new InvalidOperationException(
            $"Pipeline summary '{summary.Id}' does not match " +
            $"stage change {nameof(SalesOpportunityStageChanged)}. " +
            $"Summary stage: {summary.Stage}, " +
            $"previous stage: {@event.PreviousStage}, " +
            $"new stage: {@event.Stage}.");
    }

    private static string CreateId(
        Guid organizationId,
        SalesOpportunityStage stage,
        CurrencyCode currencyCode)
    {
        return $"{organizationId:N}:{stage}:{currencyCode}";
    }
}
