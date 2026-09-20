using HrAgencySystem.Projects.Domain;
using HrAgencySystem.SharedKernel.Snapshots;

namespace HrAgencySystem.Projects.Events;

/// <summary>
/// The project was set up under the wrong company, and it has not started yet. Only ever possible
/// while the project is a draft: once it is live, carrying on under another company is a new
/// project, not an edit of this one.
/// </summary>
public sealed record ProjectLegalEntityChanged(
    Guid ProjectId,
    Guid OrganizationId,
    DeliveringEntitySnapshot DeliveringEntity,
    UserSnapshot ModifiedBy,
    DateTimeOffset ModifiedAt
);
