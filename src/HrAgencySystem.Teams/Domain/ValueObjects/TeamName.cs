using HrAgencySystem.SharedKernel.Exception;

namespace HrAgencySystem.Teams.Domain.ValueObjects;

public sealed record TeamName
{
    private const int MaxLength = 100;
    public const string RequiredMessage = "Team name is required.";
    public const string MaxLengthMessage = "Team name cannot exceed 100 characters.";

    private TeamName(string value)
    {
        Value = value;
    }

    public string Value { get; }

    public static TeamName Create(string value)
    {
        var (teamName, error) = TryCreate(value);

        return error is not null ? throw new InValidValueException(error) : teamName!;
    }

    public static (TeamName? teamName, string? error) TryCreate(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return (null, RequiredMessage);

        var normalized = value.Trim();

        if (normalized.Length > MaxLength)
            return (null, MaxLengthMessage);

        return (new TeamName(normalized), null);
    }

    public override string ToString()
    {
        return Value;
    }
}
