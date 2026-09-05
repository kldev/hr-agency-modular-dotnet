using HrAgencySystem.SharedKernel.Exception;

namespace HrAgencySystem.SharedKernel.ValueObjects;

public sealed record FirstName
{
    private const int MaxLength = 100;

    public const string RequiredMessage = "First name is required.";

    public const string MaxLengthMessage =
        "First name cannot exceed 100 characters.";

    private FirstName(string value)
    {
        Value = value;
    }

    public string Value { get; }

    public static FirstName Create(string value, bool isRequired = true)
    {
        var (firstName, error) = TryCreate(value, isRequired);

        return error is not null
            ? throw new InValidValueException(error)
            : firstName!;
    }

    public static (FirstName? firstName, string? error) TryCreate(
        string value, bool isRequired = true)
    {

        if (string.IsNullOrWhiteSpace(value))
        {
            return isRequired ? (null, RequiredMessage) : (new FirstName(""), null);
        }


        var normalized = value.Trim();

        if (normalized.Length > MaxLength)
            return (null, MaxLengthMessage);

        return (new FirstName(normalized), null);
    }

    public override string ToString()
        => Value;
}