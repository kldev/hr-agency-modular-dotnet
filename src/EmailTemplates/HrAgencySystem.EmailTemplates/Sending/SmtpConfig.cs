using JetBrains.Annotations;

namespace HrAgencySystem.EmailTemplates.Sending;

public sealed class SmtpConfig
{
    public const string SectionName = "Smtp";

    [UsedImplicitly(ImplicitUseKindFlags.Assign)] // Set by the configuration binder only.
    public string Host { get; init; } = "localhost";

    [UsedImplicitly(ImplicitUseKindFlags.Assign)] // Set by the configuration binder only.
    public int Port { get; init; } = 1025;

    [UsedImplicitly(ImplicitUseKindFlags.Assign)] // Set by the configuration binder only.
    public string Username { get; init; } = "";

    [UsedImplicitly(ImplicitUseKindFlags.Assign)] // Set by the configuration binder only.
    public string Password { get; init; } = "";

    [UsedImplicitly(ImplicitUseKindFlags.Assign)] // Set by the configuration binder only.
    public bool UseSsl { get; init; }

    [UsedImplicitly(ImplicitUseKindFlags.Assign)] // Set by the configuration binder only.
    public string FromEmail { get; init; } = "no-reply@hr-agency.com";

    [UsedImplicitly(ImplicitUseKindFlags.Assign)] // Set by the configuration binder only.
    public string FromName { get; init; } = "HR Agency Portal";
}
