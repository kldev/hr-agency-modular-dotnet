using HrAgencySystem.SharedKernel.Snapshots;
using HrAgencySystem.SharedKernel.ValueObjects;
using HrAgencySystem.Workers.Domain;

namespace HrAgencySystem.Workers.Events;

public sealed record WorkerUpdated(
    Guid WorkerId,
    Guid OrganizationId,
    string FirstName,
    string LastName,
    DateOnly DateOfBirth,
    string Citizenship,
    IdentityDocument IdentityDocument,
    string? Email,
    string PhoneNumber,
    PostalAddress? Address,
    string Note,
    UserSnapshot ModifiedBy,
    DateTimeOffset ModifiedAt
);
