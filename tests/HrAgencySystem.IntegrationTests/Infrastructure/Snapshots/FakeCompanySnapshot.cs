using HrAgencySystem.SharedKernel.Snapshots;

namespace HrAgencySystem.IntegrationTests.Infrastructure.Snapshots;

public sealed class FakeCompanySnapshot : ICompanySnapshotService
{
    public Task<CompanySnapshot?> GetCompanyAsync(Guid companyId, CancellationToken ct)
    {
        var suffix = companyId.ToString().Substring(4);
        var result = new CompanySnapshot(companyId, "Company  " + suffix,
            "TXT 101-200" + suffix);
        
        return Task.FromResult((CompanySnapshot?)result);
    }
}