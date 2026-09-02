using HrAgencySystem.Company.Projections;
using HrAgencySystem.SharedKernel.Snapshots;
using Marten;

namespace HrAgencySystem.Company.Infrastructure.Snapshots;

// ReSharper disable once ClassNeverInstantiated.Global
public class CompanySnapshotService(IDocumentSession session) : ICompanySnapshotService
{
    public async Task<CompanySnapshot?> GetCompanyAsync(Guid companyId, CancellationToken ct)
    {
        return await session.Query<CompanyProjection>().Where(z => z.Id == companyId)
            .Select(z => new CompanySnapshot(z.Id, z.Name, z.TaxId)).FirstOrDefaultAsync(ct);
    }
}