using HrAgencySystem.SharedKernel.Tenant;

namespace HrAgencySystem.SharedKernel.Snapshots;

/// <summary>
/// A person from the worker register, as seen by a module that does not own it. Read by the forms
/// module, which fills a form for a worker and pre-fills the handful of system fields the file
/// already answers.
/// <para>
/// Only ever scoped to an organization: a form subject carries no organization of its own, so this
/// lookup is what keeps one agency's form from reaching another agency's worker.
/// </para>
/// </summary>
public interface IWorkerSnapshotRepository
{
    Task<WorkerSnapshot?> GetWorkerAsync(
        Guid workerId,
        OrganizationId organizationId,
        CancellationToken ct
    );
}

/// <summary>What a form may borrow from a worker's file. Read only; nothing is written back.</summary>
public sealed record WorkerSnapshot(
    Guid Id,
    string FirstName,
    string LastName,
    DateOnly DateOfBirth,
    string Citizenship,
    string? Email,
    string? Phone
)
{
    public string FullName => $"{FirstName} {LastName}".Trim();
}
