namespace HrAgencySystem.EmailTemplates.Sending;

public sealed class SmtpConfig
{
    public const string SectionName = "Smtp";

    public string Host { get; set; } = "localhost";
    public int Port { get; set; } = 1025;
    public string Username { get; set; } = "";
    public string Password { get; set; } = "";
    public bool UseSsl { get; set; }
    public string FromEmail { get; set; } = "no-reply@hr-agency.com";
    public string FromName { get; set; } = "HR Agency Portal";
}
