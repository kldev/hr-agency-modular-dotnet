namespace HrAgencySystem.Workers.Application.Port;

/// <summary>
/// Keeps one person from having two files in the same organization.
/// <para>
/// The key is the document rather than a name or an e-mail: two people called Jan Kowalski are two
/// people, the same Jan Kowalski with two spellings is one, and the only thing that tells those
/// cases apart is the number on his passport. A register of A1 certificates built on duplicated
/// files would not be a register.
/// </para>
/// </summary>
public interface IWorkerIdentityDocumentReservationRepository
{
    public const string AlreadyUsedMessage =
        "Somebody with this identity document is already on file in this organization.";

    Task<bool> ExistsAsync(
        Guid organizationId,
        string issuingCountry,
        string number,
        CancellationToken ct
    );

    Task ReserveAsync(Guid organizationId, Guid workerId, string issuingCountry, string number);

    Task ChangeDocumentAsync(
        Guid organizationId,
        Guid workerId,
        string issuingCountry,
        string number,
        CancellationToken ct
    );
}
