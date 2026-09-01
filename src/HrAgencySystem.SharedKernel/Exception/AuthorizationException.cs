namespace HrAgencySystem.SharedKernel.Exception;

public sealed class AuthorizationException(string message) : System.Exception(message)
{
    
}