using HrAgencySystem.SharedKernel.Exception;

namespace HrAgencySystem.JobDescription.Domain.ValueObjects;

public sealed record JobTitle
{
    public const int MaxLength = 250;

    public const string RequiredMessage = "Job title is required.";
    public const string MaxLengthMessage =
        "Job title cannot exceed 250 characters.";

    private JobTitle(string value)
    {
        Value = value;
    }

    public string Value { get; }

    public static JobTitle Create(string value)
    {
        var (title, error) = TryCreate(value);

        return error is not null
            ? throw new InValidValueException(error)
            : title!;
    }

    public static (JobTitle? title, string? error) TryCreate(
        string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return (null, RequiredMessage);

        var normalized = value.Trim();

        if (normalized.Length > MaxLength)
            return (null, MaxLengthMessage);

        return (new JobTitle(normalized), null);
    }

    public override string ToString()
    {
        return Value;
    }
}