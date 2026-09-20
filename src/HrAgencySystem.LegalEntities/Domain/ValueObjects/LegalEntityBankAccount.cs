using HrAgencySystem.SharedKernel.ValueObjects;

namespace HrAgencySystem.LegalEntities.Domain.ValueObjects;

/// <summary>
/// What the account is for. Money coming in from clients and money going out are not the same
/// account in any agency that separates them, and an invoice has to quote the first one.
/// </summary>
public enum BankAccountPurpose
{
    /// <summary>Client payments to us. This is the number that goes on an invoice.</summary>
    Incoming,

    /// <summary>Payouts we make - wages, contractors, suppliers.</summary>
    Outgoing,
}

/// <summary>
/// One account of one of our entities, identified by what it is for and in which currency.
/// <para>
/// The IBAN's checksum is not verified, for the same reason the client side does not verify it:
/// the mod 97 algorithm plus a per country length table, slightly wrong, rejects valid accounts -
/// a worse failure than a typo the first transfer would catch.
/// </para>
/// </summary>
public sealed record LegalEntityBankAccount(
    BankAccountPurpose Purpose,
    CurrencyCode Currency,
    string Iban,
    string? Bic,
    string? BankName
)
{
    public const int BankNameMaxLength = 200;

    public const string InvalidIbanMessage =
        "IBAN must be 15-34 characters starting with a two letter country code.";

    public const string InvalidBicMessage = "BIC must be 8 or 11 alphanumeric characters.";

    public const string BankNameMaxLengthMessage = "Bank name cannot exceed 200 characters.";

    public const string DuplicateMessage =
        "There can be only one account per purpose and currency.";

    public static (LegalEntityBankAccount? account, List<string> errors) TryCreate(
        BankAccountPurpose purpose,
        CurrencyCode currency,
        string? iban,
        string? bic,
        string? bankName
    )
    {
        var errors = new List<string>();

        var normalizedIban = Normalize(iban);
        var normalizedBic = Normalize(bic);

        if (
            normalizedIban.Length is < 15 or > 34
            || !char.IsAsciiLetterUpper(normalizedIban.ElementAtOrDefault(0))
            || !char.IsAsciiLetterUpper(normalizedIban.ElementAtOrDefault(1))
            || !normalizedIban.All(char.IsAsciiLetterOrDigit)
        )
            errors.Add(InvalidIbanMessage);

        if (
            normalizedBic.Length > 0
            && (
                normalizedBic.Length is not (8 or 11)
                || !normalizedBic.All(char.IsAsciiLetterOrDigit)
            )
        )
            errors.Add(InvalidBicMessage);

        var trimmedBankName = bankName?.Trim();

        if (trimmedBankName is { Length: > BankNameMaxLength })
            errors.Add(BankNameMaxLengthMessage);

        if (errors.Count > 0)
            return (null, errors);

        return (
            new LegalEntityBankAccount(
                purpose,
                currency,
                normalizedIban,
                normalizedBic.Length == 0 ? null : normalizedBic,
                string.IsNullOrWhiteSpace(trimmedBankName) ? null : trimmedBankName
            ),
            []
        );
    }

    private static string Normalize(string? value) =>
        (value ?? "").Replace(" ", "").Replace("-", "").ToUpperInvariant();
}
