namespace HrAgencySystem.Identity.Domain;

public enum OrganizationRole
{
    Admin,
    Recruiter,
    HiringManager,
    Interviewer,
    Sales,
    System
}

public enum OrganizationRoleApi
{
    Admin,
    Recruiter,
    HiringManager,
    Interviewer,
    Sales
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