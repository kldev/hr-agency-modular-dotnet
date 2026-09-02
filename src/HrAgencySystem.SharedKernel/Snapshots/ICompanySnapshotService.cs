namespace HrAgencySystem.SharedKernel.Snapshots;

public interface ICompanySnapshotService
{
    public const string NotFoundMessage = "Require company data not found.";
    Task<CompanySnapshot?> GetCompanyAsync(Guid companyId, CancellationToken ct);
}