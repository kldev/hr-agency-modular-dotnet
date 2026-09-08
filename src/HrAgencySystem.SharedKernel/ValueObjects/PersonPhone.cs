using HrAgencySystem.SharedKernel.Exception;

namespace HrAgencySystem.SharedKernel.ValueObjects;

public sealed record PersonPhone
{
    public const int MaxLength = 40;

    public const string MaxLengthMessage =
        "Phone number cannot exceed 40 characters.";

    private PersonPhone(string value)
    {
        Value = value;
    }

    public string Value { get; }

    public static PersonPhone Create(string value)
    {
        var (phone, error) = TryCreate(value);

        return error is not null
            ? throw new InValidValueException(error)
            : phone!;
    }

    public static (PersonPhone? phone, string? error) TryCreate(
        string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return (new PersonPhone(string.Empty), null);

        var normalized = value.Trim();

        if (normalized.Length > MaxLength)
            return (null, MaxLengthMessage);

        return (new PersonPhone(normalized), null);
    }

    public override string ToString()
    {
        return Value;
    }
}