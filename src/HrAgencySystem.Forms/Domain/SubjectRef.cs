namespace HrAgencySystem.Forms.Domain;

/// <summary>
/// Whose response it is, without this module knowing what that is - the <c>FileOwnerRef</c> trick.
/// Forms never references <c>Workers</c>; it holds a kind and an id, and the kind is checked against
/// a closed list so a typo fails instead of opening an orphaned bucket of answers.
/// <para>
/// It carries no organization. The organization comes from the token, and the subject is looked up
/// through a port that takes it - which is the only thing standing between one agency's form and
/// another agency's worker, and is tested as such.
/// </para>
/// </summary>
public sealed record SubjectRef(string Kind, Guid Id);

public static class SubjectKinds
{
    public const string Worker = "worker";

    public const string UnknownKindMessage = "Forms cannot be filled in for that kind of record.";

    public static readonly IReadOnlyList<string> All = [Worker];

    public static bool IsKnown(string? kind) => kind is not null && All.Contains(kind);
}
