using HrAgencySystem.Company.Domain;
using HrAgencySystem.Company.Domain.ValueObjects;
using HrAgencySystem.SharedKernel.Exception;
using HrAgencySystem.SharedKernel.Factories;
using HrAgencySystem.SharedKernel.ValueObjects;
using HrAgencySystem.SharedKernel.Web.Common;

namespace HrAgencySystem.Company.Application.CompleteProfile;

internal static class CompanyProfileFactory
{
    /// <summary>
    /// Accumulates every problem into one <see cref="ValidationException"/>, the way the other
    /// factories in this codebase do. Nothing here is required on its own - the profile is a form
    /// people fill in over time - but anything present has to be well formed.
    /// </summary>
    public static CompanyProfile Create(CompleteCompanyProfile command)
    {
        var errors = new List<string>();

        // A legal name is a company name; reusing the rule keeps one answer to "how long may it be".
        string? legalName = null;
        if (!string.IsNullOrWhiteSpace(command.LegalName))
        {
            var (name, nameError) = CompanyName.TryCreate(command.LegalName);
            if (nameError is not null)
                errors.Add(nameError);
            legalName = name?.Value;
        }

        var address = CreateAddress(command, errors);

        var (vatNumber, vatError) = VatNumber.TryCreate(command.VatNumber);
        if (vatError is not null)
            errors.Add(vatError);

        var (bankAccount, bankError) = BankAccount.TryCreate(command.Iban, command.Bic);
        if (bankError is not null)
            errors.Add(bankError);

        var representative = CreateRepresentative(command, errors);

        if (errors.Count > 0)
            throw new ValidationException(errors);

        return new CompanyProfile(
            legalName,
            address,
            string.IsNullOrWhiteSpace(vatNumber?.Value) ? null : vatNumber.Value,
            bankAccount?.Iban,
            bankAccount?.Bic,
            representative
        );
    }

    private static PostalAddress? CreateAddress(
        CompleteCompanyProfile command,
        List<string> errors
    )
    {
        string?[] parts =
        [
            command.Street,
            command.BuildingNumber,
            command.PostalCode,
            command.City,
            command.CountryCode,
        ];

        if (parts.All(string.IsNullOrWhiteSpace))
            return null;

        // Half an address is worse than none: it reads as a real one on a contract and is not.
        if (parts.Any(string.IsNullOrWhiteSpace))
        {
            errors.Add(PostalAddress.PartialMessage);
            return null;
        }

        var (address, addressErrors) = PostalAddress.TryCreate(
            command.Street,
            command.BuildingNumber,
            command.UnitNumber,
            command.PostalCode,
            command.City,
            command.CountryCode
        );

        errors.AddRange(addressErrors);

        return address;
    }

    private static ContactPerson? CreateRepresentative(
        CompleteCompanyProfile command,
        List<string> errors
    )
    {
        if (command.LegalRepresentative is null)
            return null;

        try
        {
            return ContactDataFactory.Create(new RepresentativeData(command.LegalRepresentative));
        }
        catch (ValidationException ex)
        {
            errors.AddRange(ex.Errors);
            return null;
        }
    }

    private sealed record RepresentativeData(ContactPerson Contact) : IContactData;
}
