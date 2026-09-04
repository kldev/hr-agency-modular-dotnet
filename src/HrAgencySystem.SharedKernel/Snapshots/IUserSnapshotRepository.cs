namespace HrAgencySystem.SharedKernel.Snapshots;



public interface IUserSnapshotRepository
{
    public const string NotFoundMessage = "Require user data not found.";
    Task<UserSnapshot?> GetUserAsync(Guid userId, CancellationToken ct);
}

// ReSharper disable once ClassNeverInstantiated.Global
public sealed record UserSnapshot(
    Guid Id,
    string FirstName,
    string LastName,
    string Email);