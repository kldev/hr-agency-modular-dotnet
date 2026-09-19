using HrAgencySystem.Identity.Application.Policy;
using HrAgencySystem.SharedKernel.Exception;

namespace HrAgencySystem.UnitTests.Identity.Policy;

public sealed class PasswordPolicyValidatorTests
{
    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("a")]
    // Three characters are refused, which is what the message now says as well.
    [InlineData("abc")]
    public void Validate_WithTooShortPassword_Throws(string password)
    {
        var exception = Assert.Throws<BusinessRuleException>(() =>
            PasswordPolicyValidator.Validate(password)
        );

        Assert.Equal(PasswordPolicyValidator.InvalidPasswordMessage, exception.Message);
    }

    [Theory]
    [InlineData("abcd")]
    [InlineData("agent999!")]
    public void Validate_WithPasswordAtOrAboveTheMinimum_Passes(string password)
    {
        PasswordPolicyValidator.Validate(password);
    }
}
