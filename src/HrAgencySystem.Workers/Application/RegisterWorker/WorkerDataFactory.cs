using HrAgencySystem.SharedKernel.Exception;
using HrAgencySystem.SharedKernel.ValueObjects;
using HrAgencySystem.Workers.Domain;
using HrAgencySystem.Workers.Domain.ValueObjects;

namespace HrAgencySystem.Workers.Application.RegisterWorker;

/// <summary>
/// Turns what a request carries into what the domain holds, reporting every fault at once rather
/// than the first one - filling in a person's details is a form, and a form that rejects one field
/// at a time is filled in five times.
/// </summary>
internal static class WorkerDataFactory
{
    public const string DateOfBirthInFutureMessage = "The date of birth must be in the past.";

    public static WorkerData Create(IWorkerData data, DateOnly today)
    {
        var errors = new List<string>();

        var (firstName, firstNameError) = FirstName.TryCreate(data.FirstName);
        if (firstNameError is not null)
            errors.Add(firstNameError);

        var (lastName, lastNameError) = LastName.TryCreate(data.LastName);
        if (lastNameError is not null)
            errors.Add(lastNameError);

        if (data.DateOfBirth >= today)
            errors.Add(DateOfBirthInFutureMessage);

        var (citizenship, citizenshipError) = CountryCode.TryCreate(data.Citizenship);
        if (citizenshipError is not null)
            errors.Add(citizenshipError);

        var (documentNumber, documentNumberError) = DocumentNumber.TryCreate(
            data.IdentityDocumentNumber
        );
        if (documentNumberError is not null)
            errors.Add(documentNumberError);

        var (issuingCountry, issuingCountryError) = CountryCode.TryCreate(
            data.IdentityDocumentIssuingCountry
        );
        if (issuingCountryError is not null)
            errors.Add(issuingCountryError);

        var email = ReadEmail(data.Email, errors);

        var (phone, phoneError) = PersonPhone.TryCreate(data.PhoneNumber ?? "");
        if (phoneError is not null)
            errors.Add(phoneError);

        var address = ReadAddress(data, errors);

        var (note, noteError) = LongText.TryCreate(data.Note ?? "", false);
        if (noteError is not null)
            errors.Add(noteError);

        if (errors.Count > 0)
            throw new ValidationException(errors);

        return new WorkerData(
            firstName!,
            lastName!,
            data.DateOfBirth,
            citizenship!,
            new IdentityDocument(
                data.IdentityDocumentKind,
                documentNumber!.Value,
                issuingCountry!.Value,
                data.IdentityDocumentValidUntil
            ),
            email,
            phone!,
            address,
            note!
        );
    }

    private static Email? ReadEmail(string? value, List<string> errors)
    {
        // Optional: plenty of people on a site have no work address, and refusing the file over it
        // would push somebody into inventing one.
        if (string.IsNullOrWhiteSpace(value))
            return null;

        var (email, error) = Email.TryCreate(value);

        if (error is not null)
            errors.Add(error);

        return email;
    }

    /// <summary>
    /// All of it or none of it. A person can be on file before anybody has their address, but half
    /// an address on a posting declaration is worse than none, so the moment one part is filled in
    /// the rest has to be.
    /// </summary>
    private static PostalAddress? ReadAddress(IWorkerData data, List<string> errors)
    {
        string?[] parts =
        [
            data.Street,
            data.BuildingNumber,
            data.UnitNumber,
            data.PostalCode,
            data.City,
            data.AddressCountryCode,
        ];

        if (parts.All(string.IsNullOrWhiteSpace))
            return null;

        var (address, addressErrors) = PostalAddress.TryCreate(
            data.Street ?? "",
            data.BuildingNumber ?? "",
            data.UnitNumber,
            data.PostalCode ?? "",
            data.City ?? "",
            data.AddressCountryCode ?? ""
        );

        errors.AddRange(addressErrors);

        return address;
    }
}

internal sealed record WorkerData(
    FirstName FirstName,
    LastName LastName,
    DateOnly DateOfBirth,
    CountryCode Citizenship,
    IdentityDocument IdentityDocument,
    Email? Email,
    PersonPhone PhoneNumber,
    PostalAddress? Address,
    LongText Note
);
