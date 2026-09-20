using HrAgencySystem.Projects.Domain;
using HrAgencySystem.SharedKernel.Snapshots;

namespace HrAgencySystem.Projects.Events;

/// <summary>
/// Replaces the whole set for one purpose rather than adding one address at a time. The UI edits the
/// list as a whole and no mail goes out of it, so per address events would buy only more events.
/// </summary>
public sealed record ProjectEmailRecipientsChanged(
    Guid ProjectId,
    Guid OrganizationId,
    EmailPurpose Purpose,
    IReadOnlyList<string> Emails,
    UserSnapshot ModifiedBy,
    DateTimeOffset ModifiedAt
);
