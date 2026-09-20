using HrAgencySystem.SharedKernel.Commands;
using HrAgencySystem.Workers.Application.RegisterWorker;
using HrAgencySystem.Workers.Domain;
using JasperFx;

namespace HrAgencySystem.Workers.Application.UpdateWorker;

public sealed record UpdateWorker(
    [property: Identity] Guid WorkerId,
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
    Guid ModifiedBy
) : IUpdateCommand, IWorkerData;
