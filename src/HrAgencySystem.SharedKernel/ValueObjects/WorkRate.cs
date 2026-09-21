using HrAgencySystem.SharedKernel.Exception;

namespace HrAgencySystem.SharedKernel.ValueObjects;

/// <summary>How a rate is quoted. An agency says "32 per hour", not "6000-9000 a month".</summary>
public enum RateUnit
{
    Hourly,
    Daily,
    Monthly,
}

/// <summary>
/// Before or after tax. Without it the number means nothing to the person being offered it, and
/// two people comparing offers are comparing different things.
/// </summary>
public enum RateBasis
{
    Gross,
    Net,
}

/// <summary>
/// What the position pays, as one figure rather than a range.
/// <para>
/// Deliberately not <c>SalaryRange</c>: a range with a minimum and a maximum is the language of a
/// job advert, where the number is an invitation to talk. A posting agency states the rate - the
/// unit and the basis are part of it, because "32" on its own is not an amount anybody can accept.
/// </para>
/// <para>
/// Shared, because the same figure is quoted twice: the position proposes it and the worker's own
/// contract states what was actually agreed - 5000 gross on the role, 5700 and 5550 on two
/// people's contracts.
/// </para>
/// </summary>
public sealed record WorkRate
{
    public const string NegativeAmountMessage = "A rate cannot be negative.";

    public const string CurrencyRequiredMessage = "Currency is required for a rate.";

    private WorkRate(decimal amount, string currency, RateUnit unit, RateBasis basis)
    {
        Amount = amount;
        Currency = currency;
        Unit = unit;
        Basis = basis;
    }

    public decimal Amount { get; }

    /// <summary>ISO 4217, upper cased. Kept as a string because that is what the events carry.</summary>
    public string Currency { get; }

    public RateUnit Unit { get; }

    public RateBasis Basis { get; }

    public static WorkRate Create(decimal amount, string currency, RateUnit unit, RateBasis basis)
    {
        var (rate, error) = TryCreate(amount, currency, unit, basis);

        return error is not null ? throw new InValidValueException(error) : rate!;
    }

    public static (WorkRate? rate, string? error) TryCreate(
        decimal amount,
        string currency,
        RateUnit unit,
        RateBasis basis
    )
    {
        if (amount < 0)
            return (null, NegativeAmountMessage);

        if (string.IsNullOrWhiteSpace(currency))
            return (null, CurrencyRequiredMessage);

        return (new WorkRate(amount, currency.Trim().ToUpperInvariant(), unit, basis), null);
    }
}
