using HrAgencySystem.SharedKernel.Exception;

namespace HrAgencySystem.SharedKernel.ValueObjects;

public sealed record LastName
{
    private const int MaxLength = 100;

    public const string RequiredMessage = "Last name is required.";
    public const string MaxLengthMessage =
        "Last name cannot exceed 100 characters.";

    private LastName(string value)
    {
        Value = value;
    }

    public string Value { get; }

    public static LastName Create(string value)
    {
        var (lastName, error) = TryCreate(value);

        return error is not null
            ? throw new InValidValueException(error)
            : lastName!;
    }

    public static (LastName? lastName, string? error) TryCreate(
        string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return (null, RequiredMessage);

        var normalized = value.Trim();

        if (normalized.Length > MaxLength)
            return (null, MaxLengthMessage);

        return (new LastName(normalized), null);
    }

    public override string ToString()
        => Value;
}