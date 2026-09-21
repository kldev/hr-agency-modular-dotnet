namespace HrAgencySystem.Identity.Domain;

/// <summary>
/// What somebody in an organization is allowed to do. One axis; where they sit in the company is
/// the other, and neither follows from the other - see <c>Agency</c>'s org structure. A recruiter
/// reports to the head of recruitment; payroll settles everybody's hours whoever they report to.
/// </summary>
public enum OrganizationRole
{
    Admin,
    Recruiter,
    HiringManager,
    Interviewer,
    Sales,

    /// <summary>Payroll and personnel: settles time sheets, handles absences, sees everybody.</summary>
    HumanResources,

    /// <summary>Money: rates, invoices, amounts other roles are not shown.</summary>
    Finance,

    /// <summary>The office: documents, equipment, the paperwork that is nobody else's job.</summary>
    Administration,

    System,
}

public enum OrganizationRoleApi
{
    Admin,
    Recruiter,
    HiringManager,
    Interviewer,
    Sales,
    HumanResources,
    Finance,
    Administration,
}

public static class OrganizationRoleExtension
{
    extension(OrganizationRoleApi role)
    {
        public OrganizationRole ToDomainRole()
        {
            return Enum.Parse<OrganizationRole>(role.ToString());
        }
    }
}
