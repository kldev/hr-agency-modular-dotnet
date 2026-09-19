namespace HrAgencySystem.EmailTemplates.Contracts.Teams;

/// <summary>
/// Sent when somebody changes another person's role on a team. A separate contract from
/// <see cref="SendTeamMemberAdded"/> on purpose: folding both into one record with a nullable
/// previous role would make the template branch on a variable that liquid renders as empty
/// instead of failing when it goes missing.
/// </summary>
public sealed record SendTeamMemberRoleChanged(
    Guid EventId,
    string Source,
    Guid TeamId,
    string TeamName,
    string MemberEmail,
    string MemberFullname,
    string Role,
    string PreviousRole,
    string ChangedByFullname
) : IEmailTemplateContract;
