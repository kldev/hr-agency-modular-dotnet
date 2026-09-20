using HrAgencySystem.Workers.Domain;

namespace HrAgencySystem.Workers.Application.RegisterWorker;

/// <summary>
/// What describes a person, in the shape an HTTP request carries it. Flat rather than nested, and
/// shared by registering and updating so the two cannot drift apart - one factory validates both.
/// </summary>
public interface IWorkerData
{
    string FirstName { get; }
    string LastName { get; }
    DateOnly DateOfBirth { get; }
    string Citizenship { get; }

    IdentityDocumentKind IdentityDocumentKind { get; }
    string IdentityDocumentNumber { get; }
    string IdentityDocumentIssuingCountry { get; }
    DateOnly? IdentityDocumentValidUntil { get; }

    string? Email { get; }
    string? PhoneNumber { get; }

    string? Street { get; }
    string? BuildingNumber { get; }
    string? UnitNumber { get; }
    string? PostalCode { get; }
    string? City { get; }
    string? AddressCountryCode { get; }

    string? Note { get; }
}
