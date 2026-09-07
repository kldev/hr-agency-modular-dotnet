using HrAgencySystem.Company.Events;
using HrAgencySystem.Company.Projections;
using HrAgencySystem.SharedKernel.Snapshots;
using Marten;

namespace HrAgencySystem.Company.Infrastructure.Query;

// ReSharper disable once ClassNeverInstantiated.Global
public class CompanySnapshotRepository(IDocumentSession session) : ICompanySnapshotRepository
{
    public async Task<CompanySnapshot?> GetCompanyAsync(Guid companyId, CancellationToken ct)
    {
        var result = await session.Query<CompanyProjection>()
            .WithCompanyId(companyId)
            .Select(z => new CompanySnapshot(z.Id, z.Name, z.TaxId))
            .FirstOrDefaultAsync(ct);
        if (result != null) return result;

        return await session.Query<CompanyCreated>()
            .Where(z => z.CompanyId == companyId)
            .Select(z => new CompanySnapshot(z.CompanyId, z.Name, z.TaxId))
            .FirstOrDefaultAsync(ct);
    }
}