using HrAgencySystem.SharedKernel.Exception;

namespace HrAgencySystem.JobDescription.Domain.ValueObjects;

public sealed record JobDescriptionText
{
    public const int MaxLength = 4000;

    public const string RequiredMessage = "Job description is required.";
    public const string MaxLengthMessage =
        "Job description cannot exceed 4000 characters.";

    private JobDescriptionText(string value)
    {
        Value = value;
    }

    public string Value { get; }

    public static JobDescriptionText Create(string value)
    {
        var (description, error) = TryCreate(value);

        return error is not null
            ? throw new InValidValueException(error)
            : description!;
    }

    public static (JobDescriptionText? description, string? error) TryCreate(
        string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return (null, RequiredMessage);

        var normalized = value.Trim();

        if (normalized.Length > MaxLength)
            return (null, MaxLengthMessage);

        return (new JobDescriptionText(normalized), null);
    }

    public override string ToString()
    {
        return Value;
    }
}