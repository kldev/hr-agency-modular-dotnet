using System.Net.Mail;

namespace HrAgencySystem.SharedKernel.ValueObjects;

using HrAgencySystem.SharedKernel.Exception;



public sealed record Email
{
    private const int MaxLength = 320;

    public const string RequiredMessage = "Email is required.";
    public const string MaxLengthMessage =
        "Email cannot exceed 320 characters.";
    public const string InvalidEmail =
        "Invalid email address";

    private Email(string value)
    {
        Value = value;
    }

    public string Value { get; }

    public static Email Create(string value)
    {
        var (email, error) = TryCreate(value);

        return error is not null
            ? throw new InValidValueException(error)
            : email!;
    }

    public static (Email? email, string? error) TryCreate(
        string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return (null, RequiredMessage);

        var normalized = value.Trim().ToLowerInvariant();

        if (normalized.Length > MaxLength)
            return (null, MaxLengthMessage);

        try
        {
            // ReSharper disable once ObjectCreationAsStatement
            new MailAddress(normalized);
        }
        catch (FormatException)
        {
            return (null, InvalidEmail);
        }

        return (new Email(normalized), null);
    }

    public override string ToString()
        => Value;
}