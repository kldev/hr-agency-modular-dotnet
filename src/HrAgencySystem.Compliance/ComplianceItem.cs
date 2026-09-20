namespace HrAgencySystem.Compliance;

/// <summary>
/// What we recorded about one requirement. A checklist entry, not an integration: neither Limosa nor
/// the German reporting portal offers us an API, so a person files the declaration and we keep the
/// number, the dates and the proof.
/// <para>
/// The same record serves both scopes. What differs is what it hangs off - a project or one person's
/// assignment - and the catalogue decides which of the two is allowed to hold it.
/// </para>
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
