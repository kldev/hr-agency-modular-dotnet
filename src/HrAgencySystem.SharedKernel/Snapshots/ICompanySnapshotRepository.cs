namespace HrAgencySystem.SharedKernel.Snapshots;

public interface ICompanySnapshotRepository
{
    public const string NotFoundMessage = "Require company data not found.";
    Task<CompanySnapshot?> GetCompanyAsync(Guid companyId, CancellationToken ct);
}