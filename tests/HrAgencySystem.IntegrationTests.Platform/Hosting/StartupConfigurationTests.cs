using HrAgencySystem.IntegrationTests.Infrastructure;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Options;

namespace HrAgencySystem.IntegrationTests.Hosting;

/// <summary>
/// Configuration that can only fail at runtime is refused at startup instead. These hosts never
/// reach the database: options are validated before any hosted service starts.
/// </summary>
public sealed class StartupConfigurationTests
{
    private const string ConnectionString =
        "Host=localhost;Port=1;Database=never_opened;Username=none;Password=none";

    [Fact]
    public void A_file_service_secret_equal_to_the_user_token_key_stops_the_host()
    {
        // Arrange
        const string shared = "one-key-for-two-audiences-is-a-hole-0123";
        using var factory = new ApiApplicationFactory(ConnectionString).WithWebHostBuilder(
            builder =>
            {
                builder.UseSetting("Jwt:SecretKey", shared);
                builder.UseSetting("FileService:Secret", shared);
            }
        );

        // Act
        var error = Record.Exception(factory.CreateClient);

        // Assert
        Assert.Contains(
            FailuresOf(error),
            failure => failure.Contains("FileService:Secret must differ from Jwt:SecretKey")
        );
    }

    [Fact]
    public void A_relative_portal_url_stops_the_host()
    {
        // Arrange
        using var factory = new ApiApplicationFactory(ConnectionString).WithWebHostBuilder(
            builder => builder.UseSetting("Application:PortalUrl", "/portal")
        );

        // Act
        var error = Record.Exception(factory.CreateClient);

        // Assert
        Assert.Contains(FailuresOf(error), failure => failure.Contains("Application:PortalUrl"));
    }

    // One invalid section comes as itself, several as an aggregate of them.
    private static IEnumerable<string> FailuresOf(Exception? error) =>
        error switch
        {
            OptionsValidationException validation => validation.Failures,
            AggregateException aggregate => aggregate
                .InnerExceptions.OfType<OptionsValidationException>()
                .SelectMany(validation => validation.Failures),
            _ => throw new Xunit.Sdk.XunitException($"Expected a validation failure, got {error}"),
        };
}
