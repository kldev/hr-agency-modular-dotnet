using HrAgencySystem.SharedKernel.Exception;

namespace HrAgencySystem.Recruitment.Domain.Candidate.ValueObjects;

public sealed record CandidatePhoneNumber
{
    public const int MaxLength = 20;

    public const string MaxLengthMessage =
        "Phone number cannot exceed 20 characters.";

    private CandidatePhoneNumber(string value)
    {
        Value = value;
    }

    public string Value { get; }

    public static CandidatePhoneNumber Create(string value)
    {
        var (phone, error) = TryCreate(value);

        return error is not null
            ? throw new InValidValueException(error)
            : phone!;
    }

    public static (CandidatePhoneNumber? summary, string? error) TryCreate(
        string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return (new CandidatePhoneNumber(string.Empty), null);

        var normalized = value.Trim();

        if (normalized.Length > MaxLength)
            return (null, MaxLengthMessage);

        return (new CandidatePhoneNumber(normalized), null);
    }

    public override string ToString()
    {
        return Value;
    }
}