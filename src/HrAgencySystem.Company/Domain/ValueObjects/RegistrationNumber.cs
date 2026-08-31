using HrAgencySystem.SharedKernel.Exception;

namespace HrAgencySystem.Company.Domain.ValueObjects;

public sealed record RegistrationNumber
{
    private const int MaxLength = 100;
    public const string RequiredMessage = "Registration number is required.";
    public const string MaxLengthMessage = "Registration number cannot exceed 100 characters.";


    private RegistrationNumber(string value)
    {
        Value = value;
    }

    public string Value { get; }

    public static RegistrationNumber Create(string value)
    {
        var (registrationNumber, error) = TryCreate(value);

        return error is not null ? throw new InValidValueException(error) : registrationNumber!;
    }

    public static (RegistrationNumber? registrationNumber, string? error) TryCreate(
        string value)
    {
        if (string.IsNullOrWhiteSpace(value)) return (null, RequiredMessage);

        var normalized = value.Trim();

        if (normalized.Length > MaxLength) return (null, MaxLengthMessage);

        return (new RegistrationNumber(normalized), null);
    }

    public override string ToString()
    {
        return Value;
    }
}