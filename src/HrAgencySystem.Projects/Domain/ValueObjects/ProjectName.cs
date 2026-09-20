using HrAgencySystem.SharedKernel.Exception;

namespace HrAgencySystem.Projects.Domain.ValueObjects;

public sealed record ProjectName
{
    private const int MaxLength = 250;

    public const string RequiredMessage = "Project name is required.";
    public const string MaxLengthMessage = "Project name cannot exceed 250 characters.";

    private ProjectName(string value)
    {
        Value = value;
    }

    public string Value { get; }

    public static ProjectName Create(string? value)
    {
        var (name, error) = TryCreate(value);

        return error is not null ? throw new InValidValueException(error) : name!;
    }

    public static (ProjectName? name, string? error) TryCreate(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return (null, RequiredMessage);

        var normalized = value.Trim();

        if (normalized.Length > MaxLength)
            return (null, MaxLengthMessage);

        return (new ProjectName(normalized), null);
    }

    public override string ToString()
    {
        return Value;
    }
}
