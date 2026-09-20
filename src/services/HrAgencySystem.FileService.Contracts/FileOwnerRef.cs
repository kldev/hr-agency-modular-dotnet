namespace HrAgencySystem.FileService.Contracts;

/// <summary>
/// Who a file belongs to, as an opaque pair. The file service never learns what a project is - it
/// only checks that the owner a caller claims is the owner the file was stored under.
/// </summary>
public sealed record FileOwnerRef(string Kind, Guid Id);

public static class FileOwnerKinds
{
    public const string Project = "project";
}
