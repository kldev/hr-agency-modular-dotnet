using HrAgencySystem.Compliance;
using HrAgencySystem.SharedKernel.Commands;
using JasperFx;

namespace HrAgencySystem.Projects.Application.Compliance.Record;

public sealed record RecordComplianceItem(
    [property: Identity] Guid ProjectId,
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
