namespace HrAgencySystem.Projects.Domain.Compliance;

/// <summary>
/// What we recorded about one requirement. A checklist entry, not an integration: neither Limosa nor
/// the German reporting portal offers us an API, so a person files the declaration and we keep the
/// number, the dates and the proof.
/// </summary>
public sealed record ComplianceItem(
    ComplianceRequirement Requirement,
    ComplianceStatus Status,
    string? ReferenceNumber,
    DateOnly? ValidFrom,
    DateOnly? ValidTo,
    Guid? DocumentId,
    string? Note
)
{
    /// <summary>Nothing more to chase: either it is done or it was found not to apply.</summary>
    public bool IsSettled => Status is ComplianceStatus.Confirmed or ComplianceStatus.NotApplicable;
}
