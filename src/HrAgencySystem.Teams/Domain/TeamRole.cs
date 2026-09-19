namespace HrAgencySystem.Teams.Domain;

/// <summary>
/// What somebody does inside a team. Deliberately separate from Identity's OrganizationRole: that
/// one grants permissions, this one only divides the work, and a team needs an operations seat that
/// the permission vocabulary has no reason to know about.
/// </summary>
public enum TeamRole
{
    Sales,
    Recruiter,
    Operations,
    Lead,
}
