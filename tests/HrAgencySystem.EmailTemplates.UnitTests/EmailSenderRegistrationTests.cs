using HrAgencySystem.EmailTemplates.Sending;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace HrAgencySystem.EmailTemplates.UnitTests;

public class EmailSenderRegistrationTests
{
    [Fact]
    public void AddEMailTemplates_WithMailKitProvider_RegistersMailKitSender()
    {
        using var provider = Build(
            new()
            {
                ["MailProvider"] = "mailkit",
                ["Smtp:Host"] = "mailpit",
                ["Smtp:Port"] = "1025",
            }
        );

        var sender = provider.GetRequiredService<ISendEmail>();
        var smtp = provider.GetRequiredService<IOptions<SmtpConfig>>().Value;

        Assert.IsType<MailKitEmailSender>(sender);
        Assert.Equal("mailpit", smtp.Host);
        Assert.Equal(1025, smtp.Port);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("smtp4dev")]
    public void AddEMailTemplates_WithoutMailKitProvider_FallsBackToLogging(string? configured)
    {
        using var provider = Build(new() { ["MailProvider"] = configured });

        Assert.IsType<LoggingEmailSender>(provider.GetRequiredService<ISendEmail>());
    }

    [Fact]
    public void AddEMailTemplates_IsCaseInsensitiveAboutTheProvider()
    {
        using var provider = Build(new() { ["MailProvider"] = "MailKit" });

        Assert.IsType<MailKitEmailSender>(provider.GetRequiredService<ISendEmail>());
    }

    private static ServiceProvider Build(Dictionary<string, string?> settings)
    {
        var configuration = new ConfigurationBuilder().AddInMemoryCollection(settings).Build();

        var services = new ServiceCollection();
        services.AddLogging();
        services.AddEMailTemplates(configuration);

        return services.BuildServiceProvider();
    }
}
