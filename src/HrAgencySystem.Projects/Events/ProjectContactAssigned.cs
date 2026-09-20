using HrAgencySystem.Projects.Domain;
using HrAgencySystem.SharedKernel.Snapshots;
using HrAgencySystem.SharedKernel.Web.Common;

namespace HrAgencySystem.Projects.Events;

/// <summary>
/// Carries <paramref name="PreviousPerson"/> so a read model can show "changed from Kowalski to
/// Nowak" without replaying the stream - the same trick <c>StageChanged.PreviousStage</c> uses.
/// </summary>
public sealed record ProjectContactAssigned(
    Guid ProjectId,
    Guid OrganizationId,
    ContactRole Role,
    ContactPerson Person,
    Guid? CompanyContactId,
    ContactPerson? PreviousPerson,
    UserSnapshot ModifiedBy,
    DateTimeOffset ModifiedAt
);
