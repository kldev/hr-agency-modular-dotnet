namespace HrAgencySystem.FileService.Contracts;

/// <summary>
/// Who a file belongs to, as an opaque pair. The file service never learns what a project is - it
/// only checks that the owner a caller claims is the owner the file was stored under.
/// </summary>
public sealed record FileOwnerRef(string Kind, Guid Id);

public static class FileOwnerKinds
{
    public const string Project = "project";

    /// <summary>A person's own profile picture. The owner id is the user id.</summary>
    public const string User = "user";

    /// <summary>A document about a person - passport, diploma, medical certificate.</summary>
    public const string Worker = "worker";

    /// <summary>
    /// A document about one posting - the A1, the host country filing, the contract naming one
    /// delivering company. Separate from <see cref="Worker"/> because the two answer different
    /// questions and the second dies with the posting it belongs to.
    /// </summary>
    public const string Assignment = "assignment";
}
