using HrAgencySystem.Forms.Domain.Layout;
using HrAgencySystem.Forms.Domain.SystemFields;
using HrAgencySystem.SharedKernel.Snapshots;

namespace HrAgencySystem.Forms.Events;

/*
 * Every event names the organization because the organization's catalogue is the stream: one list
 * per organization, so "a code is used once" is a check on a list in memory rather than a
 * reservation document - the OrgStructure trade.
 */

public sealed record SystemFieldDefined(
    Guid OrganizationId,
    SystemField Field,
    UserSnapshot CreatedBy,
    DateTimeOffset CreatedAt
);

/// <summary>Everything that may change about a system field. Code and type are not here, on purpose.</summary>
public sealed record SystemFieldUpdated(
    Guid OrganizationId,
    Guid SystemFieldId,
    string Label,
    string? Description,
    FieldRules Rules,
    IReadOnlyList<ChoiceOption> Options,
    SystemFieldSource Source,
    UserSnapshot ModifiedBy,
    DateTimeOffset ModifiedAt
);

public sealed record SystemFieldArchived(
    Guid OrganizationId,
    Guid SystemFieldId,
    UserSnapshot ModifiedBy,
    DateTimeOffset ModifiedAt
);
