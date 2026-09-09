using HrAgencySystem.Sales.Domain.Opportunity;
using HrAgencySystem.Sales.Events.Opportunity;
using HrAgencySystem.SharedKernel.ValueObjects;
using JasperFx.Events;
using Marten.Events.Projections;

namespace HrAgencySystem.Sales.Projections;

public sealed class PipelineProjection
    : MultiStreamProjection<PipelineStageSummary, string>
{
    public PipelineProjection()
    {
        /*
         * OpportunityCreated
         *
         * Adds the opportunity to:
         * organization + stage + currency
         */
        Identity<OpportunityCreated>(
            @event => CreateId(
                @event.OrganizationId,
                @event.Stage,
                @event.Currency));

        /*
         * OpportunityUpdated
         *
         * The opportunity can change currency.
         *
         * When currency changes, the event affects two buckets:
         *
         * previous currency -> remove
         * current currency  -> add
         *
         * When currency does not change, both identities point
         * to the same bucket and Apply() only adjusts the value.
         */
        Identities<IEvent<OpportunityUpdated>>(
            @event =>
            {
                var e = @event.Data;

                return
                [
                    CreateId(
                        e.OrganizationId,
                        e.Stage,
                        e.PreviousCurrency),

                    CreateId(
                        e.OrganizationId,
                        e.Stage,
                        e.Currency)
                ];
            });

        /*
         * StageChanged
         *
         * The opportunity moves between two stage buckets.
         */
        Identities<IEvent<StageChanged>>(
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

    public PipelineStageSummary Create(StageChanged @event)
    {
        return new PipelineStageSummary
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

    public PipelineStageSummary Create(
        OpportunityUpdated @event)
    {
        return new PipelineStageSummary
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

    public void Apply(
        PipelineStageSummary summary,
        OpportunityCreated @event)
    {
        summary.Stage = @event.Stage;
        summary.CurrencyCode = @event.Currency;
        summary.OrgId = @event.OrganizationId;
        summary.OpportunityCount++;
        summary.TotalExpectedValue += @event.ExpectedValue;
    }

    public void Apply(
        PipelineStageSummary summary,
        OpportunityUpdated @event)
    {
        var previousBucketId = CreateId(
            @event.OrganizationId,
            @event.Stage,
            @event.PreviousCurrency);

        var currentBucketId = CreateId(
            @event.OrganizationId,
            @event.Stage,
            @event.Currency);

        if (previousBucketId == currentBucketId)
        {
            summary.TotalExpectedValue +=
                @event.ExpectedValue - @event.PreviousExpectedValue;

            return;
        }

        if (summary.Id == previousBucketId)
        {
            summary.OpportunityCount--;
            summary.TotalExpectedValue -= @event.PreviousExpectedValue;

            return;
        }

        if (summary.Id == currentBucketId)
        {
            summary.OpportunityCount++;
            summary.TotalExpectedValue += @event.ExpectedValue;

            return;
        }
        throw new InvalidOperationException(
            $"Pipeline summary '{summary.Id}' does not match " +
            $"{nameof(OpportunityUpdated)}. " +
            $"Summary stage: {summary.Stage}, " +
            $"summary currency: {summary.CurrencyCode}, " +
            $"previous currency: {@event.PreviousCurrency}, " +
            $"previous stage: {@event.Stage}, " +
            $"new stage: {@event.Stage}, " +
            $"currency: {@event.Currency}.");
    }

    public void Apply(
        PipelineStageSummary summary,
        StageChanged @event)
    {
        var previousBucketId = CreateId(
            @event.OrganizationId,
            @event.PreviousStage,
            @event.CurrencyCode);

        var currentBucketId = CreateId(
            @event.OrganizationId,
            @event.Stage,
            @event.CurrencyCode);

        if (summary.Id == previousBucketId)
        {
            summary.OpportunityCount--;
            summary.TotalExpectedValue -= @event.ExpectedValue;

            return;
        }

        if (summary.Id == currentBucketId)
        {
            summary.OpportunityCount++;
            summary.TotalExpectedValue += @event.ExpectedValue;

            return;
        }

        throw new InvalidOperationException(
            $"Pipeline summary '{summary.Id}' does not match " +
            $"{nameof(StageChanged)}. " +
            $"Summary stage: {summary.Stage}, " +
            $"summary currency: {summary.CurrencyCode}, " +
            $"previous stage: {@event.PreviousStage}, " +
            $"new stage: {@event.Stage}, " +
            $"currency: {@event.CurrencyCode}.");
    }

    private static string CreateId(
        Guid organizationId,
        OpportunityStage stage,
        CurrencyCode currency)
    {
        return $"{organizationId:N}:{stage}:{currency}";
    }
}