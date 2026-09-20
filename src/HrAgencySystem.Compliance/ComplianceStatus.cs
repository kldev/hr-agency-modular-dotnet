namespace HrAgencySystem.Compliance;

public enum ComplianceStatus
{
    NotStarted,
    InProgress,
    Confirmed,

    /// <summary>Checked and found not to apply here - a decision, not a gap.</summary>
    NotApplicable,

    Expired,
}
