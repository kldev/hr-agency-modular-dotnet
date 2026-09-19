namespace HrAgencySystem.NotificationWorker;

public class RabbitMqConfig
{
    public const string SectionName = "RabbitMq";
    public string Host { get; set; } = "";
    public string Username { get; set; } = "";
    public string Password { get; set; } = "";
    public string MailExchange { get; set; } = "x.emails";
    public string RecruitmentRoutingKey { get; set; } = "recruitment";
    public string VHost { get; set; } = "/development";

    public string GetConnectionUri() => $"amqp://{Username}:{Password}@{Host}{VHost}";

    public static RabbitMqConfig FromSection(IConfigurationSection section)
    {
        return new RabbitMqConfig()
        {
            Host = section[nameof(Host)] ?? "",
            MailExchange = section[nameof(MailExchange)] ?? "",
            Username = section[nameof(Username)] ?? "",
            Password = section[nameof(Password)] ?? "",
            VHost = section[nameof(VHost)] ?? "",
            RecruitmentRoutingKey = section[nameof(RecruitmentRoutingKey)] ?? "",
        };
    }
}
