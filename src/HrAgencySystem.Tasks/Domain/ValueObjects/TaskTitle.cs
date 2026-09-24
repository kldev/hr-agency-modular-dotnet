using HrAgencySystem.SharedKernel.Exception;

namespace HrAgencySystem.Tasks.Domain.ValueObjects;

public sealed record TaskTitle
{
    public const int MaxLength = 200;

    public const string RequiredMessage = "Task title is required.";

    public const string MaxLengthMessage = "Task title cannot exceed 200 characters.";

    private TaskTitle(string value)
    {
        Value = value;
    }

    public string Value { get; }

    public static TaskTitle Create(string? value)
    {
        var (title, error) = TryCreate(value);

        return error is not null ? throw new InValidValueException(error) : title!;
    }

    public static (TaskTitle? value, string? error) TryCreate(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return (null, RequiredMessage);

        var normalized = value.Trim();

        return normalized.Length > MaxLength
            ? (null, MaxLengthMessage)
            : (new TaskTitle(normalized), null);
    }

    public override string ToString() => Value;
}
