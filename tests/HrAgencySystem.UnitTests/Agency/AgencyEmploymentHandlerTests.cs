using HrAgencySystem.Agency.Application.Employment.ChangeTerms;
using HrAgencySystem.Agency.Application.Employment.Start;
using HrAgencySystem.Agency.Domain.Employment;
using HrAgencySystem.Agency.Events;
using HrAgencySystem.SharedKernel.Exception;
using HrAgencySystem.SharedKernel.ValueObjects;
using Marten;
using NSubstitute;

namespace HrAgencySystem.UnitTests.Agency;

/// <summary>
/// The rate as a term of the contract. Optional, because a rate is often agreed a day after
/// somebody is taken on; validated by <see cref="WorkRate.TryCreate"/> once it is there.
/// </summary>
public class AgencyEmploymentHandlerTests : BaseTest
{
    private static readonly DateOnly StartsOn = new(2026, 1, 1);

    private static readonly RateInput Hourly = new(45m, " pln ", RateUnit.Hourly, RateBasis.Gross);

    private Task<AgencyEmploymentStarted> Start(RateInput? rate, bool mayQuoteRate = true) =>
        StartAgencyEmploymentHandler.Handle(
            new StartAgencyEmployment(
                OrgScenario.OrganizationId,
                OrgScenario.PayrollClerk,
                WorkerContractType.MandateContract,
                StartsOn,
                null,
                rate,
                mayQuoteRate,
                OrgScenario.Ceo
            ),
            OrgScenario.Service(),
            Substitute.For<IDocumentSession>(),
            TestClock,
            CancellationToken.None
        );

    private async Task<AgencyEmploymentTermsChanged> ChangeTerms(
        RateInput? rate,
        bool mayQuoteRate = true,
        RateInput? startedWith = null
    )
    {
        var employment = AgencyEmployment.Empty();
        employment.Apply(await Start(startedWith));

        var (changed, _) = await ChangeAgencyEmploymentTermsHandler.Handle(
            new ChangeAgencyEmploymentTerms(
                OrgScenario.OrganizationId,
                OrgScenario.PayrollClerk,
                WorkerContractType.EmploymentContract,
                StartsOn.AddMonths(1),
                40m,
                rate,
                mayQuoteRate,
                OrgScenario.Ceo
            ),
            employment,
            OrgScenario.Service(),
            TestClock,
            CancellationToken.None
        );

        return changed;
    }

    [Fact]
    public async Task Start_WithARate_CarriesItOnTheEvent()
    {
        var started = await Start(Hourly);

        Assert.Equal(new WorkRate(45m, "PLN", RateUnit.Hourly, RateBasis.Gross), started.Rate);
    }

    /// <summary>Nothing quoted yet is an ordinary state, not a missing field.</summary>
    [Fact]
    public async Task Start_WithoutARate_IsAccepted()
    {
        var started = await Start(null);

        Assert.Null(started.Rate);
    }

    [Fact]
    public async Task Start_WithANegativeRate_IsRefused()
    {
        var error = await Assert.ThrowsAsync<ValidationException>(() =>
            Start(Hourly with { Amount = -1m })
        );

        Assert.Contains(WorkRate.NegativeAmountMessage, error.Message);
    }

    [Fact]
    public async Task Start_WithARateButNoCurrency_IsRefused()
    {
        var error = await Assert.ThrowsAsync<ValidationException>(() =>
            Start(Hourly with { Currency = " " })
        );

        Assert.Contains(WorkRate.CurrencyRequiredMessage, error.Message);
    }

    [Fact]
    public async Task ChangeTerms_WithARate_CarriesItOnTheEvent()
    {
        var changed = await ChangeTerms(Hourly with { Amount = 50m });

        Assert.Equal(new WorkRate(50m, "PLN", RateUnit.Hourly, RateBasis.Gross), changed.Rate);
    }

    [Fact]
    public async Task ChangeTerms_WithANegativeRate_IsRefused()
    {
        var error = await Assert.ThrowsAsync<ValidationException>(() =>
            ChangeTerms(Hourly with { Amount = -0.01m })
        );

        Assert.Contains(WorkRate.NegativeAmountMessage, error.Message);
    }

    /// <summary>The aggregate keeps the terms in force, so a new rate replaces the old one.</summary>
    [Fact]
    public async Task ChangeTerms_ReplacesTheRateOnTheAggregate()
    {
        var employment = AgencyEmployment.Empty();
        employment.Apply(await Start(Hourly));
        employment.Apply(await ChangeTerms(null));

        Assert.Null(employment.Rate);
    }

    [Fact]
    public async Task Start_WithARate_ByARoleNotShownPay_IsRefused()
    {
        var error = await Assert.ThrowsAsync<BusinessRuleException>(() =>
            Start(Hourly, mayQuoteRate: false)
        );

        Assert.Equal(StartAgencyEmploymentHandler.RateNotYoursMessage, error.Message);
    }

    [Fact]
    public async Task Start_WithoutARate_ByARoleNotShownPay_IsAccepted()
    {
        var started = await Start(null, mayQuoteRate: false);

        Assert.Null(started.Rate);
    }

    [Fact]
    public async Task ChangeTerms_WithARate_ByARoleNotShownPay_IsRefused()
    {
        var error = await Assert.ThrowsAsync<BusinessRuleException>(() =>
            ChangeTerms(Hourly, mayQuoteRate: false)
        );

        Assert.Equal(StartAgencyEmploymentHandler.RateNotYoursMessage, error.Message);
    }

    /// <summary>
    /// Somebody who cannot see the rate sends none back. Taking that as "no rate" would let a
    /// change of hours erase what the person is paid.
    /// </summary>
    [Fact]
    public async Task ChangeTerms_ByARoleNotShownPay_KeepsTheRateInForce()
    {
        var changed = await ChangeTerms(null, mayQuoteRate: false, startedWith: Hourly);

        Assert.Equal(new WorkRate(45m, "PLN", RateUnit.Hourly, RateBasis.Gross), changed.Rate);
    }
}
