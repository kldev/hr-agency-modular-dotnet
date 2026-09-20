using HrAgencySystem.Workers.Domain;

namespace HrAgencySystem.Workers.Application.Port;

/// <summary>
/// Loads a person's file by replaying their stream.
/// <para>
/// A port rather than a Marten session on the module service, because Wolverine builds handler
/// dependencies itself and refuses to service locate one that asks for a session directly.
/// </para>
/// </summary>
public interface IWorkerRepository
{
    Task<Worker?> LoadAsync(Guid workerId, CancellationToken ct);
}
