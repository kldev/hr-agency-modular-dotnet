using Microsoft.Extensions.Configuration;

namespace HrAgencySystem.EmailTemplates.Messaging;

/// <summary>
/// Broker coordinates shared by every host that touches the mail exchange, so that the producer and
/// the consumers cannot drift apart into an exchange redeclaration conflict.
/// </summary>
public sealed class RabbitMqConfig
{
    public const string SectionName = "RabbitMq";

    public string Host { get; init; } = "";
    public string Username { get; init; } = "";
    public string Password { get; init; } = "";
    public string VHost { get; init; } = "/development";
    public string MailExchange { get; init; } = "x.emails";

    public string GetConnectionUri() => $"amqp://{Username}:{Password}@{Host}{VHost}";

    /// <summary>The coordinates without the password, for anything that ends up in a log.</summary>
    public override string ToString() => $"amqp://{Username}@{Host}{VHost}";

    /// <summary>
    /// Read before the host is built, so it cannot be a validated option; it fails the same way one
    /// would - at startup, naming the setting - instead of as an unparsable amqp uri inside Wolverine.
    /// A key that is not set keeps its default rather than becoming an empty string.
    /// </summary>
    public static RabbitMqConfig FromSection(IConfigurationSection section)
    {
        var defaults = new RabbitMqConfig();
        var config = new RabbitMqConfig
        {
            Host = section[nameof(Host)] ?? defaults.Host,
            Username = section[nameof(Username)] ?? defaults.Username,
            Password = section[nameof(Password)] ?? defaults.Password,
            VHost = section[nameof(VHost)] ?? defaults.VHost,
            MailExchange = section[nameof(MailExchange)] ?? defaults.MailExchange,
        };

        if (
            string.IsNullOrWhiteSpace(config.Host) || string.IsNullOrWhiteSpace(config.MailExchange)
        )
            throw new InvalidOperationException(
                $"{SectionName}:{nameof(Host)} and {SectionName}:{nameof(MailExchange)} are required."
            );

        return config;
    }
}
