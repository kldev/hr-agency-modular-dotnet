using HrAgencySystem.Projects.Domain;
using HrAgencySystem.SharedKernel.Snapshots;
using HrAgencySystem.SharedKernel.Web.Common;

namespace HrAgencySystem.Projects.Events;

public sealed record ProjectContactRemoved(
    Guid ProjectId,
    Guid OrganizationId,
    ContactRole Role,
    ContactPerson Person,
    UserSnapshot ModifiedBy,
    DateTimeOffset ModifiedAt
);
