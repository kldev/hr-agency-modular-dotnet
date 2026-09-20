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
    string FirstName,
    string LastName,
    DateOnly DateOfBirth,
    string Citizenship,
    IdentityDocumentKind IdentityDocumentKind,
    string IdentityDocumentNumber,
    string IdentityDocumentIssuingCountry,
    DateOnly? IdentityDocumentValidUntil = null,
    string? Email = null,
    string? PhoneNumber = null,
    string? Street = null,
    string? BuildingNumber = null,
    string? UnitNumber = null,
    string? PostalCode = null,
    string? City = null,
    string? AddressCountryCode = null,
    string? Note = null,
    Guid? SourceCandidateId = null
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
