using HrAgencySystem.Identity;
using HrAgencySystem.Identity.Infrastructure.IAM;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace HrAgencySystem.UnitTests.Identity.Infrastructure;

public sealed class JwtConfigValidationTests
{
    private const string ValidSecret = "a-signing-key-that-is-at-least-32-bytes";

    [Fact]
    public void A_complete_section_is_accepted()
    {
        var config = Resolve(Settings(secret: ValidSecret));

        Assert.Equal("hr-agency-api", config.Issuer);
    }

    [Theory]
    [InlineData("")]
    [InlineData("short-key")]
    public void A_key_too_short_for_hmac_sha256_is_refused(string secret)
    {
        var error = Assert.Throws<OptionsValidationException>(() =>
            Resolve(Settings(secret: secret))
        );

        Assert.Contains("Jwt:SecretKey", error.Message);
    }

    [Fact]
    public void A_missing_issuer_is_refused()
    {
        var settings = Settings(secret: ValidSecret);
        settings.Remove("Jwt:Issuer");

        var error = Assert.Throws<OptionsValidationException>(() => Resolve(settings));

        Assert.Contains("Jwt:Issuer", error.Message);
    }

    [Fact]
    public void A_lifetime_of_zero_is_refused()
    {
        var settings = Settings(secret: ValidSecret);
        settings["Jwt:ImpersonationExpiresInMinutes"] = "0";

        var error = Assert.Throws<OptionsValidationException>(() => Resolve(settings));

        Assert.Contains("lifetimes", error.Message);
    }

    private static Dictionary<string, string?> Settings(string secret) =>
        new()
        {
            ["Jwt:Issuer"] = "hr-agency-api",
            ["Jwt:Audience"] = "hr-agency",
            ["Jwt:SecretKey"] = secret,
        };

    private static JwtConfig Resolve(Dictionary<string, string?> settings)
    {
        var configuration = new ConfigurationBuilder().AddInMemoryCollection(settings).Build();
        var services = new ServiceCollection();
        services.AddIdentityModule(configuration);

        using var provider = services.BuildServiceProvider();
        return provider.GetRequiredService<IOptions<JwtConfig>>().Value;
    }
}
