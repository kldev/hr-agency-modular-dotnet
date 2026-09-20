using HrAgencySystem.Projects.Domain;
using HrAgencySystem.SharedKernel.Commands;
using JasperFx;

namespace HrAgencySystem.Projects.Application.Emails.Set;

public sealed record SetProjectEmailRecipients(
    [property: Identity] Guid ProjectId,
    Guid OrganizationId,
    EmailPurpose Purpose,
    IReadOnlyList<string> Emails,
    Guid ModifiedBy
) : IUpdateCommand;
