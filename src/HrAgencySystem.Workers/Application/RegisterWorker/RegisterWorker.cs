using HrAgencySystem.SharedKernel.Commands;
using HrAgencySystem.Workers.Domain;

namespace HrAgencySystem.Workers.Application.RegisterWorker;

public sealed record RegisterWorker(
    Guid OrganizationId,
    string FirstName,
    string LastName,
    DateOnly DateOfBirth,
    string Citizenship,
    IdentityDocumentKind IdentityDocumentKind,
    string IdentityDocumentNumber,
    string IdentityDocumentIssuingCountry,
    DateOnly? IdentityDocumentValidUntil,
    string? Email,
    string? PhoneNumber,
    string? Street,
    string? BuildingNumber,
    string? UnitNumber,
    string? PostalCode,
    string? City,
    string? AddressCountryCode,
    string? Note,
    Guid? SourceCandidateId,
    Guid CreatedBy
) : ICreateCommand, IWorkerData;
