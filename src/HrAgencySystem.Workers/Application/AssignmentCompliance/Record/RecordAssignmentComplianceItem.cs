using HrAgencySystem.Compliance;
using HrAgencySystem.SharedKernel.Commands;
using JasperFx;

namespace HrAgencySystem.Workers.Application.AssignmentCompliance.Record;

public sealed record RecordAssignmentComplianceItem(
    [property: Identity] Guid AssignmentId,
    Guid OrganizationId,
    ComplianceRequirement Requirement,
    ComplianceStatus Status,
    string? ReferenceNumber,
    DateOnly? ValidFrom,
    DateOnly? ValidTo,
    Guid? DocumentId,
    string? Note,
    Guid ModifiedBy
) : IUpdateCommand;
