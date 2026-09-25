using Microsoft.Extensions.Configuration;

namespace HrAgencySystem.EmailTemplates.Messaging;

/// <summary>
/// Broker coordinates shared by every host that touches the mail exchange, so that the producer and
/// the consumers cannot drift apart into an exchange redeclaration conflict.
/// </summary>
public class RabbitMqConfig
{
    public const string SectionName = "RabbitMq";

    public string Host { get; set; } = "";
    public string Username { get; set; } = "";
    public string Password { get; set; } = "";
    public string VHost { get; set; } = "/development";
    public string MailExchange { get; set; } = "x.emails";

    public string GetConnectionUri() => $"amqp://{Username}:{Password}@{Host}{VHost}";

    /// <summary>The coordinates without the password, for anything that ends up in a log.</summary>
    public override string ToString() => $"amqp://{Username}@{Host}{VHost}";

    public static RabbitMqConfig FromSection(IConfigurationSection section) =>
        new()
        {
            Host = section[nameof(Host)] ?? "",
            Username = section[nameof(Username)] ?? "",
            Password = section[nameof(Password)] ?? "",
            VHost = section[nameof(VHost)] ?? "",
            MailExchange = section[nameof(MailExchange)] ?? "",
        };
}
