using HrAgencySystem.SharedKernel.Exception;

namespace HrAgencySystem.Identity.Application.Policy;

public static class PasswordPolicyValidator
{
    public const string InvalidPasswordMessage = "Password must contain at least 4 characters.";

    private const int MinimumLength = 4;

    public static void Validate(string password)
    {
        if (string.IsNullOrWhiteSpace(password) || password.Length < MinimumLength)
        {
            throw new BusinessRuleException(InvalidPasswordMessage);
        }
    }
}
