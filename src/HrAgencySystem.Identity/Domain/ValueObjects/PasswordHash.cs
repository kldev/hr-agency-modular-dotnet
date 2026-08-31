using HrAgencySystem.SharedKernel.Exception;

namespace HrAgencySystem.Identity.Domain.ValueObjects;



public sealed record PasswordHash
{
    public const string RequiredMessage = "Password hash is required.";

    private PasswordHash(string value)
    {
        Value = value;
    }

    public string Value { get; }

    public static PasswordHash Create(string value)
    {
        var (passwordHash, error) = TryCreate(value);

        return error is not null
            ? throw new InValidValueException(error)
            : passwordHash!;
    }

    public static (PasswordHash? passwordHash, string? error) TryCreate(
        string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return (null, RequiredMessage);

        return (new PasswordHash(value.Trim()), null);
    }

    public override string ToString()
        => Value;
}