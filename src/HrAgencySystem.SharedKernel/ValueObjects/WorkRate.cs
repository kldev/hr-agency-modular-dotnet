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
/// What the work pays, as one figure rather than a range.
/// <para>
/// Deliberately not <see cref="SalaryRange"/>: a minimum and a maximum are the language of a job
/// advert, where the number is an invitation to talk. A posting agency states the rate - and the
/// unit and the basis are part of it, because "32" on its own is not an amount anybody can accept.
/// </para>
/// <para>
/// Shared, because the same figure is quoted twice: the position proposes it and the worker's own
/// contract states what was actually agreed - 5000 gross on the role, 5700 and 5550 on two
/// people's contracts.
/// </para>
/// <para>
/// A positional record with a public constructor, like <see cref="PostalAddress"/>: it travels
/// inside events and responses, and a private constructor is one System.Text.Json cannot call.
/// Validation lives in <see cref="TryCreate"/>, which is what every caller goes through.
/// </para>
/// </summary>
public sealed record WorkRate(decimal Amount, string Currency, RateUnit Unit, RateBasis Basis)
{
    public const string NegativeAmountMessage = "A rate cannot be negative.";

    public const string CurrencyRequiredMessage = "Currency is required for a rate.";

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
