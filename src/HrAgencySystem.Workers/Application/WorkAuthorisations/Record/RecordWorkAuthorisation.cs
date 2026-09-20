using HrAgencySystem.SharedKernel.Commands;
using HrAgencySystem.Workers.Domain;
using JasperFx;

namespace HrAgencySystem.Workers.Application.WorkAuthorisations.Record;

/// <summary>
/// <paramref name="AuthorisationId"/> is supplied rather than minted, so that renewing a permit is
/// an update of the same record and obtaining a different one is a new record. The caller decides
/// which of the two happened, because only the caller knows.
/// </summary>
public sealed record RecordWorkAuthorisation(
    [property: Identity] Guid WorkerId,
    Guid OrganizationId,
    Guid? AuthorisationId,
    WorkAuthorisationKind Kind,
    string Country,
    string Number,
    DateOnly ValidFrom,
    DateOnly ValidUntil,
    Guid? DocumentId,
    string? Note,
    Guid ModifiedBy
) : IUpdateCommand;
