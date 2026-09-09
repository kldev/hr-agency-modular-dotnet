namespace HrAgencySystem.SharedKernel.Exception;

public sealed class OrganizationAccessDeniedException()
    : System.Exception(ProblemMessage)
{
    public const string ProblemTitle = "Organization access denied";
    public const string ProblemMessage = "You do not have permission to access a resource belonging to another organization";
    
}

