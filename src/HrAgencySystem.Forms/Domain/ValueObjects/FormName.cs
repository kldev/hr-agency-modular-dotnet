using HrAgencySystem.SharedKernel.Exception;

namespace HrAgencySystem.Forms.Domain.ValueObjects;

public sealed record FormName
{
    public const int MaxLength = 200;

    public const string RequiredMessage = "A form needs a name.";
    public const string MaxLengthMessage = "A form name cannot exceed 200 characters.";

    private FormName(string value) => Value = value;

    public string Value { get; }

    public static FormName Create(string? value)
    {
        var (name, error) = TryCreate(value);

        return error is not null ? throw new InValidValueException(error) : name!;
    }

    public static (FormName? name, string? error) TryCreate(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return (null, RequiredMessage);

        var normalized = value.Trim();

        if (normalized.Length > MaxLength)
            return (null, MaxLengthMessage);

        return (new FormName(normalized), null);
    }

    public override string ToString() => Value;
}
