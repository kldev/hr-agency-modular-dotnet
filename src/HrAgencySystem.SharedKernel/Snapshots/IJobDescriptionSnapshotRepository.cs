namespace HrAgencySystem.SharedKernel.Snapshots;

public interface IJobDescriptionSnapshotRepository
{
    public const string NotFoundMessage = "Require Job description data not found.";
    
    Task<JobDescriptionSnapshot?> GetAsync(Guid jobDescriptionId, Guid organizationId,
        CancellationToken ct);
}