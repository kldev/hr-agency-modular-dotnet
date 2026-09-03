using HrAgencySystem.Company.Projections;
using HrAgencySystem.SharedKernel.Snapshots;
using Marten;

namespace HrAgencySystem.Company.Infrastructure.Query;

// ReSharper disable once ClassNeverInstantiated.Global
public class CompanySnapshotRepository(IDocumentSession session) : ICompanySnapshotRepository
{
    public async Task<CompanySnapshot?> GetCompanyAsync(Guid companyId, CancellationToken ct)
    {
        return await session.Query<CompanyProjection>().Where(z => z.Id == companyId)
            .Select(z => new CompanySnapshot(z.Id, z.Name, z.TaxId)).FirstOrDefaultAsync(ct);
    }
}