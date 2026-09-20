using HrAgencySystem.Compliance;
using HrAgencySystem.SharedKernel.Snapshots;

namespace HrAgencySystem.Workers.Events;

/// <summary>
/// What was recorded against one of this posting's per person obligations - the A1, the Limosa
/// declaration, the local employment contract. The register CLAUDE.md said did not exist yet.
/// </summary>
public sealed record AssignmentComplianceItemRecorded(
    Guid AssignmentId,
    Guid OrganizationId,
    Guid WorkerId,
    ComplianceItem Item,
    UserSnapshot ModifiedBy,
    DateTimeOffset ModifiedAt
);
