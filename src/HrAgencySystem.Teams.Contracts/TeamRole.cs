namespace HrAgencySystem.Teams.Contracts;

/// <summary>
/// What somebody does inside a team. Deliberately separate from Identity's OrganizationRole: that
/// one grants permissions, this one only divides the work, and a team needs an operations seat that
/// the permission vocabulary has no reason to know about.
///
/// Lives in the contracts project rather than in Teams' domain because Identity carries the role on
/// its user read model and cannot reference Teams. One definition, two modules — a second copy would
/// drift apart at the first new seat.
/// </summary>
public enum TeamRole
{
    Sales,
    Recruiter,
    Operations,
    Lead,
}
