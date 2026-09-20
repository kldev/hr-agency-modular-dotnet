namespace HrAgencySystem.Workers.Application.Port;

/// <summary>
/// The hard half of "one person, one file". An e-mail address belongs to one human being, so the
/// second file claiming it is refused by a unique index rather than by a read model that might be
/// a moment behind.
/// <para>
/// Only for people who gave one. Plenty of people on a site have no work address, which is why
/// this cannot be the only check - see <c>IWorkersQueryRepository.FindDuplicate</c> for the rest.
/// </para>
/// </summary>
public interface IWorkerEmailReservationRepository
{
    public const string AlreadyUsedMessage =
        "Somebody with this e-mail address is already on file in this organization.";

    Task<bool> ExistsAsync(Guid organizationId, string email, CancellationToken ct);

    Task ReserveAsync(Guid organizationId, Guid workerId, string email);

    /// <summary>Moves, adds or drops the reservation as the address on the file changes.</summary>
    Task ChangeEmailAsync(Guid organizationId, Guid workerId, string? email, CancellationToken ct);
}
