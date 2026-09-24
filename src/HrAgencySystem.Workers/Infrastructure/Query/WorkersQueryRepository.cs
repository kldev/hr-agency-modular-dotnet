using HrAgencySystem.SharedKernel.Tenant;
using HrAgencySystem.SharedKernel.Web;
using HrAgencySystem.Workers.Application.Port;
using HrAgencySystem.Workers.Projections;
using Marten;

namespace HrAgencySystem.Workers.Infrastructure.Query;

// ReSharper disable once ClassNeverInstantiated.Global
public sealed class WorkersQueryRepository(IQuerySession session) : IWorkersQueryRepository
{
    public async Task<SliceResponse<WorkerProjection>> GetWorkers(
        OrganizationId organizationId,
        WorkerQuery query,
        CancellationToken ct
    )
    {
        var workers = session
            .Query<WorkerProjection>()
            .WithOrganizationId(organizationId)
            .WithSearch(query.Search)
            .WithFilters(query)
            .OrderBy(w => w.LastName)
            .ThenBy(w => w.FirstName)
            .ThenBy(w => w.Id);

        return await workers.ToSlice(query, ct);
    }

    public async Task<WorkerProjection?> FindDuplicate(
        OrganizationId organizationId,
        string? email,
        string firstName,
        string lastName,
        string phoneNumber,
        Guid? exceptWorkerId,
        CancellationToken ct
    )
    {
        var address = string.IsNullOrWhiteSpace(email) ? null : email.Trim().ToLowerInvariant();
        var digits = new string([.. phoneNumber.Where(char.IsDigit)]);
        var fullName = $"{firstName} {lastName}".Trim();

        var candidates = session
            .Query<WorkerProjection>()
            .WithOrganizationId(organizationId)
            .Where(w =>
                (address != null && w.Email == address)
                || (digits != "" && w.PhoneDigits == digits && w.FullName == fullName)
            );

        if (exceptWorkerId is not null)
            candidates = candidates.Where(w => w.Id != exceptWorkerId);

        return await candidates.FirstOrDefaultAsync(ct);
    }

    public async Task<WorkerProjection?> GetWorker(
        OrganizationId organizationId,
        Guid workerId,
        CancellationToken ct
    ) =>
        await session
            .Query<WorkerProjection>()
            .WithOrganizationId(organizationId)
            .Where(w => w.Id == workerId)
            .FirstOrDefaultAsync(ct);
}
