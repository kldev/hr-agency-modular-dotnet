using HrAgencySystem.SharedKernel.Exception;

namespace HrAgencySystem.Company.Domain.ValueObjects;

/// <summary>
/// An IBAN with an optional BIC.
/// <para>
/// The checksum is not verified. Doing it properly means the mod 97 algorithm plus a per country
/// length table, and getting either slightly wrong rejects valid accounts - a worse failure than
/// accepting a typo that the first transfer would catch anyway.
/// </para>
/// </summary>
public sealed record BankAccount
{
    public const string InvalidIbanMessage =
        "IBAN must be 15-34 characters starting with a two letter country code.";
    public const string InvalidBicMessage = "BIC must be 8 or 11 alphanumeric characters.";
    public const string BicWithoutIbanMessage = "A BIC cannot be given without an IBAN.";

    private BankAccount(string iban, string? bic)
    {
        Iban = iban;
        Bic = bic;
    }

    public string Iban { get; }
    public string? Bic { get; }

    public static BankAccount Create(string? iban, string? bic)
    {
        var (account, error) = TryCreate(iban, bic);

        return error is not null ? throw new InValidValueException(error) : account!;
    }

    /// <returns>A null account with no error when both parts are blank - having no account is normal.</returns>
    public static (BankAccount? value, string? error) TryCreate(string? iban, string? bic)
    {
        var normalizedIban = Normalize(iban);
        var normalizedBic = Normalize(bic);

        if (normalizedIban.Length == 0)
            return normalizedBic.Length == 0 ? (null, null) : (null, BicWithoutIbanMessage);

        if (normalizedIban.Length is < 15 or > 34)
            return (null, InvalidIbanMessage);

        if (
            !char.IsAsciiLetterUpper(normalizedIban[0])
            || !char.IsAsciiLetterUpper(normalizedIban[1])
            || !normalizedIban.All(char.IsAsciiLetterOrDigit)
        )
            return (null, InvalidIbanMessage);

        if (normalizedBic.Length == 0)
            return (new BankAccount(normalizedIban, null), null);

        if (normalizedBic.Length is not (8 or 11) || !normalizedBic.All(char.IsAsciiLetterOrDigit))
            return (null, InvalidBicMessage);

        return (new BankAccount(normalizedIban, normalizedBic), null);
    }

    public override string ToString()
    {
        return Bic is null ? Iban : $"{Iban} ({Bic})";
    }

    private static string Normalize(string? value) =>
        (value ?? "").Replace(" ", "").Replace("-", "").ToUpperInvariant();
}
