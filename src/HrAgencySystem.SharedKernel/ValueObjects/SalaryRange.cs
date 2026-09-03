using HrAgencySystem.SharedKernel.Exception;

namespace HrAgencySystem.SharedKernel.ValueObjects;

public sealed record SalaryRange
{
    public const string NegativeSalaryMessage =
        "Salary cannot be negative.";

    public const string MinimumExceedsMaximumMessage =
        "Minimum salary cannot exceed maximum salary.";
    
    private SalaryRange(
        decimal min,
        decimal max,
        CurrencyCode currency)
    {
        Min = min;
        Max = max;
        Currency = currency;
    }

    public decimal Min { get; }

    public decimal Max { get; }

    public CurrencyCode Currency { get; }

    public static SalaryRange Create(
        decimal min,
        decimal max,
        CurrencyCode currency)
    {
        var (salaryRange, error) = TryCreate(min, max, currency);

        return error is not null
            ? throw new InValidValueException(error)
            : salaryRange!;
    }

    public static (
        SalaryRange? salaryRange,
        string? error) TryCreate(
        decimal min,
        decimal max,
        CurrencyCode currency)
    {

        if (min < 0 || max < 0)
            return (null, NegativeSalaryMessage);

        if (min> max)
            return (null, MinimumExceedsMaximumMessage);

        return (
            new SalaryRange(
                min,
                max,
                currency),
            null);
    }

    public override string ToString()
    {
        return $"{Min} - {Max} {Currency}";
    }
}