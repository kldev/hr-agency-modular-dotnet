using HrAgencySystem.SharedKernel.Tenant;

namespace HrAgencySystem.SharedKernel.Snapshots;

public interface IUserSnapshotRepository
{
    public const string NotFoundMessage = "Require user data not found.";

    Task<UserSnapshot?> GetUserAsync(Guid userId, CancellationToken ct);

    /// <summary>
    /// Resolves a user only when they belong to the given organization. The unscoped overload above
    /// answers "does this id exist anywhere", which is not the same question and cannot enforce a
    /// same-organization rule.
    /// </summary>
    Task<UserSnapshot?> GetUserAsync(
        Guid userId,
        OrganizationId organizationId,
        CancellationToken ct
    );
}

// ReSharper disable once ClassNeverInstantiated.Global
public sealed record UserSnapshot(Guid Id, string FirstName, string LastName, string Email)
{
    public string Fullname => $"{FirstName} {LastName}".Trim();

    public static UserSnapshot System => new UserSnapshot(Guid.NewGuid(), "", "", "system");
}
