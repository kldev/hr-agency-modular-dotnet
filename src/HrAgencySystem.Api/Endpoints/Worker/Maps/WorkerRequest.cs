using System.ComponentModel;
using HrAgencySystem.SharedKernel.Tenant;
using HrAgencySystem.Workers.Application.RegisterWorker;
using HrAgencySystem.Workers.Application.UpdateWorker;
using HrAgencySystem.Workers.Domain;

namespace HrAgencySystem.Api.Endpoints.Worker.Maps;

/// <summary>
/// One request body for registering and updating, because both carry the same description of a
/// person. The two commands differ only in what the endpoint already knows - the id and who is
/// doing it - so those are arguments rather than fields.
/// </summary>
internal sealed record WorkerRequest(
    [property: Description("First name, as in the identity document.")] string FirstName,
    [property: Description("Last name, as in the identity document.")] string LastName,
    [property: Description("Date of birth; must be in the past.")] DateOnly DateOfBirth,
    [property: Description(
        "Country of citizenship, ISO 3166-1 alpha-2. It decides whether legalisation applies: citizens of free movement countries need no work permit."
    )]
        string Citizenship,
    [property: Description("IdentityCard, Passport, ResidenceCard or Other.")]
        IdentityDocumentKind IdentityDocumentKind,
    [property: Description(
        "The document's number. With the issuing country it identifies the person: a second file with the same document is refused."
    )]
        string IdentityDocumentNumber,
    [property: Description("Country that issued the document, ISO 3166-1 alpha-2.")]
        string IdentityDocumentIssuingCountry,
    [property: Description("Last day the document is valid. Optional.")]
        DateOnly? IdentityDocumentValidUntil = null,
    [property: Description(
        "Optional e-mail address. Unique within the agency's register when given."
    )]
        string? Email = null,
    [property: Description(
        "Optional phone number. With the name it is how a person without an e-mail is recognised as already on file."
    )]
        string? PhoneNumber = null,
    [property: Description(
        "Street. Home address: street, building number, postal code, city and country together, or none of them."
    )]
        string? Street = null,
    [property: Description(
        "Building number. Home address: street, building number, postal code, city and country together, or none of them."
    )]
        string? BuildingNumber = null,
    [property: Description("Optional flat or unit number.")] string? UnitNumber = null,
    [property: Description(
        "Postal code. Home address: street, building number, postal code, city and country together, or none of them."
    )]
        string? PostalCode = null,
    [property: Description(
        "City. Home address: street, building number, postal code, city and country together, or none of them."
    )]
        string? City = null,
    [property: Description(
        "Country of the home address, ISO 3166-1 alpha-2. Home address: street, building number, postal code, city and country together, or none of them."
    )]
        string? AddressCountryCode = null,
    [property: Description("Optional free note.")] string? Note = null,
    [property: Description(
        "When registering: the candidate the file comes from. Recruitment then marks that candidate as registered. Ignored on update - an origin does not change."
    )]
        Guid? SourceCandidateId = null,
    [property: Description(
        "When registering: the job application the decision was made on; requires SourceCandidateId. Ignored on update."
    )]
        Guid? SourceApplicationId = null
)
{
    public RegisterWorker ToRegisterCommand(OrganizationId organizationId, Guid createdBy) =>
        new(
            organizationId.Value,
            FirstName,
            LastName,
            DateOfBirth,
            Citizenship,
            IdentityDocumentKind,
            IdentityDocumentNumber,
            IdentityDocumentIssuingCountry,
            IdentityDocumentValidUntil,
            Email,
            PhoneNumber,
            Street,
            BuildingNumber,
            UnitNumber,
            PostalCode,
            City,
            AddressCountryCode,
            Note,
            SourceCandidateId,
            SourceApplicationId,
            createdBy
        );

    public UpdateWorker ToUpdateCommand(
        Guid workerId,
        OrganizationId organizationId,
        Guid modifiedBy
    ) =>
        new(
            workerId,
            organizationId.Value,
            FirstName,
            LastName,
            DateOfBirth,
            Citizenship,
            IdentityDocumentKind,
            IdentityDocumentNumber,
            IdentityDocumentIssuingCountry,
            IdentityDocumentValidUntil,
            Email,
            PhoneNumber,
            Street,
            BuildingNumber,
            UnitNumber,
            PostalCode,
            City,
            AddressCountryCode,
            Note,
            modifiedBy
        );
}
