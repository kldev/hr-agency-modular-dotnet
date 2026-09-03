using HrAgencySystem.SharedKernel.Exception;

namespace HrAgencySystem.SharedKernel.ValueObjects;

public sealed record JobLocation
{
    public const int MaxLength = 300;

    public const string MaxLengthMessage =
        "Job location cannot exceed 300 characters.";

    private JobLocation(string value)
    {
        Value = value;
    }

    public string Value { get; }

    public static JobLocation Create(string value)
    {
        var (location, error) = TryCreate(value);

        return error is not null
            ? throw new InValidValueException(error)
            : location!;
    }

    public static (JobLocation? summary, string? error) TryCreate(
        string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return (new JobLocation(string.Empty), null);

        var normalized = value.Trim();

        if (normalized.Length > MaxLength)
            return (null, MaxLengthMessage);

        return (new JobLocation(normalized), null);
    }

    public override string ToString()
    {
        return Value;
    }
}