using HrAgencySystem.SharedKernel.Exception;

namespace HrAgencySystem.Recruitment.Domain.Posting.ValueObjects;

public sealed record PostTitle
{
    public const int MaxLength = 250;

    public const string RequiredMessage = "Post title is required.";
    public const string MaxLengthMessage =
        "Post title cannot exceed 250 characters.";

    private PostTitle(string value)
    {
        Value = value;
    }

    public string Value { get; }

    public static PostTitle Create(string value)
    {
        var (title, error) = TryCreate(value);

        return error is not null
            ? throw new InValidValueException(error)
            : title!;
    }

    public static (PostTitle? title, string? error) TryCreate(
        string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return (null, RequiredMessage);

        var normalized = value.Trim();

        if (normalized.Length > MaxLength)
            return (null, MaxLengthMessage);

        return (new PostTitle(normalized), null);
    }

    public override string ToString()
    {
        return Value;
    }
}