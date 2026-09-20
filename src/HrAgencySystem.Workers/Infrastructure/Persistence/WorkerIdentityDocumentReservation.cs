namespace HrAgencySystem.Workers.Infrastructure.Persistence;

/// <summary>
/// One row per person per organization, keyed by the document they are identified by. The unique
/// index behind it is what actually enforces "one file per person" - see
/// <c>IWorkerIdentityDocumentReservationRepository</c> for why that key and not a name.
/// </summary>
public sealed record WorkerIdentityDocumentReservation(
    Guid Id,
    Guid OrganizationId,
    Guid WorkerId,
    string IssuingCountry,
    string Number
);
