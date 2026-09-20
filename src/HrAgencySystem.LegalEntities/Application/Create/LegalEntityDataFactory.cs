using HrAgencySystem.LegalEntities.Domain.ValueObjects;
using HrAgencySystem.SharedKernel.Exception;
using HrAgencySystem.SharedKernel.ValueObjects;

namespace HrAgencySystem.LegalEntities.Application.Create;

/// <summary>
/// Everything a legal entity is described by, validated in one pass. Every fault is collected before
/// anything is thrown, so somebody filling the form in is told all of it at once rather than one
/// field per attempt.
/// </summary>
public sealed record LegalEntityData(
    LegalEntityName Name,
    LegalEntityName LegalName,
    TaxId TaxId,
    VatNumber VatNumber,
    PostalAddress RegisteredAddress,
    LongText Description,
    President President,
    IReadOnlyList<LegalEntityBankAccount> BankAccounts
);

public static class LegalEntityDataFactory
{
    public const string ClosedBeforeOpenedMessage =
        "The last day of trading cannot be earlier than the first one.";

    public static LegalEntityData Create(ILegalEntityData data)
    {
        var errors = new List<string>();

        var (name, nameError) = LegalEntityName.TryCreate(data.Name);
        var (legalName, legalNameError) = LegalEntityName.TryCreate(data.LegalName);
        var (taxId, taxIdError) = TaxId.TryCreate(data.TaxId);
        var (vatNumber, vatNumberError) = VatNumber.TryCreate(data.VatNumber);
        var (description, descriptionError) = LongText.TryCreate(data.Description ?? "");

        var (address, addressErrors) = PostalAddress.TryCreate(
            data.Street,
            data.BuildingNumber,
            data.UnitNumber,
            data.PostalCode,
            data.City,
            data.CountryCode
        );

        var (president, presidentErrors) = President.TryCreate(
            data.PresidentFirstName,
            data.PresidentLastName,
            data.PresidentEmail
        );

        if (nameError is not null)
            errors.Add(nameError);
        if (legalNameError is not null)
            errors.Add(legalNameError);
        if (taxIdError is not null)
            errors.Add(taxIdError);
        if (vatNumberError is not null)
            errors.Add(vatNumberError);
        if (descriptionError is not null)
            errors.Add(descriptionError);
        errors.AddRange(addressErrors);
        errors.AddRange(presidentErrors);

        var accounts = new List<LegalEntityBankAccount>();

        foreach (var account in data.BankAccounts)
        {
            var (parsed, accountErrors) = LegalEntityBankAccount.TryCreate(
                account.Purpose,
                account.Currency,
                account.Iban,
                account.Bic,
                account.BankName
            );

            errors.AddRange(accountErrors);

            if (parsed is not null)
                accounts.Add(parsed);
        }

        // One account per purpose and currency: "which account goes on a EUR invoice" has to have
        // exactly one answer, and two rows would make it a guess.
        var distinct = accounts.Select(a => (a.Purpose, a.Currency)).Distinct().Count();

        if (distinct != accounts.Count)
            errors.Add(LegalEntityBankAccount.DuplicateMessage);

        // An entity that stopped trading before it started is a typo, not a company.
        if (data.ActiveTo is not null && data.ActiveTo < data.ActiveFrom)
            errors.Add(ClosedBeforeOpenedMessage);

        if (errors.Count > 0)
            throw new ValidationException(errors);

        return new LegalEntityData(
            name!,
            legalName!,
            taxId!,
            vatNumber!,
            address!,
            description!,
            president!,
            accounts
        );
    }
}
