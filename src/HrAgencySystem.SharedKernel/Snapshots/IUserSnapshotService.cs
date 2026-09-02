namespace HrAgencySystem.SharedKernel.Snapshots;

public interface IUserSnapshotService
{
    public const string NotFoundMessage = "Require user data not found.";
    Task<UserSnapshot?> GetUserAsync(Guid userId, CancellationToken ct);
}