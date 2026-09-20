namespace HrAgencySystem.Projects.Domain;

/// <summary>
/// The life of a delivery, which is not the life of a sale. An opportunity ends with a decision; a
/// project begins with one and then runs.
/// <para>
/// There is no <c>Archived</c>. A job post has one because a post can be brought back to publication;
/// a finished project does not come back. "Archive" is a filter on a list, not a state of the work,
/// and a status nobody reads is worse than none.
/// </para>
/// </summary>
public enum ProjectStatus
{
    Draft,
    Active,
    Suspended,
    Completed,
    Cancelled,
}
