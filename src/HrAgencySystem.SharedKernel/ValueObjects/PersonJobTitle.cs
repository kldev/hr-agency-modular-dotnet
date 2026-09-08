using HrAgencySystem.SharedKernel.Exception;

namespace HrAgencySystem.SharedKernel.ValueObjects;

public sealed record PersonJobTitle
{
    private const int MaxLength = 200;
    
    private const string MaxLengthMessage =
        "Job title cannot exceed 200 characters.";
    
    private const string RequiredMessage = "Job title is required.";

    private PersonJobTitle(string value)
    {
        Value = value;
    }

    public string Value { get; }

    public static PersonJobTitle Create(string value, bool isRequired = false)
    {
        var (jobTitle, error) = TryCreate(value, false);

        return error is not null
            ? throw new InValidValueException(error)
            : jobTitle!;
    }

    public static (PersonJobTitle? jobTitle, string? error) TryCreate(
        string? value, bool isRequired = false)
    {
        if (string.IsNullOrWhiteSpace(value))
            return isRequired ? (null, RequiredMessage) :
                (new PersonJobTitle(string.Empty), null);

        var normalized = value.Trim();

        if (normalized.Length > MaxLength)
            return (null, MaxLengthMessage);

        return (new PersonJobTitle(normalized), null);
    }

    public override string ToString()
    {
        return Value;
    }
}