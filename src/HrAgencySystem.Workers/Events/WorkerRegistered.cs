using HrAgencySystem.SharedKernel.Snapshots;
using HrAgencySystem.SharedKernel.ValueObjects;
using HrAgencySystem.Workers.Domain;

namespace HrAgencySystem.Workers.Events;

/// <summary>
/// A person enters the register. <paramref name="SourceCandidateId"/> is a trace, not a link: it
/// says where the file came from without making a worker a kind of candidate, which they are not -
/// one applies for work, the other does it.
/// </summary>
public sealed record WorkerRegistered(
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
    Guid? SourceCandidateId,
    UserSnapshot CreatedBy,
    DateTimeOffset CreatedAt
);
