using HrAgencySystem.SharedKernel.Exception;

namespace HrAgencySystem.JobDescription.Domain.ValueObjects;

public sealed record JobSummary
{
    public const int MaxLength = 4000;

    public const string MaxLengthMessage =
        "Job summary cannot exceed 4000 characters.";

    private JobSummary(string value)
    {
        Value = value;
    }

    public string Value { get; }

    public static JobSummary Create(string value)
    {
        var (summary, error) = TryCreate(value);

        return error is not null
            ? throw new InValidValueException(error)
            : summary!;
    }

    public static (JobSummary? summary, string? error) TryCreate(
        string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return (new JobSummary(string.Empty), null);

        var normalized = value.Trim();

        if (normalized.Length > MaxLength)
            return (null, MaxLengthMessage);

        return (new JobSummary(normalized), null);
    }

    public override string ToString()
    {
        return Value;
    }
}